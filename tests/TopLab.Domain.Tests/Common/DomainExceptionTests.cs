using TopLab.Domain.Common;
using Xunit;

namespace TopLab.Domain.Tests.Common;

public sealed class SampleDomainException : DomainException
{
    public SampleDomainException(string message)
        : base(message)
    {
    }
}

public class DomainExceptionTests
{
    [Fact]
    public void GivenMessage_WhenConstructed_ThenMessageIsPreserved()
    {
        var ex = new SampleDomainException("Invariant violated");

        Assert.Equal("Invariant violated", ex.Message);
        Assert.IsAssignableFrom<System.Exception>(ex);
    }
}
