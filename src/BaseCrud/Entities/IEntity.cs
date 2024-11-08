namespace BaseCrud.Entities;

public interface IEntity<TKey> where TKey : struct, IEquatable<TKey>
{
    TKey Id { get; set; }

    bool Active { get; set; }

    string? CreatedBy { get; set; }

    DateTime? CreatedDate { get; set; }

    string? LastModifiedBy { get; set; }

    DateTime? LastModifiedDate { get; set; }
}

public interface IEntity : IEntity<int>;