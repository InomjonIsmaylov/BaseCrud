using BaseCrud.ServiceResults;

namespace BaseCrud.Abstractions.Services;

/// <summary>
///     Defines the general rules that Crud Service should implement
/// </summary>
/// <typeparam name="TEntity">Type that represents Database Entity</typeparam>
/// <typeparam name="TDto">DataTransferObjectType that maps the entity</typeparam>
/// <typeparam name="TDtoFull">
///     DataTransferObjectType that maps the entity more detailed (Can be the same as
///     <typeparamref name="TDto" />)
/// </typeparam>
/// <typeparam name="TKey">Key property type</typeparam>
/// <typeparam name="TUserKey">
///     Type of the User (<see cref="IUserProfile{T}"/>) key value
///     (e.g. <see cref="Guid"/>, <see cref="int"/>, <see cref="long"/> ...)
/// </typeparam>
public interface ICrudService<TEntity, TDto, TDtoFull, TKey, TUserKey>
    where TKey : struct, IEquatable<TKey>
    where TEntity : IEntity<TKey>
    where TDto : class, IDataTransferObject<TEntity, TKey>
    where TDtoFull : class, IDataTransferObject<TEntity, TKey>
    where TUserKey : struct, IEquatable<TUserKey>
{
    /// <summary>
    ///     Gets all entities by executing query to database table and maps entities to <typeparamref name="TDto" />
    /// </summary>
    /// <typeparam name="TDto">Dto to be mapper</typeparam>
    /// <param name="dataTableMeta">Meta data as <see cref="IDataTableMetaData" /></param>
    /// <param name="userProfile"><see cref="IUserProfile{TUserKey}" /> value or <see langword="null"/> when user is unauthorized</param>
    /// <param name="customAction">Additional custom action to perform over query</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>
    ///     <see cref="QueryResult{TDto}" /> of entities, wrapped with <see cref="ServiceResult{T}"/>
    /// </returns>
    /// <exception cref="OperationCanceledException" />
    Task<ServiceResult<QueryResult<TDto>>> GetAllAsync(
        IDataTableMetaData dataTableMeta,
        IUserProfile<TUserKey>? userProfile,
        Func<CrudActionContext<TEntity, TKey, TUserKey>, ValueTask<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the first entity by executing query to database table
    /// </summary>
    /// <param name="id">The id of the entity</param>
    /// <param name="userProfile"><see cref="IUserProfile{TUserKey}" /> value or <see langword="null"/> when user is unauthorized</param>
    /// <param name="customAction">Additional custom action to perform over query</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>
    ///     The first entity or <see langword="null" /> taken by executing query to database table,
    ///     wrapped with <see cref="ServiceResult{T}"/>
    /// </returns>
    /// <exception cref="OperationCanceledException" />
    Task<ServiceResult<TEntity?>> GetEntityByIdAsync(
        TKey id,
        IUserProfile<TUserKey>? userProfile,
        Func<CrudActionContext<TEntity, TKey, TUserKey>, ValueTask<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the first entity by executing query to database table and maps to <typeparamref name="TDto" />
    /// </summary>
    /// <param name="id">PK of the Entity</param>
    /// <param name="userProfile"><see cref="IUserProfile{TUserKey}" /> value or <see langword="null"/> when user is unauthorized</param>
    /// <param name="customAction">Additional custom action to perform over query</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>
    ///     The first entity mapped as <typeparamref name="TDto" /> or <see langword="null" /> taken by executing query to
    ///     database table, wrapped with <see cref="ServiceResult{T}"/>
    /// </returns>
    /// <exception cref="OperationCanceledException" />
    Task<ServiceResult<TDtoFull?>> GetByIdAsync(
        TKey id,
        IUserProfile<TUserKey>? userProfile,
        Func<CrudActionContext<TEntity, TKey, TUserKey>, ValueTask<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the <see cref="IAsyncEnumerable{TEntity}" /> of <typeparamref name="TEntity"/> by executing query to database table
    /// </summary>
    /// <param name="userProfile"><see cref="IUserProfile{TUserKey}" /> value or <see langword="null"/> when user is unauthorized</param>
    /// <param name="customAction">Additional custom action to perform over query</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>
    ///     <see cref="IAsyncEnumerable{TEntity}" /> of <typeparamref name="TEntity"/>,
    ///     wrapped with <see cref="ServiceResult{T}"/>
    /// </returns>
    /// <exception cref="OperationCanceledException" />
    Task<ServiceResult<IAsyncEnumerable<TEntity>>> GetEntityListAsync(
        IUserProfile<TUserKey>? userProfile,
        Func<CrudActionContext<TEntity, TKey, TUserKey>, ValueTask<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the <see cref="IAsyncEnumerable{TDto}" /> of <typeparamref name="TDto"/> by executing query to database table
    /// </summary>
    /// <param name="userProfile"><see cref="IUserProfile{TUserKey}" /> value or <see langword="null"/> when user is unauthorized</param>
    /// <param name="customAction">Additional custom action to perform over query</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>
    ///     <see cref="IAsyncEnumerable{TDto}" />, wrapped with <see cref="ServiceResult{T}"/>
    /// </returns>
    /// <exception cref="OperationCanceledException" />
    Task<ServiceResult<IAsyncEnumerable<TDto>>> GetListAsync(
        IUserProfile<TUserKey> userProfile,
        Func<CrudActionContext<TEntity, TKey, TUserKey>, ValueTask<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the <see cref="IAsyncEnumerable{TDtoFull}" /> of <typeparamref name="TDtoFull"/> by executing query to database table
    /// </summary>
    /// <param name="userProfile"><see cref="IUserProfile{TUserKey}" /> value or <see langword="null"/> when user is unauthorized</param>
    /// <param name="customAction">Additional custom action to perform over query</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>
    ///     <see cref="IAsyncEnumerable{TDtoFull}" /> of <typeparamref name="TDtoFull"/>,
    ///     wrapped with <see cref="ServiceResult{T}"/>
    /// </returns>
    /// <exception cref="OperationCanceledException" />
    Task<ServiceResult<IAsyncEnumerable<TDtoFull>>> GetFullEntityListAsync(
        IUserProfile<TUserKey>? userProfile,
        Func<CrudActionContext<TEntity, TKey, TUserKey>, ValueTask<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sets the value of the <strong><see cref="IEntity{TKey}.Active" /></strong> property of
    ///     <typeparamref name="TEntity" /> to <see langword="false" /> and saves to the Database
    /// </summary>
    /// <param name="id">PK of the Entity</param>
    /// <param name="userProfile"><see cref="IUserProfile{TUserKey}" /> value or <see langword="null"/> when user is unauthorized</param>
    /// <param name="customAction">Additional custom action to perform over query</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>
    ///     operation result as <see cref="ServiceResult"/>
    /// </returns>
    /// <exception cref="OperationCanceledException" />
    Task<ServiceResult> DeactivateByIdAsync(
        TKey id,
        IUserProfile<TUserKey>? userProfile,
        Func<CrudActionContext<TEntity, TKey, TUserKey>, ValueTask<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Insert <paramref name="entity" /> to the database
    /// </summary>
    /// <param name="entity">Entity to insert</param>
    /// <param name="userProfile"><see cref="IUserProfile{TUserKey}" /> value or <see langword="null"/> when user is unauthorized</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>
    ///     <paramref name="entity"/>, with Primary Key value set, wrapped with <see cref="ServiceResult{T}"/>
    /// </returns>
    /// <exception cref="OperationCanceledException" />
    Task<ServiceResult<TEntity>> InsertAsync(TEntity entity, IUserProfile<TUserKey>? userProfile, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Insert <paramref name="entity" /> to the database
    /// </summary>
    /// <param name="entity">Entity to insert</param>
    /// <param name="userProfile"><see cref="IUserProfile{TUserKey}" /> value or <see langword="null"/> when user is unauthorized</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>
    ///     <paramref name="entity"/>, with Primary Key value set, wrapped with <see cref="ServiceResult{T}"/>
    /// </returns>
    /// <exception cref="OperationCanceledException" />
    Task<ServiceResult<TDtoFull>> InsertAsync(TDtoFull entity, IUserProfile<TUserKey>? userProfile, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Update <paramref name="entity"/> in the database
    /// </summary>
    /// <param name="entity">Entity to update</param>
    /// <param name="userProfile"><see cref="IUserProfile{TUserKey}" /> value or <see langword="null"/> when user is unauthorized</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>
    ///     updated <paramref name="entity"/>, wrapped with <see cref="ServiceResult{T}"/>
    /// </returns>
    /// <exception cref="OperationCanceledException" />
    Task<ServiceResult<TEntity>> UpdateAsync(TEntity entity, IUserProfile<TUserKey>? userProfile, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Update <paramref name="entity"/> in the database
    /// </summary>
    /// <param name="entity">Entity's Data Transfer Object</param>
    /// <param name="userProfile"><see cref="IUserProfile{TUserKey}" /> value or <see langword="null"/> when user is unauthorized</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>
    ///     updated <paramref name="entity"/>, wrapped with <see cref="ServiceResult{T}"/>
    /// </returns>
    /// <exception cref="OperationCanceledException" />
    Task<ServiceResult<TDtoFull>> UpdateAsync(TDtoFull entity, IUserProfile<TUserKey>? userProfile, CancellationToken cancellationToken = default);
}