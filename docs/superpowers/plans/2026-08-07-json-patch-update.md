# JSON Patch Update Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add an RFC 6902 `JsonPatchDocument<TDtoFull>` overload of `PatchUpdateAsync` that patches the details DTO then persists via existing `UpdateAsync(TDtoFull)`, and demonstrate it in WebTester.

**Architecture:** Guard `/id` and `move`/`copy`, apply the patch in memory to a mapped `TDtoFull`, convert apply failures to `ServiceResult` BadRequest, then call `UpdateAsync`. Expression-based `PatchUpdateAsync` overloads stay unchanged.

**Tech Stack:** .NET 8, EF Core 8, `Microsoft.AspNetCore.JsonPatch` 8.0.x, WebTester with Newtonsoft JSON input for `JsonPatchDocument` binding.

**Spec:** `docs/superpowers/specs/2026-08-07-json-patch-update-design.md`

## Global Constraints

- Additive API only — do **not** change existing `SetProperty` / `ExecuteUpdate` `PatchUpdateAsync` overloads
- Ops supported: **`add`**, **`remove`**, **`replace`**, **`test`** — reject **`move`** and **`copy`**
- Entity key immutable: JSON Pointer **`/id`** / **`/Id`** only (not nested `/address/id`)
- Failures → `ServiceResult` **`BadRequest`** + `ValidationServiceError` (no exceptions to callers)
- Persist only after successful apply; reuse **`UpdateAsync(TDtoFull)`**
- **No** new unit-test project (verify with `dotnet build` + WebTester manual PATCH)
- Package versions stay **2.0.0** unless a patch bump is required for the new dependency alone (prefer leave 2.0.0)
- After code changes: `graphify update .` from repo root if `graphify-out/graph.json` exists; never create `src/graphify-out`
- Always run graphify / build from **repo root** (`/home/user01/RiderProjects/BaseCrud`)

---

## File Structure

| File | Responsibility |
|---|---|
| `src/BaseCrud/Errors/Keys/ErrorKeys.Validation.JsonPatch.cs` | **Create** — error key constants for JSON Patch |
| `src/BaseCrud.EntityFrameworkCore/BaseCrud.EntityFrameworkCore.csproj` | **Modify** — add `Microsoft.AspNetCore.JsonPatch` 8.0.4 |
| `src/BaseCrud.EntityFrameworkCore/JsonPatch/JsonPatchOperationGuards.cs` | **Create** — id-path + unsupported-op checks |
| `src/BaseCrud.EntityFrameworkCore/Services/IEfCrudService.cs` | **Modify** — declare new overload |
| `src/BaseCrud.EntityFrameworkCore/BaseCrudService.cs` | **Modify** — implement overload after existing patch methods |
| `src/BaseCrud.EntityFrameworkCore/GlobalUsings.cs` | **Modify** — optional `global using Microsoft.AspNetCore.JsonPatch` |
| `src/WebTester/WebTester.csproj` | **Modify** — `Microsoft.AspNetCore.Mvc.NewtonsoftJson` for patch binding |
| `src/WebTester/Program.cs` | **Modify** — `AddNewtonsoftJson()` |
| `src/WebTester/Controllers/WeatherForecastController.cs` | **Modify** — PATCH uses `JsonPatchDocument`; remove `WeatherForecastPatchRequest` |
| `src/WebTester/WebTester.http` | **Modify** — JSON Patch example requests |

---

### Task 1: Error keys + JsonPatch package

**Files:**
- Create: `src/BaseCrud/Errors/Keys/ErrorKeys.Validation.JsonPatch.cs`
- Modify: `src/BaseCrud.EntityFrameworkCore/BaseCrud.EntityFrameworkCore.csproj`

**Interfaces:**
- Consumes: existing `ErrorKeys.Validation` partial (`Prefix = "validation."` in `ErrorKeys.Validation.cs`)
- Produces: `ErrorKeys.Validation.JsonPatch.{IdImmutable,UnsupportedOperation,ApplyFailed}`

- [ ] **Step 1: Create error keys file**

```csharp
namespace BaseCrud.Errors.Keys;

public static partial class ErrorKeys
{
    public static partial class Validation
    {
        public static class JsonPatch
        {
            private const string JsonPatchPrefix = Prefix + "jsonPatch.";

            public const string IdImmutable = JsonPatchPrefix + "idImmutable";

            public const string UnsupportedOperation = JsonPatchPrefix + "unsupportedOperation";

            public const string ApplyFailed = JsonPatchPrefix + "applyFailed";
        }
    }
}
```

