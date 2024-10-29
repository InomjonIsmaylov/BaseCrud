using BaseCrud.EntityFrameworkCore.Services;
using BaseCrud.Errors;
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
                    Mapper,
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
                    Mapper,
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
                    Mapper,
                    DataTableMetaData: null,
                    cancellationToken
                )
            );

        IAsyncEnumerable<TDtoFull> result = Mapper.ProjectTo<TDtoFull>(query).AsAsyncEnumerable();

        return Ok(result);
    }

    public virtual async Task<ServiceResult<TEntity?>> GetEntityByIdAsync(
        TKey id,
        IUserProfile<TUserKey>? userProfile,
        Func<CrudActionContext<TEntity, TKey, TUserKey>, ValueTask<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default)
    {
        if (id is int intId)
            return BadRequest(new IdValidationServiceError("Id " + intId + "must be greater than zero"));

        IQueryable<TEntity> query = QueryableOfUntrackedActive;

        if (customAction != null)
            query = await customAction(
                new CrudActionContext<TEntity, TKey, TUserKey>(
                    query,
                    userProfile,
                    Mapper,
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
        if (id is int intId)
            return BadRequest(new IdValidationServiceError("Id " + intId + "must be greater than zero"));

        IQueryable<TEntity> query = QueryableOfUntrackedActive;

        if (customAction != null)
            query = await customAction(
                new CrudActionContext<TEntity, TKey, TUserKey>(
                    query,
                    userProfile,
                    Mapper,
                    DataTableMetaData: null,
                    cancellationToken
                )
            );

        TEntity? entity = await query.FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);

        if (entity is null)
            return NotFound(new NotFoundServiceError());

        var result = Mapper.Map<TDtoFull>(entity);

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
        var mapped = Mapper.Map<TEntity>(entity);

        ServiceResult validationResult = CheckInsertValidity(mapped.Id);

        if (!validationResult.IsSuccess)
            return validationResult;
        
        ServiceResult<TEntity> insertResult = await HandleInsertAsync(mapped, cancellationToken);

        if (!insertResult.IsSuccess)
            return ServiceResult.FromFailed(insertResult).ToType<TDtoFull>();

        var dto = Mapper.Map<TDtoFull>(insertResult.Result);

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
        var mapped = Mapper.Map<TEntity>(entity);

        ServiceResult validationResult = await CheckUpdateValidityAsync(mapped.Id, cancellationToken);

        if (!validationResult.IsSuccess)
            return validationResult;

        ServiceResult<EntityEntry<TEntity>> updateResult = await HandleUpdateAsync(mapped, cancellationToken);

        if (!updateResult.IsSuccess)
            return ServiceResult.FromFailed(updateResult).ToType<TDtoFull>();

        EntityEntry<TEntity> result = updateResult.Result!;

        return Mapper.Map<TDtoFull>(result.Entity);
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

        return Mapper.Map<TDtoFull>(entity);
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

        return Mapper.Map<TDtoFull>(entity);
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