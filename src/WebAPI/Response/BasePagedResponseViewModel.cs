namespace NeoServer.Web.API.Response;

[Serializable]
public class BasePagedResponseViewModel<T>
{
    public BasePagedResponseViewModel(T data, int pageNumber, int pageSize, int totalRecords, int totalPages)
    {
        TotalPages = totalPages;
        PageNumber = pageNumber;
        PageSize = pageSize;
        Data = data;
        TotalRecords = totalRecords;
    }

    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public T Data { get; set; }

}