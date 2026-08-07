# Remove AutoMapper Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove AutoMapper from BaseCrud and require explicit `IDtoMapping` for every CRUD list and details DTO, shipping as breaking major 2.0 while leaving 1.x unchanged for existing consumers.

**Architecture:** Assembly-scan `IDtoMapping` implementations at `AddBaseCrudService`, validate each discovered CRUD service’s `TDto`/`TDtoFull` pairs, register a singleton `IDtoMappingRegistry`, and use it for EF `Select` projection plus compiled insert/update/Entity→DTO maps. Delete AutoMapper packages, profiles, `ICustomMappedDto`, and `IMapper` from constructors/`CrudActionContext`.

**Tech Stack:** .NET 8, EF Core 8, Microsoft.Extensions.DependencyInjection, existing BaseCrud reflection helpers (`ReflectionExtensions`).

**Spec:** `docs/superpowers/specs/2026-08-06-remove-automapper-design.md`

## Global Constraints

- Breaking change → bump package versions to **2.0.0** (BaseCrud, BaseCrud.Abstractions, BaseCrud.EntityFrameworkCore, BaseCrud.ALL; PrimeNg only if its package version tracks the suite)
- **No AutoMapper** package reference or type usage remaining in the solution after Task 5
- **No** AutoMapper compatibility shim / optional fallback package
- **No** Mapperly or other mapping library
- **No** new unit-test project (verify with `dotnet build` + sample apps)
- DTOs keep the existing convention: public property **`Id`** of type `TKey` (used to load the entity on Update)
- Insert/Update mapping expressions are **in-memory only** (always `.Compile()`); only `SelectExpression` is passed to EF `IQueryable.Select`
- graphify rule: after modifying code files, run `graphify update .` if `graphify-out/` exists; if `graph.json` is missing, skip

---

## File Structure

| File | Responsibility |
|---|---|
| `src/BaseCrud.Abstractions/Expressions/IDtoMapping.cs` | **Create** — required mapping contract per Entity+DTO |
| `src/BaseCrud.Abstractions/Expressions/ISelectExpression.cs` | **Delete** in Task 4 — superseded by `IDtoMapping.SelectExpression` |
| `src/BaseCrud.Abstractions/Mapping/IDtoMappingRegistry.cs` | **Create** — resolve compiled maps by entity/DTO types |
| `src/BaseCrud.Abstractions/Mapping/DtoMappingRegistry.cs` | **Create** — registry implementation + compiled caches |
| `src/BaseCrud.Abstractions/Mapping/DtoMappingDiscovery.cs` | **Create** — scan assemblies, detect duplicates, validate CRUD pairs |
| `src/BaseCrud.Abstractions/Mapping/MissingDtoMappingException.cs` | **Create** — startup failure type with pair + service names |
| `src/BaseCrud.Abstractions/Mapping/DuplicateDtoMappingException.cs` | **Create** — startup failure for duplicate pair registrations |
| `src/BaseCrud.Abstractions/BaseCrudService.cs` | **Modify** — replace `AddAutoMapper` with mapping discovery/registry DI |
| `src/BaseCrud.Abstractions/Entities/CrudActionContext.cs` | **Modify** — remove `IMapper` parameter |
| `src/BaseCrud.EntityFrameworkCore/BaseCrudProtectedMethods.cs` | **Modify** — inject registry; rewrite `HandleSelection`; drop AutoMapper usings |
| `src/BaseCrud.EntityFrameworkCore/BaseCrudService.cs` | **Modify** — all `Mapper.Map` / `ProjectTo` / `CrudActionContext` call sites |
| `src/BaseCrud.EntityFrameworkCore/BaseCrudServiceKeyTyped.cs` | **Modify** — ctors take `IDtoMappingRegistry` instead of `IMapper` |
| `src/BaseCrud.EntityFrameworkCore/GlobalUsings.cs` | **Modify** — remove `global using AutoMapper` |
| `src/BaseCrud/BaseCrud.csproj` | **Modify** — remove AutoMapper PackageReference; bump to 2.0.0 |
| `src/BaseCrud/AutoMappers/*` | **Delete** |
| `src/BaseCrud/Entities/ICustomMappedDto.cs` | **Delete** |
| Samples + docs + versions | **Modify** — see Tasks 6–7 |