- [ ] **Step 2: Add package reference**

In `src/BaseCrud.EntityFrameworkCore/BaseCrud.EntityFrameworkCore.csproj`, inside the existing `ItemGroup` that has EF Core (or a sibling `ItemGroup`):

```xml
<PackageReference Include="Microsoft.AspNetCore.JsonPatch" Version="8.0.4" />
```

- [ ] **Step 3: Restore and build**

Run from repo root:

```bash
dotnet restore src/BaseCrud.EntityFrameworkCore/BaseCrud.EntityFrameworkCore.csproj
dotnet build src/BaseCrud/BaseCrud.csproj
dotnet build src/BaseCrud.EntityFrameworkCore/BaseCrud.EntityFrameworkCore.csproj
```

Expected: PASS (package restores; keys compile)

- [ ] **Step 4: Commit**

```bash
git add src/BaseCrud/Errors/Keys/ErrorKeys.Validation.JsonPatch.cs \
  src/BaseCrud.EntityFrameworkCore/BaseCrud.EntityFrameworkCore.csproj
git commit -m "$(cat <<'EOF'
Add JSON Patch validation error keys and AspNetCore.JsonPatch package.

EOF
)"
```

---

### Task 2: Operation guards helper

**Files:**
- Create: `src/BaseCrud.EntityFrameworkCore/JsonPatch/JsonPatchOperationGuards.cs`

**Interfaces:**
- Consumes: `Microsoft.AspNetCore.JsonPatch.Operations.Operation<TModel>`, `OperationType` (enum: Add, Remove, Replace, Move, Copy, Test, Invalid)
- Produces:
  - `JsonPatchOperationGuards.TryGetGuardError<TModel>(IList<Operation<TModel>> operations, out ValidationServiceError? error) -> bool`
  - `JsonPatchOperationGuards.TargetsEntityId(string? path) -> bool`

Note: On `Microsoft.AspNetCore.JsonPatch` 8.0.4, `JsonPatchDocument<T>.Operations` is `List<Operation<T>>` with properties `path`, `from`, `OperationType`.

- [ ] **Step 1: Create the guards class**

```csharp
using BaseCrud.Errors;
using BaseCrud.Errors.Keys;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace BaseCrud.EntityFrameworkCore.JsonPatch;

internal static class JsonPatchOperationGuards
{
    /// <summary>
    /// Returns true when a guard failed and <paramref name="error"/> is set.
    /// </summary>
    public static bool TryGetGuardError<TModel>(
        IList<Operation<TModel>> operations,
        out ValidationServiceError? error)
    {
        foreach (Operation<TModel> op in operations)
        {
            if (op.OperationType is OperationType.Move or OperationType.Copy)
            {
                error = new ValidationServiceError(
                    $"JSON Patch operation '{op.OperationType}' is not supported.",
                    ErrorKeys.Validation.JsonPatch.UnsupportedOperation);
                return true;
            }

            if (TargetsEntityId(op.path) || TargetsEntityId(op.from))
            {
                error = new ValidationServiceError(
                    "JSON Patch must not target the entity Id (/id).",
                    ErrorKeys.Validation.JsonPatch.IdImmutable);
                return true;
            }
        }

        error = null;
        return false;
    }

    /// <summary>
    /// True only for JSON Pointer <c>/id</c> (case-insensitive). Nested paths like <c>/address/id</c> are false.
    /// </summary>
    public static bool TargetsEntityId(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        ReadOnlySpan<char> span = path.AsSpan().TrimEnd('/');
        return span.Equals("/id", StringComparison.OrdinalIgnoreCase);
    }
}
```

- [ ] **Step 2: Build**

```bash
dotnet build src/BaseCrud.EntityFrameworkCore/BaseCrud.EntityFrameworkCore.csproj
```

Expected: PASS

- [ ] **Step 3: Commit**

```bash
git add src/BaseCrud.EntityFrameworkCore/JsonPatch/JsonPatchOperationGuards.cs
git commit -m "$(cat <<'EOF'
Add JSON Patch guards for Id immutability and unsupported ops.

EOF
)"
```

---

### Task 3: `PatchUpdateAsync` overload (interface + implementation)

