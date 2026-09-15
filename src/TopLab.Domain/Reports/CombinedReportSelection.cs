namespace TopLab.Domain.Reports;

/// <summary>
/// Ephemeral, in-memory ordered selection of patient-test lines for a combined report.
/// Never persisted; constructed and consumed within a single Application handler call.
/// </summary>
public sealed class CombinedReportSelection
{
    private readonly List<int> _orderedIds = new();

    /// <summary>Projection of the selected patient-test ids in their current 1..n order.</summary>
    public IReadOnlyList<int> OrderedIds => _orderedIds.AsReadOnly();

    public int Count => _orderedIds.Count;

    public void Add(int patientTestId, bool isReviewed)
    {
        if (!isReviewed)
        {
            throw new ArgumentException("Cannot add a non-reviewed result to the report.", nameof(isReviewed));
        }

        if (_orderedIds.Contains(patientTestId))
        {
            throw new ArgumentException("Cannot add the same test twice to the report.", nameof(patientTestId));
        }

        _orderedIds.Add(patientTestId);
    }

    public void MoveUp(int patientTestId)
    {
        int index = _orderedIds.IndexOf(patientTestId);
        if (index < 0)
        {
            throw new ArgumentException("The patient-test is not part of the selection.", nameof(patientTestId));
        }

        if (index == 0)
        {
            return;
        }

        (_orderedIds[index - 1], _orderedIds[index]) = (_orderedIds[index], _orderedIds[index - 1]);
    }

    public void MoveDown(int patientTestId)
    {
        int index = _orderedIds.IndexOf(patientTestId);
        if (index < 0)
        {
            throw new ArgumentException("The patient-test is not part of the selection.", nameof(patientTestId));
        }

        if (index == _orderedIds.Count - 1)
        {
            return;
        }

        (_orderedIds[index + 1], _orderedIds[index]) = (_orderedIds[index], _orderedIds[index + 1]);
    }
}