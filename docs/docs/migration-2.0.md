# Migrate from 1.x to 2.0

BaseCrud 2.0 removes its AutoMapper dependency and replaces implicit DTO mapping with explicit expression-based mappings. This is a breaking change. Applications that cannot migrate yet should stay on the latest 1.x packages.

## 1. Update service constructors

Remove AutoMapper and `IMapper` from each CRUD service constructor. Inject `IDtoMappingRegistry` and pass it to the base service instead:

```csharp
public WeatherService(
    AppDbContext dbContext,
    IDtoMappingRegistry mappingRegistry,
    ILogger<IWeatherService> logger
) : base(dbContext, mappingRegistry)
{
}
```

Remove application-level AutoMapper registration if no other part of the application uses it.

## 2. Add mappings for every DTO

Implement `IDtoMapping<TEntity, TDto>` for both the list DTO and details DTO used by each CRUD service. Every mapping supplies:

- `SelectExpression` for Entity-to-DTO projection. It must be translatable by Entity Framework.
- `InsertMappingToEntity` for creating a new entity from a DTO.
- `UpdateMappingToEntity` for applying DTO values while preserving entity values that must not change.

One class may implement mappings for multiple DTOs:

```csharp
public sealed class WeatherForecastExpressions :
    IDtoMapping<WeatherForecast, WeatherForecastDto>,
    IDtoMapping<WeatherForecast, WeatherForecastDetailsDto>
{
    // Implement SelectExpression, InsertMappingToEntity, and
    // UpdateMappingToEntity for each DTO type.
}
```

The getting-started guide contains a complete minimal example.

## 3. Replace custom AutoMapper configuration

`ICustomMappedDto` and AutoMapper profiles are no longer supported. Move their behavior into the three `IDtoMapping` expressions:

- Replace Entity-to-DTO profile configuration with `SelectExpression`.
- Replace DTO-to-new-entity configuration with `InsertMappingToEntity`.
- Replace DTO-to-existing-entity configuration with `UpdateMappingToEntity`.

Review update expressions carefully and preserve keys, activation state, and other fields that the DTO must not overwrite.

## 4. Remove mapper access from action contexts

`CrudActionContext.Mapper` has been removed. Replace direct mapper calls in custom CRUD actions with explicit application mapping code. CRUD service DTO operations use the registered `IDtoMapping` automatically.

## 5. Replace `ISelectExpression`

`ISelectExpression` has been removed. Put the projection directly in the mapping's `IDtoMapping.SelectExpression` property.

## 6. Verify startup

Ensure every mapping class is in an assembly supplied to `AddBaseCrudService`. BaseCrud scans those assemblies and validates all list and details DTO pairs at startup. Missing and duplicate mappings fail startup with an exception identifying the invalid mapping pair.
