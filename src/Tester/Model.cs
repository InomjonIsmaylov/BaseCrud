using System.Linq.Expressions;
using BaseCrud.Abstractions.Entities;
using BaseCrud.Abstractions.Expressions;
using BaseCrud.Expressions;
using BaseCrud.Expressions.Filter;

namespace Tester;

public sealed class ModelExpressions :
    IDtoMapping<Model, ModelDto>,
    IDtoMapping<Model, ModelDetailsDto>,
    IGlobalFilterExpression<Model>,
    IFilterExpression<Model>
{
    public Expression<Func<Model, bool>> GlobalSearchExpression(string strSearch)
    {
        return x => x.Name!.Contains(strSearch) || x.Address!.Contains(strSearch) || x.Email!.Contains(strSearch) || x.Phone!.Contains(strSearch);
    }

    Expression<Func<Model, ModelDto>> IDtoMapping<Model, ModelDto, int>.SelectExpression =>
        model => new ModelDto { Id = model.Id, Name = model.Name, Age = model.Age };

    Expression<Func<ModelDto, Model>> IDtoMapping<Model, ModelDto, int>.InsertMappingToEntity =>
        dto => new Model { Name = dto.Name, Age = dto.Age };

    Expression<Func<Model, ModelDto, Model>> IDtoMapping<Model, ModelDto, int>.UpdateMappingToEntity =>
        (entity, dto) => new Model
        {
            Id = entity.Id,
            Name = dto.Name,
            Age = dto.Age,
            Surname = entity.Surname,
            Patronymic = entity.Patronymic,
            Address = entity.Address,
            Email = entity.Email,
            Phone = entity.Phone,
            Active = entity.Active
        };

    Expression<Func<Model, ModelDetailsDto>> IDtoMapping<Model, ModelDetailsDto, int>.SelectExpression =>
        model => new ModelDetailsDto
        {
            Id = model.Id,
            Name = model.Name,
            Surname = model.Surname,
            Patronymic = model.Patronymic,
            Fullname = ((model.Surname ?? "") + " " + (model.Name ?? "") + " " + (model.Patronymic ?? "")).Trim(),
            Age = model.Age.ToString(),
            Address = model.Address,
            Email = model.Email,
            Phone = model.Phone
        };

    Expression<Func<ModelDetailsDto, Model>> IDtoMapping<Model, ModelDetailsDto, int>.InsertMappingToEntity =>
        dto => new Model
        {
            Name = dto.Name,
            Surname = dto.Surname,
            Patronymic = dto.Patronymic,
            Address = dto.Address,
            Email = dto.Email,
            Phone = dto.Phone
        };

    Expression<Func<Model, ModelDetailsDto, Model>> IDtoMapping<Model, ModelDetailsDto, int>.UpdateMappingToEntity =>
        (entity, dto) => new Model
        {
            Id = entity.Id,
            Name = dto.Name,
            Surname = dto.Surname,
            Patronymic = dto.Patronymic,
            Address = dto.Address,
            Email = dto.Email,
            Phone = dto.Phone,
            Age = entity.Age,
            Active = entity.Active
        };

    public Func<FilterExpressions<Model>, FilterExpressions<Model>> FilterExpressions
        => modelExpressions => modelExpressions
            .ForProperty(model => model.Age,
                propFilterBuilder: ageFilterBuilder => ageFilterBuilder
                    .HasFilter((model, value) => model.Age > value, when: ExpressionConstraintsEnum.GreaterThan)
                    .HasFilter((model, value) => model.Age == value, when: ExpressionConstraintsEnum.Equals)
            )
            .ForProperty(model => model.Email,
                propFilterBuilder: emailFilterBuilder => emailFilterBuilder
                    .HasFilter((model, value) => value != null && model.Email!.Contains(value),
                        when: ExpressionConstraintsEnum.Contains)
                    .HasFilter((model, value) => model.Email == value, when: ExpressionConstraintsEnum.Equals)
            )
            .ForProperty(x => x.Address,
                propFilterBuilder: addressExpression => addressExpression
                    .HasFilter((m, value) => m.Address == value, ExpressionConstraintsEnum.Equals)
            )
            .AddRule("is_adult", model => model.Age >= 18)
            .AddRule("is_older_than", (Model model, int value) => model.Age > value)
            .AddRule<DateTime>("born_after", (model, date) => model.Age > DateTime.Now.Year - date.Year);
}

public class Model : EntityBase
{
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Patronymic { get; set; }

    public int Age { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
