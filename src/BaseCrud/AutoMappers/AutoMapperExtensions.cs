using System.Reflection;
using BaseCrud.Entities;
using BaseCrud.Extensions;

namespace BaseCrud.AutoMappers;

internal static class AutoMapperExtensions
{
    internal static IEnumerable<(Type, Type)> MapDtosFromAssembly(Assembly assembly)
        => assembly
            .GetImplementingTypeWithGenericArguments(typeof(IDataTransferObject<,>))
            .Select(tuple => (tuple.Item1, tuple.Item2[0]));
}