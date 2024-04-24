namespace BaseCrud.General.AutoMappers;

public partial class DtoMapperProfile
{
    //public class NullIntTypeConverter : ITypeConverter<string, int?>
    //{
    //    public int? Convert(string? source, int? destination, ResolutionContext context)
    //    {
    //        if (source == null)
    //            return null;

    //        return int.TryParse(source, out int result) ? result : null;
    //    }
    //}

    //// Automapper string to int
    //public class IntTypeConverter : ITypeConverter<string, int>
    //{
    //    public int Convert(string source, int destination, ResolutionContext context)
    //    {
    //        return string.IsNullOrEmpty(source) ? 0 : int.Parse(source);
    //    }
    //}
}