**Files:**
- Modify: `src/BaseCrud.EntityFrameworkCore/Services/IEfCrudService.cs` (append after existing patch methods, before closing `}`)
- Modify: `src/BaseCrud.EntityFrameworkCore/BaseCrudService.cs` (implement after the last `PatchUpdateAsync<TResult>` overload, before `DeactivateByIdAsync`)
- Modify (optional): `src/BaseCrud.EntityFrameworkCore/GlobalUsings.cs` — add `global using Microsoft.AspNetCore.JsonPatch;`

**Interfaces:**
- Consumes: `JsonPatchDocument<TDtoFull>`; `JsonPatchOperationGuards`; `MappingRegistry.Get<TEntity,TDtoFull,TKey>().MapToDto`; `UpdateAsync(TDtoFull, ...)`; `CheckUpdateValidityAsync`; `BadRequest` / `NotFound` helpers; `ValidationServiceError`; `NotFoundServiceError`
- Produces: `Task<ServiceResult<TDtoFull>> PatchUpdateAsync(TKey id, JsonPatchDocument<TDtoFull> patch, IUserProfile<TUserKey>? userProfile, CancellationToken cancellationToken = default)`

- [ ] **Step 1: Add interface method**

Add to `IEfCrudService` (with `using Microsoft.AspNetCore.JsonPatch;` at file top if not global):

```csharp
    /// <summary>
    ///     Apply an RFC 6902 JSON Patch to the <typeparamref name="TDtoFull"/> for the entity with
    ///     primary key <paramref name="id"/>, then persist via <c>UpdateAsync</c>.
    /// </summary>
    /// <param name="id">Primary key of the entity to update.</param>
    /// <param name="patch">JSON Patch document targeting <typeparamref name="TDtoFull"/> properties.</param>
    /// <param name="userProfile"><see cref="IUserProfile{TUserKey}"/> or <see langword="null"/> when unauthorized.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated <typeparamref name="TDtoFull"/> wrapped in <see cref="ServiceResult{T}"/>.</returns>
    /// <remarks>
    ///     Supported ops: add, remove, replace, test. Ops targeting <c>/id</c> and move/copy are rejected.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="patch"/> is null.</exception>
    /// <exception cref="OperationCanceledException" />
    Task<ServiceResult<TDtoFull>> PatchUpdateAsync(
        TKey id,
        JsonPatchDocument<TDtoFull> patch,
        IUserProfile<TUserKey>? userProfile,
        CancellationToken cancellationToken = default);
```

- [ ] **Step 2: Implement on `BaseCrudService`**

```csharp
    public async Task<ServiceResult<TDtoFull>> PatchUpdateAsync(
        TKey id,
        JsonPatchDocument<TDtoFull> patch,
        IUserProfile<TUserKey>? userProfile,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(patch);

        ServiceResult validationResult = await CheckUpdateValidityAsync(id, cancellationToken);

        if (!validationResult.IsSuccess)
            return validationResult;

        TEntity? entity = await Set.FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);

        if (entity is null)
            return NotFound(new NotFoundServiceError());

        if (JsonPatchOperationGuards.TryGetGuardError(patch.Operations, out ValidationServiceError? guardError))
            return BadRequest(guardError!);

        TDtoFull dto = MappingRegistry.Get<TEntity, TDtoFull, TKey>().MapToDto(entity);

        var applyErrors = new List<string>();
        patch.ApplyTo(dto, error => applyErrors.Add(error.ErrorMessage));

        if (applyErrors.Count > 0)
        {
            return BadRequest(new ValidationServiceError(
                string.Join("; ", applyErrors),
                ErrorKeys.Validation.JsonPatch.ApplyFailed));
        }

        return await UpdateAsync(dto, userProfile, cancellationToken);
    }
```

Required usings in `BaseCrudService.cs` (if not covered by globals):

```csharp
using BaseCrud.EntityFrameworkCore.JsonPatch;
using BaseCrud.Errors;
using BaseCrud.Errors.Keys;
using Microsoft.AspNetCore.JsonPatch;
```

`IEfCrudServiceKeyed` aliases inherit the new method automatically — do **not** redeclare there.

- [ ] **Step 3: Build solution libraries + WebTester compile check**

```bash
dotnet build src/BaseCrud.sln
```

Expected: PASS (WebTester may still use old PATCH until Task 4; that is fine if it still compiles)

- [ ] **Step 4: Commit**

