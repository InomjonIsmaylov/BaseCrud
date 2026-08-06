namespace BaseCrud.Abstractions.Mapping;

public sealed class DuplicateDtoMappingException : InvalidOperationException
{
    public DuplicateDtoMappingException(Type entityType, Type dtoType, Type first, Type second)
        : base(
            $"Duplicate IDtoMapping for {entityType.Name} → {dtoType.Name}: '{first.FullName}' and '{second.FullName}'.")
    {
    }
}
