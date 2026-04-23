namespace Healthcare.Contracts.Common;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public IReadOnlyCollection<ErrorDetail>? Errors { get; init; }

    public static ApiResponse<T> Ok(T? data, string message = "Request processed successfully") =>
        new()
        {
            Success = true,
            Message = message,
            Data = data
        };

    public static ApiResponse<T> Fail(string message, params ErrorDetail[] errors) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = errors
        };
}
