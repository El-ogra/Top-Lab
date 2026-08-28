using TopLab.Domain.Common;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Patients;

public sealed class PatientPhoneNumber : Entity<PatientPhoneNumberId>
{
    public PatientId PatientId { get; private set; } = default!;

    public string PhoneNumber { get; private set; } = default!;

    public byte SortOrder { get; private set; }

    private PatientPhoneNumber()
    {
    }

    private PatientPhoneNumber(PatientPhoneNumberId id, PatientId patientId, string phoneNumber, byte sortOrder)
        : base(id)
    {
        PatientId = patientId;
        PhoneNumber = phoneNumber;
        SortOrder = sortOrder;
    }

    public static PatientPhoneNumber Create(PatientPhoneNumberId id, PatientId patientId, string phoneNumber, byte sortOrder)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new ArgumentException("PhoneNumber is required.", nameof(phoneNumber));
        }

        return new PatientPhoneNumber(id, patientId, phoneNumber.Trim(), sortOrder);
    }
}