```bash
git add src/BaseCrud.EntityFrameworkCore/Services/IEfCrudService.cs \
  src/BaseCrud.EntityFrameworkCore/BaseCrudService.cs \
  src/BaseCrud.EntityFrameworkCore/GlobalUsings.cs
git commit -m "$(cat <<'EOF'
Add JsonPatchDocument PatchUpdateAsync overload on IEfCrudService.

EOF
)"
```

---

### Task 4: WebTester JSON Patch sample

**Files:**
- Modify: `src/WebTester/WebTester.csproj`
- Modify: `src/WebTester/Program.cs`
- Modify: `src/WebTester/Controllers/WeatherForecastController.cs`
- Modify: `src/WebTester/WebTester.http`

**Interfaces:**
- Consumes: `PatchUpdateAsync(TKey, JsonPatchDocument<TDtoFull>, ...)` from Task 3
- Produces: HTTP `PATCH /weatherforecast/{id}` accepting `application/json-patch+json`

- [ ] **Step 1: Add Newtonsoft MVC package**

In `src/WebTester/WebTester.csproj` `ItemGroup`:

```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.NewtonsoftJson" Version="8.0.4" />
```

- [ ] **Step 2: Enable Newtonsoft JSON for controllers**

In `Program.cs`, change:

```csharp
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.Converters.Add(new FilterMetadataConverter());
    o.JsonSerializerOptions.Converters.Add(new PrimeTableMetaConverter());
});
```

to:

```csharp
builder.Services.AddControllers()
    .AddNewtonsoftJson()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new FilterMetadataConverter());
        o.JsonSerializerOptions.Converters.Add(new PrimeTableMetaConverter());
    });
```

Keep `AddJsonOptions` for any STJ-based paths; `AddNewtonsoftJson` is required so `[FromBody] JsonPatchDocument<T>` binds. If PrimeNg converters break under Newtonsoft for `GetAll`, prefer registering those converters only on the STJ options that still apply, or move PrimeNg endpoints to stay on STJ — verify `POST .../GetAll` still works after this change. If it breaks, use:

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new FilterMetadataConverter());
        o.JsonSerializerOptions.Converters.Add(new PrimeTableMetaConverter());
    })
    .AddNewtonsoftJson();
```

and confirm Swagger/PATCH binding; adjust only as needed so both PrimeNg meta and JSON Patch work.

- [ ] **Step 3: Update PATCH action**

Replace the Patch action and delete `WeatherForecastPatchRequest` at the bottom of the controller file:

```csharp
    /// <summary>
    /// Partial update via RFC 6902 JSON Patch / <c>PatchUpdateAsync(JsonPatchDocument)</c>
    /// </summary>
    [HttpPatch("{id:int}")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(WeatherForecastDetailsDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ServiceError[]))]
    [SwaggerResponse(StatusCodes.Status404NotFound, typeof(ServiceError[]))]
    public async Task<ActionResult<WeatherForecastDetailsDto?>> Patch(
        int id,
        [FromBody] JsonPatchDocument<WeatherForecastDetailsDto> patch)
    {
        await EnsureInitAsync();
        logger.LogInformation("JSON Patch weather forecast {Id}", id);
        return await FromServiceResult(service.PatchUpdateAsync(id, patch, UserProfile));
    }
```

Add: `using Microsoft.AspNetCore.JsonPatch;`

Remove the record:

```csharp
public record WeatherForecastPatchRequest(string Summary, int TemperatureC);
```

- [ ] **Step 4: Update `WebTester.http`**

Replace the patch section with:

```http
### JSON Patch update (PatchUpdateAsync / JsonPatchDocument)
PATCH {{WebTester_HostAddress}}/weatherforecast/1
Content-Type: application/json-patch+json
Accept: application/json

[
  { "op": "replace", "path": "/summary", "value": "patched via JSON Patch" },
  { "op": "replace", "path": "/temperatureC", "value": 18 }
]

###

### JSON Patch reject Id mutation (expect 400)
PATCH {{WebTester_HostAddress}}/weatherforecast/1
Content-Type: application/json-patch+json
Accept: application/json

[
  { "op": "replace", "path": "/id", "value": 999 }
]

