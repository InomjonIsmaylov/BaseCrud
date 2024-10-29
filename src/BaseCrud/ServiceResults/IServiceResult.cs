using BaseCrud.Errors;

namespace BaseCrud.ServiceResults;

public interface IServiceResult
{
    int StatusCode { get; }

    bool IsSuccess { get; }

    IEnumerable<ServiceError> Errors { get; }
}