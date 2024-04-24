namespace BaseCrud.Abstractions.Entities;

public interface IUserProfile
{
    int Id { get; set; }

    string? UserName { get; set; }

    string? Fullname { get; set; }
}