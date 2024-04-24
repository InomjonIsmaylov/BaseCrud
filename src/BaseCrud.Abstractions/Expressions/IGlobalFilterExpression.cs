using BaseCrud.General.Entities;

namespace BaseCrud.Abstractions.Expressions;

public interface IGlobalFilterExpression<TEntity> : IGlobalFilterExpression<TEntity, int> where TEntity : IEntity;

public interface IGlobalFilterExpression<TEntity, in TKey>
    where TKey : struct, IEquatable<TKey>
    where TEntity : IEntity<TKey>
{
    public Expression<Func<TEntity, bool>> GlobalSearchExpression(string strSearch);
}
