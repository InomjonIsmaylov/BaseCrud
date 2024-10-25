namespace BaseCrud.Abstractions.Entities;

public interface IUserProfile<TKey>
    where TKey : struct, IEquatable<TKey>
{
    TKey Id { get; set; }

    string? UserName { get; set; }

    string? Fullname { get; set; }
}

public interface IUserProfile : IUserProfile<int>;