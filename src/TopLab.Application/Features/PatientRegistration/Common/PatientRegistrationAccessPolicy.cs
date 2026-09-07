namespace TopLab.Application.Features.PatientRegistration.Common;

/// <summary>
/// Permission codes consumed by M02 write commands. Both are already seeded
/// in the 13-row permission catalog (settled OD-8); M02 reuses them rather than
/// adding new rows.
/// </summary>
public static class PatientRegistrationAccessPolicy
{
    public const string AddEditPatient = "ADD_EDIT_PATIENT";

    public const string DeletePatient = "DELETE_PATIENT";
}