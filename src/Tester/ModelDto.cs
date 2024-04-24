using System.Text.Json;
using BaseCrud.General.Entities;

namespace Tester;

public class ModelDto : IDataTransferObject<Model>
{
    public int Id { get; set; }

    public string? Name { get; set; }
    
    public int Age { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}