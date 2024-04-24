using System.Text.Json;
using BaseCrud.General.Entities;

namespace Tester;

public class ModelDetailsDto : IDataTransferObject<Model>
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string Age { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}