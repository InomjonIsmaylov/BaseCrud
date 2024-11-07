using AutoMapper.QueryableExtensions;
using BaseCrud.Abstractions.Expressions;
using BaseCrud.Expressions;
using BaseCrud.Extensions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using BaseCrud.Errors;
using BaseCrud.Errors.Keys;

namespace BaseCrud.EntityFrameworkCore;

/// <summary>
///     Provides basic implementation for Crud Operations of <typeparamref name="TEntity" />
/// </summary>
/// <typeparam name="TEntity">Type that represents Database Entity</typeparam>
/// <typeparam name="TDto">DataTransferObjectType that maps the entity</typeparam>
/// <typeparam name="TDtoFull">
///     DataTransferObjectType that maps the entity more detailed (Can be the same as
///     <typeparamref name="TDto" />)
/// </typeparam>
/// <typeparam name="TKey">key property type</typeparam>
/// <typeparam name="TUserKey">
///     Type of the User (<see cref="IUserProfile{T}"/>) key value
///     (e.g. <see cref="Guid"/>, <see cref="int"/>, <see cref="long"/> ...)
/// </typeparam>
public abstract partial class BaseCrudService<TEntity, TDto, TDtoFull, TKey, TUserKey>
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

    /// <exception cref="OperationCanceledException" />
    protected async Task<ServiceResult<(int totalCount, IEnumerable<TDto> data)>> HandleGetAllQueryAsync(
        IDataTableMetaData dataTableMeta,
        IUserProfile<TUserKey>? userProfile,
        CancellationToken cancellationToken,
        Func<CrudActionContext<TEntity, TKey, TUserKey>, ValueTask<IQueryable<TEntity>>>? customAction = null)
    {
        if (dataTableMeta.PaginationMetaData.Rows <= 0)
            return BadRequest(
                new DataTableValidationServiceError(
                    "Rows must be greater than 0",
                    ErrorKey: ErrorKeys.Validation.Datatable.RowsCountMustBeGreaterThanZero)
            );

        if (dataTableMeta.PaginationMetaData.First < 0)
            return BadRequest(
                new DataTableValidationServiceError(
                    "First must be greater than or equal to 0",
                    ErrorKey: ErrorKeys.Validation.Datatable.FirstMustBeGreaterThanOrEqualToZero)
            );

        IQueryable<TEntity> query = GetQuery(dataTableMeta);

        query = HandleGlobalFilter(dataTableMeta, query);

        if (customAction != null)
            query = await customAction(
                new CrudActionContext<TEntity, TKey, TUserKey>(
                    query,
                    userProfile,
                    Mapper,
                    dataTableMeta,
                    cancellationToken
                )
            );

        int totalCount = await query.CountAsync(cancellationToken);

        List<TDto> data = await RetrieveDataAsync(dataTableMeta, query, cancellationToken);

        return (totalCount, data);
    }

    protected IQueryable<TEntity> GetQuery(IDataTableMetaData dataTableMeta)
    {
        IQueryable<TEntity> query = QueryableOfUntrackedActive;

        query = ExpressionBuilder.BuildFilterExpression(query, dataTableMeta);

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
        PaginationMetaData paginationMeta = dataTableMeta.PaginationMetaData;

        query = query
            .Skip(paginationMeta.First)
            .Take(paginationMeta.Rows);
            
        IQueryable<TDto> queryableOfSelected = HandleSelection(query);

        return queryableOfSelected.ToListAsync(cancellationToken);
    }

    protected IQueryable<TEntity> HandleGlobalFilter(
        IDataTableMetaData dataTableMeta,
        IQueryable<TEntity> query)
    {
        if (!dataTableMeta.GlobalFilterExpressionMetaData.Any())
            return query;

        Type? globalFilter = typeof(TEntity).Assembly
            .GetTypeAssignableFromInterface(typeof(IGlobalFilterExpression<,>));

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
        Type? selectorType = typeof(TEntity).Assembly
            .GetTypeAssignableFromInterface(typeof(ISelectExpression<,,>));

        if (selectorType is null)
            return query.ProjectTo<TDto>(Mapper.ConfigurationProvider);

        return Activator.CreateInstance(selectorType) is not ISelectExpression<TEntity, TDto, TKey> selectorInstance
            ? query.ProjectTo<TDto>(Mapper.ConfigurationProvider)
            : query.Select(selectorInstance.SelectExpression);
    }

    protected static ServiceResult CheckInsertValidity(TKey id)
        => id switch
        {
            Guid guid when guid != Guid.Empty
                => BadRequest(new IdValidationServiceError(
                    "Entry with Id value other than GUID.EMPTY can not be inserted into the database",
                    ErrorKey: ErrorKeys.Validation.Id.EmptyGuid)),
            int intId when intId != 0
                => BadRequest(new IdValidationServiceError(
                    "Entry with Id value other than zero can not be inserted into the database",
                    ErrorKey: ErrorKeys.Validation.Id.ShouldBeZero)),
            _ => id.Equals(default)
                ? ServiceResult.NoContent()
                : BadRequest(
                    new IdValidationServiceError(
                        "Entry with Id value other than default can not be inserted into the database",
                        ErrorKey: ErrorKeys.Validation.Id.ShouldBeDefault
                    )
                )
        };

    /// <exception cref="DbUpdateException" />
    /// <exception cref="OperationCanceledException" />
    protected async Task<ServiceResult> CheckUpdateValidityAsync(TKey id, CancellationToken cancellationToken = default)
    {
        if (id is 0)
            return BadRequest(
                new IdValidationServiceError("Entry with Id value zero can not be updated in the database",
                    ErrorKey: ErrorKeys.Validation.Id.ShouldNotBeZero)
            );

        if (id.Equals(default))
            return ServiceResult.BadRequest(
                new IdValidationServiceError(ErrorKey: ErrorKeys.Validation.Id.ShouldNotBeDefault));

        var model = await Set
                        .Where(e => e.Id.Equals(id))
                        .Select(e => new { e.Active })
                        .FirstOrDefaultAsync(cancellationToken);

        if (model is null)
            return NotFound(new NotFoundServiceError());

        if (!model.Active)
            return ServiceResult.BadRequest(
                new DatabaseUpdateError(
                    ErrorMessage: "Entity to update has been deactivated",
                    ErrorKey: ErrorKeys.Database.EntityDeactivated
                )
            );

        return ServiceResult.NoContent();
    }

    /// <exception cref="DbUpdateException" />
    /// <exception cref="DbUpdateConcurrencyException" />
    /// <exception cref="OperationCanceledException" />
    protected async Task<bool> HandleSaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int count = await DbContext.SaveChangesAsync(cancellationToken);

        return count > 0;
    }

    /// <exception cref="DbUpdateException" />
    /// <exception cref="DbUpdateConcurrencyException" />
    /// <exception cref="OperationCanceledException" />
    protected async Task<ServiceResult<EntityEntry<TEntity>>> HandleUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Set.Entry(entity).State = EntityState.Detached;

        EntityEntry<TEntity> result = Set.Update(entity);

        bool saved = await HandleSaveChangesAsync(cancellationToken);

        if (saved)
            return result;

        return ServiceResult.BadRequest(
            new DatabaseUpdateError(
                ErrorMessage: "Update operation succeeded but database did not change",
                ErrorKey: ErrorKeys.Database.EntitiesToChange
            )
        );

    }

    /// <exception cref="DbUpdateException" />
    /// <exception cref="DbUpdateConcurrencyException" />
    /// <exception cref="OperationCanceledException" />
    protected async Task<ServiceResult<TEntity>> HandleInsertAsync(TEntity mapped, CancellationToken cancellationToken = default)
    {
        EntityEntry<TEntity> entry = Set.Add(mapped);

        bool saved = await HandleSaveChangesAsync(cancellationToken);

        if (saved)
            return entry.Entity;

        return ServiceResult.BadRequest(
            new DatabaseInsertError(
                ErrorMessage: "Update operation succeeded but database did not change",
                ErrorKey: ErrorKeys.Database.EntitiesToChange
            )
        );
    }

    public void Dispose()
    {
        DbContext.Dispose();

        GC.SuppressFinalize(this);
    }
}