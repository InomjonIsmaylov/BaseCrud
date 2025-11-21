using BaseCrud.Entities;

namespace Tester;

public class ModelPartFirstDto:IDataTransferObject<Model>
{
    public string? Surname { get; set; }
    public string? Patronymic { get; set; }
}
