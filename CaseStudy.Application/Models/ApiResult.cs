namespace CaseStudy.Application.Models;

public class ApiResult<T>
{
    private ApiResult()
    {
    }

    private ApiResult(bool succeeded, T result, IEnumerable<string> errors, int? count = null)
    {
        this.Succeeded = succeeded;
        this.Result = result;
        this.Errors = errors;
        this.Count = count;
    }

    public bool Succeeded { get; set; }

    public string SucceededMessage { get; set; }

    public T Result { get; set; }

    public IEnumerable<string> Errors { get; set; }

    public int? Count { get; set; }

    public static ApiResult<T> Success(T result, int count)
    {
        return new ApiResult<T>(true, result, (IEnumerable<string>)new List<string>(), count);
    }

    public static ApiResult<T> Failure(IEnumerable<string> errors)
    {
        return new ApiResult<T>(false, default(T), errors);
    }
}