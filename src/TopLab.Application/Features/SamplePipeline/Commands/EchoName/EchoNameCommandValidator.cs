using FluentValidation;

namespace TopLab.Application.Features.SamplePipeline.Commands.EchoName;

/// <summary>
/// Validates <see cref="EchoNameCommand"/> — name is required and non-empty.
/// </summary>
public sealed class EchoNameCommandValidator : AbstractValidator<EchoNameCommand>
{
    public EchoNameCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("الاسم مطلوب.");
    }
}
