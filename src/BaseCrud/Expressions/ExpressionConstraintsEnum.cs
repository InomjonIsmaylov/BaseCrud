using Ardalis.SmartEnum;

namespace BaseCrud.Expressions;

public sealed class ExpressionConstraintsEnum
    : SmartEnum<ExpressionConstraintsEnum, int>
{
    private ExpressionConstraintsEnum(string name, ConstraintApplyableTo value) : base(name, (int)value)
    {

    }

    public static readonly ExpressionConstraintsEnum StartsWith = new(nameof(StartsWith), ConstraintApplyableTo.String);

    public static readonly ExpressionConstraintsEnum EndsWith = new(nameof(EndsWith), ConstraintApplyableTo.String);

    public static readonly ExpressionConstraintsEnum Contains = new(nameof(Contains),
        ConstraintApplyableTo.String | ConstraintApplyableTo.Collection);

    public new static readonly ExpressionConstraintsEnum Equals = new(nameof(Equals), ConstraintApplyableTo.Any);

    public static readonly ExpressionConstraintsEnum NotEquals = new(nameof(NotEquals), ConstraintApplyableTo.Any);

    public static readonly ExpressionConstraintsEnum GreaterThan = new(nameof(GreaterThan), ConstraintApplyableTo.Any);

    public static readonly ExpressionConstraintsEnum GreaterThanOrEqual =
        new(nameof(GreaterThanOrEqual), ConstraintApplyableTo.Any);

    public static readonly ExpressionConstraintsEnum LessThan = new(nameof(LessThan), ConstraintApplyableTo.Any);

    public static readonly ExpressionConstraintsEnum LessThanOrEqual =
        new(nameof(LessThanOrEqual), ConstraintApplyableTo.Any);

    public static readonly ExpressionConstraintsEnum In = new(nameof(In), ConstraintApplyableTo.Collection);

    public static readonly ExpressionConstraintsEnum NotIn = new(nameof(NotIn), ConstraintApplyableTo.Collection);

    public static readonly ExpressionConstraintsEnum IsNull = new(nameof(IsNull), ConstraintApplyableTo.Any);

    public static readonly ExpressionConstraintsEnum IsNotNull = new(nameof(IsNotNull), ConstraintApplyableTo.Any);

    public static readonly ExpressionConstraintsEnum IsEmpty = new(nameof(IsEmpty),
        ConstraintApplyableTo.String | ConstraintApplyableTo.Collection);

    public static readonly ExpressionConstraintsEnum IsNotEmpty = new(nameof(IsNotEmpty),
        ConstraintApplyableTo.String | ConstraintApplyableTo.Collection);

    public static readonly ExpressionConstraintsEnum After = new(nameof(After), ConstraintApplyableTo.Date);

    public static readonly ExpressionConstraintsEnum Before = new(nameof(Before), ConstraintApplyableTo.Date);
}