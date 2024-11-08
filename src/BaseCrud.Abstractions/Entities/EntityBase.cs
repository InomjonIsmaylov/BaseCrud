namespace BaseCrud.Abstractions.Entities;

public abstract class EntityBase : EntityBase<int>, IEntity;

public abstract class EntityBase<TKey> : IEntity<TKey> where TKey : struct, IEquatable<TKey>
{
    public TKey Id { get; set; }

    public bool Active { get; set; } = true;

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }
}