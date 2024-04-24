namespace BaseCrud.Abstractions.Entities;

public class QueryResult<T>
{
    public IEnumerable<T> Items { get; set; } = [];

    public int TotalItems { get; set; }
}