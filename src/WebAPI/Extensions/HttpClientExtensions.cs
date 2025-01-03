using System.Text;
using NeoServer.Web.API.Response;
using NeoServer.Web.Shared.Exceptions;
using Newtonsoft.Json;

namespace NeoServer.Web.API.Extensions;

public static class HttpClientExtensions
{
    #region public methods implementations

    public static async Task<T> GetAndDeserialize<T>(this HttpClient client, string requestUri)
    {
        return await SendAndDeserialize<T>(HttpMethod.Get, client, requestUri, null);
    }

    public static async Task<T> PostAndDeserialize<T>(this HttpClient client, string requestUri, object data)
    {
        return await SendAndDeserialize<T>(HttpMethod.Post, client, requestUri, data);
    }

    public static async Task<T> PutAndDeserialize<T>(this HttpClient client, string requestUri, object data)
    {
        return await SendAndDeserialize<T>(HttpMethod.Put, client, requestUri, data);
    }

    public static async Task<T> DeleteAndDeserialize<T>(this HttpClient client, string requestUri)
    {
        return await SendAndDeserialize<T>(HttpMethod.Delete, client, requestUri, null);
    }

    #endregion

    #region Private Methods

    private static async Task<T> SendAndDeserialize<T>(HttpMethod method, HttpClient client, string requestUri,
        object data)
    {
        var response = await client.SendAsync(CreateRequestMessage(method, requestUri, data));
        return await ValidateAndReturnResponseData<T>(response);
    }

    private static HttpRequestMessage CreateRequestMessage(HttpMethod method, string requestUri, object content = null)
    {
        var request = new HttpRequestMessage
        {
            RequestUri = new Uri(requestUri, UriKind.RelativeOrAbsolute),
            Method = method
        };

        if (content != null)
            request.Content =
                new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");

        return request;
    }

    private static async Task<T> ValidateAndReturnResponseData<T>(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var resultString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonConvert.DeserializeObject<BaseResponseViewModel>(resultString);
    
        if (baseResponse.Errors.Any())
            throw new NeoException(baseResponse.Errors.FirstOrDefault());
    
        var getResponseDataJson = JsonConvert.SerializeObject(baseResponse?.Data);
        return JsonConvert.DeserializeObject<T>(getResponseDataJson);
    }

    #endregion
}