using MediatR;
using NeoServer.Data.Interfaces;
using NeoServer.Web.API.Requests.Commands;
using NeoServer.Web.API.Response;
using NeoServer.Web.API.Response.Constants;

namespace NeoServer.Web.API.Application.UseCases.Commands;

public class AddPremiumDaysAccountCommand(IAccountRepository accountRepository) : IRequestHandler<AddPremioumDaysAccountRequest, OutputResponse>
{
    public async Task<OutputResponse> Handle(AddPremioumDaysAccountRequest request, CancellationToken cancellationToken)
    {
        var anotherAccount = await accountRepository.GetAsync(request.AccountId);
        
        if (anotherAccount is null)
            return new OutputResponse(ErrorMessage.AccountDoesNotExist);
        
        anotherAccount.PremiumTime += request.Days;
        await accountRepository.Update(anotherAccount);

        return new();
    }
}