---

### Task 1: Add `IDtoMapping`

**Files:**
- Create: `src/BaseCrud.Abstractions/Expressions/IDtoMapping.cs`

**Interfaces:**
- Produces:
  - `IDtoMapping<TEntity, TDto, TKey>` with three members below
  - `IDtoMapping<TEntity, TDto>` alias (`TKey = int`)

- [ ] **Step 1: Create `IDtoMapping.cs`**

Match `using` / namespaces from sibling `ISelectExpression.cs` / `IFilterExpression` for `IEntity` and `IDataTransferObject`.

```csharp
using System.Linq.Expressions;
using BaseCrud.Entities;

namespace BaseCrud.Abstractions.Expressions;

public interface IDtoMapping<TEntity, TDto> : IDtoMapping<TEntity, TDto, int>
    where TEntity : IEntity
    where TDto : IDataTransferObject<TEntity, int>;

public interface IDtoMapping<TEntity, TDto, TKey>
    where TKey : struct, IEquatable<TKey>
    where TEntity : IEntity<TKey>
    where TDto : IDataTransferObject<TEntity, TKey>
{
    /// <summary>EF-translatable projection; also compiled for in-memory Entity→DTO.</summary>
    Expression<Func<TEntity, TDto>> SelectExpression { get; }

    /// <summary>In-memory only. Builds a new entity for insert.</summary>
    Expression<Func<TDto, TEntity>> InsertMappingToEntity { get; }

    /// <summary>In-memory only. Produces updated entity values from existing + dto.</summary>
    Expression<Func<TEntity, TDto, TEntity>> UpdateMappingToEntity { get; }
}
```

- [ ] **Step 2: Build Abstractions project**

Run: `dotnet build src/BaseCrud.Abstractions/BaseCrud.Abstractions.csproj`

Expected: PASS

- [ ] **Step 3: Commit**

```bash
git add src/BaseCrud.Abstractions/Expressions/IDtoMapping.cs
git commit -m "$(cat <<'EOF'
Add IDtoMapping contract for required manual DTO mappings.

EOF
)"
```

---

### Task 2: Registry, discovery, and startup exceptions

**Files:**
- Create: `src/BaseCrud.Abstractions/Mapping/IDtoMappingRegistry.cs`
- Create: `src/BaseCrud.Abstractions/Mapping/DtoMappingRegistry.cs`
- Create: `src/BaseCrud.Abstractions/Mapping/DtoMappingDiscovery.cs`
- Create: `src/BaseCrud.Abstractions/Mapping/MissingDtoMappingException.cs`
- Create: `src/BaseCrud.Abstractions/Mapping/DuplicateDtoMappingException.cs`

**Interfaces:**
- Consumes: `IDtoMapping<,,>`; `ReflectionExtensions.GetImplementingTypeWithGenericArguments`; `ICrudService<,,,,>`
- Produces:
  - `IDtoMappingRegistry.Get<TEntity,TDto,TKey>()` → `DtoMappingEntry<TEntity,TDto,TKey>`
  - `DtoMappingDiscovery.Build(assemblies, crudServiceTypes)` → registry or throws
  - `DtoMappingEntry` exposes: `SelectExpression`, `MapToDto`, `InsertToEntity`, `UpdateEntity`

- [ ] **Step 1: Add exception types**

