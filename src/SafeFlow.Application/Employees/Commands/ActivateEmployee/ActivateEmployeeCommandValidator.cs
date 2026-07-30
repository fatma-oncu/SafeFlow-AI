using FluentValidation;

namespace SafeFlow.Application.Employees.Commands.ActivateEmployee;

public sealed class ActivateEmployeeCommandValidator : AbstractValidator<ActivateEmployeeCommand>
{
    public ActivateEmployeeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Employee ID is required.");
    }
}
