using System.Text.Json;
using BaseCrud.Abstractions.Entities;
using BaseCrud.General.AutoMappers;
using BaseCrud.PrimeNg;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tester;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddScoped<AppDbContext>();

builder.Services.AddAutoMapper(c =>
{
    ILogger logger = LoggerFactory
        .Create(bb => bb.SetMinimumLevel(LogLevel.Debug).AddConsole())
        .CreateLogger<DtoMapperProfile>();

    c.AddProfile(new DtoMapperProfile(typeof(Service).Assembly, logger));
});

builder.Services.AddScoped<IService, Service>();

using IHost host = builder.Build();






const string b = """
                 {
                   "first": 0,
                   "rows": 10,
                   "sortField": "id",
                   "sortOrder": 1,
                   "filters": {},
                   "globalFilter": null
                 }
                 """;

var m = JsonSerializer.Deserialize<DataTableMetaData>(b)!;

await PlayGroundWithDiAsync(host.Services, m);

await host.RunAsync();

return;

static async Task PlayGroundWithDiAsync(IServiceProvider hostProvider, IDataTableMetaData metaData)
{
    using IServiceScope serviceScope = hostProvider.CreateScope();
    var service = serviceScope.ServiceProvider.GetRequiredService<IService>();

    var a = new ModelDetailsDto
    {
        Age = "1",
        Address = "address",
        Name = "Boby",
        Surname = "Fischer",
        Patronymic = "ChessPlayer",
        Email = "email",
        Phone = "phone",
    };

    var b = JsonSerializer.Deserialize<ModelDetailsDto>(JsonSerializer.Serialize(a))!;

    var user = new UserProfile();

    try
    {
        ModelDetailsDto entity = await service.InsertAsync(b);

        ModelDetailsDto? entity1 = await service.GetByIdAsync(entity.Id, user);

        Console.WriteLine(entity1);

        QueryResult<ModelDto> all =
            await service.GetAllAsync(metaData, user, context =>
            {
                return ValueTask.FromResult(
                    context.Queryable.Select(x => new Model
                    {
                        Active = x.Active,
                        Age = x.Age,
                        Name = "Some Address"
                    })
                );
            });
        
        Console.WriteLine(all);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}