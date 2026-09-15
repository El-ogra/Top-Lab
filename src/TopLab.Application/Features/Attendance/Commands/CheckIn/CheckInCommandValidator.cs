using FluentValidation;

namespace TopLab.Application.Features.Attendance.Commands.CheckIn;

public sealed class CheckInCommandValidator : AbstractValidator<CheckInCommand>
{
    public CheckInCommandValidator()
    {
        // No parameters: the user is taken from the session (SD-18-2).
        // The single-open-record invariant lives in the handler (SD-18-5).
    }
}
