using FluentValidation;

namespace AutismCenter.Application.Features.ContentManagement.Commands.DeleteLocalizedContent;

public class DeleteLocalizedContentValidator : AbstractValidator<DeleteLocalizedContentCommand>
{
    public DeleteLocalizedContentValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required");
    }
}