using BaseCrud.Expressions;

namespace BaseCrud.Internal;

internal record PredicateExpressionWith2Params(
    ExpressionConstraintsEnum WhenEnum,
    Expression Body,
    ParameterExpression EntityParam,
    ParameterExpression FilterParam);

internal record RulePredicateExpression(
    Expression Body,
    ParameterExpression EntityParam);

internal record RulePredicateExpressionWithFilterParam(
    Expression Body,
    ParameterExpression EntityParam,
    ParameterExpression FilterParam);