using System.Reflection;
using BaseCrud.Internal;

namespace BaseCrud;

public static class BaseCrudEntry
{
    public static BaseCrudServiceOptions Options { get; private set; } = null!;

    public static void AddBaseCrudCore(Assembly[] assemblies, BaseCrudServiceOptions options)
    {
        Options = options;

        DiscoverAndSaveFilterExpressions(assemblies);
    }

    private static void DiscoverAndSaveFilterExpressions(Assembly[] assemblies)
    {
        foreach (Assembly assembly in assemblies)
        {
            CustomFilterExpressionsHandler.Scan(assembly);
        }
    }
}