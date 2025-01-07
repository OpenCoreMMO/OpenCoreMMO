using MediatR;
using NeoServer.Web.API.Response;

namespace NeoServer.Web.API.Requests.Commands;

public class BanAccountRequest : IRequest<OutputResponse>, ICommandBase
{
    public int AccountId { get; private set; }
    
    public string Reason { get; set; }
    
    public int Days { get; set; }

    public void SetAccountId(int accountId)
    {
        AccountId = accountId;
    }
}