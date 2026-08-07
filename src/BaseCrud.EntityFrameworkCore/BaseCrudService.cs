using BaseCrud.EntityFrameworkCore.JsonPatch;
using BaseCrud.EntityFrameworkCore.Services;
using BaseCrud.Errors;
using BaseCrud.Errors.Keys;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;

namespace BaseCrud.EntityFrameworkCore;

public abstract partial class BaseCrudService<TEntity, TDto, TDtoFull, TKey, TUserKey>
    : IEfCrudService<TEntity, TDto, TDtoFull, TKey, TUserKey>, IDisposable
    where TEntity : class, IEntity<TKey>
    where TDto : class, IDataTransferObject<TEntity, TKey>
    where TDtoFull : class, IDataTransferObject<TEntity, TKey>
    where TKey : struct, IEquatable<TKey>
    where TUserKey : struct, IEquatable<TUserKey>
{
    public virtual async Task<ServiceResult<QueryResult<TDto>>> GetAllAsync(
        IDataTableMetaData dataTableMeta,
        IUserProfile<TUserKey>? userProfile,
        Func<CrudActionContext<TEntity, TKey, TUserKey>, ValueTask<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default)
    {
        ServiceResult<(int totalCount, IEnumerable<TDto> data)> queryResult =
            await HandleGetAllQueryAsync(dataTableMeta, userProfile, cancellationToken, customAction);

        if (!queryResult.IsSuccess)
            return ServiceResult.FromFailed(queryResult).ToType<QueryResult<TDto>>();

        (int totalCount, IEnumerable<TDto> data) = queryResult.Result;

        var result = new QueryResult<TDto>
        {
            TotalItems = totalCount,
            Items = data
        };

        return result;
    }

    public virtual async Task<ServiceResult<IAsyncEnumerable<TEntity>>> GetEntityListAsync(
        IUserProfile<TUserKey>? userProfile,
        Func<CrudActionContext<TEntity, TKey, TUserKey>, ValueTask<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = QueryableOfUntrackedActive;

        if (customAction != null)
            query = await customAction(
                new CrudActionContext<TEntity, TKey, TUserKey>(
                    query,
                    userProfile,
                    DataTableMetaData: null,
                    cancellationToken
                )
            );

        return Ok(query.AsAsyncEnumerable());
    }

    public virtual async Task<ServiceResult<IAsyncEnumerable<TDto>>> GetListAsync(
        IUserProfile<TUserKey>? userProfile,
        Func<CrudActionContext<TEntity, TKey, TUserKey>, ValueTask<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = QueryableOfUntrackedActive;

        if (customAction != null)
            query = await customAction(
                new CrudActionContext<TEntity, TKey, TUserKey>(
                    query,
                    userProfile,
                    DataTableMetaData: null,
                    cancellationToken
                )
            );

        IQueryable<TDto> queryableOfSelected = HandleSelection(query);

        return Ok(queryableOfSelected.AsAsyncEnumerable());
    }

    public virtual async Task<ServiceResult<IAsyncEnumerable<TDtoFull>>> GetFullEntityListAsync(
        IUserProfile<TUserKey>? userProfile,
        Func<CrudActionContext<TEntity, TKey, TUserKey>, ValueTask<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = QueryableOfUntrackedActive;

        if (customAction != null)
            query = await customAction(
                new CrudActionContext<TEntity, TKey, TUserKey>(
                    query,
                    userProfile,
                    DataTableMetaData: null,
                    cancellationToken
                )
            );

        var mapping = MappingRegistry.Get<TEntity, TDtoFull, TKey>();

        IAsyncEnumerable<TDtoFull> result = query.Select(mapping.SelectExpression).AsAsyncEnumerable();

        return Ok(result);
    }

    public virtual async Task<ServiceResult<TEntity?>> GetEntityByIdAsync(
        TKey id,
        IUserProfile<TUserKey>? userProfile,
        Func<CrudActionContext<TEntity, TKey, TUserKey>, ValueTask<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default)
    {
        if (id is int intId and < 1)
            return BadRequest(new IdValidationServiceError("Id " + intId + "must be greater than zero"));

        IQueryable<TEntity> query = QueryableOfUntrackedActive;

        if (customAction != null)
            query = await customAction(
                new CrudActionContext<TEntity, TKey, TUserKey>(
                    query,
                    userProfile,
                    DataTableMetaData: null,
                    cancellationToken
                )
            );

        TEntity? result = await query.FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);

        return result;
    }

    public virtual async Task<ServiceResult<TDtoFull?>> GetByIdAsync(
    TKey id,
    IUserProfile<TUserKey>? userProfile,
    Func<CrudActionContext<TEntity, TKey, TUserKey>, ValueTask<IQueryable<TEntity>>>? customAction = null,
    CancellationToken cancellationToken = default)
    {
        if (id is int intId and < 1)
            return BadRequest(new IdValidationServiceError("Id " + intId + "must be greater than zero"));

        IQueryable<TEntity> query = QueryableOfUntrackedActive.Where(x => x.Id.Equals(id));

        if (customAction != null)
            query = await customAction(
                new CrudActionContext<TEntity, TKey, TUserKey>(
                    query,
                    userProfile,
                    DataTableMetaData: null,
                    cancellationToken
                )
            );

        var mapping = MappingRegistry.Get<TEntity, TDtoFull, TKey>();

        TDtoFull? result = await query
            .Select(mapping.SelectExpression)
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
            return NotFound(new NotFoundServiceError());

        return result;
    }

    public virtual async Task<ServiceResult<TEntity>> InsertAsync(
        TEntity entity,
        IUserProfile<TUserKey>? userProfile,
        CancellationToken cancellationToken = default)
    {
        ServiceResult validationResult = CheckInsertValidity(entity.Id);

        if (!validationResult.IsSuccess)
            return validationResult;

        ServiceResult<TEntity> insertResult = await HandleInsertAsync(entity, cancellationToken);

        if (!insertResult.IsSuccess)
            return insertResult;

        return insertResult.Result;
    }

    public virtual async Task<ServiceResult<TDtoFull>> InsertAsync(
        TDtoFull entity,
        IUserProfile<TUserKey>? userProfile,
        CancellationToken cancellationToken = default)
    {
        var mapping = MappingRegistry.Get<TEntity, TDtoFull, TKey>();

        TEntity mapped = mapping.InsertToEntity(entity);

        ServiceResult validationResult = CheckInsertValidity(mapped.Id);

        if (!validationResult.IsSuccess)
            return validationResult;
        
        ServiceResult<TEntity> insertResult = await HandleInsertAsync(mapped, cancellationToken);

        if (!insertResult.IsSuccess)
            return ServiceResult.FromFailed(insertResult).ToType<TDtoFull>();

        TDtoFull dto = mapping.MapToDto(insertResult.Result!);

        return dto;
    }

    public virtual async Task<ServiceResult<TEntity>> UpdateAsync(
        TEntity entity,
        IUserProfile<TUserKey>? userProfile,
        CancellationToken cancellationToken = default)
    {
        ServiceResult validationResult = await CheckUpdateValidityAsync(entity.Id, cancellationToken);

        if (!validationResult.IsSuccess)
            return validationResult;

        ServiceResult<EntityEntry<TEntity>> updateResult = await HandleUpdateAsync(entity, cancellationToken);

        if (!updateResult.IsSuccess)
            return ServiceResult.FromFailed(updateResult).ToType<TEntity>();

        EntityEntry<TEntity> result = updateResult.Result!;

        return result.Entity;
    }

    public virtual async Task<ServiceResult<TDtoFull>> UpdateAsync(
        TDtoFull entity,
        IUserProfile<TUserKey>? userProfile,
        CancellationToken cancellationToken = default)
    {
        var mapping = MappingRegistry.Get<TEntity, TDtoFull, TKey>();

        TKey id = GetDtoId(entity);

        ServiceResult validationResult = await CheckUpdateValidityAsync(id, cancellationToken);

        if (!validationResult.IsSuccess)
            return validationResult;

        TEntity? existing = await Set.FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);

        if (existing is null)
            return NotFound(new NotFoundServiceError());

        TEntity updatedValues = mapping.UpdateEntity(existing, entity);

        updatedValues.Id = id;

        DbContext.Entry(existing).CurrentValues.SetValues(updatedValues);

        ServiceResult<EntityEntry<TEntity>> updateResult = await HandleUpdateAsync(existing, cancellationToken);

        if (!updateResult.IsSuccess)
            return ServiceResult.FromFailed(updateResult).ToType<TDtoFull>();

        EntityEntry<TEntity> result = updateResult.Result!;

        return mapping.MapToDto(result.Entity);
    }

    private static TKey GetDtoId(TDtoFull dto)
    {
        var prop = typeof(TDtoFull).GetProperty("Id")
            ?? throw new InvalidOperationException(
                $"{typeof(TDtoFull).Name} must have a public Id property of type {typeof(TKey).Name}.");
        return (TKey)prop.GetValue(dto)!;
    }

    public async Task<ServiceResult<int>> PatchUpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls,
        IUserProfile<TUserKey>? userProfile,
        CancellationToken cancellationToken = default)
    {
        return await Set
            .Where(predicate)
            .ExecuteUpdateAsync(setPropertyCalls, cancellationToken);
    }

    public async Task<ServiceResult<TDtoFull>> PatchUpdateAsync(
        TKey id,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls,
        IUserProfile<TUserKey>? userProfile,
        CancellationToken cancellationToken = default)
    {
        ServiceResult validationResult = await CheckUpdateValidityAsync(id, cancellationToken);

        if (!validationResult.IsSuccess)
            return validationResult;

        ArgumentNullException.ThrowIfNull(setPropertyCalls, nameof(setPropertyCalls));

        int result = await Set
            .Where(x => x.Id.Equals(id))
            .ExecuteUpdateAsync(setPropertyCalls, cancellationToken);

        if (result == 0)
            return InternalServerError(new DatabaseUpdateError());

        TEntity entity = (await Set.FindAsync([id], cancellationToken))!;

        return MappingRegistry.Get<TEntity, TDtoFull, TKey>().MapToDto(entity);
    }

    public async Task<ServiceResult<int>> PatchUpdateAsync<TResult>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<SetPropertyCalls<TResult>, SetPropertyCalls<TResult>>> setPropertyCalls,
        IUserProfile<TUserKey>? userProfile,
        CancellationToken cancellationToken = default)
    {
        return await Set
            .Where(predicate)
            .Select(selector)
            .ExecuteUpdateAsync(setPropertyCalls, cancellationToken);
    }

    public async Task<ServiceResult<TDtoFull>> PatchUpdateAsync<TResult>(
        TKey id,
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<SetPropertyCalls<TResult>, SetPropertyCalls<TResult>>> setPropertyCalls,
        IUserProfile<TUserKey>? userProfile,
        CancellationToken cancellationToken = default)
    {
        ServiceResult validationResult = await CheckUpdateValidityAsync(id, cancellationToken);

        if (!validationResult.IsSuccess)
            return validationResult;

        int result = await Set
            .Where(x => x.Id.Equals(id))
            .Select(selector)
            .ExecuteUpdateAsync(setPropertyCalls, cancellationToken);

        if (result == 0)
            return InternalServerError(new DatabaseUpdateError());

        TEntity entity = (await Set.FindAsync([id], cancellationToken))!;

        return MappingRegistry.Get<TEntity, TDtoFull, TKey>().MapToDto(entity);
    }

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

    public async Task<ServiceResult> DeactivateByIdAsync(
        TKey id,
        IUserProfile<TUserKey>? userProfile,
        Func<CrudActionContext<TEntity, TKey, TUserKey>, ValueTask<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default)
    {
        ServiceResult validationResult = await CheckUpdateValidityAsync(id, cancellationToken);

        if (!validationResult.IsSuccess)
            return validationResult;

        TEntity? entity = await Set.FindAsync([id], cancellationToken);

        entity!.Active = false;

        bool saved = await HandleSaveChangesAsync(cancellationToken);

        return saved ? NoContent() : BadRequest();
    }
}