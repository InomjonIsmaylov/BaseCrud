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

    /// <summary>
    /// Capitalize first letter of provided property names
    /// used in sorting and filtering of models in Data Tables
    /// (default <see langword="true"/>)
    /// </summary>
    public bool CapitalizeFirstLetterOfProvidedPropertyNames { get; set; } = true;
}