using BaseCrud.EntityFrameworkCore.Services;
using WebTester.Models;

namespace WebTester.Services;

public interface IWeatherService : IEfCrudService<WeatherForecast, WeatherForecastDto, WeatherForecastDetailsDto>;