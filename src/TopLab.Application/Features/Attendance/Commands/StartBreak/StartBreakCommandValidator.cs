using FluentValidation;

namespace TopLab.Application.Features.Attendance.Commands.StartBreak;

public sealed class StartBreakCommandValidator : AbstractValidator<StartBreakCommand>
{
    public StartBreakCommandValidator()
    {
        // No parameters: the open record is resolved from the session (SD-18-2).
        // Break sequencing guards live in the Domain (SD-18-6).
    }
}
