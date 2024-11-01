using System.Reflection;
using BaseCrud.Abstractions.Services;
using BaseCrud.AutoMappers;
using BaseCrud.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BaseCrud.Abstractions;

public static class BaseCrudServiceExtensions
{
    private static ServiceProvider _sp = null!;
    private static IServiceCollection _services = null!;
    private static BaseCrudServiceOptions _options = null!;

    /// <summary>
    /// Adds the BaseCrud service to the service collection.
    /// </summary>
    /// <remarks>
    /// <para>
    ///     <strong>* Scans</strong> the provided assemblies for services that implement <see cref="ICrudService{TEntity,TDto,TDtoFull,TKey,TUserKey}"/>
    ///     or its derived types and registers those services in Dependency Injection Container.<br />
    /// </para>
    /// <para>
    ///     <strong>* Scans</strong> the provided assemblies for Models and Dtos (<see cref="IDataTransferObject{TEntity, TKey}"/>).<br />
    ///     1. Find all classes that implement the <see cref="IDataTransferObject{TEntity, TKey}"/> interface
    ///        and map them to their corresponding entity classes.<br />
    ///     2. If a class implements the <see cref="ICustomMappedDto{TEntity, TDto}"/> interface, then
    ///        it will be mapped using the provided custom mapping methods.
    /// </para>
    /// </remarks>
    /// <param name="services">Service collection for DI</param>
    /// <param name="options">Options for the BaseCrud service</param>
    /// <returns>The same collection</returns>
    public static IServiceCollection AddBaseCrudService(this IServiceCollection services, BaseCrudServiceOptions options)
    {
        _services = services;
        _options = options;
        _sp = _services.BuildServiceProvider();

        AddAutoMapper();

        DiscoverAndRegisterCrudServices();

        BaseCrudEntry.AddBaseCrudCore(options.Assemblies, _options);

        return services;
    }

    private static void AddAutoMapper()
    {
        // Add AutoMapper
        // Note: Adding AutoMapper will not break the application even if
        // the AddAutoMapper is called multiple times. It will only add the
        // profiles once. There is a check in the AddAutoMapper method to
        // prevent adding the same profiles multiple times.
        _services.AddAutoMapper(exp =>
        {
            var logger = _sp.GetRequiredService<ILogger<DtoMapperProfile>>();

            exp.AddProfile(new DtoMapperProfile(logger, _options));
        });
    }

    private static void DiscoverAndRegisterCrudServices()
    {
        Type interfaceType = typeof(ICrudService<,,,,>);

        foreach (Assembly assembly in _options.Assemblies)
        {
            IEnumerable<(Type, Type)> types = assembly
                .GetImplementingTypeWithGenericArguments(interfaceType)
                .Select<(Type, Type[]), (Type, Type)>(selector: para =>
                {
                    (Type serviceType, Type[] _) = para;

                    Type[] interfaces = serviceType.GetInterfaces();

                    Type ownInterfaceType = interfaces
                        .First(x => !x.IsGenericType && x.GetInterfaces().Any(i => i.Name.Contains("ICrudService")));

                    return (serviceType, ownInterfaceType);
                });

            foreach ((Type implementationType, Type iServiceType) in types)
            {
                _services.AddScoped(iServiceType, implementationType);
            }
        }
    }
}