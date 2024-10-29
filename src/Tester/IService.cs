using BaseCrud.EntityFrameworkCore.Services;

namespace Tester;

public interface IService : IEfCrudService<Model, ModelDto, ModelDetailsDto>;