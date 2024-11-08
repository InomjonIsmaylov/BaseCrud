using Microsoft.EntityFrameworkCore;
using WebTester.Models;

namespace WebTester.DataBase;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<WeatherForecast> WeatherForecasts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("TestWebDb");

        base.OnConfiguring(optionsBuilder);
    }
}