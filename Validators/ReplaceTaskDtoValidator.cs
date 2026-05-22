using FluentValidation;
using TaskFlow.Api.Contracts.Tasks;

namespace TaskFlow.Api.Validators;

public class ReplaceTaskDtoValidator : AbstractValidator<ReplaceTaskDto>
{
    public ReplaceTaskDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(120);
        
        RuleFor(x => x.Description)
            .MaximumLength(1000);
    }
}