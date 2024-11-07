using System.Reflection;
using System.Text.Json;
using BaseCrud;
using BaseCrud.Abstractions;
using BaseCrud.Abstractions.Entities;
using BaseCrud.Entities;
using BaseCrud.PrimeNg;
using BaseCrud.ServiceResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tester;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);



builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddScoped<AppDbContext>();

builder.Logging.SetMinimumLevel(LogLevel.Debug).AddConsole();

builder.Services.AddBaseCrudService(
    new BaseCrudServiceOptions
    {
        Assemblies = [Assembly.GetExecutingAssembly()]
    }
);

// Now automatically added as scoped inside BaseCrudService
//builder.Services.AddScoped<IService, Service>();

using IHost host = builder.Build();

const string metaJson = """
                        {
                          "first": 0,
                          "rows": 10,
                          "sortField": "id",
                          "sortOrder": 1,
                          "filters": {
                            "address": { "matchMode": "rule" },
                            "is_adult": { "matchMode": "rule" }
                          },
                          "globalFilter": null
                        }
                        """;



var m = JsonSerializer.Deserialize<PrimeTableMetaData>(metaJson, new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    Converters = { new ObjectToInferredTypesConverter() }
})!;

await PlayGroundWithDiAsync(host.Services, m);

await host.RunAsync();

return;

static async Task PlayGroundWithDiAsync(IServiceProvider hostProvider, IDataTableMetaData metaData)
{
    using IServiceScope serviceScope = hostProvider.CreateScope();

    var service = serviceScope.ServiceProvider.GetRequiredService<IService>();

    var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<IService>>();

    var a = new ModelDetailsDto
    {
        Age = "14",
        Address = "address",
        Name = "Boby",
        Surname = "Fischer",
        Patronymic = "ChessPlayer",
        Email = "email",
        Phone = "phone",
    };

    var a2 = new ModelDetailsDto
    {
        Age = "19",
        Address = "address",
        Name = "Magnus",
        Surname = "Carlsen",
        Patronymic = "ChessPlayer",
        Email = "email",
        Phone = "phone",
    };

    var b = JsonSerializer.Deserialize<ModelDetailsDto>(JsonSerializer.Serialize(a))!;

    var user = new UserProfile();

    try
    {
        b = await ControllerInsertAsync(service, b, user, logger);

        await ControllerInsertAsync(service, a2, user, logger);

        await ControllerGetAllAsync(service, metaData, user);

        await ControllerGetByIdAsync(service, b, user, logger);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}

static async Task<ModelDetailsDto> ControllerInsertAsync(IService service, ModelDetailsDto modelDetailsDto, UserProfile user, ILogger<IService> logger1)
{
    ServiceResult<ModelDetailsDto> insertResult = await service.InsertAsync(modelDetailsDto, user);

    if (!insertResult.TryGetResult(out ModelDetailsDto? entity))
        return modelDetailsDto;

    logger1.LogInformation("entity is {entityString}", entity?.ToString());

    return entity!;

}

static async Task ControllerGetByIdAsync(IService service, ModelDetailsDto modelDetailsDto, UserProfile user, ILogger<IService> logger1)
{
    ServiceResult<ModelDetailsDto?> entity1Res = await service
        .GetByIdAsync(modelDetailsDto.Id, user);

    if (entity1Res.TryGetResult(out ModelDetailsDto? entity1))
    {
        logger1.LogInformation("entity is {entityString}", entity1?.ToString());
    }
    else
    {
        Console.WriteLine(entity1Res.Errors);
    }
}

static async Task ControllerGetAllAsync(IService service, IDataTableMetaData metaData, UserProfile user)
{
    QueryResult<ModelDto>? allResult = await service
        .GetAllAsync(metaData, user,
            context => ValueTask.FromResult(
                context.Queryable.Select(x => new Model
                {
                    Active = x.Active,
                    Age = x.Age,
                })
            ));

    Console.WriteLine(allResult);
}