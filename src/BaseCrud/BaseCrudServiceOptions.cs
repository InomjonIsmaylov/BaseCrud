using System.Reflection;

namespace BaseCrud;

/// <summary>
/// Options for the BaseCrud service.
/// </summary>
/// <param name="Assemblies">
///     Assemblies to scan for custom filter expressions and DTO mappings.
/// </param>
public record BaseCrudServiceOptions(Assembly[] Assemblies);