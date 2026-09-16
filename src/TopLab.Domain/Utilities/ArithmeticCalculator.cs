using System.Globalization;
using System.Text;

namespace TopLab.Domain.Utilities;

/// <summary>
/// Pure static Domain service: safe four-function arithmetic evaluator (SD-23-9).
/// Supports + − × ÷, parentheses, decimals, and unary minus via recursive descent.
/// Never uses DataTable.Compute or dynamic code evaluation.
/// Malformed input → <see cref="ArgumentException"/>; division by zero → <see cref="CalculatorException"/>.
/// </summary>
public static class ArithmeticCalculator
{
    /// <exception cref="ArgumentException">Malformed expression.</exception>
    /// <exception cref="CalculatorException">Division by zero.</exception>
    public static decimal Evaluate(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new ArgumentException("Expression is required.", nameof(expression));
        }

        var text = Normalize(expression);
        var index = 0;
        var value = ParseExpression(text, ref index);
        SkipWhitespace(text, ref index);
        if (index != text.Length)
        {
            throw new ArgumentException("Unexpected trailing input in expression.", nameof(expression));
        }

        return value;
    }

    private static string Normalize(string expression)
    {
        var builder = new StringBuilder(expression.Length);
        foreach (var ch in expression)
        {
            builder.Append(ch switch
            {
                '×' or '*' => '*',
                '÷' or '/' => '/',
                '−' or '–' or '—' => '-',
                _ => ch
            });
        }

        return builder.ToString();
    }

    private static void SkipWhitespace(string text, ref int index)
    {
        while (index < text.Length && char.IsWhiteSpace(text[index]))
        {
            index++;
        }
    }

    private static decimal ParseExpression(string text, ref int index)
    {
        var left = ParseTerm(text, ref index);
        while (true)
        {
            SkipWhitespace(text, ref index);
            if (index >= text.Length)
            {
                return left;
            }

            var op = text[index];
            if (op is not ('+' or '-'))
            {
                return left;
            }

            index++;
            var right = ParseTerm(text, ref index);
            left = op == '+' ? left + right : left - right;
        }
    }

    private static decimal ParseTerm(string text, ref int index)
    {
        var left = ParseFactor(text, ref index);
        while (true)
        {
            SkipWhitespace(text, ref index);
            if (index >= text.Length)
            {
                return left;
            }

            var op = text[index];
            if (op is not ('*' or '/'))
            {
                return left;
            }

            index++;
            var right = ParseFactor(text, ref index);
            if (op == '*')
            {
                left *= right;
            }
            else
            {
                if (right == 0m)
                {
                    throw new CalculatorException("لا يمكن القسمة على صفر.");
                }

                left /= right;
            }
        }
    }

    private static decimal ParseFactor(string text, ref int index)
    {
        SkipWhitespace(text, ref index);
        if (index >= text.Length)
        {
            throw new ArgumentException("Unexpected end of expression.");
        }

        var ch = text[index];
        if (ch == '+')
        {
            index++;
            return ParseFactor(text, ref index);
        }

        if (ch == '-')
        {
            index++;
            return -ParseFactor(text, ref index);
        }

        if (ch == '(')
        {
            index++;
            var value = ParseExpression(text, ref index);
            SkipWhitespace(text, ref index);
            if (index >= text.Length || text[index] != ')')
            {
                throw new ArgumentException("Missing closing parenthesis.");
            }

            index++;
            return value;
        }

        if (char.IsDigit(ch) || ch == '.')
        {
            return ParseNumber(text, ref index);
        }

        throw new ArgumentException($"Unexpected character '{ch}'.");
    }

    private static decimal ParseNumber(string text, ref int index)
    {
        var start = index;
        while (index < text.Length && (char.IsDigit(text[index]) || text[index] == '.'))
        {
            index++;
        }

        var span = text.AsSpan(start, index - start);
        if (!decimal.TryParse(span, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value))
        {
            throw new ArgumentException($"Invalid number '{span.ToString()}'.");
        }

        return value;
    }
}
