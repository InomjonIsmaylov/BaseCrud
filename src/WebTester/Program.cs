using System.Reflection;
using BaseCrud;
using BaseCrud.Abstractions;
using BaseCrud.PrimeNg;
using WebTester.DataBase;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.Converters.Add(new FilterMetadataConverter());
    o.JsonSerializerOptions.Converters.Add(new PrimeTableMetaConverter());
});

builder.Services.AddOpenApiDocument();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    if (!builder.Environment.IsDevelopment())
        return;

    string xmlPath = Path.Combine(AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");

    Console.WriteLine("Include xml");

    Console.WriteLine(xmlPath);

    Console.WriteLine(builder.Environment.EnvironmentName);

    options.IncludeXmlComments(xmlPath);
});
builder.Services.AddDbContext<AppDbContext>();
builder.Logging.SetMinimumLevel(LogLevel.Debug).AddConsole();

builder.Services.AddBaseCrudService(new BaseCrudServiceOptions
{
    Assemblies = [Assembly.GetExecutingAssembly()]
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

Console.WriteLine(builder.Environment.EnvironmentName);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
