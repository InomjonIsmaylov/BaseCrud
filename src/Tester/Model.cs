using System.Linq.Expressions;
using BaseCrud.Abstractions.Entities;
using BaseCrud.Abstractions.Expressions;

namespace Tester;

public sealed class ModelExpressions : IGlobalFilterExpression<Model>, ISelectExpression<Model, ModelDto>
{
    public Expression<Func<Model, bool>> GlobalSearchExpression(string strSearch)
    {
        return x => x.Name!.Contains(strSearch) || x.Address!.Contains(strSearch) || x.Email!.Contains(strSearch) || x.Phone!.Contains(strSearch);
    }

    public Expression<Func<Model, ModelDto>> SelectExpression
    {
        get
        {
            return x => new ModelDto
            {
                Id = x.Id,
                Name = x.Name
            };
        }
    }
}

public class Model : EntityBase
{
    public string? Name { get; set; }
    public int Age { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    public Expression<Func<Model, bool>> GlobalSearchExpression(string strSearch)
    {
        return x => x.Name!.Contains(strSearch) || x.Address!.Contains(strSearch) || x.Email!.Contains(strSearch) || x.Phone!.Contains(strSearch);
    }

    public Expression<Func<Model, ModelDto>> SelectExpression
    {
        get
        {
            return x => new ModelDto
            {
                Id = x.Id,
                Name = x.Name
            };
        }
    }
}