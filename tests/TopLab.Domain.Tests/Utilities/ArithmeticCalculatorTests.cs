using TopLab.Domain.Utilities;
using Xunit;

namespace TopLab.Domain.Tests.Utilities;

public class ArithmeticCalculatorTests
{
    [Theory]
    [InlineData("1+2", 3)]
    [InlineData("10-4", 6)]
    [InlineData("3*4", 12)]
    [InlineData("15/3", 5)]
    [InlineData("2+3*4", 14)]
    [InlineData("(2+3)*4", 20)]
    [InlineData("10.5+0.5", 11)]
    [InlineData("-5+10", 5)]
    [InlineData("2*-3", -6)]
    [InlineData("100/4/5", 5)]
    [InlineData("((1+2)*(3+4))", 21)]
    [InlineData("2 × 3", 6)]
    [InlineData("8 ÷ 4", 2)]
    [InlineData("10 − 3", 7)]
    public void Evaluate_Grammar(string expression, decimal expected)
    {
        Assert.Equal(expected, ArithmeticCalculator.Evaluate(expression));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1+")]
    [InlineData("(1+2")]
    [InlineData("1+2)")]
    [InlineData("abc")]
    [InlineData("1..2")]
    public void MalformedExpression_ThrowsArgumentException(string expression)
    {
        Assert.Throws<ArgumentException>(() => ArithmeticCalculator.Evaluate(expression));
    }

    [Fact]
    public void UnaryPlus_IsAccepted()
    {
        Assert.Equal(1m, ArithmeticCalculator.Evaluate("++1"));
    }

    [Fact]
    public void DivisionByZero_ThrowsCalculatorException_WithFrozenMessage()
    {
        var ex = Assert.Throws<CalculatorException>(() => ArithmeticCalculator.Evaluate("1/0"));
        Assert.Equal("لا يمكن القسمة على صفر.", ex.Message);
    }
}
