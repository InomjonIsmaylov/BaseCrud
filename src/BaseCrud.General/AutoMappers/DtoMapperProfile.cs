using System.Reflection;
using AutoMapper;
using AutoMapper.Internal;
using BaseCrud.General.Entities;
using Microsoft.Extensions.Logging;

namespace BaseCrud.General.AutoMappers;

public class DtoMapperProfile : Profile
{
    private readonly ILogger _logger;

    public DtoMapperProfile(Assembly assembly, ILogger logger)
    {
        _logger = logger;
        MapDtosFromAssembly(assembly);
    }

    private void MapDtosFromAssembly(Assembly assembly)
    {
        Tuple<Type, Type>[] values = GetIDataTransferObjectsWithGenericType(assembly).ToArray();

        foreach ((Type dto, Type entityType) in values)
        {
            try
            {
                bool dtoHasCustomMapping = dto.GetInterfaces().Any(x =>
                    x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICustomMappedDto<,>));

                if (dtoHasCustomMapping)
                {
                    MethodInfo createMapGenericMethodInfo = GetType().BaseType!.GetMethod("CreateMap", 2, [])!;
                    
                    MethodInfo createMapMethodInfo = createMapGenericMethodInfo.MakeGenericMethod(entityType, dto);

                    object mappingExpression = createMapMethodInfo.Invoke(this, [])!;
                    
                    dto.GetStaticMethod("MapEntityToDto").Invoke(null, [mappingExpression]);

                    object reverseMappingExpression = mappingExpression.GetType().GetMethod("ReverseMap")!.Invoke(mappingExpression, [])!;

                    dto.GetStaticMethod("MapDtoToEntity").Invoke(null, [reverseMappingExpression]);
                }
                else
                {
                    CreateMap(entityType, dto).ReverseMap();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not create a map of {dto} and {entityType}", dto, entityType);
            }
        }
    }

    // Generate the method that takes an assembly and returns a list of types that implement IDataTransferObject<>
    private static IEnumerable<Tuple<Type, Type>> GetIDataTransferObjectsWithGenericType(Assembly assembly)
    {
        return from type in assembly.GetTypes()
               where !type.IsInterface
               let interfaces = type.GetInterfaces()
               from interfaceType in interfaces
               where interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IDataTransferObject<>)
               let genericArgument = interfaceType.GetGenericArguments()[0]
               select Tuple.Create(type, genericArgument);
    }
}