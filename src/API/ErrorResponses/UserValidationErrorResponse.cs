namespace API.ErrorResponses;

public class CustomErrorObject
{
    public string Message { get; set; }
    public string ErrorCode { get; set; }
}
public class UserValidationErrorResponse : ErrorResponse
{
    public UserValidationErrorResponse() : base(400, null)
    {
    }

    public IReadOnlyDictionary<string,CustomErrorObject> Errors { get; set; }
}