namespace NeoServer.Web.API.Response;

public record OutputResponse
{
    public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);
    public string ErrorMessage { get; }

    public OutputResponse() {}
    public OutputResponse(string errorMessage)
      =>  ErrorMessage = errorMessage;
}