using BaseCrud.General.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;

namespace BaseCrud.EntityFrameworkCore;

public abstract partial class BaseCrudService<TEntity, TDto, TDtoFull, TKey>
    : ICrudService<TEntity, TDto, TDtoFull, TKey>, IDisposable
    where TKey : struct, IEquatable<TKey>
    where TEntity : EntityBase<TKey>
    where TDto : class, IDataTransferObject<TEntity, TKey>
    where TDtoFull : class, IDataTransferObject<TEntity, TKey>
{
    public virtual async Task<QueryResult<TDto>> GetAllAsync(
        IDataTableMetaData dataTableMeta,
        IUserProfile userProfile,
        Func<IQueryable<TEntity>, IUserProfile, Task<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default)
    {
        (int totalCount, IEnumerable<TDto> data) = await HandleGetAllQueryAsync(dataTableMeta, userProfile, cancellationToken, customAction);

        var result = new QueryResult<TDto>
        {
            TotalItems = totalCount,
            Items = data
        };

        return result;
    }

    public virtual async Task<IAsyncEnumerable<TEntity>> GetEntityListAsync(
        IUserProfile userProfile,
        Func<IQueryable<TEntity>, IUserProfile, Task<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = QueryableOfUntrackedActive;

        if (customAction != null)
            query = await customAction(query, userProfile);

        return query.AsAsyncEnumerable();
    }

    public virtual async Task<IAsyncEnumerable<TDto>> GetListAsync(
        IUserProfile userProfile,
        Func<IQueryable<TEntity>, IUserProfile, Task<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = QueryableOfUntrackedActive;

        if (customAction != null)
            query = await customAction(query, userProfile);

        IQueryable<TDto> queryableOfSelected = HandleSelection(query);

        
        return queryableOfSelected.AsAsyncEnumerable();
    }

    public virtual async Task<IAsyncEnumerable<TDtoFull>> GetFullEntityListAsync(
        IUserProfile userProfile,
        Func<IQueryable<TEntity>, IUserProfile, Task<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = QueryableOfUntrackedActive;

        if (customAction != null)
            query = await customAction(query, userProfile);

        IAsyncEnumerable<TDtoFull> result = Mapper.ProjectTo<TDtoFull>(query).AsAsyncEnumerable();
        
        //var data = await query.ToListAsync(cancellationToken);

        //var mapped = MapDataToFullDtoList(data);

        //return mapped.ToList();
        
        return result;
    }

    public virtual async Task<TEntity?> GetEntityByIdAsync(
        TKey id,
        IUserProfile userProfile,
        Func<IQueryable<TEntity>, IUserProfile, Task<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default)
    {
        if (id is int intId)
            InvalidIdArgumentException.ThrowIfZero(intId);

        IQueryable<TEntity> query = QueryableOfActive;

        if (customAction != null)
            query = await customAction(query, userProfile);

        TEntity? result = await query.FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);

        return result;
    }

    public virtual async Task<TDtoFull?> GetByIdAsync(
        TKey id,
        IUserProfile userProfile,
        Func<IQueryable<TEntity>, IUserProfile, Task<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default)
    {
        
        InvalidIdArgumentException.ThrowIfInvalid(id);

        IQueryable<TEntity> query = QueryableOfActive;

        if (customAction != null)
            query = await customAction(query, userProfile);

        TEntity? entity = await query.FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);

        if (entity is null)
            return null;

        var result = Mapper.Map<TDtoFull>(entity);

        return result;
    }

    public virtual async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        CheckInsertValidity(entity.Id);

        entity = await HandleInsertAsync(entity, cancellationToken);

        return entity;
    }

    public virtual async Task<TDtoFull> InsertAsync(TDtoFull entity, CancellationToken cancellationToken = default)
    {
        var mapped = Mapper.Map<TEntity>(entity);

        CheckInsertValidity(mapped.Id);

        mapped = await HandleInsertAsync(mapped, cancellationToken);

        entity = Mapper.Map<TDtoFull>(mapped);

        return entity;
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await CheckUpdateValidityAsync(entity.Id, cancellationToken);

        EntityEntry<TEntity> result = await HandleUpdateAsync(entity, cancellationToken);

        return result.Entity;
    }

    public virtual async Task<TDtoFull> UpdateAsync(TDtoFull entity, CancellationToken cancellationToken = default)
    {
        var mapped = Mapper.Map<TEntity>(entity);

        await CheckUpdateValidityAsync(mapped.Id, cancellationToken);

        EntityEntry<TEntity> result = await HandleUpdateAsync(mapped, cancellationToken);

        return Mapper.Map<TDtoFull>(result.Entity);
    }

    public Task<int> PatchUpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        ArgumentNullException.ThrowIfNull(setPropertyCalls, nameof(setPropertyCalls));

        return Set
            .Where(predicate)
            .ExecuteUpdateAsync(setPropertyCalls, cancellationToken);
    }

    public Task<int> PatchUpdateAsync(
        TKey id,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls,
        CancellationToken cancellationToken = default)
    {
        if (id is int intId)
            InvalidIdArgumentException.ThrowIfZero(intId);

        ArgumentNullException.ThrowIfNull(setPropertyCalls, nameof(setPropertyCalls));

        return Set
            .Where(x => x.Id.Equals(id))
            .ExecuteUpdateAsync(setPropertyCalls, cancellationToken);
    }

    public Task<int> PatchUpdateAsync<TResult>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<SetPropertyCalls<TResult>, SetPropertyCalls<TResult>>> setPropertyCalls,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
        ArgumentNullException.ThrowIfNull(setPropertyCalls, nameof(setPropertyCalls));

        return Set
            .Where(predicate)
            .Select(selector)
            .ExecuteUpdateAsync(setPropertyCalls, cancellationToken);
    }

    public Task<int> PatchUpdateAsync<TResult>(
        TKey id,
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<SetPropertyCalls<TResult>, SetPropertyCalls<TResult>>> setPropertyCalls,
        CancellationToken cancellationToken = default)
    {
        if (id is int intId)
            InvalidIdArgumentException.ThrowIfZero(intId);
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
        ArgumentNullException.ThrowIfNull(setPropertyCalls, nameof(setPropertyCalls));

        return Set
            .Where(x => x.Id.Equals(id))
            .Select(selector)
            .ExecuteUpdateAsync(setPropertyCalls, cancellationToken);
    }

    public virtual async Task<bool> DeactivateByIdAsync(
        TKey id,
        IUserProfile userProfile,
        Func<IQueryable<TEntity>, IUserProfile, Task<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default)
    {

        await CheckUpdateValidityAsync(id, cancellationToken);

        TEntity? entity = await Set.FindAsync([id], cancellationToken);

        entity!.Active = false;

        bool saved = await HandleSaveChangesAsync(cancellationToken);

        return saved;
    }
}