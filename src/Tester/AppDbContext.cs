using Microsoft.EntityFrameworkCore;

namespace Tester;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Model> Models { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("TestDb");

        base.OnConfiguring(optionsBuilder);
    }
}