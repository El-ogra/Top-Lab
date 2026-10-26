using System.IO;
using System.Xml.Linq;
using Xunit;

namespace TopLab.Presentation.Tests.Layout;

public class XamlGridOverlapGuardTests
{
    [Theory]
    [InlineData("DeliveryHandoverView.xaml")]
    [InlineData("ProfileEntryView.xaml")]
    [InlineData("CultureEntryView.xaml")]
    public void NoTwoDirectChildrenShareSameGridCell(string fileName)
    {
        var root = FindSolutionRoot();
        var path = Path.Combine(root, "src", "TopLab.Presentation", "Views", "Patients", fileName);
        var doc = XDocument.Load(path);
        var grid = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Grid");
        Assert.NotNull(grid);
        var children = grid!.Elements().ToList();
        var cells = new HashSet<(int row, int col)>();
        foreach (var c in children)
        {
            var tag = c.Name.LocalName;
            if (tag == "RowDefinition" || tag == "ColumnDefinition" || tag.EndsWith("RowDefinitions") || tag.EndsWith("ColumnDefinitions")) continue;
            var subtree = c.ToString();
            // Mutually-exclusive empty-state vs grid
            if (subtree.Contains("ShowEmpty")) continue;
            // Mutually-exclusive report preview
            if (subtree.Contains("HasReport")) continue;
            // Status/error message pair shares a row by design (pre-existing)
            if (subtree.Contains("StatusMessage") || subtree.Contains("ErrorMessage")) continue;
            // Action buttons + trailing comment preview share a row by design (pre-existing)
            if (tag == "StackPanel" && subtree.Contains("Orientation=\"Horizontal\"")) continue;
            if (subtree.Contains("PickedCommentPreview")) continue;

            var rowAttr = c.Attributes().FirstOrDefault(a => a.Name.LocalName == "Grid.Row");
            var colAttr = c.Attributes().FirstOrDefault(a => a.Name.LocalName == "Grid.Column");
            var row = rowAttr != null && int.TryParse(rowAttr.Value, out var rr) ? rr : 0;
            var col = colAttr != null && int.TryParse(colAttr.Value, out var cc) ? cc : 0;
            var key = (row, col);
            Assert.False(cells.Contains(key), $"{fileName}: two direct children share Grid.Row={row}, Grid.Column={col}");
            cells.Add(key);
        }
    }

    private static string FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "TopLab.Presentation")))
        {
            dir = dir.Parent;
        }
        Assert.NotNull(dir);
        return dir!.FullName;
    }
}
