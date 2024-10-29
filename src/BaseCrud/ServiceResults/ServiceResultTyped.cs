using System.Net;
using BaseCrud.Errors;

namespace BaseCrud.ServiceResults;

public record ServiceResult<T> : IServiceResult
{
    private ServiceResult(int statusCode, T? result, params ServiceError[]? errors)
    {
        StatusCode = statusCode;
        Result = result;
        Errors = errors ?? Errors;
    }

    public T? Result { get; }

    public int StatusCode { get; }

    public bool IsSuccess
        => StatusCode is >= 200 and < 300;

    public IEnumerable<ServiceError> Errors { get; } = [];

    public bool TryGetResult(out T? result)
    {
        result = Result;

        return IsSuccess;
    }

    public static implicit operator ServiceResult<T>(ServiceResult serviceResult)
        => new(serviceResult.StatusCode, default, serviceResult.Errors.ToArray());

    public static implicit operator ServiceResult(ServiceResult<T> serviceResult)
        => new(serviceResult.StatusCode, serviceResult.Errors.ToArray());

    public static implicit operator T?(ServiceResult<T>? serviceResult)
        => serviceResult is null ? default : serviceResult.Result;

    public static implicit operator ServiceResult<T>(T? result)
        => result is null
            ? new ServiceResult<T>((int)HttpStatusCode.NoContent, result: default, errors: null)
            : new ServiceResult<T>((int)HttpStatusCode.OK, result, errors: null);

    internal static class ServiceResultFactory
    {
        public static ServiceResult<T> Create(int statusCode, T? result, params ServiceError[]? errors)
            => new(statusCode, result, errors);
    }
}