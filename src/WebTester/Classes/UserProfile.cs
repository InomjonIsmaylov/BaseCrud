using BaseCrud.Abstractions.Entities;

namespace WebTester.Classes;

public class UserProfile : IUserProfile
{
    public int Id { get; set; }
    public string? UserName { get; set; }
    public string? Fullname { get; set; }
}