using TopLab.Application.Features.ResultDelivery.Common;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultDelivery;

public class ResultDeliveryDomainFailureTranslatorTests
{
    [Fact]
    public void UnprintedGuard_MapsToFrozenMessage()
    {
        Assert.Equal("النتيجة غير مطبوعة.", DomainFailureTranslator.Translate(new InvalidOperationException("Result not printed.")));
    }

    [Fact]
    public void UnknownFailure_MapsToFallback()
    {
        Assert.Equal("بيانات غير صالحة.", DomainFailureTranslator.Translate(new InvalidOperationException("boom")));
    }
}
