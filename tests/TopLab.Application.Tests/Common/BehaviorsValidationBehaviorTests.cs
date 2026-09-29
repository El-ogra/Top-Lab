using FluentValidation;
using MediatR;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using Xunit;

namespace TopLab.Application.Tests.Common;

public class BehaviorsValidationBehaviorTests
{
    /// <summary>Test-local request; not product code.</summary>
    private sealed record LocalValidationProbe(string Name) : IRequest<Result<string>>;

    [Fact]
    public async Task ValidationBehavior_ShortCircuits_WhenInputInvalid()
    {
        var failingValidator = new FailingValidator();
        var behavior = new ValidationBehavior<LocalValidationProbe, Result<string>>(failingValidator);
        var request = new LocalValidationProbe(Name: "");

        var response = await behavior.Handle(request, _ => throw new Exception("handler must not run"), CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ErrorType.Validation, response.Error!.Type);
        Assert.Single(response.Errors);
    }

    [Fact]
    public async Task ValidationBehavior_CallsNext_WhenInputValid()
    {
        var passingValidator = new PassingValidator();
        var behavior = new ValidationBehavior<LocalValidationProbe, Result<string>>(passingValidator);
        var request = new LocalValidationProbe(Name: "Sara");

        var response = await behavior.Handle(request, _ => Task.FromResult(Result<string>.Success("ok")), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal("ok", response.Value);
    }

    private sealed class FailingValidator : AbstractValidator<LocalValidationProbe>
    {
        public FailingValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("الاسم مطلوب.");
        }
    }

    private sealed class PassingValidator : AbstractValidator<LocalValidationProbe>
    {
    }
}
