using BaseCrud.Errors;

namespace BaseCrud.ServiceResults;

public static class ServiceResultExtensions
{
    public static async Task<ServiceResult> MatchAsync(this Task<ServiceResult> serviceCall, Func<Task> onSuccess, Func<Task> onFail)
    {
        ServiceResult result = await serviceCall;

        if (result.IsSuccess)
        {
            await onSuccess();
        }
        else
        {
            await onFail();
        }

        return result;
    }

    public static async Task<T?> MatchAsync<T>(
        this Task<ServiceResult<T>> serviceCall,
        Func<T?, T?> onSuccess,
        Func<IEnumerable<ServiceError>, T?> onFail)
    {
        ServiceResult<T> result = await serviceCall;

        return result.IsSuccess ? onSuccess(result.Result) : onFail(result.Errors);
    }

    public static async Task<T?> MatchAsync<T>(
        this Task<ServiceResult<T>> serviceCall,
        Func<T?, Task<T?>> onSuccess,
        Func<Task<T?>> onFail)
    {
        ServiceResult<T> result = await serviceCall;

        if (result.IsSuccess)
        {
            return await onSuccess(result.Result);
        }

        return await onFail();
    }
}