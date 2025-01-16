using FluentValidation;
using NeoServer.Web.API.Requests.Commands;

namespace NeoServer.Web.API.Requests.Validators;

public class UpdatePlayerInfosRequestValidator : AbstractValidator<UpdatePlayerInfosRequest>
{
    public UpdatePlayerInfosRequestValidator() 
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Player ID must be greater than 0.");

        RuleFor(x => x.TownId)
            .GreaterThan(0)
            .WithMessage("Town ID must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.");

        RuleFor(x => x.Group)
            .InclusiveBetween((byte)1, (byte)5)
            .WithMessage("Group must be between 1 and 5.");

        RuleFor(x => x.Capacity)
            .GreaterThanOrEqualTo((uint)470)
            .WithMessage("Capacity must be at least 470.");

        RuleFor(x => x.Level)
            .GreaterThanOrEqualTo((ushort)1)
            .WithMessage("Level must be at least 1.");

        RuleFor(x => x.Mana)
            .GreaterThanOrEqualTo((ushort)0)
            .WithMessage("Mana cannot be negative.")
            .LessThanOrEqualTo(x => x.MaxMana)
            .WithMessage("Mana cannot exceed MaxMana.");

        RuleFor(x => x.MaxMana)
            .GreaterThan((ushort)0)
            .WithMessage("MaxMana must be greater than 0.");

        RuleFor(x => x.Health)
            .GreaterThanOrEqualTo((ushort)0)
            .WithMessage("Health cannot be negative.")
            .LessThanOrEqualTo(x => x.MaxHealth)
            .WithMessage("Health cannot exceed MaxHealth.");

        RuleFor(x => x.MaxHealth)
            .GreaterThan((ushort)0)
            .WithMessage("MaxHealth must be greater than 0.");

        RuleFor(x => x.Soul)
            .GreaterThan((byte)0)
            .LessThanOrEqualTo(x => x.MaxSoul)
            .WithMessage("Soul must be between 0 and MaxSoul.");

        RuleFor(x => x.MaxSoul)
            .GreaterThanOrEqualTo((byte)0)
            .WithMessage("MaxSoul must be at least 0.");

        RuleFor(x => x.Speed)
            .InclusiveBetween((ushort)200,(ushort)2500)
            .WithMessage("Speed must be between 200 and 2500.");

        RuleFor(x => x.ChaseMode)
            .IsInEnum()
            .WithMessage("Invalid ChaseMode.");

        RuleFor(x => x.FightMode)
            .IsInEnum()
            .WithMessage("Invalid FightMode.");

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithMessage("Invalid Gender.");

        RuleFor(x => x.Vocation)
            .InclusiveBetween((byte)0, (byte)11)
            .WithMessage("Vocation must be between 0 and 11.");

        RuleFor(x => x.WorldId)
            .GreaterThan(0)
            .WithMessage("World ID must be greater than 0.");
    }
}