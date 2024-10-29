using System.Text.Json;
using AutoMapper;
using BaseCrud.Entities;

namespace Tester;

public class ModelDetailsDto
    : IDataTransferObject<Model>, ICustomMappedDto<Model, ModelDetailsDto>
{
    public int Id { get; set; }
    
    public string? Name { get; set; }
    
    public string? Surname { get; set; }
    
    public string? Patronymic { get; set; }
    
    public string? Fullname { get; set; }
    
    public string? Age { get; set; }
    
    public string? Address { get; set; }
    
    public string? Email { get; set; }
    
    public string? Phone { get; set; }

    public static IMappingExpression<Model, ModelDetailsDto> MapEntityToDto(IMappingExpression<Model, ModelDetailsDto> mappingExpression)
    {
        return mappingExpression
            .ForMember(dto => dto.Fullname,
                opt => opt.MapFrom(model => $"{model.Surname} {model.Name} {model.Patronymic}"));
    }

    public static IMappingExpression<ModelDetailsDto, Model> MapDtoToEntity(IMappingExpression<ModelDetailsDto, Model> mappingExpression)
    {
        return mappingExpression;
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}