using FluentValidation;
using NeoServer.Web.API.Requests.Commands;

namespace NeoServer.Web.API.Requests.Validators;

public class CreateWorldValidator : AbstractValidator<CreateWorldRequest>
{
    public CreateWorldValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Length(3, 100).WithMessage("Name must be between 3 and 100 characters.");

        RuleFor(x => x.Ip)
            .NotEmpty().WithMessage("IP address is required.")
            .Matches(@"^(\d{1,3}\.){3}\d{1,3}$").WithMessage("IP address must be a valid IP format.");

        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535).WithMessage("Port must be between 1 and 65535.");

        RuleFor(x => x.MaxCapacity)
            .GreaterThan(0).WithMessage("Max capacity must be greater than 0.");

        RuleFor(x => x.Region)
            .IsInEnum().WithMessage("Continent must be a valid value.");

        RuleFor(x => x.PvpType)
            .IsInEnum().WithMessage("PvpType must be a valid value.");

        RuleFor(x => x.Mode)
            .IsInEnum().WithMessage("Mode must be a valid value.");

        RuleFor(x => x.RequiresPremium)
            .NotNull().WithMessage("RequiresPremium is required.");

        RuleFor(x => x.TransferEnabled)
            .NotNull().WithMessage("TransferEnabled is required.");

        RuleFor(x => x.AntiCheatEnabled)
            .NotNull().WithMessage("AntiCheatEnabled is required.");
    }
}