using BaseCrud.Abstractions.Mapping;
using BaseCrud.EntityFrameworkCore;

namespace Tester;

public class Service(AppDbContext dbContext, IDtoMappingRegistry mappingRegistry)
    : BaseCrudService<Model, ModelDto, ModelDetailsDto>(dbContext, mappingRegistry), IService;
