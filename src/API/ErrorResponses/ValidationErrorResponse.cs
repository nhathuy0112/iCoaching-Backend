namespace API.ErrorResponses;

public class ValidationErrorResponse : ErrorResponse
{
    public ValidationErrorResponse() : base(400, null)
    {
    }
    public IReadOnlyDictionary<string,string[]> Errors { get; set; }
}