```csharp
namespace BaseCrud.Abstractions.Mapping;

public sealed class MissingDtoMappingException : InvalidOperationException
{
    public MissingDtoMappingException(Type entityType, Type dtoType, Type? serviceType)
        : base(
            serviceType is null
                ? $"No IDtoMapping found for {entityType.Name} → {dtoType.Name}."
                : $"No IDtoMapping found for {entityType.Name} → {dtoType.Name} required by service {serviceType.Name}.")
    {
        EntityType = entityType;
        DtoType = dtoType;
        ServiceType = serviceType;
    }

    public Type EntityType { get; }
    public Type DtoType { get; }
    public Type? ServiceType { get; }
}

public sealed class DuplicateDtoMappingException : InvalidOperationException
{
    public DuplicateDtoMappingException(Type entityType, Type dtoType, Type first, Type second)
        : base(
            $"Duplicate IDtoMapping for {entityType.Name} → {dtoType.Name}: '{first.FullName}' and '{second.FullName}'.")
    {
    }
}
```

(Put each exception in its own file as listed above.)

- [ ] **Step 2: Add `DtoMappingEntry` + `IDtoMappingRegistry`**

```csharp
using System.Linq.Expressions;
using BaseCrud.Entities;

namespace BaseCrud.Abstractions.Mapping;

public sealed class DtoMappingEntry<TEntity, TDto, TKey>
{
    public required Expression<Func<TEntity, TDto>> SelectExpression { get; init; }
    public required Func<TEntity, TDto> MapToDto { get; init; }
    public required Func<TDto, TEntity> InsertToEntity { get; init; }
    public required Func<TEntity, TDto, TEntity> UpdateEntity { get; init; }
}

public interface IDtoMappingRegistry
{
    DtoMappingEntry<TEntity, TDto, TKey> Get<TEntity, TDto, TKey>()
        where TKey : struct, IEquatable<TKey>
        where TEntity : class, IEntity<TKey>
        where TDto : class, IDataTransferObject<TEntity, TKey>;
}
```

- [ ] **Step 3: Implement `DtoMappingRegistry`**

Store entries in `Dictionary<(Type entity, Type dto), object>`.

```csharp
using BaseCrud.Abstractions.Expressions;
using BaseCrud.Entities;

namespace BaseCrud.Abstractions.Mapping;

public sealed class DtoMappingRegistry : IDtoMappingRegistry
{
    private readonly Dictionary<(Type, Type), object> _entries = new();

    public void Add<TEntity, TDto, TKey>(IDtoMapping<TEntity, TDto, TKey> mapping)
        where TKey : struct, IEquatable<TKey>
        where TEntity : class, IEntity<TKey>
        where TDto : class, IDataTransferObject<TEntity, TKey>
    {
        var key = (typeof(TEntity), typeof(TDto));
        _entries[key] = new DtoMappingEntry<TEntity, TDto, TKey>
        {
            SelectExpression = mapping.SelectExpression,
            MapToDto = mapping.SelectExpression.Compile(),
            InsertToEntity = mapping.InsertMappingToEntity.Compile(),
            UpdateEntity = mapping.UpdateMappingToEntity.Compile()
        };
    }

    public DtoMappingEntry<TEntity, TDto, TKey> Get<TEntity, TDto, TKey>()
        where TKey : struct, IEquatable<TKey>
        where TEntity : class, IEntity<TKey>
        where TDto : class, IDataTransferObject<TEntity, TKey>
    {
        if (_entries.TryGetValue((typeof(TEntity), typeof(TDto)), out var entry))
            return (DtoMappingEntry<TEntity, TDto, TKey>)entry;

        throw new MissingDtoMappingException(typeof(TEntity), typeof(TDto), serviceType: null);
    }
}
```

- [ ] **Step 4: Implement `DtoMappingDiscovery.Build`**

Algorithm:

1. Scan all assemblies for closed `IDtoMapping<,,>` via `GetImplementingTypeWithGenericArguments(typeof(IDtoMapping<,,>))`.
2. For each `(implementorType, genericArgs)` with length 3 (`TEntity`, `TDto`, `TKey`):
   - Key = `(TEntity, TDto)`. If key already claimed by a **different** implementor type → `DuplicateDtoMappingException`.
   - Same implementor appearing twice for the same pair is fine (skip).
