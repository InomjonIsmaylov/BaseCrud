namespace BaseCrud.Abstractions.Mapping;

public sealed class MissingDtoMappingException : InvalidOperationException
{
    public MissingDtoMappingException(Type entityType, Type dtoType, Type? serviceType)
        : base(
            serviceType is null
                ? $"No IDtoMapping found for {entityType.Name} → {dtoType.Name}."
                : $"No IDtoMapping found for {entityType.Name} → {dtoType.Name} required by service {serviceType.Name}.")
    {
        EntityType = entityType;
        DtoType = dtoType;
        ServiceType = serviceType;
    }

    public Type EntityType { get; }
    public Type DtoType { get; }
    public Type? ServiceType { get; }
}
