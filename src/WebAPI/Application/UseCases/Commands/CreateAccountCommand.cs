using MediatR;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using NeoServer.Web.API.Requests.Commands;
using NeoServer.Web.API.Response;
using NeoServer.Web.API.Response.Constants;

namespace NeoServer.Web.API.Application.UseCases.Commands;

public class CreateAccountCommand(IAccountRepository accountRepository) : IRequestHandler<CreateAccountRequest, OutputResponse>
{
    public async Task<OutputResponse> Handle(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var anotherAccount = await accountRepository.GetByEmail(request.Email);
        
        if (anotherAccount is not null)
            return new OutputResponse(ErrorMessage.AccountEmailAlreadyExist);
        
        await accountRepository.Insert(new AccountEntity
        {
            Password = request.Password,
            CreatedAt = DateTime.UtcNow,
            EmailAddress = request.Email,
        });

        return new();
    }
}