using MediatR;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Common;

public class BehaviorsLoggingBehaviorTests
{
    [Fact]
    public async Task LoggingBehavior_RecordsSuccessOutcome()
    {
        var logger = new FakeAppLogger();
        var behavior = new LoggingBehavior<Result, Result>(logger);

        await behavior.Handle(Result.Success(), _ => Task.FromResult(Result.Success()), CancellationToken.None);

        var entry = Assert.Single(logger.Entries);
        Assert.Equal("Success", entry.Outcome);
    }

    [Fact]
    public async Task LoggingBehavior_RecordsFailureOutcomeWithErrorType()
    {
        var logger = new FakeAppLogger();
        var behavior = new LoggingBehavior<Result, Result>(logger);
        var failed = Result.Failure(Error.Validation("bad"));

        await behavior.Handle(failed, _ => Task.FromResult(failed), CancellationToken.None);

        var entry = Assert.Single(logger.Entries);
        Assert.StartsWith("Failure:", entry.Outcome);
    }
}
