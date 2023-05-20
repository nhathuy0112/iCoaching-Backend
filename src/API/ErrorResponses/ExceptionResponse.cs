namespace API.ErrorResponses;

public class ExceptionResponse : ErrorResponse
{
    public ExceptionResponse(int statusCode, string message = null!, string details = null!) : base(statusCode, message)
    {
        Details = details;
    }

    public string Details { get; set; }
}