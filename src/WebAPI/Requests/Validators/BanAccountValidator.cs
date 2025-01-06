using FluentValidation;
using NeoServer.Web.API.Requests.Commands;

namespace NeoServer.Web.API.Requests.Validators;

public class BanAccountValidator : AbstractValidator<BanAccountRequest>
{
    public BanAccountValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().MinimumLength(10).WithMessage("reason is required.");
        
        RuleFor(x => x.Days)
            .NotEmpty().GreaterThan(0).WithMessage("days is required.");
    }
}