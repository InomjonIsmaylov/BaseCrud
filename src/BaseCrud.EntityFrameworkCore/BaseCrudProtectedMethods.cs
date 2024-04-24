using AutoMapper.QueryableExtensions;
using BaseCrud.Abstractions.Expressions;
using BaseCrud.EntityFrameworkCore.Extensions;
using BaseCrud.General.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BaseCrud.EntityFrameworkCore;

/// <summary>
///     Provides basic implementation for Crud Services of <typeparamref name="TEntity" />
/// </summary>
/// <typeparam name="TEntity">Type that represents Database Entity</typeparam>
/// <typeparam name="TDto">DataTransferObjectType that maps the entity</typeparam>
/// <typeparam name="TDtoFull">
///     DataTransferObjectType that maps the entity more detailed (Can be the same as
///     <typeparamref name="TDto" />)
/// </typeparam>
/// <typeparam name="TKey">key property type</typeparam>
public abstract partial class BaseCrudService<TEntity, TDto, TDtoFull, TKey>
{
    protected readonly DbContext DbContext;
    protected readonly IMapper Mapper;
    protected readonly IQueryable<TEntity> QueryableOfActive;
    protected readonly IQueryable<TEntity> QueryableOfUntrackedActive;
    protected readonly DbSet<TEntity> Set;
    protected static readonly ExpressionBuilder<TEntity> ExpressionBuilder = new();

    protected BaseCrudService(
        DbContext dbContext,
        IMapper mapper
    )
    {
        Mapper = mapper;

        DbContext = dbContext;

        Set = DbContext.Set<TEntity>();

        QueryableOfActive = Set.Where(e => e.Active);

        QueryableOfUntrackedActive = QueryableOfActive.AsNoTracking();
    }

    /// <exception cref="ArgumentNullException" />
    /// <exception cref="TableMetaDataInvalidException" />
    /// <exception cref="OperationCanceledException" />
    protected async Task<(int totalCount, IEnumerable<TDto> data)> HandleGetAllQueryAsync(
        IDataTableMetaData dataTableMeta,
        IUserProfile userProfile,
        CancellationToken cancellationToken,
        Func<IQueryable<TEntity>, IUserProfile, Task<IQueryable<TEntity>>>? customAction = null)
    {
        dataTableMeta.ThrowIfValid();

        var query = GetQuery(dataTableMeta);

        query = HandleGlobalFilter(dataTableMeta, query);

        // ****************  Use filter by role  ****************
        if (customAction != null)
            query = await customAction(query, userProfile);

        var totalCount = await query.CountAsync(cancellationToken);

        var data = await RetrieveDataAsync(dataTableMeta, query, cancellationToken);

        return (totalCount, data);
    }

    /// <exception cref="ArgumentException" />
    protected IQueryable<TEntity> GetQuery(IDataTableMetaData dataTableMeta)
    {
        var query = QueryableOfUntrackedActive;

        query = ExpressionBuilder.BuildFilterExpression(query, dataTableMeta.FilterExpressionMetaData);

        query = ExpressionBuilder.BuildSortingExpression(query, dataTableMeta.SortingExpressionMetaData);

        return query;
    }

    /// <exception cref="OperationCanceledException" />
    protected Task<List<TDto>> RetrieveDataAsync(
        IDataTableMetaData dataTableMeta,
        IQueryable<TEntity> query,
        CancellationToken cancellationToken
    )
    {
        var paginationMeta = dataTableMeta.PaginationMetaData;

        query = query
            .Skip(paginationMeta.First)
            .Take(paginationMeta.Rows);
            
        var queryableOfSelected = HandleSelection(query);

        return queryableOfSelected.ToListAsync(cancellationToken);
    }

