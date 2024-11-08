using System.Reflection;

namespace BaseCrud.Extensions;

public static class ReflectionExtensions
{
    /// <summary>
    /// Get the first type that implements the interface
    /// </summary>
    public static Type? GetTypeAssignableFromInterface(this Assembly assembly, Type interfaceType)
        => assembly.GetTypes()
            .FirstOrDefault(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface);

    /// <summary>
    /// Get all types that implement the interface
    /// </summary>
    public static IEnumerable<Type> GetTypesAssignableFromInterface(this Assembly assembly, Type interfaceType)
        => assembly.GetTypes()
            .Where(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface);

    /// <summary>
    /// Check if a class implements a generic interface
    /// </summary>
    public static bool ImplementsInterface(this Type classType, Type interfaceType)
        => classType.GetInterfaces().Any(x =>
            x.IsGenericType && x.GetGenericTypeDefinition() == interfaceType);

    /// <summary>
    /// Generate the method that takes an assembly and returns a list of types that
    /// implement <paramref name="interfaceType"/>
    /// </summary>
    /// <param name="assembly">Assembly to scan</param>
    /// <param name="interfaceType">Interface of generic type</param>
    /// <returns>
    /// A list of tuples with the first item being the type that implements the interface and
    /// the second item being the generic argument of the interface
    /// </returns>
    public static IEnumerable<(Type, Type[])> GetImplementingTypeWithGenericArguments(this Assembly assembly,
        Type interfaceType)
        => from implementingType in assembly.GetTypes()
            where !implementingType.IsInterface
            let interfaces = implementingType.GetInterfaces()
            from iType in interfaces
            where iType.IsGenericType && iType.GetGenericTypeDefinition() == interfaceType
            let genericArguments = iType.GetGenericArguments()
            select (implementingType, genericArguments);
}