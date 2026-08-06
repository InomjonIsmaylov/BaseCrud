using System.Reflection;
using BaseCrud.Abstractions.Expressions;
using BaseCrud.Abstractions.Services;
using BaseCrud.Extensions;

namespace BaseCrud.Abstractions.Mapping;

public static class DtoMappingDiscovery
{
    public static DtoMappingRegistry Build(
        IEnumerable<Assembly> assemblies,
        IEnumerable<Type> crudServiceTypes)
    {
        var registry = new DtoMappingRegistry();
        var claimed = new Dictionary<(Type Entity, Type Dto), Type>();
        MethodInfo addMethod = typeof(DtoMappingRegistry).GetMethod(nameof(DtoMappingRegistry.Add))!;

        foreach (Assembly assembly in assemblies)
        {
            foreach ((Type implementorType, Type[] genericArgs) in
                     assembly.GetImplementingTypeWithGenericArguments(typeof(IDtoMapping<,,>)))
            {
                if (genericArgs.Length != 3)
                    continue;

                Type entityType = genericArgs[0];
                Type dtoType = genericArgs[1];
                var key = (Entity: entityType, Dto: dtoType);

                if (claimed.TryGetValue(key, out Type? firstImplementor))
                {
                    if (firstImplementor != implementorType)
                    {
                        throw new DuplicateDtoMappingException(
                            entityType,
                            dtoType,
                            firstImplementor,
                            implementorType);
                    }

                    continue;
                }

                object mapping = Activator.CreateInstance(implementorType)!;
                addMethod.MakeGenericMethod(genericArgs).Invoke(registry, [mapping]);
                claimed.Add(key, implementorType);
            }
        }

        foreach (Type serviceType in crudServiceTypes)
        {
            Type[]? genericArgs = GetCrudServiceGenericArgs(serviceType);
            if (genericArgs is null)
                continue;

            Type entityType = genericArgs[0];
            Type dtoType = genericArgs[1];
            Type dtoFullType = genericArgs[2];

            EnsureMappingExists(claimed, entityType, dtoType, serviceType);

            if (dtoFullType != dtoType)
                EnsureMappingExists(claimed, entityType, dtoFullType, serviceType);
        }

        return registry;
    }

    private static Type[]? GetCrudServiceGenericArgs(Type serviceType)
    {
        Type? crud = serviceType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType
                                 && i.GetGenericTypeDefinition() == typeof(ICrudService<,,,,>));
        return crud?.GetGenericArguments();
    }

    private static void EnsureMappingExists(
        IReadOnlyDictionary<(Type Entity, Type Dto), Type> claimed,
        Type entityType,
        Type dtoType,
        Type serviceType)
    {
        if (!claimed.ContainsKey((entityType, dtoType)))
            throw new MissingDtoMappingException(entityType, dtoType, serviceType);
    }
}
