using BaseCrud.Abstractions.Entities;

namespace WebTester.Classes;

/// <inheritdoc />
public class ApiUser : IUserProfile
{
    public int Id { get; set; }

    public string? UserName { get; set; }

    public string? Fullname { get; set; }
}