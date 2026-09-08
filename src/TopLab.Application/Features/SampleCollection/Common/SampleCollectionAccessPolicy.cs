namespace TopLab.Application.Features.SampleCollection.Common;

/// <summary>
/// Permission codes consumed by M21 write commands. The single code is already
/// seeded in the 13-row permission catalog (settled OD-8); M21 reuses the M02
/// code rather than adding a new row (recorded in ADR-0033).
/// </summary>
public static class SampleCollectionAccessPolicy
{
    public const string AddEditPatient = "ADD_EDIT_PATIENT";
}
