namespace API.ErrorResponses;

public class ErrorResponse
{
    public ErrorResponse(int statusCode, string? message)
    {
        StatusCode = statusCode;
        Message = message ?? GetDefaultMessageForStatusCode(statusCode);
    }

    public int StatusCode { get; set; }
    public string? Message { get; set; }

    private string? GetDefaultMessageForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            400 => "Bad request",
            401 => "Not authorized",
            403 => "Not allowed",
            404 => "Resource not found",
            500 => "Error",
            _ => null
        };
    }
}