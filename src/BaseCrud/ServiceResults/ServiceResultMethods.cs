using System.Data;
using BaseCrud.Errors;

namespace BaseCrud.ServiceResults;

public partial record ServiceResult
{
    /// <summary>
    /// 200 OK
    /// </summary>
    public static ServiceResult<T> Ok<T>(T result)
        => ServiceResult<T>
            .ServiceResultFactory
            .Create(200, result, null);

    /// <summary>
    /// 201 Created
    /// </summary>
    public static ServiceResult<T> Created<T>(T result)
        => ServiceResult<T>
            .ServiceResultFactory
            .Create(201, result, null);

    /// <summary>
    /// 202 Accepted
    /// </summary>
    public static ServiceResult<T> Accepted<T>(T result)
        => ServiceResult<T>
            .ServiceResultFactory
            .Create(202, result, null);

    /// <summary>
    /// 204 No Content
    /// </summary>
    public static ServiceResult NoContent()
        => new(204);

    /// <summary>
    /// 400 Bad Request
    /// </summary>
    public static ServiceResult BadRequest(params ServiceError[] errors)
        => new(400, errors);

    /// <summary>
    /// 401 Unauthorized
    /// </summary>
    public static ServiceResult Unauthorized(params ServiceError[] errors)
        => new(401, errors);

    /// <summary>
    /// 403 Forbidden
    /// </summary>
    public static ServiceResult Forbidden(params ServiceError[] errors)
        => new(403, errors);

    /// <summary>
    /// 404 Not Found
    /// </summary>
    public static ServiceResult NotFound(params ServiceError[] errors)
        => new(404, errors);

    /// <summary>
    /// 409 Conflict
    /// </summary>
    public static ServiceResult Conflict(params ServiceError[] errors)
        => new(409, errors);

    /// <summary>
    /// 422 Unprocessable Entity
    /// </summary>
    public static ServiceResult UnprocessableEntity(params ServiceError[] errors)
        => new(422, errors);

    /// <summary>
    /// 500 Internal Server Error
    /// </summary>
    public static ServiceResult InternalServerError(params ServiceError[] errors)
        => new(500, errors);

    /// <summary>
    /// 501 Not Implemented
    /// </summary>
    public static ServiceResult NotImplemented(params ServiceError[] errors)
        => new(501, errors);

    /// <summary>
    /// 502 Bad Gateway
    /// </summary>
    public static ServiceResult BadGateway(params ServiceError[] errors)
        => new(502, errors);

    /// <summary>
    /// 503 Service Unavailable
    /// </summary>
    public static ServiceResult ServiceUnavailable(params ServiceError[] errors)
        => new(503, errors);

    /// <summary>
    /// 504 Gateway Timeout
    /// </summary>
    public static ServiceResult GatewayTimeout(params ServiceError[] errors)
        => new(504, errors);

    /// <summary>
    /// 511 Network Authentication Required
    /// </summary>
    public static ServiceResult NetworkAuthenticationRequired(params ServiceError[] errors)
        => new(511, errors);

    /// <summary>
    /// 599 Network Connection Timeout Error
    /// </summary>
    public static ServiceResult NetworkConnectTimeoutError(params ServiceError[] errors)
        => new(599, errors);

    /// <summary>
    /// When service result is failed, maps the result to a new service result of a different type,
    /// or throws an exception if given service result is successful.
    /// </summary>
    /// <typeparam name="T">The type of the original service result.</typeparam>
    /// <param name="result">The original service result.</param>
    /// <returns>A <see cref="MapFailedResult{T}"/> instance for further mapping.</returns>
    /// <exception cref="InvalidExpressionException">Thrown if the given service result is successful.</exception>
    public static MapFailedResult<T> FromFailed<T>(ServiceResult<T> result)
        => result.IsSuccess
            ? throw new InvalidExpressionException("Cannot change the type of a service result that is successful")
            : new MapFailedResult<T>(result);
}

public class MapFailedResult<T1>
{
    private readonly ServiceResult<T1> _serviceResult;

    internal MapFailedResult(ServiceResult<T1> serviceResult)
    {
        _serviceResult = serviceResult;
    }

    /// <summary>
    /// Maps a failed service result to a new service result of a different type.
    /// </summary>
    /// <typeparam name="T2">The type of the new service result.</typeparam>
    /// <returns>A new service result of type <typeparamref name="T2"/> with the same status code and errors as the original result.</returns>
    public ServiceResult<T2> ToType<T2>()
    {
        return ServiceResult<T2>.ServiceResultFactory.Create(_serviceResult.StatusCode, default,
            _serviceResult.Errors.ToArray());
    }
}

