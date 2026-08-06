# Remove AutoMapper — Design Spec

**Date:** 2026-08-06  
**Status:** Approved for implementation planning  
**Impact:** Breaking change → major version bump (1.x stays as-is for existing consumers)

## Problem

AutoMapper is becoming a paid dependency for newer .NET ecosystems. BaseCrud currently depends on AutoMapper for:

- Query projection (`ProjectTo`) when `ISelectExpression` is absent
- In-memory Entity ↔ DTO mapping on Insert/Update/Get paths (`Mapper.Map`)
- Optional custom maps via `ICustomMappedDto`
- DI registration (`AddAutoMapper` / `DtoMapperProfile`)
- `IMapper` on service constructors and `CrudActionContext`

The library already has a manual projection path (`ISelectExpression`). The goal is to remove AutoMapper entirely and make explicit DTO mappings required.

## Goals

- Zero AutoMapper package references and types in BaseCrud public/internal APIs
- Required manual mappings for every CRUD `TDto` and `TDtoFull`
- Clear startup failure when a required mapping is missing
- Preserve 1.x for users who cannot migrate yet; document migration for 2.0

## Non-goals

- Keeping an AutoMapper compatibility shim or optional fallback package
- Introducing Mapperly or another mapping library
- Changing filter / global-search expression design beyond what mapping removal requires
- Adding a full unit-test project (samples remain the primary verification surface unless planned separately)

## Approach

**Assembly scan + registry (chosen).** On `AddBaseCrudService`, discover `IDtoMapping` implementations, validate every registered CRUD service’s DTO pair, register a singleton registry, and use it in `BaseCrudService` instead of `IMapper`.

Rejected alternatives:

- Explicit-only `AddDtoMapping<T>()` registration (more boilerplate; still needs startup validation)
- Source-generator / Mapperly adapter (conflicts with “manual required expressions”)

## Architecture

```
AddBaseCrudService
  → discover ICrudService implementations (existing)
  → discover IDtoMapping implementations (new)
  → validate TDto + TDtoFull mappings exist for each service
  → register IDtoMappingRegistry (singleton)
  → BaseCrudService uses registry (no IMapper)

Removed:
  AutoMapper package, DtoMapperProfile, ICustomMappedDto,
  AutoMapperExtensions, IMapper ctor params, CrudActionContext.Mapper
```

## Components

### `IDtoMapping<TEntity, TDto, TKey>`

Single interface per Entity+DTO pair (replaces optional `ISelectExpression` for CRUD DTO projection and AutoMapper maps):

| Member | Role |
|---|---|
| `SelectExpression` | `Expression<Func<TEntity, TDto>>` — EF `Select` + Entity→DTO via cached `.Compile()` |
| `InsertMappingToEntity` | `Expression<Func<TDto, TEntity>>` — in-memory only (compiled); build new entity for insert |
| `UpdateMappingToEntity` | `Expression<Func<TEntity, TDto, TEntity>>` — in-memory only (compiled); apply DTO onto existing entity |

Provide `IDtoMapping<TEntity, TDto>` non-key overload (`TKey = int`) consistent with existing expression interfaces.

`ISelectExpression` is superseded for CRUD DTO selection by `IDtoMapping.SelectExpression`. Migration: fold select into `IDtoMapping`; remove or obsolete `ISelectExpression` in 2.0 (prefer remove to avoid two ways to do the same thing).

### `IDtoMappingRegistry`

- Resolve mapping by `(TEntity, TDto)` / `(TEntity, TDto, TKey)`
- Cache compiled delegates: `Select` → `Func<TEntity,TDto>`, insert/update funcs
- Used by `HandleSelection`, Insert(DTO), Update(DTO), and DTO return mapping after persist

### Startup validator

For each discovered CRUD service generic args `TEntity`, `TDto`, `TDtoFull`:

- Require mapping for `TDto`
- Require mapping for `TDtoFull` (even when `TDto` and `TDtoFull` are the same type — one mapping satisfies both)
- Fail on missing mapping with an exception message naming the entity/DTO pair and related service type
- Fail on duplicate mapping types for the same pair

### Consumer shape

One expressions class may implement multiple mappings (same pattern as today’s `ModelExpressions`):

```csharp
public sealed class ModelExpressions :
    IDtoMapping<Model, ModelDto>,
    IDtoMapping<Model, ModelDetailsDto>,
    IFilterExpression<Model>
{
    // SelectExpression + InsertMappingToEntity + UpdateMappingToEntity per DTO
}
```

### Service constructor

`BaseCrudService` / derived services no longer take `IMapper`. Typical ctor: `DbContext` (+ logger where already used). Inject `IDtoMappingRegistry` into the base (from DI) rather than requiring every derived service to pass mappings.

### `CrudActionContext`

Remove `IMapper`. Do not add registry to the context unless a concrete consumer need appears; keep the context focused on query + user + cancellation.

## Data flow

### Startup

1. Register CRUD services (existing scan)
2. Scan assemblies for `IDtoMapping<,,>` implementors
3. Validate required pairs; throw if invalid
4. Build registry with compiled caches
5. Register registry in DI

### Read (list / get-by-id → DTO)

`IQueryable<TEntity>` → `Select(mapping.SelectExpression)` → materialize. No `ProjectTo`.

### Insert(`TDtoFull`)

1. `InsertMappingToEntity` (compiled) → `TEntity`
2. Existing insert validation + `HandleInsertAsync`
3. Map result entity → `TDtoFull` via compiled `SelectExpression`

### Update(`TDtoFull`)

Improves on current AutoMapper behavior (which mapped DTO → full entity without an explicit existing-entity apply step):

1. Load existing entity by id (or fail NotFound / existing validation)
2. Apply `UpdateMappingToEntity(existing, dto)`
3. Persist via existing update handling
4. Map result → `TDtoFull` via compiled `SelectExpression`

### Insert/Update(`TEntity`)

Unchanged: no DTO mapping involved.

## Error handling

| Case | Behavior |
|---|---|
| Missing `IDtoMapping` for a service DTO | Startup exception; message includes pair + service |
| Duplicate mapping for same pair | Startup exception |
| `SelectExpression` not EF-translatable | EF runtime exception (consumer responsibility) |
| Update target missing | Existing NotFound / validation path |
| 1.x consumers | Unaffected; stay on 1.x packages |

## Versioning & migration (1.x users)

- Ship as **major 2.0** breaking release
- **1.x remains** the AutoMapper-based line; no dual-mode in one package
- Publish migration notes covering:
  1. Remove AutoMapper registration assumptions / `IMapper` from service ctors
  2. Implement `IDtoMapping` for each list and details DTO
  3. Replace `ICustomMappedDto` / AutoMapper profiles with insert/update expressions
  4. Replace any direct `CrudActionContext.Mapper` usage
  5. Update docs (`getting-started.md`) that currently say BaseCrud auto-registers AutoMapper maps

## Testing / verification

- Update `Tester` and `WebTester` to `IDtoMapping`; drop `IMapper` from service ctors
- Manually verify: missing mapping fails startup; happy-path list/get/insert/update with DTO; `TEntity` overloads still work
- No new test project required by this spec

## Scope checklist for implementation plan

1. Add `IDtoMapping` + registry + startup validation
2. Rewire EF `BaseCrudService` / protected methods off AutoMapper
3. Remove AutoMapper artifacts and package references
4. Update samples + docs + changelog / migration guide
5. Bump package versions to 2.0.0
