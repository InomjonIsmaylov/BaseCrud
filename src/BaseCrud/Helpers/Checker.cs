using System.Diagnostics.CodeAnalysis;

namespace BaseCrud.Helpers;

/// <summary>
/// Utility class for checking the state of the params and objects
/// </summary>
public static class Checker
{
    /// <summary>
    /// Checks whether the given argument is null
    /// </summary>
    /// <param name="o">Object to check to null</param>
    /// <param name="name">Name of the argument</param>
    /// <param name="message">Message to pass to exception.
    ///     Default: Значение аргумента "<paramref name="name"/>" не может быть NULL (Пустым) </param>
    /// <exception cref="ArgumentNullException">Throws an Exception when object is null</exception>
    public static void ArgumentNotNull([NotNull] object? o, string name, string? message = null)
    {
        if (o == null)
            throw new ArgumentNullException(name, message ?? $"Value of argument \"{name}\" can not be NULL (Empty)");
    }

    /// <summary>
    /// Checks whether the given int argument is 0
    /// </summary>
    /// <param name="arg">Argument to check to 0</param>
    /// <param name="name">Name of the argument</param>
    /// <param name="message">Message to pass to exception.
    ///     Default: Значение аргумента "<paramref name="name"/>" не может быть 0 </param>
    /// <exception cref="ArgumentException">Throws an Exception when object is 01</exception>
    public static void ArgumentNotZero(int arg, string name, string? message = null)
    {
        if (arg == 0)
            throw new ArgumentException(message ?? $"Value of argument \"{name}\" can not be 0", name);
    }

    /// <summary>
    /// Checks whether the given string argument is null or empty or whitespace
    /// </summary>
    /// <param name="arg">String argument to check</param>
    /// <param name="name">Name of the argument</param>
    /// <param name="message">Message to pass to exception.
    ///     Default: Значение строкого аргумента "<paramref name="name"/>" не может быть NULL или пустым </param>
    /// <exception cref="ArgumentException" />
    public static void ArgumentStringNotEmpty([NotNull] string? arg, string name, string? message = null)
    {
        switch (arg)
        {
            case "":
                throw new ArgumentException(name,
                    message ?? $"The value of string argument \"{name}\" can not be EMPTY");
            case null:
                throw new ArgumentException(name,
                    message ?? $"The value of string argument \"{name}\" can not be NULL");
        }

        if (string.IsNullOrWhiteSpace(arg))
            throw new ArgumentException(name,
                message ?? $"The value of string argument \"{name}\" can not be only WhiteSpace");
    }

    /// <summary>
    /// Checks whether the given <paramref name="guid"/> argument is <see langword="null"/> or <see cref="Guid.Empty"/>
    /// </summary>
    /// <param name="guid">Guid argument to check</param>
    /// <param name="name">Name of the argument</param>
    /// <param name="message">Message to pass to exception.</param>
    /// <exception cref="ArgumentException" />
    public static void ArgumentGuidIsValidAndNotEmpty([NotNull] Guid? guid, string name, string? message = null)
    {
        if (guid is null)
            throw new ArgumentException(name, message ?? $"The value of GUID argument \"{name}\" can not be NULL");

        if (guid == Guid.Empty)
            throw new ArgumentException(name, message ?? $"The value of GUID argument \"{name}\" can not be EMPTY");

    }

    /// <summary>
    /// Checks whether the given <paramref name="coll"/> argument is <see langword="null"/>
    /// or Empty
    /// </summary>
    /// <param name="coll"><see cref="IEnumerable{T}"/> argument to check</param>
    /// <param name="name">Name of the argument</param>
    /// <param name="message">Message to pass to exception.
    ///     Default: Значение строкого аргумента "<paramref name="name"/>" не может быть NULL или пустым </param>
    /// <exception cref="ArgumentNullException">Throws an Exception when is null, empty or whitespace</exception>
    /// <exception cref="ArgumentException">Throws an Exception when is null, empty or whitespace</exception>
    public static void ArgumentCollectionNotEmpty([NotNull] IEnumerable<object>? coll, string name,
        string? message = null)
    {
        ArgumentNotNull(coll, name, message);

        if (!coll.Any())
            throw new ArgumentException(name, message ?? $"Аргумента типа коллекция \"{name}\" не может быть пустым");
    }

    ///// <summary>
    ///// Checks whether <paramref name="tableMetaData"/> is valid
    ///// </summary>
    ///// <param name="tableMetaData"><see cref="TableMetaData"/> value</param>
    ///// <param name="name">Name of param</param>
    ///// <exception cref="ArgumentNullException"></exception>
    ///// <exception cref="TableMetaDataInvalidException"></exception>
    //public static void ArgumentTableMetaDataIsValid([NotNull] ITableMetaData? tableMetaData, string name)
    //{
    //    ArgumentNotNull(tableMetaData, name);

    //    if (string.IsNullOrEmpty(tableMetaData.Filters))
    //        throw new TableMetaDataInvalidException(
    //            $"{nameof(TableMetaData.Filters)} property is null"
    //        );

    //    if (tableMetaData.First < 0)
    //        throw new TableMetaDataInvalidException(
    //            $"Value of {nameof(TableMetaData.First)} is less than 0 (value: {tableMetaData.First})"
    //        );

    //    if (tableMetaData.Rows < 1)
    //        throw new TableMetaDataInvalidException(
    //            $"Value of {nameof(TableMetaData.Rows)} is less than 1 (value: {tableMetaData.Rows})"
    //        );
    //}

}