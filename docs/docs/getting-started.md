# Getting Started

## Define classes

1. Models, DTOs
2. Services (interfaces, implementations)
3. UserProfile

### Models, DTOs

* BaseCrud requires an `IDtoMapping<TEntity, TDto>` implementation for every list and details DTO used by a CRUD service.
* Mapping implementations are discovered from the assemblies passed to `AddBaseCrudService`.

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
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    public int TemperatureC { get; set; }
}

public class WeatherForecastDetailsDto : IDataTransferObject<WeatherForecast>
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    public int TemperatureC { get; set; }

    [NotMapped]
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string? Summary { get; set; }
}
```

#### DTO mappings

Define the select, insert, and update expressions for both DTO types. `SelectExpression` must be translatable by Entity Framework when it is used in a database query.

```csharp
public sealed class WeatherForecastExpressions :
    IDtoMapping<WeatherForecast, WeatherForecastDto>,
    IDtoMapping<WeatherForecast, WeatherForecastDetailsDto>
{
    Expression<Func<WeatherForecast, WeatherForecastDto>>
        IDtoMapping<WeatherForecast, WeatherForecastDto, int>.SelectExpression =>
        entity => new WeatherForecastDto
        {
            Id = entity.Id,
            Date = entity.Date,
            TemperatureC = entity.TemperatureC
        };

    Expression<Func<WeatherForecastDto, WeatherForecast>>
        IDtoMapping<WeatherForecast, WeatherForecastDto, int>.InsertMappingToEntity =>
        dto => new WeatherForecast
        {
            Date = dto.Date,
            TemperatureC = dto.TemperatureC
        };

    Expression<Func<WeatherForecast, WeatherForecastDto, WeatherForecast>>
        IDtoMapping<WeatherForecast, WeatherForecastDto, int>.UpdateMappingToEntity =>
        (entity, dto) => new WeatherForecast
        {
            Id = entity.Id,
            Active = entity.Active,
            Date = dto.Date,
            TemperatureC = dto.TemperatureC,
            Summary = entity.Summary
        };

    Expression<Func<WeatherForecast, WeatherForecastDetailsDto>>
        IDtoMapping<WeatherForecast, WeatherForecastDetailsDto, int>.SelectExpression =>
        entity => new WeatherForecastDetailsDto
        {
            Id = entity.Id,
            Date = entity.Date,
            TemperatureC = entity.TemperatureC,
            Summary = entity.Summary
        };

    Expression<Func<WeatherForecastDetailsDto, WeatherForecast>>
        IDtoMapping<WeatherForecast, WeatherForecastDetailsDto, int>.InsertMappingToEntity =>
        dto => new WeatherForecast
        {
            Date = dto.Date,
            TemperatureC = dto.TemperatureC,
            Summary = dto.Summary
        };

    Expression<Func<WeatherForecast, WeatherForecastDetailsDto, WeatherForecast>>
        IDtoMapping<WeatherForecast, WeatherForecastDetailsDto, int>.UpdateMappingToEntity =>
        (entity, dto) => new WeatherForecast
        {
            Id = entity.Id,
            Active = entity.Active,
            Date = dto.Date,
            TemperatureC = dto.TemperatureC,
            Summary = dto.Summary
        };
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
* Provide the `DbContext` and `IDtoMappingRegistry` instances from DI to the base class

```csharp
public class WeatherService : BaseCrudService<WeatherForecast, WeatherForecastDto, WeatherForecastDetailsDto>, IWeatherService
{
    public WeatherService(
        AppDbContext dbContext,
        IDtoMappingRegistry mappingRegistry,
        ILogger<IWeatherService> logger
        ) : base(dbContext, mappingRegistry)
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

## Additional

### DbContext

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

### Add BaseCrud to service collection

* BaseCrud should be provided with BaseCrudOptions with a set of assemblies
* Assemblies are used for

> Assemblies are scanned for CRUD services, `IDtoMapping` implementations, and custom filter expressions.

`AddBaseCrudService` validates the required list and details DTO mappings during startup. A missing or duplicate mapping throws an exception immediately.

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
