using MediatR;
using NeoServer.Web.API.Response;

namespace NeoServer.Web.API.Requests.Commands;

public class ChangePasswordRequest : IRequest<OutputResponse>, ICommandBase
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmPassword { get; set; }
    
    public int AccountId { get; private set; }

    public void SetAccountId(int accountId)
    {
        AccountId = accountId;
    }
}