using TopLab.Domain.Common;

namespace TopLab.Domain.Utilities;

/// <summary>Domain-originated failure from the arithmetic calculator (SD-23-9).</summary>
public sealed class CalculatorException : DomainException
{
    public CalculatorException(string message)
        : base(message)
    {
    }
}
