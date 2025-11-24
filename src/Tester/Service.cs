using AutoMapper;
using BaseCrud.Abstractions.Entities;
using BaseCrud.EntityFrameworkCore;
using BaseCrud.ServiceResults;

namespace Tester;

public class Service(AppDbContext dbContext, IMapper mapper)
    : BaseCrudService<Model, ModelDto, ModelDetailsDto>(dbContext, mapper), IService
{
    public override Task<ServiceResult<ModelDetailsDto>> Update(ModelDetailsDto entityDto, Model entity, IUserProfile<int>? userProfile,
        CancellationToken cancellationToken = default)
    {
        //before update logic here

        var res = base.Update(entityDto, entity, userProfile, cancellationToken);

        //after update logic here

        return res;

    }
}