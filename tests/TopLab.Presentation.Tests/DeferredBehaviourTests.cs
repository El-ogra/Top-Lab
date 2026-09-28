using System.IO;
using System.Reflection;

namespace TopLab.Presentation.Tests;

/// <summary>
/// S-07 Slice 5/6: deferred behavioural tests for lock-workstation and navigation.
/// Structural assertions only — no WPF element is instantiated.
/// </summary>
public class DeferredBehaviourTests
{
    [Fact]
    public void LockWorkstation_ResultsAreChecked()
    {
        // S-05 VG-05: LockWorkstationAsync must capture the result and branch on IsSuccess
        var srcDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "TopLab.Presentation");
        var vmPath = Path.Combine(srcDir, "ViewModels", "Shell", "ShellViewModel.cs");
        Assert.True(File.Exists(vmPath), "ShellViewModel.cs not found");

        var content = File.ReadAllText(vmPath);
        Assert.True(content.Contains("lockResult.IsSuccess"),
            "LockWorkstationAsync must check lockResult.IsSuccess");
        Assert.False(content.Contains("await _mediator.Send(new LockWorkstationCommand());\n        await LoadStatusAsync();"),
            "LockWorkstationAsync must not discard the LockWorkstationCommand result");
    }

    [Fact]
    public void NavigationItems_AreFilteredByPermission()
    {
        // S-06 VG-06: BuildNavigationItems must use permission-based IsEnabled
        var srcDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "TopLab.Presentation");
        var vmPath = Path.Combine(srcDir, "ViewModels", "Shell", "ShellViewModel.cs");
        Assert.True(File.Exists(vmPath), "ShellViewModel.cs not found");

        var content = File.ReadAllText(vmPath);

        // Must NOT contain a literal IsEnabled = true in BuildNavigationItems
        var buildNavStart = content.IndexOf("BuildNavigationItems", StringComparison.Ordinal);
        Assert.True(buildNavStart > 0, "BuildNavigationItems not found");

        var buildNavSection = content.Substring(buildNavStart);
        Assert.False(buildNavSection.Contains("IsEnabled = true,"), "BuildNavigationItems must not contain a literal IsEnabled = true");

        // Must contain the four mapped permission codes
        Assert.Contains("PRINT_WORKSHEET", content);
        Assert.Contains("STATISTICS", content);
        Assert.Contains("PT_AUDIT_ACCESS", content);
        Assert.Contains("EDIT_SYSTEM_SETTINGS", content);
    }

    [Fact]
    public void LoginPath_Requests_HaveNoNewGuards()
    {
        // SD-7: SignInCommand, SignOutCommand, GetCurrentSessionQuery, VerifySecondaryPasswordQuery
        // must not implement IAuthorizedRequest
        var appDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "TopLab.Application");

        foreach (var name in new[] { "SignInCommand", "SignOutCommand", "GetCurrentSessionQuery", "VerifySecondaryPasswordQuery" })
        {
            var files = Directory.GetFiles(appDir, $"{name}.cs", SearchOption.AllDirectories);
            Assert.True(files.Length > 0, $"{name}.cs not found");

            var content = File.ReadAllText(files[0]);
            Assert.False(content.Contains("IAuthorizedRequest"),
                $"{name} must not implement IAuthorizedRequest");
        }
    }
}