3. `Activator.CreateInstance(implementorType)` (public parameterless ctor, same as today’s expression classes).
4. Register into `DtoMappingRegistry` (typed `Add` via reflection/`MakeGenericMethod` if needed).
5. For each CRUD service implementation type, read generic args from `ICrudService<,,,,>`:

```csharp
static Type[]? GetCrudServiceGenericArgs(Type serviceType)
{
    var crud = serviceType.GetInterfaces()
        .FirstOrDefault(i => i.IsGenericType
            && i.GetGenericTypeDefinition() == typeof(ICrudService<,,,,>));
    return crud?.GetGenericArguments(); // [TEntity, TDto, TDtoFull, TKey, TUserKey]
}
```

6. Ensure registry contains `(TEntity, TDto)` and `(TEntity, TDtoFull)`. If `TDto == TDtoFull`, one entry covers both. On miss → `MissingDtoMappingException(entity, dto, serviceType)`.
7. Return the filled registry.

Use `BaseCrud.Extensions.ReflectionExtensions`.

- [ ] **Step 5: Build**

Run: `dotnet build src/BaseCrud.Abstractions/BaseCrud.Abstractions.csproj`

Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add src/BaseCrud.Abstractions/Mapping
git commit -m "$(cat <<'EOF'
Add DTO mapping registry and startup discovery/validation.

EOF
)"
```

---

### Task 3: Wire discovery into `AddBaseCrudService`

**Files:**
- Modify: `src/BaseCrud.Abstractions/BaseCrudService.cs`

**Interfaces:**
- Consumes: `DtoMappingDiscovery.Build`
- Produces: `IDtoMappingRegistry` registered as singleton; AutoMapper registration removed from this method

- [ ] **Step 1: Replace `AddAutoMapper()` with mapping registration**

In `AddBaseCrudService`:

1. Keep `DiscoverAndRegisterCrudServices()`.
2. Remove call to `AddAutoMapper()` and delete the private `AddAutoMapper` method.
3. Remove `using BaseCrud.AutoMappers` and any `ILogger<DtoMapperProfile>` usage.
4. Refactor discovery so you have the list of CRUD service **implementation** `Type`s (return them from `DiscoverAndRegisterCrudServices` or scan again with the same helper).
5. Call:

```csharp
IDtoMappingRegistry registry = DtoMappingDiscovery.Build(_options.Assemblies, crudServiceImplementationTypes);
_services.AddSingleton(registry);
```

6. Update XML doc remarks: remove AutoMapper / `ICustomMappedDto` bullets; document `IDtoMapping` scan + startup validation.

- [ ] **Step 2: Build Abstractions**

Run: `dotnet build src/BaseCrud.Abstractions/BaseCrud.Abstractions.csproj`

Expected: PASS

- [ ] **Step 3: Commit**

```bash
git add src/BaseCrud.Abstractions/BaseCrudService.cs
git commit -m "$(cat <<'EOF'
Register IDtoMappingRegistry during AddBaseCrudService startup.

EOF
)"
```

---

### Task 4: Rewire EF `BaseCrudService` off AutoMapper

**Files:**
- Modify: `src/BaseCrud.EntityFrameworkCore/BaseCrudProtectedMethods.cs`
- Modify: `src/BaseCrud.EntityFrameworkCore/BaseCrudService.cs`
- Modify: `src/BaseCrud.EntityFrameworkCore/BaseCrudServiceKeyTyped.cs`
- Modify: `src/BaseCrud.EntityFrameworkCore/GlobalUsings.cs`
- Modify: `src/BaseCrud.Abstractions/Entities/CrudActionContext.cs`
- Delete: `src/BaseCrud.Abstractions/Expressions/ISelectExpression.cs` (after `HandleSelection` no longer uses it)

**Interfaces:**
- Consumes: `IDtoMappingRegistry.Get<TEntity,TDto,TKey>()` and `Get<TEntity,TDtoFull,TKey>()`
- Produces: AutoMapper-free CRUD read/write paths

- [ ] **Step 1: Remove `IMapper` from `CrudActionContext`**

```csharp
namespace BaseCrud.Abstractions.Entities;

