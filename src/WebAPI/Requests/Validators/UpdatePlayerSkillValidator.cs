using FluentValidation;
using NeoServer.Web.API.Requests.Commands;

namespace NeoServer.Web.API.Requests.Validators;

public class UpdatePlayerSkillValidator : AbstractValidator<UpdatePlayerSkillsRequest>
{
    public UpdatePlayerSkillValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required.");
        
        RuleFor(x => x.SkillAxe)
            .NotEmpty()
            .InclusiveBetween(10, 120)
            .WithMessage("SkillAxe must be between 10 and 120");

        RuleFor(x => x.SkillDist)
            .NotEmpty()
            .InclusiveBetween(10, 120)
            .WithMessage("SkillDist must be between 10 and 120.");

        RuleFor(x => x.SkillClub)
            .NotEmpty()
            .InclusiveBetween(10, 120)
            .WithMessage("SkillClub must be between 10 and 120.");

        RuleFor(x => x.SkillSword)
            .NotEmpty()
            .InclusiveBetween(10, 120)
            .WithMessage("SkillSword must be between 10 and 120.");

        RuleFor(x => x.SkillShielding)
            .NotEmpty()
            .InclusiveBetween(10, 120)
            .WithMessage("SkillShielding must be between 10 and 120.");

        RuleFor(x => x.SkillFist)
            .NotEmpty()
            .InclusiveBetween(10, 120)
            .WithMessage("SkillFist must be between 10 and 120.");

        RuleFor(x => x.SkillFishing)
            .NotEmpty()
            .InclusiveBetween(10, 120)
            .WithMessage("SkillFishing must be between 10 and 120.");
    }
}