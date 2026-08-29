using System.Reflection;
using NetArchTest.Rules;

namespace TopLab.Architecture.Tests;

/// <summary>
/// Guards the composition-root exception (ADR-0024): only App.xaml.cs may
/// reference Infrastructure. ViewModels and Views must never depend on
/// TopLab.Infrastructure. See Architecture §2.2 and Coding Standards §3.1.
/// </summary>
public sealed class PresentationArchitectureTests
{
    private static readonly Assembly PresentationAssembly = typeof(TopLab.Presentation.App).Assembly;

    [Fact]
    public void ViewModels_ShouldNotDependOnInfrastructure()
    {
        var result = Types.InAssembly(PresentationAssembly)
            .That()
            .ResideInNamespaceMatching(@"TopLab\.Presentation\.ViewModels.*")
            .ShouldNot()
            .HaveDependencyOn("TopLab.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"ViewModels must not depend on TopLab.Infrastructure. Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Views_ShouldNotDependOnInfrastructure()
    {
        var result = Types.InAssembly(PresentationAssembly)
            .That()
            .ResideInNamespaceMatching(@"TopLab\.Presentation\.Views.*")
            .ShouldNot()
            .HaveDependencyOn("TopLab.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Views must not depend on TopLab.Infrastructure. Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void ViewModelsAndViews_ShouldNotDependOnInfrastructure_Combined()
    {
        // Combined guard — covers both namespaces in a single assertion.
        // Uses predicate so App (TopLab.Presentation namespace) is naturally excluded;
        // only ViewModels.* and Views.* are checked.
        var viewModelResult = Types.InAssembly(PresentationAssembly)
            .That()
            .ResideInNamespaceMatching(@"TopLab\.Presentation\.ViewModels.*")
            .ShouldNot()
            .HaveDependencyOn("TopLab.Infrastructure")
            .GetResult();

        var viewsResult = Types.InAssembly(PresentationAssembly)
            .That()
            .ResideInNamespaceMatching(@"TopLab\.Presentation\.Views.*")
            .ShouldNot()
            .HaveDependencyOn("TopLab.Infrastructure")
            .GetResult();

        var allFailing = (viewModelResult.FailingTypes ?? Enumerable.Empty<Type>())
            .Concat(viewsResult.FailingTypes ?? Enumerable.Empty<Type>())
            .Select(t => t.FullName)
            .ToArray();

        Assert.True(viewModelResult.IsSuccessful && viewsResult.IsSuccessful,
            $"No ViewModel or View type may depend on TopLab.Infrastructure. Failing types: {string.Join(", ", allFailing)}");
    }
}