    protected IQueryable<TEntity> HandleGlobalFilter(
        IDataTableMetaData dataTableMeta,
        IQueryable<TEntity> query)
    {
        if (!dataTableMeta.GlobalFilterExpressionMetaData.Any())
            return query;

        var globalFilter = typeof(TEntity).Assembly 
            .GetTypes()
            .FirstOrDefault(t => typeof(IGlobalFilterExpression<>).IsAssignableFrom(t) && !t.IsInterface);

        if (globalFilter is null)
            return query;

        return Activator.CreateInstance(globalFilter) is not IGlobalFilterExpression<TEntity, TKey> globalFilterInstance
            ? query
            : dataTableMeta.GlobalFilterExpressionMetaData
                .Aggregate(query, (current, g) =>
                    current.Where(
                        globalFilterInstance.GlobalSearchExpression(g.SearchString)
                    )
                );
    }

    protected IQueryable<TDto> HandleSelection(IQueryable<TEntity> query)
    {
        var selectorType = typeof(TEntity).Assembly 
            .GetTypes()
            .FirstOrDefault(t => typeof(ISelectExpression<,,>).IsAssignableFrom(t) && !t.IsInterface);

        if (selectorType is null)
            return query.ProjectTo<TDto>(Mapper.ConfigurationProvider);

        return Activator.CreateInstance(selectorType) is not ISelectExpression<TEntity, TDto, TKey> selectorInstance
            ? query.ProjectTo<TDto>(Mapper.ConfigurationProvider)
            : query.Select(selectorInstance.SelectExpression);
    }

    /// <exception cref="DatabaseOperationException" />
    protected static void CheckInsertValidity(TKey id)
    {
        switch (id)
        {
            case Guid guid when guid != Guid.Empty:
                throw new InvalidOperationException(
                    "Entry with Id value other than GUID.EMPTY can not be inserted into the database");
            case int intId when intId != 0:
                throw new DatabaseOperationException(
                    "Entry with Id value other than zero can not be inserted into the database");
        }
    }

    /// <exception cref="InvalidIdArgumentException" />
    /// <exception cref="DbUpdateException" />
    /// <exception cref="DatabaseOperationException" />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    protected async Task CheckUpdateValidityAsync(TKey id, CancellationToken cancellationToken = default)
    {
        if (id is 0)
            throw new InvalidIdArgumentException(
                "Entry with Id value zero can not be updated in the database");

        var model = await Set
                        .Where(e => e.Id.Equals(id))
                        .Select(e => new { e.Active })
                        .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new DbUpdateException("Entity to update is not found!");

        if (!model.Active)
            throw new DbUpdateException("Entity to update has been deactivated");
    }

    /// <exception cref="DbUpdateException" />
    /// <exception cref="DbUpdateConcurrencyException" />
    /// <exception cref="OperationCanceledException" />
    protected async Task<bool> HandleSaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var count = await DbContext.SaveChangesAsync(cancellationToken);

        return count > 0;
    }

    /// <exception cref="DbUpdateException" />
    /// <exception cref="DbUpdateConcurrencyException" />
    /// <exception cref="OperationCanceledException" />
    protected async Task<EntityEntry<TEntity>> HandleUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Set.Entry(entity).State = EntityState.Detached;

        var result = Set.Update(entity);

        var saved = await HandleSaveChangesAsync(cancellationToken);

        if (!saved)
            throw new DbUpdateException("Update operation succeeded but database did not change");

        return result;
    }

    /// <exception cref="DbUpdateException" />
    /// <exception cref="DbUpdateConcurrencyException" />
    /// <exception cref="DatabaseOperationException" />
    /// <exception cref="OperationCanceledException" />
    protected async Task<TEntity> HandleInsertAsync(TEntity mapped, CancellationToken cancellationToken = default)
    {
        var entry = Set.Add(mapped);

        var saved = await HandleSaveChangesAsync(cancellationToken);

        if (!saved)
            throw new DatabaseOperationException("Database Insert operation succeeded " +
                                                 "but entity has not been saved into the database");

        return entry.Entity;
    }

    public void Dispose()
    {
        DbContext.Dispose();

        GC.SuppressFinalize(this);
    }
}