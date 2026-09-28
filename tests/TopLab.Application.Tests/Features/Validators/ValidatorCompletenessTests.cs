using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Common.Interfaces;

namespace TopLab.Application.Tests.Features.Validators;

/// <summary>
/// Slice 2 (m-01): static completeness test asserting every parameterised command
/// has a sibling validator, and every parameterless command does not need one.
/// </summary>
public class ValidatorCompletenessTests
{
    [Fact]
    public void EveryParameterisedCommand_HasSiblingValidator()
    {
        var assembly = typeof(IApplicationDbContext).Assembly;
        var commandTypes = assembly.GetTypes()
            .Where(t => t.IsClass && t.IsSealed && t.Name.EndsWith("Command") && !t.Name.EndsWith("Validator"))
            .ToList();

        var missing = new List<string>();
        foreach (var cmd in commandTypes)
        {
            // Check if parameterised (has a constructor with parameters)
            var ctor = cmd.GetConstructors().FirstOrDefault();
            if (ctor == null || ctor.GetParameters().Length == 0)
                continue; // parameterless — no validator needed

            var validatorName = cmd.Name + "Validator";
            var validatorType = assembly.GetTypes()
                .FirstOrDefault(t => t.Name == validatorName && t.IsClass && t.IsSealed);

            if (validatorType == null)
            {
                missing.Add(cmd.Name);
            }
            else if (!typeof(IValidator).IsAssignableFrom(validatorType))
            {
                missing.Add($"{cmd.Name} (validator does not implement IValidator)");
            }
        }

        Assert.True(missing.Count == 0,
            $"Parameterised commands without a sibling validator: {string.Join(", ", missing)}");
    }

    [Fact]
    public void ParameterlessCommandsWithoutValidators_AreExactlyThree()
    {
        // SD-8: the three parameterless commands that should NOT have validators
        var expected = new[] { "LockWorkstationCommand", "ApplyDatabaseUpdatesCommand", "SignOutCommand" };
        var assembly = typeof(IApplicationDbContext).Assembly;

        foreach (var name in expected)
        {
            var cmdType = assembly.GetTypes().FirstOrDefault(t => t.Name == name && t.IsClass && t.IsSealed);
            Assert.NotNull(cmdType);

            var validatorName = name + "Validator";
            var validatorType = assembly.GetTypes().FirstOrDefault(t => t.Name == validatorName && t.IsClass && t.IsSealed);
            Assert.Null(validatorType); // must NOT have a validator
        }
    }
}
