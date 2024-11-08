namespace BaseCrud.Expressions;

[Flags]
public enum ConstraintApplyableTo
{
    String = 1,
    Number = 2,
    Date = 4,
    Boolean = 8,
    Guid = 16,
    Collection = 32,
    Any = 64
}