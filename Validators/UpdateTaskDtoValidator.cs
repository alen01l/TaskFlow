using FluentValidation;
using TaskFlow.Api.Contracts.Tasks;

namespace TaskFlow.Api.Validators;

public class UpdateTaskDtoValidator : AbstractValidator<UpdateTaskDto>
{
    public UpdateTaskDtoValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(120);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        When(x => x.Title is not null, () =>
        {
            RuleFor(x => x.Title)
                .NotEmpty();
        });
    }
}