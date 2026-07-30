using FluentValidation;

namespace SafeFlow.Application.Employees.Commands.TransferEmployee;

public sealed class TransferEmployeeCommandValidator : AbstractValidator<TransferEmployeeCommand>
{
    public TransferEmployeeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Employee ID is required.");

        RuleFor(x => x.NewDepartmentId)
            .NotEmpty().WithMessage("New department ID is required.");
    }
}
