using BaseCrud.Errors;

namespace BaseCrud.EntityFrameworkCore;

public partial class BaseCrudService<TEntity, TDto, TDtoFull, TKey, TUserKey>
{
    /// <summary>
    /// 200 OK
    /// </summary>
    protected static ServiceResult<T> Ok<T>(T result)
        => ServiceResult.Ok(result);

    /// <summary>
    /// 201 Created
    /// </summary>
    protected static ServiceResult<T> Created<T>(T result)
        => ServiceResult.Created(result);

    /// <summary>
    /// 202 Accepted
    /// </summary>
    protected static ServiceResult<T> Accepted<T>(T result)
        => ServiceResult.Accepted(result);

    /// <summary>
    /// 204 No Content
    /// </summary>
    protected static ServiceResult NoContent()
        => ServiceResult.NoContent();

    /// <summary>
    /// 400 Bad Request
    /// </summary>
    protected static ServiceResult BadRequest(params ServiceError[] errors)
        => ServiceResult.BadRequest(errors);

    /// <summary>
    /// 401 Unauthorized
    /// </summary>
    protected static ServiceResult Unauthorized(params ServiceError[] errors)
        => ServiceResult.Unauthorized(errors);

    /// <summary>
    /// 403 Forbidden
    /// </summary>
    protected static ServiceResult Forbidden(params ServiceError[] errors)
        => ServiceResult.Forbidden(errors);

    /// <summary>
    /// 404 Not Found
    /// </summary>
    protected static ServiceResult NotFound(params ServiceError[] errors)
        => ServiceResult.NotFound(errors);

    /// <summary>
    /// 409 Conflict
    /// </summary>
    protected static ServiceResult Conflict(params ServiceError[] errors)
        => ServiceResult.Conflict(errors);

    /// <summary>
    /// 422 Unprocessable Entity
    /// </summary>
    protected static ServiceResult UnprocessableEntity(params ServiceError[] errors)
        => ServiceResult.UnprocessableEntity(errors);

    /// <summary>
    /// 500 Internal Server Error
    /// </summary>
    protected static ServiceResult InternalServerError(params ServiceError[] errors)
        => ServiceResult.InternalServerError(errors);

    /// <summary>
    /// 501 Not Implemented
    /// </summary>
    protected static ServiceResult NotImplemented(params ServiceError[] errors)
        => ServiceResult.NotImplemented(errors);

    /// <summary>
    /// 502 Bad Gateway
    /// </summary>
    protected static ServiceResult BadGateway(params ServiceError[] errors)
        => ServiceResult.BadGateway(errors);

    /// <summary>
    /// 503 Service Unavailable
    /// </summary>
    protected static ServiceResult ServiceUnavailable(params ServiceError[] errors)
        => ServiceResult.ServiceUnavailable(errors);

    /// <summary>
    /// 504 Gateway Timeout
    /// </summary>
    protected static ServiceResult GatewayTimeout(params ServiceError[] errors)
        => ServiceResult.GatewayTimeout(errors);

    /// <summary>
    /// 511 Network Authentication Required
    /// </summary>
    protected static ServiceResult NetworkAuthenticationRequired(params ServiceError[] errors)
        => ServiceResult.NetworkAuthenticationRequired(errors);

    /// <summary>
    /// 599 Network Connection Timeout Error
    /// </summary>
    protected static ServiceResult NetworkConnectionTimeoutError(params ServiceError[] errors)
        => ServiceResult.NetworkConnectTimeoutError(errors);
}
