using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Common.Interfaces;

namespace TopLab.Application.Tests.Features.Validators;

/// <summary>
/// Slice 2 (m-01): completeness via the real validation DI container.
/// Every parameterised command must resolve IValidator&lt;T&gt;; parameterless
/// commands without validators are counted exactly (not name-listed only).
/// </summary>
public class ValidatorCompletenessTests
{
    private static IEnumerable<Type> CommandTypes()
    {
        var assembly = typeof(IApplicationDbContext).Assembly;
        return assembly.GetTypes()
            .Where(t => t.IsClass && t.IsSealed && t.Name.EndsWith("Command") && !t.Name.EndsWith("Validator"))
            .ToList();
    }

    private static bool IsParameterless(Type cmd)
    {
        // Positional records with no parameters: either no ctor params or an empty primary ctor.
        var ctors = cmd.GetConstructors();
        return ctors.Length > 0 && ctors.All(c => c.GetParameters().Length == 0);
    }

    private static ServiceProvider BuildApplicationProvider()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        return services.BuildServiceProvider();
    }

    [Fact]
    public void EveryParameterisedCommand_ResolvesIValidator_FromDi()
    {
        using var provider = BuildApplicationProvider();
        var missing = new List<string>();

        foreach (var cmd in CommandTypes().Where(c => !IsParameterless(c)))
        {
            var validatorService = typeof(IValidator<>).MakeGenericType(cmd);
            if (provider.GetService(validatorService) is null)
            {
                missing.Add(cmd.Name);
            }
        }

        Assert.True(missing.Count == 0,
            $"Parameterised commands without IValidator<T> in DI: {string.Join(", ", missing)}");
    }

    [Fact]
    public void ParameterlessCommandsWithoutValidators_AreExactlyThree()
    {
        using var provider = BuildApplicationProvider();

        var withoutValidator = new List<string>();
        foreach (var cmd in CommandTypes().Where(IsParameterless))
        {
            var validatorService = typeof(IValidator<>).MakeGenericType(cmd);
            if (provider.GetService(validatorService) is null)
            {
                withoutValidator.Add(cmd.Name);
            }
        }

        // SD-8: exactly these three parameterless commands have no validator.
        var expected = new[] { "ApplyDatabaseUpdatesCommand", "LockWorkstationCommand", "SignOutCommand" };

        Assert.Equal(3, withoutValidator.Count);
        Assert.Equal(expected.OrderBy(n => n), withoutValidator.OrderBy(n => n));
    }

    [Fact]
    public void ParameterlessCommandCount_IsExact()
    {
        var parameterless = CommandTypes().Where(IsParameterless).Select(t => t.Name).OrderBy(n => n).ToList();

        // Current set of parameterless commands — count is part of the contract.
        var expected = new[]
        {
            "ApplyDatabaseUpdatesCommand",
            "CheckInCommand",
            "CheckOutCommand",
            "EndBreakCommand",
            "LockWorkstationCommand",
            "SignOutCommand",
            "StartBreakCommand"
        };

        Assert.Equal(expected.OrderBy(n => n), parameterless);
    }
}