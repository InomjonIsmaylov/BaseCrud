namespace BaseCrud.Abstractions.Entities;

/// <summary>
/// A type definition of the Application user
/// </summary>
/// <typeparam name="TKey">Type of User key</typeparam>
public interface IUserProfile<TKey>
    where TKey : struct, IEquatable<TKey>
{
    TKey Id { get; set; }

    string? UserName { get; set; }

    string? Fullname { get; set; }
}

/// <inheritdoc />
public interface IUserProfile : IUserProfile<int>;