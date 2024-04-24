using System.Reflection;
using AutoMapper;
using BaseCrud.General.Entities;

namespace BaseCrud.General.AutoMappers;

public partial class DtoMapperProfile : Profile
{
    public DtoMapperProfile(Assembly assembly)
    {
        RegisterDefaultMappers();

        MapDtosFromAssembly(assembly);
    }

    private void MapDtosFromAssembly(Assembly assembly)
    {
        var values = GetIDataTransferObjectsWithGenericType(assembly).ToArray();

        foreach (var (dto, entityType) in values)
        {
            try
            {
                CreateMap(entityType, dto).ReverseMap();
            }
            catch (Exception e)
            {
                //_logger.LogError(e, "Could not create a map of {dto} and {entityType}", dto, entityType);
            }
        }
    }

    // Generate the method that takes an assembly and returns a list of types that implement IDataTransferObject<>
    public static IEnumerable<Tuple<Type, Type>> GetIDataTransferObjectsWithGenericType(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsInterface)
            {
                continue;
            }

            var interfaces = type.GetInterfaces();
            foreach (var iface in interfaces)
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IDataTransferObject<>))
                {
                    var genericArgument = iface.GetGenericArguments()[0];
                    yield return Tuple.Create(type, genericArgument);
                }
            }
        }
    }



   

    private void RegisterDefaultMappers()
    {
        //CreateMap<string, int>().ConvertUsing<IntTypeConverter>();
        //CreateMap<string, int?>().ConvertUsing<NullIntTypeConverter>();
    }
}