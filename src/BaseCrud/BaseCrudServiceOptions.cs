using System.Reflection;

namespace BaseCrud;

/// <summary>
/// Options for the BaseCrud service.
/// </summary>
public record BaseCrudServiceOptions
{
    /// <summary>
    ///     Assemblies to scan for custom filter expressions and DTO mappings.
    /// </summary>
    public required Assembly[] Assemblies { get; set; }
}