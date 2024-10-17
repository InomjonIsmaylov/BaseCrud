using BaseCrud.General.Entities;
using BaseCrud.General.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

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
public interface ICrudService<TEntity, TDto, TDtoFull, TKey>
    where TKey : struct, IEquatable<TKey>
    where TEntity : IEntity<TKey>
    where TDto : class, IDataTransferObject<TEntity, TKey>
    where TDtoFull : class, IDataTransferObject<TEntity, TKey>
{
    /// <summary>
    ///     Gets all entities by executing query to database table and maps entities to <typeparamref name="TDto" />
    /// </summary>
    /// <typeparam name="TDto">Dto to be mapper</typeparam>
    /// <param name="dataTableMeta"><see cref="IDataTableMetaData" /> of PrimeNG Filters</param>
    /// <param name="userProfile"><see cref="IUserProfile" /> value</param>
    /// <param name="customAction">Additional custom action to perform over query</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns><see cref="QueryResult{TDto}" /> of entities </returns>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="TableMetaDataInvalidException" />
    /// <exception cref="OperationCanceledException" />
    Task<QueryResult<TDto>> GetAllAsync(
        IDataTableMetaData dataTableMeta,
        IUserProfile userProfile,
        Func<CrudActionContext<TEntity, TKey>, ValueTask<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the first entity by executing query to database table
    /// </summary>
    /// <param name="id">The id of the entity</param>
    /// <param name="userProfile"><see cref="IUserProfile" /> value</param>
    /// <param name="customAction">Additional custom action to perform over query</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>The first entity or <see langword="null" /> taken by executing query to database table </returns>
    /// <exception cref="ArgumentException" />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="InvalidIdArgumentException" />
    /// <exception cref="OperationCanceledException" />
    Task<TEntity?> GetEntityByIdAsync(
        TKey id,
        IUserProfile userProfile,
        Func<CrudActionContext<TEntity, TKey>, ValueTask<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the first entity by executing query to database table and maps to <typeparamref name="TDto" />
    /// </summary>
    /// <param name="id">id of the Entity</param>
    /// <param name="userProfile"><see cref="IUserProfile" /> value</param>
    /// <param name="customAction">Additional custom action to perform over query</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>
    ///     The first entity mapped as <typeparamref name="TDto" /> or <see langword="null" /> taken by executing query to
    ///     database table
    /// </returns>
    /// <exception cref="ArgumentException" />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="InvalidIdArgumentException" />
    /// <exception cref="OperationCanceledException" />
    Task<TDtoFull?> GetByIdAsync(
        TKey id,
        IUserProfile userProfile,
        Func<CrudActionContext<TEntity, TKey>, ValueTask<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a <see cref="List{TEntity}" /> by executing query to database table
    /// </summary>
    /// <param name="userProfile"><see cref="IUserProfile" /> value</param>
    /// <param name="customAction">Additional custom action to perform over query</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>
    ///     <see cref="List{TEntity}" />
    /// </returns>
    /// <exception cref="ArgumentException" />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="InvalidIdArgumentException" />
    /// <exception cref="OperationCanceledException" />
    Task<IAsyncEnumerable<TEntity>> GetEntityListAsync(
        IUserProfile userProfile,
        Func<CrudActionContext<TEntity, TKey>, ValueTask<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a <see cref="List{TDto}" /> by executing query to database table
    /// </summary>
    /// <param name="userProfile"><see cref="IUserProfile" /> value</param>
    /// <param name="customAction">Additional custom action to perform over query</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>
    ///     <see cref="List{TDto}" />
    /// </returns>
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="ArgumentNullException" />
    Task<IAsyncEnumerable<TDto>> GetListAsync(
        IUserProfile userProfile,
        Func<CrudActionContext<TEntity, TKey>, ValueTask<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a <see cref="List{TDtoFull}" /> by executing query to database table
    /// </summary>
    /// <param name="userProfile"><see cref="IUserProfile" /> value</param>
    /// <param name="customAction">Additional custom action to perform over query</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>
    ///     <see cref="List{TDtoFull}" />
    /// </returns>
    /// <exception cref="OperationCanceledException" />
    Task<IAsyncEnumerable<TDtoFull>> GetFullEntityListAsync(
        IUserProfile userProfile,
        Func<CrudActionContext<TEntity, TKey>, ValueTask<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sets the value of the <see cref="IEntity.Active" /> property of <typeparamref name="TEntity" /> to
    ///     <see langword="false" /> and saves to the Database
    /// </summary>
    /// <param name="id">id of the Entity</param>
    /// <param name="userProfile"><see cref="IUserProfile" /> value</param>
    /// <param name="customAction">Additional custom action to perform over query</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns><see langword="true" /> if entity has been saved successfully</returns>
    /// <exception cref="InvalidIdArgumentException" />
    /// <exception cref="OperationCanceledException" />
    Task<bool> DeactivateByIdAsync(
        TKey id,
        IUserProfile userProfile,
        Func<CrudActionContext<TEntity, TKey>, ValueTask<IQueryable<TEntity>>>? customAction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Insert
    ///     <param name="entity"></param>
    ///     to the database
    /// </summary>
    /// <param name="entity">Entity to insert</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns><see langword="true" /> if entity has been saved successfully</returns>
    /// <exception cref="DatabaseOperationException" />
    /// <exception cref="OperationCanceledException" />
    Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Insert
    ///     <param name="entity"></param>
    ///     to the database
    /// </summary>
    /// <param name="entity">Entity to insert</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns><see langword="true" /> if entity has been saved successfully</returns>
    /// <exception cref="DatabaseOperationException" />
    /// <exception cref="OperationCanceledException" />
    Task<TDtoFull> InsertAsync(TDtoFull entity, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Update
    ///     <param name="entity"></param>
    ///     in the database
    /// </summary>
    /// <param name="entity">Entity to update</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns><see langword="true" /> if entity has been saved successfully</returns>
    /// <exception cref="InvalidIdArgumentException" />
    /// <exception cref="DatabaseOperationException" />
    /// <exception cref="OperationCanceledException" />
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Update
    ///     <param name="entity"></param>
    ///     in the database
    /// </summary>
    /// <param name="entity">Entity's Data Transfer Object</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns><see langword="true" /> if entity has been saved successfully</returns>
    /// <exception cref="InvalidIdArgumentException" />
    /// <exception cref="DatabaseOperationException" />
    /// <exception cref="OperationCanceledException" />
    Task<TDtoFull> UpdateAsync(TDtoFull entity, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Partially update <typeparamref name="TEntity" /> in the database or bulk update <typeparamref name="TEntity" /> in
    ///     the database
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="setPropertyCalls">A function to set the property value.</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>The number of entries updated in the database.</returns>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    Task<int> PatchUpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Partially update <typeparamref name="TEntity" /> in the database or bulk update <typeparamref name="TEntity" /> in
    ///     the database
    /// </summary>
    /// <param name="id">The id of the <typeparamref name="TEntity" /> to update.</param>
    /// <param name="setPropertyCalls">A function to set the property value.</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>The number of entries updated in the database.</returns>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="InvalidIdArgumentException" />
    /// <exception cref="OperationCanceledException" />
    Task<int> PatchUpdateAsync(
        TKey id,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Partially update <typeparamref name="TEntity" /> in the database or bulk update <typeparamref name="TEntity" /> in
    ///     the database
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="selector">
    ///     A selector to create projectile to db entity <typeparamref name="TResult" /> from main entity
    ///     <typeparamref name="TEntity" />
    /// </param>
    /// <param name="setPropertyCalls">A function to set the property value.</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>The number of entries updated in the database.</returns>
    /// <remarks>
    ///     Use this method to update <see cref="OwnedAttribute" /> properties of <typeparamref name="TEntity" /> in the
    ///     database or bulk update
    ///     <typeparamref name="TEntity" /> in the database
    /// </remarks>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    Task<int> PatchUpdateAsync<TResult>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<SetPropertyCalls<TResult>, SetPropertyCalls<TResult>>> setPropertyCalls,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Partially update <typeparamref name="TEntity" /> in the database or bulk update <typeparamref name="TEntity" /> in
    ///     the database
    /// </summary>
    /// <param name="id">The id of the <typeparamref name="TEntity" /> to update.</param>
    /// <param name="selector">
    ///     A selector to create projectile to db entity <typeparamref name="TResult" /> from main entity
    ///     <typeparamref name="TEntity" />
    /// </param>
    /// <param name="setPropertyCalls">A function to set the property value.</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>The number of entries updated in the database.</returns>
    /// <remarks>
    ///     Use this method to update <see cref="OwnedAttribute" /> properties of <typeparamref name="TEntity" /> in the
    ///     database or bulk update
    ///     <typeparamref name="TEntity" /> in the database
    /// </remarks>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    Task<int> PatchUpdateAsync<TResult>(
        TKey id,
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<SetPropertyCalls<TResult>, SetPropertyCalls<TResult>>> setPropertyCalls,
        CancellationToken cancellationToken = default);
}