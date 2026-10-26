using System.Windows;
using TopLab.Presentation.Common.Converters;
using Xunit;

namespace TopLab.Presentation.Tests.Converters;

public class NullToVisibilityConverterTests
{
    private readonly NullToVisibilityConverter _c = new();

    [Fact]
    public void Null_Collapsed()
    {
        Assert.Equal(Visibility.Collapsed, _c.Convert(null!, typeof(Visibility), null!, System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public void EmptyString_Collapsed()
    {
        Assert.Equal(Visibility.Collapsed, _c.Convert(string.Empty, typeof(Visibility), null!, System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Whitespace_Collapsed()
    {
        Assert.Equal(Visibility.Collapsed, _c.Convert("   ", typeof(Visibility), null!, System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Text_Visible()
    {
        Assert.Equal(Visibility.Visible, _c.Convert("note", typeof(Visibility), null!, System.Globalization.CultureInfo.InvariantCulture));
    }
}
