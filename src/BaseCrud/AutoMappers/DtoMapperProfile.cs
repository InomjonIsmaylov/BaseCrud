using System.Diagnostics;
using System.Reflection;
using AutoMapper;
using BaseCrud.Entities;
using BaseCrud.Extensions;
using Microsoft.Extensions.Logging;

namespace BaseCrud.AutoMappers;

public class DtoMapperProfile : Profile
{
    private readonly Stopwatch _sw = new();

    public DtoMapperProfile(
        ILogger<DtoMapperProfile> logger,
        BaseCrudServiceOptions options)
    {
        Assembly[] assemblies = options.Assemblies;

        _sw.Start();

        IEnumerable<(Type, Type)> values = assemblies.SelectMany(AutoMapperExtensions.MapDtosFromAssembly);

        short counter = 0;
        short counterSuccess = 0;

        foreach ((Type dto, Type entityType) in values)
        {
            ++counter;
            try
            {
                bool dtoHasCustomMapping = dto.ImplementsInterface(typeof(ICustomMappedDto<,>));

                if (dtoHasCustomMapping)
                {
                    MethodInfo createMapGenericMethodInfo = GetType().BaseType!
                        .GetMethod(nameof(CreateMap), 2, Type.EmptyTypes)!;

                    MethodInfo createMapMethodInfo = createMapGenericMethodInfo.MakeGenericMethod(entityType, dto);

                    object mappingExpression = createMapMethodInfo.Invoke(this, [])!;

                    dto.GetMethod(nameof(ICustomMappedDto<IEntity, IDataTransferObject<IEntity>>.MapEntityToDto))!
                        .Invoke(null, [mappingExpression]);

                    object reverseMappingExpression = mappingExpression.GetType()
                        .GetMethod(nameof(IMappingExpression.ReverseMap))!
                        .Invoke(mappingExpression, [])!;

                    dto.GetMethod(nameof(ICustomMappedDto<IEntity, IDataTransferObject<IEntity>>.MapDtoToEntity))!
                        .Invoke(null, [reverseMappingExpression]);

                    logger.LogDebug("Type {dtoType} has custom dto mapping", dto.Name);
                }
                else
                {
                    CreateMap(entityType, dto).ReverseMap();
                }

                ++counterSuccess;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Could not create a map of {dto} and {entityType}", dto, entityType);
            }
        }

        _sw.Stop();

        logger.LogInformation("DtoMapperProfile: {counterSuccess} of {counter} mappings were created successfully", counterSuccess, counter);

        logger.LogDebug(
            "Time spent on scanning {countAll} and registering {countSuccess} mappings is {elapsedTime} ms",
            counter,
            counterSuccess,
            _sw.ElapsedMilliseconds);

        _sw.Reset();
    }
}