public record CrudActionContext<TEntity, TKey, TUserKey>(
    IQueryable<TEntity> Queryable,
    IUserProfile<TUserKey>? UserProfile,
    IDataTableMetaData? DataTableMetaData,
    CancellationToken CancellationToken
)
    where TKey : struct, IEquatable<TKey>
    where TEntity : IEntity<TKey>
    where TUserKey : struct, IEquatable<TUserKey>;

public record CrudActionContext<TEntity, TUserKey>(
    IQueryable<TEntity> Queryable,
    IUserProfile<TUserKey>? UserProfile,
    IDataTableMetaData? DataTableMetaData,
    CancellationToken CancellationToken
)
    : CrudActionContext<TEntity, int, TUserKey>(
        Queryable,
        UserProfile,
        DataTableMetaData,
        CancellationToken
    )
    where TEntity : IEntity<int>
    where TUserKey : struct, IEquatable<TUserKey>;
```

Update **every** `new CrudActionContext<...>(..., Mapper, ...)` call site in EF files to drop the `Mapper` argument.

- [ ] **Step 2: Change base ctor to take `IDtoMappingRegistry`**

In `BaseCrudProtectedMethods.cs`:

```csharp
protected readonly DbContext DbContext;
protected readonly IDtoMappingRegistry MappingRegistry;

protected BaseCrudService(
    DbContext dbContext,
    IDtoMappingRegistry mappingRegistry
)
{
    MappingRegistry = mappingRegistry;
    DbContext = dbContext;
    Set = DbContext.Set<TEntity>();
    QueryableOfActive = Set.Where(e => e.Active);
    QueryableOfUntrackedActive = QueryableOfActive.AsNoTracking();
}
```

Update `BaseCrudServiceKeyTyped.cs` primary constructors the same way. Remove `using AutoMapper.QueryableExtensions` and `global using AutoMapper`.

- [ ] **Step 3: Rewrite `HandleSelection`**

```csharp
protected IQueryable<TDto> HandleSelection(IQueryable<TEntity> query)
{
    var mapping = MappingRegistry.Get<TEntity, TDto, TKey>();
    return query.Select(mapping.SelectExpression);
}
```

- [ ] **Step 4: Rewrite read paths that used `ProjectTo`**

```csharp
var mapping = MappingRegistry.Get<TEntity, TDtoFull, TKey>();
IAsyncEnumerable<TDtoFull> result = query.Select(mapping.SelectExpression).AsAsyncEnumerable();
```

```csharp
TDtoFull? result = await query
    .Select(mapping.SelectExpression)
    .FirstOrDefaultAsync(cancellationToken);
```

- [ ] **Step 5: Rewrite `InsertAsync(TDtoFull)`**

```csharp
var mapping = MappingRegistry.Get<TEntity, TDtoFull, TKey>();
TEntity mapped = mapping.InsertToEntity(entity);
// existing CheckInsertValidity + HandleInsertAsync
TDtoFull dto = mapping.MapToDto(insertResult.Result!);
return dto;
```

- [ ] **Step 6: Rewrite `UpdateAsync(TDtoFull)`**

```csharp
var mapping = MappingRegistry.Get<TEntity, TDtoFull, TKey>();
TKey id = GetDtoId(entity);

ServiceResult validationResult = await CheckUpdateValidityAsync(id, cancellationToken);
if (!validationResult.IsSuccess)
    return validationResult;

TEntity? existing = await Set.FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);
if (existing is null)
    return NotFound(new NotFoundServiceError());

