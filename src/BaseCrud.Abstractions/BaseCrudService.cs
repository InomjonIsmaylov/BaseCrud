using System.Reflection;
using BaseCrud.Abstractions.Mapping;
using BaseCrud.Abstractions.Services;
using BaseCrud.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace BaseCrud.Abstractions;

public static class BaseCrudServiceExtensions
{
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
    ///     <strong>* Scans</strong> the provided assemblies for <see cref="IDtoMapping{TEntity,TDto,TKey}"/>
    ///     implementations and registers them in an <see cref="IDtoMappingRegistry"/> singleton.<br />
    ///     Mapping requirements for discovered CRUD services are validated during application startup.
    /// </para>
    /// </remarks>
    /// <param name="services">Service collection for DI</param>
    /// <param name="options">Options for the BaseCrud service</param>
    /// <returns>The same collection</returns>
    public static IServiceCollection AddBaseCrudService(this IServiceCollection services, BaseCrudServiceOptions options)
    {
        _services = services;
        _options = options;

        IReadOnlyCollection<Type> crudServiceImplementationTypes = DiscoverAndRegisterCrudServices();
        IDtoMappingRegistry registry =
            DtoMappingDiscovery.Build(_options.Assemblies, crudServiceImplementationTypes);
        _services.AddSingleton(registry);

        BaseCrudEntry.AddBaseCrudCore(options.Assemblies, _options);

        return services;
    }

    private static IReadOnlyCollection<Type> DiscoverAndRegisterCrudServices()
    {
        Type interfaceType = typeof(ICrudService<,,,,>);
        var implementationTypes = new List<Type>();

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
                implementationTypes.Add(implementationType);
            }
        }

        return implementationTypes;
    }
}