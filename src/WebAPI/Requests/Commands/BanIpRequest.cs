using MediatR;
using NeoServer.Web.API.Response;

namespace NeoServer.Web.API.Requests.Commands;

public class BanIpRequest : IRequest<OutputResponse>, ICommandBase
{
    public string Ip { get; set; }
    
    public string Reason { get; set; }
    
    public int Days { get; set; }
}