TEntity updatedValues = mapping.UpdateEntity(existing, entity);
DbContext.Entry(existing).CurrentValues.SetValues(updatedValues);

ServiceResult<EntityEntry<TEntity>> updateResult = await HandleUpdateAsync(existing, cancellationToken);
// ... existing failure handling ...
return mapping.MapToDto(updateResult.Result!.Entity);
```

Read `HandleUpdateAsync` before editing. If it expects a detached entity, keep equivalent behavior but prefer tracked entity + `SetValues` then save. Force key stability after `SetValues` if needed (`existing.Id = id`).

Helper:

```csharp
private static TKey GetDtoId(TDtoFull dto)
{
    var prop = typeof(TDtoFull).GetProperty("Id")
        ?? throw new InvalidOperationException(
            $"{typeof(TDtoFull).Name} must have a public Id property of type {typeof(TKey).Name}.");
    return (TKey)prop.GetValue(dto)!;
}
```

- [ ] **Step 7: Replace remaining `Mapper.Map<TDtoFull>(entity)` (PatchUpdate paths)**

```csharp
return MappingRegistry.Get<TEntity, TDtoFull, TKey>().MapToDto(entity);
```

- [ ] **Step 8: Delete `ISelectExpression.cs`**

Remove the file. Sample references are fixed in Task 6.

- [ ] **Step 9: Build EF + Abstractions**

Run:

```bash
dotnet build src/BaseCrud.EntityFrameworkCore/BaseCrud.EntityFrameworkCore.csproj
```

Expected: PASS for library projects. Samples may still fail until Task 6.

- [ ] **Step 10: Commit**

```bash
git add src/BaseCrud.Abstractions/Entities/CrudActionContext.cs \
  src/BaseCrud.Abstractions/Expressions \
  src/BaseCrud.EntityFrameworkCore
git commit -m "$(cat <<'EOF'
Rewire BaseCrudService to IDtoMappingRegistry and drop IMapper.

EOF
)"
```

---

### Task 5: Remove AutoMapper artifacts and package reference

**Files:**
- Delete: `src/BaseCrud/AutoMappers/DtoMapperProfile.cs`
- Delete: `src/BaseCrud/AutoMappers/AutoMapperExtensions.cs`
- Delete: `src/BaseCrud/Entities/ICustomMappedDto.cs`
- Modify: `src/BaseCrud/BaseCrud.csproj` — remove AutoMapper `PackageReference`

- [ ] **Step 1: Delete AutoMapper-related library files listed above**

- [ ] **Step 2: Remove AutoMapper `PackageReference` from `BaseCrud.csproj`**

- [ ] **Step 3: Grep library leftovers**

Run: `rg -n "AutoMapper|IMapper|ICustomMappedDto|ISelectExpression" src/BaseCrud src/BaseCrud.Abstractions src/BaseCrud.EntityFrameworkCore`

Expected: no matches

- [ ] **Step 4: Build core libraries**

Run:

```bash
dotnet build src/BaseCrud/BaseCrud.csproj
dotnet build src/BaseCrud.EntityFrameworkCore/BaseCrud.EntityFrameworkCore.csproj
```

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add -u src/BaseCrud src/BaseCrud.Abstractions src/BaseCrud.EntityFrameworkCore
git commit -m "$(cat <<'EOF'
Remove AutoMapper package and related BaseCrud mapping types.

EOF
)"
```

---

### Task 6: Update Tester and WebTester samples

**Files:**
- Modify: `src/Tester/Model.cs`
- Modify: `src/Tester/ModelDetailsDto.cs`
- Modify: `src/Tester/Service.cs`
- Modify: `src/Tester/Tester.csproj` (remove AutoMapper if present)
- Modify: `src/WebTester/Services/WeatherService.cs`
- Create: `src/WebTester` expressions class for WeatherForecast DTOs (path of your choosing under WebTester, e.g. `Expressions/WeatherForecastExpressions.cs`)
- Modify: `src/WebTester/WebTester.csproj` if it references AutoMapper
- Modify: `src/WebTester/Program.cs` only if assemblies list must include the new expressions type’s assembly (usually already the WebTester assembly)

