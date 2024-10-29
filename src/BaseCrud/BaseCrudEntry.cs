using BaseCrud.Expressions;
using System.Reflection;
using BaseCrud.Internal;

namespace BaseCrud;

public static class BaseCrudEntry
{
    public static void AddBaseCrudCore(Assembly[] assemblies)
    {

    }

    private static void DiscoverAndSaveFilterExpressions(Assembly[] assemblies)
    {
        foreach (Assembly assembly in assemblies)
        {
            CustomFilterExpressionsHandler.Scan(assembly);
        }
    }
}