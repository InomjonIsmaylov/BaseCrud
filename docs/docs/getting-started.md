# Getting Started

## Predefine classes

1. Models, DTOs
2. Services (interfaces, implementations)
3. UserProfile

### Models, DTOs

* BaseCrud uses [AutoMapper](https://automapper.org/) package. Developers **should not** create mappings for Models and DTOs as BaseCrud automatically registers those mappings in AutoMapper profile

#### Models

* Model has to implement `IEntity<TKey>`, `IEntity` or derive from `EntityBase<TKey>` or `EntityBase`

```csharp
public class WeatherForecast : EntityBase
{
    public DateOnly Date { get; set; }

    public int TemperatureC { get; set; }

    [NotMapped]
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string? Summary { get; set; }
}
```

#### DTOs

* BaseCrud is designed to work with 2 DTO types for any model. One with fewer properties (for data-table view), second for detailed description. *However, it is also possible to use BaseCrud with only one DTO for a given model*
* DTOs has to implement `IDataTransferObject<TEntity>`

```csharp
public class WeatherForecastDto : IDataTransferObject<WeatherForecast>
{
    public DateOnly Date { get; set; }

    public int TemperatureC { get; set; }
}

public class WeatherForecastDetailsDto : IDataTransferObject<WeatherForecast>
{
    public DateOnly Date { get; set; }

    public int TemperatureC { get; set; }

    [NotMapped]
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string? Summary { get; set; }
}
```

### Services (interfaces and implementations)

* **Should not** be registered in DI container since BaseCrud will automatically register defined services and interfaces as scoped in DI

#### interfaces

* interfaces must inherit from either `ICrudService` or `IEfCrudService`. *`IEfCrudService` provides richer functionality and uses EntityFramework*

```csharp
public interface IWeatherService : IEfCrudService<WeatherForecast, WeatherForecastDto, WeatherForecastDetailsDto>;
```

#### implementations

* implementations must inherit from `BaseCrudService` and implement just created interface
* Provide `DbContext` and `IMapper` instance from DI to base

```csharp
public class WeatherService : BaseCrudService<WeatherForecast, WeatherForecastDto, WeatherForecastDetailsDto>, IWeatherService
{
    public WeatherService(
        AppDbContext dbContext,
        IMapper mapper,
        ILogger<IWeatherService> logger
        ) : base(dbContext, mapper)
    {

    }
}
```

### UserProfile

#### UserProfile class

* UserProfile class must implement `IUserProfile` interface

```csharp
public class ApiUser : IUserProfile
{
    public int Id { get; set; }

    public string? UserName { get; set; }

    public string? Fullname { get; set; }
}
```

### Additional

#### DbContext

```csharp
public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<WeatherForecast> WeatherForecasts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("TestWebDb");

        base.OnConfiguring(optionsBuilder);
    }
}
```

#### Add BaseCrud to service collection

* BaseCrud should be provided with BaseCrudOptions with a set of assemblies
* Assemblies are used for

> Assemblies scanned for Model-DTO mappings, custom mapping configuration, custom filter expressions

```csharp

var builder = WebApplication.CreateBuilder(args);

...

builder.Services.AddBaseCrudService(new BaseCrudServiceOptions
{
    Assemblies = [Assembly.GetExecutingAssembly(), ...]
});

...

var app = builder.Build();
```

### *voila (that's it)

Now you are ready to use BaseCrud.
Like this:

```csharp
public class WeatherForecastController(IWeatherService service) : BaseController
{
    /// <summary>
    /// Gets all WeatherForecast entities from db
    /// </summary>
    /// <param name="metaData">meta data for filtering, sorting and pagination</param>
    /// <returns>A query result of WeatherForecastDto</returns>
    [HttpPost("[action]")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(QueryResult<WeatherForecastDto>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ServiceError[]))]
    public async Task<ActionResult<QueryResult<WeatherForecastDto>?>> GetAll(PrimeTableMetaData metaData)
    {
        try
        {
            var actionResult = await service.GetAllAsync(metaData, UserProfile);
        
            if (actionResult.TryGetResult(out var result))
            {
                return result;
            }
        
            return StatusCode(actionResult.StatusCode, actionResult.Errors);
        }
        catch (Exception e)
        {
            return StatusCode(500,
                new ServiceError(
                    e.Message,
                    e.GetType().Name.ToSnakeCase().Replace("exception", "error")
                )
            );
        }
    }
}
```
