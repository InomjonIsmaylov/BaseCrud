using AutoMapper;
using BaseCrud.EntityFrameworkCore;

namespace Tester;

public class Service(AppDbContext dbContext, IMapper mapper)
    : BaseCrudService<Model, ModelDto, ModelDetailsDto>(dbContext, mapper), IService;