###
```

- [ ] **Step 5: Build WebTester**

```bash
dotnet build src/WebTester/WebTester.csproj
```

Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add src/WebTester/WebTester.csproj \
  src/WebTester/Program.cs \
  src/WebTester/Controllers/WeatherForecastController.cs \
  src/WebTester/WebTester.http
git commit -m "$(cat <<'EOF'
Wire WebTester PATCH to JsonPatchDocument PatchUpdateAsync.

EOF
)"
```

---

### Task 5: Manual verification + graphify

**Files:**
- None (verification only); graphify updates `graphify-out/` (gitignored)

**Interfaces:**
- Consumes: full feature from Tasks 1–4

- [ ] **Step 1: Run WebTester**

```bash
dotnet run --project src/WebTester/WebTester.csproj --urls http://localhost:5069
```

- [ ] **Step 2: Smoke success path**

```bash
curl -s -X PATCH http://localhost:5069/weatherforecast/1 \
  -H 'Content-Type: application/json-patch+json' \
  -H 'Accept: application/json' \
  -d '[{"op":"replace","path":"/summary","value":"patched via JSON Patch"},{"op":"replace","path":"/temperatureC","value":18}]'
```

Expected: HTTP 200; body has `"summary":"patched via JSON Patch"` and `"temperatureC":18`

- [ ] **Step 3: Smoke Id guard**

```bash
curl -s -o /tmp/patch-id.json -w "%{http_code}" -X PATCH http://localhost:5069/weatherforecast/1 \
  -H 'Content-Type: application/json-patch+json' \
  -d '[{"op":"replace","path":"/id","value":999}]'
echo
cat /tmp/patch-id.json
```

Expected: HTTP 400; error key `validation.jsonPatch.idImmutable`

- [ ] **Step 4: Smoke unsupported op**

```bash
curl -s -o /tmp/patch-move.json -w "%{http_code}" -X PATCH http://localhost:5069/weatherforecast/1 \
  -H 'Content-Type: application/json-patch+json' \
  -d '[{"op":"move","from":"/summary","path":"/date"}]'
echo
cat /tmp/patch-move.json
```

Expected: HTTP 400; error key `validation.jsonPatch.unsupportedOperation`

- [ ] **Step 5: Smoke apply failure (unknown path)**

```bash
curl -s -o /tmp/patch-bad.json -w "%{http_code}" -X PATCH http://localhost:5069/weatherforecast/1 \
  -H 'Content-Type: application/json-patch+json' \
  -d '[{"op":"replace","path":"/doesNotExist","value":1}]'
echo
cat /tmp/patch-bad.json
```

Expected: HTTP 400; error key `validation.jsonPatch.applyFailed`

- [ ] **Step 6: Confirm PrimeNg list still works (regression)**

```bash
curl -s -X POST http://localhost:5069/weatherforecast/GetAll \
  -H 'Content-Type: application/json' \
  -d '{"first":0,"rows":10}'
```

Expected: HTTP 200 with a query result (not a server error). If this fails due to Newtonsoft, fix Program.cs converter registration per Task 4 Step 2 notes and re-verify Steps 2–5.

- [ ] **Step 7: Update knowledge graph from repo root**

```bash
graphify update .
```

Expected: updates `./graphify-out/` only (no `src/graphify-out`)

- [ ] **Step 8: Final commit only if Step 6 required code fixes**

If Program.cs or converters were adjusted during verification:

```bash
git add src/WebTester/Program.cs
git commit -m "$(cat <<'EOF'
Fix WebTester JSON options so JSON Patch and PrimeNg coexist.

EOF
)"
```

Otherwise skip — no commit for graphify-out (ignored).

---

## Spec coverage checklist

| Spec requirement | Task |
|---|---|
| `Microsoft.AspNetCore.JsonPatch` on EF project | Task 1 |
| Error keys `validation.jsonPatch.*` | Task 1 |
| Guard `/id`, `move`/`copy` | Task 2 |
| `PatchUpdateAsync(id, JsonPatchDocument, …)` on `IEfCrudService` / `BaseCrudService` | Task 3 |
| Apply then `UpdateAsync` | Task 3 |
| `ArgumentNullException` on null patch | Task 3 |
| Empty patch allowed | Task 3 (no special reject) |
| WebTester PATCH + http file | Task 4 |
| Newtonsoft binding note | Task 4 |
| Manual verify success / 400 cases | Task 5 |
| Expression `PatchUpdateAsync` unchanged | All tasks (do not edit those methods) |
| No merge patch / no entity-direct patch / no `ICrudService` change | Out of scope — no tasks |
