using BaseCrud.Expressions;

namespace BaseCrud.PrimeNg;

public class PrimeNgExtensions
{
    public static ExpressionConstraintsEnum ConvertToConstraintsEnum(string constraint) =>
        constraint switch
        {
            "startsWith" => ExpressionConstraintsEnum.StartsWith,
            "contains" => ExpressionConstraintsEnum.Contains,
            "endsWith" => ExpressionConstraintsEnum.EndsWith,
            "equals" => ExpressionConstraintsEnum.Equals,
            "notEquals" => ExpressionConstraintsEnum.NotEquals,
            "in" => ExpressionConstraintsEnum.In,
            "lessThan" => ExpressionConstraintsEnum.LessThan,
            "lessThanOrEqual" => ExpressionConstraintsEnum.LessThanOrEqual,
            "greaterThan" => ExpressionConstraintsEnum.GreaterThan,
            "greaterThanOrEqual" => ExpressionConstraintsEnum.GreaterThanOrEqual,
            "is" => ExpressionConstraintsEnum.Equals,
            "isNot" => ExpressionConstraintsEnum.NotEquals,
            "after" => ExpressionConstraintsEnum.After,
            "before" => ExpressionConstraintsEnum.Before,
            _ => ExpressionConstraintsEnum.Equals
        };
}