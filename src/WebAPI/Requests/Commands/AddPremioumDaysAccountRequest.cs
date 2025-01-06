using MediatR;
using NeoServer.Web.API.Response;

namespace NeoServer.Web.API.Requests.Commands;

public class AddPremioumDaysAccountRequest : IRequest<OutputResponse>, ICommandBase
{
    public int Days { get; set; }
    public string Description { get; set; }
    public int AccountId { get; private set; }
    

    public void SetAccountId(int accountId)
    {
        AccountId = accountId;
    }
}