**Interfaces:**
- Consumes: `IDtoMapping<,>` / `IDtoMappingRegistry`
- Produces: compiling sample apps

- [ ] **Step 1: Update `ModelExpressions` in Tester**

Replace `ISelectExpression<Model, ModelDto>` with `IDtoMapping` for **both** DTOs. Use explicit interface implementation when two mappings share member names:

```csharp
public sealed class ModelExpressions :
    IDtoMapping<Model, ModelDto>,
    IDtoMapping<Model, ModelDetailsDto>,
    IGlobalFilterExpression<Model>,
    IFilterExpression<Model>
{
    Expression<Func<Model, ModelDto>> IDtoMapping<Model, ModelDto, int>.SelectExpression =>
        model => new ModelDto { Id = model.Id, Name = model.Name, Age = model.Age };

    Expression<Func<ModelDto, Model>> IDtoMapping<Model, ModelDto, int>.InsertMappingToEntity =>
        dto => new Model { Name = dto.Name, Age = dto.Age };

    Expression<Func<Model, ModelDto, Model>> IDtoMapping<Model, ModelDto, int>.UpdateMappingToEntity =>
        (entity, dto) => new Model
        {
            Id = entity.Id,
            Name = dto.Name,
            Age = dto.Age,
            Surname = entity.Surname,
            Patronymic = entity.Patronymic,
            Address = entity.Address,
            Email = entity.Email,
            Phone = entity.Phone,
            Active = entity.Active
        };

    Expression<Func<Model, ModelDetailsDto>> IDtoMapping<Model, ModelDetailsDto, int>.SelectExpression =>
        model => new ModelDetailsDto
        {
            Id = model.Id,
            Name = model.Name,
            Surname = model.Surname,
            Patronymic = model.Patronymic,
            Fullname = ((model.Surname ?? "") + " " + (model.Name ?? "") + " " + (model.Patronymic ?? "")).Trim(),
            Age = model.Age.ToString(),
            Address = model.Address,
            Email = model.Email,
            Phone = model.Phone
        };

    Expression<Func<ModelDetailsDto, Model>> IDtoMapping<Model, ModelDetailsDto, int>.InsertMappingToEntity =>
        dto => new Model
        {
            Name = dto.Name,
            Surname = dto.Surname,
            Patronymic = dto.Patronymic,
            Address = dto.Address,
            Email = dto.Email,
            Phone = dto.Phone
        };

    Expression<Func<Model, ModelDetailsDto, Model>> IDtoMapping<Model, ModelDetailsDto, int>.UpdateMappingToEntity =>
        (entity, dto) => new Model
        {
            Id = entity.Id,
            Name = dto.Name,
            Surname = dto.Surname,
            Patronymic = dto.Patronymic,
            Address = dto.Address,
            Email = dto.Email,
            Phone = dto.Phone,
            Age = entity.Age,
            Active = entity.Active
        };

    // keep existing GlobalSearchExpression + FilterExpressions
}
```

- [ ] **Step 2: Strip `ICustomMappedDto` from `ModelDetailsDto`**

Remove AutoMapper usings and static map methods; keep properties.

- [ ] **Step 3: Update `Service` ctor**

```csharp
public class Service(AppDbContext dbContext, IDtoMappingRegistry mappingRegistry)
    : BaseCrudService<Model, ModelDto, ModelDetailsDto>(dbContext, mappingRegistry), IService
{
}
```

Adjust to the actual base class arity used today.

- [ ] **Step 4: Update WebTester**

- `WeatherService` ctor: `IDtoMappingRegistry` instead of `IMapper`.
- Add `WeatherForecastExpressions` implementing `IDtoMapping` for `WeatherForecastDto` and `WeatherForecastDetailsDto` (map fields from the existing model/DTO types).
- Ensure that class’s assembly is in `AddBaseCrudService` `Assemblies`.

