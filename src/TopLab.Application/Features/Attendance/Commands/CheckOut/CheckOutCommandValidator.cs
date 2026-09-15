using FluentValidation;

namespace TopLab.Application.Features.Attendance.Commands.CheckOut;

public sealed class CheckOutCommandValidator : AbstractValidator<CheckOutCommand>
{
    public CheckOutCommandValidator()
    {
        // No parameters: the open record is resolved from the session (SD-18-2).
        // The open-break guard lives in the Domain (SD-18-6).
    }
}
