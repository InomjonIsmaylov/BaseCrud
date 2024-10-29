using Microsoft.EntityFrameworkCore.Query;

namespace BaseCrud.EntityFrameworkCore.Services;

/// <inheritdoc />
public interface IEfCrudService<TEntity, TDto, TDtoFull, TKey, TUserKey>
    : ICrudService<TEntity, TDto, TDtoFull, TKey, TUserKey>
    where TKey : struct, IEquatable<TKey>
    where TEntity : IEntity<TKey>
    where TDto : class, IDataTransferObject<TEntity, TKey>
    where TDtoFull : class, IDataTransferObject<TEntity, TKey>
    where TUserKey : struct, IEquatable<TUserKey>
{
    /// <summary>
    ///     Patch update <typeparamref name="TEntity" /> entries that match the <paramref name="predicate"/> in the database
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="setPropertyCalls">A function to set the property value.</param>
    /// <param name="userProfile"><see cref="IUserProfile{TUserKey}" /> value or <see langword="null"/> when user is unauthorized</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>
    ///     The number of entries updated in the database.
    /// </returns>
    /// <exception cref="OperationCanceledException" />
    Task<ServiceResult<int>> PatchUpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls,
        IUserProfile<TUserKey>? userProfile,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Patch update the <typeparamref name="TEntity" /> whose PK value is <paramref name="id"/>
    /// </summary>
    /// <param name="id">The id of the <typeparamref name="TEntity" /> to update.</param>
    /// <param name="setPropertyCalls">A function to set the property value.</param>
    /// <param name="userProfile"><see cref="IUserProfile{TUserKey}" /> value or <see langword="null"/> when user is unauthorized</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>
    ///     updated <typeparamref name="TEntity" /> as <typeparamref name="TDtoFull"/>, wrapped with <see cref="ServiceResult{T}"/>
    /// </returns>
    /// <exception cref="OperationCanceledException" />
    Task<ServiceResult<TDtoFull>> PatchUpdateAsync(
        TKey id,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls,
        IUserProfile<TUserKey>? userProfile,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Patch update <typeparamref name="TEntity" /> entries that match the <paramref name="predicate"/> in the database
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="selector">
    ///     A selector to create projectile to db entity <typeparamref name="TResult" /> from main entity
    ///     <typeparamref name="TEntity" />
    /// </param>
    /// <param name="setPropertyCalls">A function to set the property value.</param>
    /// <param name="userProfile"><see cref="IUserProfile{TUserKey}" /> value or <see langword="null"/> when user is unauthorized</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>
    ///     The number of entries updated in the database.
    /// </returns>
    /// <remarks>
    ///     Use this method to update <see cref="OwnedAttribute" /> properties of <typeparamref name="TEntity" /> in the
    ///     database or bulk update <typeparamref name="TEntity" /> in the database
    /// </remarks>
    /// <exception cref="OperationCanceledException" />
    Task<ServiceResult<int>> PatchUpdateAsync<TResult>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<SetPropertyCalls<TResult>, SetPropertyCalls<TResult>>> setPropertyCalls,
        IUserProfile<TUserKey>? userProfile,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Patch update the <typeparamref name="TEntity" /> whose PK value is <paramref name="id"/>
    /// </summary>
    /// <param name="id">The id of the <typeparamref name="TEntity" /> to update.</param>
    /// <param name="selector">
    ///     A selector to create projectile to db entity <typeparamref name="TResult" /> from main entity
    ///     <typeparamref name="TEntity" />
    /// </param>
    /// <param name="setPropertyCalls">A function to set the property value.</param>
    /// <param name="userProfile"><see cref="IUserProfile{TUserKey}" /> value or <see langword="null"/> when user is unauthorized</param>
    /// <param name="cancellationToken">Token to cancel long execution</param>
    /// <returns>
    ///    updated <typeparamref name="TEntity" /> as <typeparamref name="TDtoFull"/>, wrapped with <see cref="ServiceResult{T}"/>
    /// </returns>
    /// <remarks>
    ///     Use this method to update <see cref="OwnedAttribute" /> properties of <typeparamref name="TEntity" /> in the
    ///     database or bulk update <typeparamref name="TEntity" /> in the database
    /// </remarks>
    /// <exception cref="OperationCanceledException" />
    Task<ServiceResult<TDtoFull>> PatchUpdateAsync<TResult>(
        TKey id,
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<SetPropertyCalls<TResult>, SetPropertyCalls<TResult>>> setPropertyCalls,
        IUserProfile<TUserKey>? userProfile,
        CancellationToken cancellationToken = default);
}