using System.Linq.Expressions;
using BaseCrud.Abstractions.Expressions;

namespace WebTester.Models;

public sealed class WeatherForecastExpressions :
    IDtoMapping<WeatherForecast, WeatherForecastDto>,
    IDtoMapping<WeatherForecast, WeatherForecastDetailsDto>
{
    Expression<Func<WeatherForecast, WeatherForecastDto>> IDtoMapping<WeatherForecast, WeatherForecastDto, int>.SelectExpression =>
        forecast => new WeatherForecastDto
        {
            Id = forecast.Id,
            Date = forecast.Date,
            TemperatureC = forecast.TemperatureC
        };

    Expression<Func<WeatherForecastDto, WeatherForecast>> IDtoMapping<WeatherForecast, WeatherForecastDto, int>.InsertMappingToEntity =>
        dto => new WeatherForecast
        {
            Date = dto.Date,
            TemperatureC = dto.TemperatureC
        };

    Expression<Func<WeatherForecast, WeatherForecastDto, WeatherForecast>> IDtoMapping<WeatherForecast, WeatherForecastDto, int>.UpdateMappingToEntity =>
        (entity, dto) => new WeatherForecast
        {
            Id = entity.Id,
            Date = dto.Date,
            TemperatureC = dto.TemperatureC,
            Summary = entity.Summary,
            Active = entity.Active
        };

    Expression<Func<WeatherForecast, WeatherForecastDetailsDto>> IDtoMapping<WeatherForecast, WeatherForecastDetailsDto, int>.SelectExpression =>
        forecast => new WeatherForecastDetailsDto
        {
            Id = forecast.Id,
            Date = forecast.Date,
            TemperatureC = forecast.TemperatureC,
            Summary = forecast.Summary
        };

    Expression<Func<WeatherForecastDetailsDto, WeatherForecast>> IDtoMapping<WeatherForecast, WeatherForecastDetailsDto, int>.InsertMappingToEntity =>
        dto => new WeatherForecast
        {
            Date = dto.Date,
            TemperatureC = dto.TemperatureC,
            Summary = dto.Summary
        };

    Expression<Func<WeatherForecast, WeatherForecastDetailsDto, WeatherForecast>> IDtoMapping<WeatherForecast, WeatherForecastDetailsDto, int>.UpdateMappingToEntity =>
        (entity, dto) => new WeatherForecast
        {
            Id = entity.Id,
            Date = dto.Date,
            TemperatureC = dto.TemperatureC,
            Summary = dto.Summary,
            Active = entity.Active
        };
}
