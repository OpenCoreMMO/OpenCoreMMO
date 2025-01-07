using MediatR;
using NeoServer.Data.Interfaces;
using NeoServer.Web.API.Requests.Commands;
using NeoServer.Web.API.Response;
using NeoServer.Web.API.Response.Constants;

namespace NeoServer.Web.API.Application.UseCases.Commands;

public class BanAccountCommand(IAccountRepository accountRepository) : IRequestHandler<BanAccountRequest, OutputResponse>
{
    public async Task<OutputResponse> Handle(BanAccountRequest request, CancellationToken cancellationToken)
    {
        var anotherAccount = await accountRepository.GetAsync(request.AccountId);
        
        if (anotherAccount is null)
            return new OutputResponse(ErrorMessage.AccountDoesNotExist);
        
        if (anotherAccount.BanishedAt is not null)
            return new OutputResponse(ErrorMessage.AccountAlreadyBanished);
        
        anotherAccount.BanishmentReason = request.Reason;
        anotherAccount.BanishedAt = anotherAccount.BanishedAt.HasValue ? anotherAccount.BanishedAt : DateTime.UtcNow;
        anotherAccount.BanishedEndAt = anotherAccount.BanishedAt?.AddDays(request.Days);
        anotherAccount.BannedBy = 1; // TODO: Get the user id from the request with the token
        
        await accountRepository.Update(anotherAccount);

        return new();
    }
}