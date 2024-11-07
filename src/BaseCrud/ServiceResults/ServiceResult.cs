using BaseCrud.Errors;

namespace BaseCrud.ServiceResults;

public partial record ServiceResult : IServiceResult
{
    protected internal ServiceResult(int statusCode)
    {
        StatusCode = statusCode;
    }

    protected internal ServiceResult(int statusCode, params ServiceError[] errors)
    {
        StatusCode = statusCode;
        Errors = errors;
    }

    public int StatusCode { get; }

    public IEnumerable<ServiceError> Errors { get; } = [];

    public bool IsSuccess
        => StatusCode is >= 200 and < 300;
    
    public T Match<T>(Func<IServiceResult, T> onSuccess, Func<IServiceResult, T> onFail)
    {
        return IsSuccess ? onSuccess(this) : onFail(this);
    }

    public void Match(Action<IServiceResult> onSuccess, Action<IServiceResult> onFail)
    {
        if (IsSuccess)
        {
            onSuccess(this);
        }
        else
        {
            onFail(this);
        }
    }
}