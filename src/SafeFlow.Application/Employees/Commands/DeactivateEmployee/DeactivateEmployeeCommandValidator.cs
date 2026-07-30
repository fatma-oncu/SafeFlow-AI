using FluentValidation;

namespace SafeFlow.Application.Employees.Commands.DeactivateEmployee;

public sealed class DeactivateEmployeeCommandValidator : AbstractValidator<DeactivateEmployeeCommand>
{
    public DeactivateEmployeeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Employee ID is required.");
    }
}
