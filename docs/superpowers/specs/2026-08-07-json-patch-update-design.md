# JSON Patch Update — Design Spec

**Date:** 2026-08-07  
**Status:** Approved for implementation planning  
**Impact:** Additive API on `IEfCrudService` / `BaseCrudService`; new package dependency on `BaseCrud.EntityFrameworkCore`

## Problem

BaseCrud already supports:

- Full DTO/entity update via `UpdateAsync`
- Expression-based partial update via `PatchUpdateAsync` + EF `ExecuteUpdate` / `SetProperty`

Callers who want standard HTTP JSON Patch (RFC 6902) must hand-map request bodies to `SetProperty` calls (as WebTester does today with `WeatherForecastPatchRequest`). There is no first-class way to accept a `JsonPatchDocument` and apply it through the existing DTO mapping pipeline.

## Goals

- Accept RFC 6902 JSON Patch against `TDtoFull`, then persist via existing `UpdateAsync(TDtoFull)`
- Expose as a `PatchUpdateAsync` overload (no new method name; no conflict with `SetProperty` overloads)
- Support ops: `add`, `remove`, `replace`, `test`
- Keep entity `Id` immutable (route/id parameter wins; `/id` ops rejected)
- Failures return `ServiceResult` `BadRequest` / existing status patterns — not unhandled exceptions to callers
- Demonstrate usage in WebTester (`application/json-patch+json`)

## Non-goals

- RFC 7396 JSON Merge Patch
- `move` / `copy` operations
- Applying patches directly to entities (bypass DTO)
- Translating JSON Patch into `ExecuteUpdate` / `SetProperty`
- Adding the overload to `ICrudService` (EF-only, alongside existing patch APIs)
- Adding a dedicated unit-test project (samples remain primary verification)

## Approach

**Patch DTO, then reuse `UpdateAsync(TDtoFull)` (chosen).**

1. Validate id / load entity  
2. Map entity → `TDtoFull`  
3. Guard unsupported / id-targeting ops  
4. Apply `JsonPatchDocument<TDtoFull>` to the DTO  
5. Call existing `UpdateAsync(dto, userProfile, cancellationToken)`

Rejected alternatives:

- Dedicated patch persistence pipeline (duplicates update flow; drifts from `UpdateAsync`)
- Translate patch ops to `ExecuteUpdate` (poor fit for nested/`add`/`remove`/`test`)

Trade-off accepted for v1: possible double load (patch path loads entity; `UpdateAsync` loads again). Prefer correctness and reuse over micro-optimization.

## Architecture

```
PATCH body (JsonPatchDocument<TDtoFull>)
  → IEfCrudService.PatchUpdateAsync(id, patch, userProfile, ct)
      → CheckUpdateValidityAsync(id)
      → load TEntity (NotFound if missing)
      → MapToDto → TDtoFull
      → reject /id ops and move/copy
      → ApplyTo(dto) → BadRequest on failure
      → UpdateAsync(dto, …)   // existing SetValues + HandleUpdateAsync path
  → ServiceResult<TDtoFull>
```

Existing expression-based `PatchUpdateAsync` overloads remain unchanged for bulk/`ExecuteUpdate` scenarios.

## Components

### Package

Add `Microsoft.AspNetCore.JsonPatch` to `BaseCrud.EntityFrameworkCore` (aligned with `net8.0`).

### API

On `IEfCrudService` and `BaseCrudService`:

```csharp
Task<ServiceResult<TDtoFull>> PatchUpdateAsync(
    TKey id,
    JsonPatchDocument<TDtoFull> patch,
    IUserProfile<TUserKey>? userProfile,
    CancellationToken cancellationToken = default);
```

### Guards (before `ApplyTo`)

| Rule | Behavior |
|------|----------|
| `patch` is null | `ArgumentNullException` (same as existing `SetProperty` overload) |
| Any operation whose `path` (or `from`, if present) targets the DTO id property via JSON Pointer `/id` (segment match case-insensitive) | `BadRequest` + `ValidationServiceError` |
| Op type `move` or `copy` | `BadRequest` + `ValidationServiceError` |
| Empty patch document (`Operations` empty) | Allowed; proceeds to `UpdateAsync` (no field changes if DTO unchanged) |

Id path matching: treat pointer `/id` and `/Id` as id; nested paths like `/address/id` are **not** treated as the entity key and are allowed if present on the DTO.

### Apply

- Use `JsonPatchDocument.ApplyTo` with an error/exception path that converts failures (`test` mismatch, unknown path, type errors) into `BadRequest` + `ValidationServiceError` (include useful message text).
- Do not leave a half-applied DTO persisted: only call `UpdateAsync` after a successful apply.

### Error keys

Extend `ErrorKeys.Validation` with:

| Constant | Value |
|----------|--------|
| `ErrorKeys.Validation.JsonPatch.IdImmutable` | `validation.jsonPatch.idImmutable` |
| `ErrorKeys.Validation.JsonPatch.UnsupportedOperation` | `validation.jsonPatch.unsupportedOperation` |
| `ErrorKeys.Validation.JsonPatch.ApplyFailed` | `validation.jsonPatch.applyFailed` |

### WebTester sample

- Replace `WeatherForecastPatchRequest` + hand-written `SetProperty` PATCH action with `[FromBody] JsonPatchDocument<WeatherForecastDetailsDto>` calling the new overload.
- Update `WebTester.http` to send `Content-Type: application/json-patch+json` and an ops array.
- Ensure model binding works for `JsonPatchDocument` (add Newtonsoft JSON input formatter if the default STJ pipeline does not bind patch documents correctly on this stack).

## Data flow

1. Controller binds JSON Patch document  
2. Service validates id and loads entity  
3. Registry `MapToDto` produces current full DTO  
4. Guards run over `patch.Operations`  
5. Patch applied in memory to DTO  
6. `UpdateAsync(TDtoFull)` runs: re-validate, load, `UpdateEntity`, `SetValues`, `HandleUpdateAsync`, map result DTO  

## Error handling

| Case | Result |
|------|--------|
| Id pre-check fail | Existing `CheckUpdateValidityAsync` result |
| Entity not found | `NotFound` + `NotFoundServiceError` |
| `/id` targeted | `BadRequest` + id-immutable validation error |
| `move` / `copy` | `BadRequest` + unsupported-op validation error |
| Apply failure | `BadRequest` + apply-failed validation error |
| Persist / mapping failure | Same as `UpdateAsync` today |

## Testing

- Manual: WebTester PATCH with replace/add/remove/test; assert `/id` and `move`/`copy` return 400; assert unknown path returns 400; assert successful replace updates and returns DTO.
- No new test project in this feature’s scope.

## Open decisions resolved

| Decision | Choice |
|----------|--------|
| Patch format | RFC 6902 only |
| Apply target | `TDtoFull`, then mapping |
| Ops | `add`, `remove`, `replace`, `test` |
| Id mutability | Forbidden |
| API shape | `PatchUpdateAsync` overload |
| Document type | `JsonPatchDocument<TDtoFull>` |
| Failure surface | `ServiceResult` BadRequest |
| Persistence | Reuse `UpdateAsync(TDtoFull)` |
