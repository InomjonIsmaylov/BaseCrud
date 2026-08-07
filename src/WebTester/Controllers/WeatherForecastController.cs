using BaseCrud.Abstractions.Entities;
using BaseCrud.Errors;
using BaseCrud.PrimeNg;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using WebTester.DataBase;
using WebTester.Models;
using WebTester.Services;

namespace WebTester.Controllers;

public class WeatherForecastController(
    ILogger<WeatherForecastController> logger,
    IWeatherService service,
    AppDbContext context
) : BaseController
{
    /// <summary>
    /// Gets all weather forecasts from db
    /// </summary>
    [HttpGet(Name = "GetWeatherForecast")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(WeatherForecast[]))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ServiceError[]))]
    public async Task<ActionResult<IAsyncEnumerable<WeatherForecastDetailsDto>?>> Get()
    {
        await EnsureInitAsync();
        return await FromServiceResult(service.GetFullEntityListAsync(UserProfile));
    }

    /// <summary>
    /// Gets all WeatherForecast entities from db
    /// </summary>
    [HttpPost("[action]")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(QueryResult<WeatherForecastDto>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ServiceError[]))]
    public async Task<ActionResult<QueryResult<WeatherForecastDto>?>> GetAll(PrimeTableMetaData metaData)
    {
        await EnsureInitAsync();
        return await FromServiceResult(service.GetAllAsync(metaData, UserProfile));
    }

    /// <summary>
    /// Full update via <c>IDtoMapping.UpdateMappingToEntity</c>
    /// </summary>
    [HttpPut("{id:int}")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(WeatherForecastDetailsDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ServiceError[]))]
    [SwaggerResponse(StatusCodes.Status404NotFound, typeof(ServiceError[]))]
    public async Task<ActionResult<WeatherForecastDetailsDto?>> Update(int id, [FromBody] WeatherForecastDetailsDto dto)
    {
        await EnsureInitAsync();
        dto.Id = id;
        logger.LogInformation("Updating weather forecast {Id}", id);
        return await FromServiceResult(service.UpdateAsync(dto, UserProfile));
    }

    /// <summary>
    /// Partial update via EF <c>ExecuteUpdate</c> / <c>PatchUpdateAsync</c>
    /// </summary>
    [HttpPatch("{id:int}")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(WeatherForecastDetailsDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ServiceError[]))]
    [SwaggerResponse(StatusCodes.Status404NotFound, typeof(ServiceError[]))]
    public async Task<ActionResult<WeatherForecastDetailsDto?>> Patch(int id, [FromBody] WeatherForecastPatchRequest patch)
    {
        await EnsureInitAsync();
        logger.LogInformation("Patching weather forecast {Id}", id);
        return await FromServiceResult(service.PatchUpdateAsync(
            id,
            setters => setters
                .SetProperty(x => x.Summary, patch.Summary)
                .SetProperty(x => x.TemperatureC, patch.TemperatureC),
            UserProfile));
    }

    private static bool _init;

    private async Task EnsureInitAsync()
    {
        if (_init)
            return;

        await context.Database.EnsureCreatedAsync();

        if (!await context.WeatherForecasts.AnyAsync())
        {
            context.WeatherForecasts.AddRange(
                new WeatherForecast
                {
                    TemperatureC = 15,
                    Summary = "humid air",
                    Date = DateOnly.FromDateTime(DateTime.Now.Date.AddDays(-2))
                },
                new WeatherForecast
                {
                    TemperatureC = 30,
                    Summary = "hot weather",
                    Date = DateOnly.FromDateTime(DateTime.Now.Date.AddDays(-1))
                },
                new WeatherForecast
                {
                    TemperatureC = -5,
                    Summary = "freezing",
                    Date = DateOnly.FromDateTime(DateTime.Now.Date)
                },
                new WeatherForecast
                {
                    TemperatureC = 40,
                    Summary = "rain",
                    Date = DateOnly.FromDateTime(DateTime.Now.Date.AddDays(1))
                });

            await context.SaveChangesAsync();
        }

        _init = true;
    }
}

public record WeatherForecastPatchRequest(string Summary, int TemperatureC);
