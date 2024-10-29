namespace BaseCrud.Entities;

/// <summary>
/// Type used to map Entities of the type <typeparamref name="TEntity"/> to <em>DataTransferObject</em>
/// </summary>
/// <typeparam name="TEntity">Entity type of the Database</typeparam>
/// <typeparam name="TKey">Type param of Key property</typeparam>
public interface IDataTransferObject<TEntity, TKey> where TEntity : IEntity<TKey> where TKey : struct, IEquatable<TKey>;

/// <summary>
/// Type used to map Entities of the type <typeparamref name="TEntity"/> to <em>DataTransferObject</em>
/// </summary>
/// <typeparam name="TEntity">Entity type of the Database</typeparam>
public interface IDataTransferObject<TEntity> : IDataTransferObject<TEntity, int> where TEntity : IEntity;