- [ ] **Step 5: Remove AutoMapper package refs from sample csproj files if present**

- [ ] **Step 6: Build full solution**

Run: `dotnet build` against the solution under `src/` (locate `.sln` via `ls src/*.sln`).

Expected: PASS

- [ ] **Step 7: Smoke-check startup validation**

Temporarily break one mapping registration, run Tester, confirm `MissingDtoMappingException` at startup, then restore.

- [ ] **Step 8: Commit**

```bash
git add src/Tester src/WebTester
git commit -m "$(cat <<'EOF'
Migrate sample apps to required IDtoMapping registrations.

EOF
)"
```

---

### Task 7: Docs, migration guide, version bump to 2.0.0

**Files:**
- Modify: `docs/docs/getting-started.md`
- Modify: `docs/docs/introduction.md` (only if it mentions AutoMapper)
- Create: `docs/docs/migration-2.0.md`
- Modify: `docs/docs/toc.yml` to include the migration page
- Modify versions to `2.0.0`:
  - `src/BaseCrud/BaseCrud.csproj`
  - `src/BaseCrud.Abstractions/BaseCrud.Abstractions.csproj`
  - `src/BaseCrud.EntityFrameworkCore/BaseCrud.EntityFrameworkCore.csproj`
  - `src/BaseCrud.ALL/BaseCrud.ALL.csproj`

- [ ] **Step 1: Rewrite getting-started mapping section**

- Developers **must** implement `IDtoMapping<TEntity, TDto>` for list and details DTOs
- Show a minimal expressions class (select + insert + update)
- Service ctor uses `IDtoMappingRegistry` instead of `IMapper`
- Note: missing mappings fail at `AddBaseCrudService` startup

- [ ] **Step 2: Write `migration-2.0.md`**

1. Stay on 1.x if you cannot migrate yet  
2. Remove AutoMapper / `IMapper` from service ctors — inject `IDtoMappingRegistry`  
3. Implement `IDtoMapping` for each list and details DTO  
4. Replace `ICustomMappedDto` / profiles with insert/update expressions  
5. Remove any `CrudActionContext.Mapper` usage  
6. `ISelectExpression` removed — use `IDtoMapping.SelectExpression`

- [ ] **Step 3: Bump package versions to `2.0.0`**

- [ ] **Step 4: Final solution build**

Run: `dotnet build`

Expected: PASS

- [ ] **Step 5: `graphify update .`** if `graphify-out/graph.json` exists; otherwise skip

- [ ] **Step 6: Commit**

```bash
git add docs src/BaseCrud/BaseCrud.csproj \
  src/BaseCrud.Abstractions/BaseCrud.Abstractions.csproj \
  src/BaseCrud.EntityFrameworkCore/BaseCrud.EntityFrameworkCore.csproj \
  src/BaseCrud.ALL/BaseCrud.ALL.csproj
git commit -m "$(cat <<'EOF'
Document AutoMapper removal and bump packages to 2.0.0.

EOF
)"
```

---

## Self-Review (plan vs spec)

| Spec requirement | Task |
|---|---|
| Full AutoMapper removal | 4, 5 |
| `IDtoMapping` with Select + Insert + Update | 1 |
| Required for each `TDto` and `TDtoFull` | 2, 3 |
| `SelectExpression.Compile()` for Entity→DTO | 2 (`MapToDto`) |
| Startup fail missing/duplicate | 2, 3 |
| Assembly scan + registry | 2, 3 |
| Update loads existing + apply mapping | 4 Step 6 |
| Remove `ICustomMappedDto`, profiles, `IMapper`, context mapper | 4, 5 |
| Remove `ISelectExpression` | 4 Step 8 |
| Major 2.0; 1.x stays; migration docs | 7 |
| Samples updated; no new test project | 6 |
| No AutoMapper shim / Mapperly | honored throughout |
