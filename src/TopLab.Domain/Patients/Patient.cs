using TopLab.Domain.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Patients;

/// <summary>
/// One row per visit (registration). LabId is shared grouping value across visits.
/// Auditable per §4.2.
/// </summary>
public sealed class Patient : AuditableEntity<PatientId>
{
    public string? LabId { get; private set; }

    public string? Title { get; private set; }

    public string FullName { get; private set; } = default!;

    public Sex Sex { get; private set; }

    public int AgeValue { get; private set; }

    public AgeUnit AgeUnit { get; private set; }

    public string? NationalId { get; private set; }

    public string? Address { get; private set; }

    public ExternalEntityId? TreatingDoctorId { get; private set; }

    public ExternalEntityId? ReferralEntityId { get; private set; }

    public AccountType AccountType { get; private set; }

    public bool IsVip { get; private set; }

    public DateTime RegistrationDateUtc { get; private set; }

    public DateTime? PickupDateUtc { get; private set; }

    public bool IsFastingIndicated { get; private set; }

    public int? FastingHours { get; private set; }

    public bool RecentContrastImaging { get; private set; }

    public string? Notes { get; private set; }

    private readonly List<PatientPhoneNumber> _phoneNumbers = [];
    public IReadOnlyCollection<PatientPhoneNumber> PhoneNumbers => _phoneNumbers.AsReadOnly();

    private readonly List<PatientMedicalCondition> _medicalConditions = [];
    public IReadOnlyCollection<PatientMedicalCondition> MedicalConditions => _medicalConditions.AsReadOnly();

    private Patient()
    {
    }

    private Patient(
        PatientId id,
        string fullName,
        Sex sex,
        int ageValue,
        AgeUnit ageUnit,
        DateTime registrationDateUtc,
        AccountType accountType,
        bool isVip,
        string? labId,
        string? title,
        string? nationalId,
        string? address,
        ExternalEntityId? treatingDoctorId,
        ExternalEntityId? referralEntityId,
        DateTime? pickupDateUtc,
        bool isFastingIndicated,
        int? fastingHours,
        bool recentContrastImaging,
        string? notes)
        : base(id)
    {
        FullName = fullName;
        Sex = sex;
        AgeValue = ageValue;
        AgeUnit = ageUnit;
        RegistrationDateUtc = registrationDateUtc;
        AccountType = accountType;
        IsVip = isVip;
        LabId = labId;
        Title = title;
        NationalId = nationalId;
        Address = address;
        TreatingDoctorId = treatingDoctorId;
        ReferralEntityId = referralEntityId;
        PickupDateUtc = pickupDateUtc;
        IsFastingIndicated = isFastingIndicated;
        FastingHours = fastingHours;
        RecentContrastImaging = recentContrastImaging;
        Notes = notes;
    }

    public static Patient Create(
        PatientId id,
        string fullName,
        Sex sex,
        int ageValue,
        AgeUnit ageUnit,
        DateTime registrationDateUtc,
        AccountType accountType = AccountType.Individual,
        bool isVip = false,
        string? labId = null,
        string? title = null,
        string? nationalId = null,
        string? address = null,
        ExternalEntityId? treatingDoctorId = null,
        ExternalEntityId? referralEntityId = null,
        DateTime? pickupDateUtc = null,
        bool isFastingIndicated = false,
        int? fastingHours = null,
        bool recentContrastImaging = false,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("FullName is required.", nameof(fullName));
        }

        if (ageValue < 0)
        {
            throw new ArgumentException("AgeValue must be >= 0.", nameof(ageValue));
        }

        if (!isFastingIndicated && fastingHours is not null)
        {
            throw new ArgumentException("FastingHours requires IsFastingIndicated.", nameof(fastingHours));
        }

        return new Patient(id, fullName.Trim(), sex, ageValue, ageUnit, registrationDateUtc, accountType, isVip,
            labId, title, nationalId, address, treatingDoctorId, referralEntityId, pickupDateUtc,
            isFastingIndicated, fastingHours, recentContrastImaging, notes);
    }

    public void Update(
        string fullName,
        Sex sex,
        int ageValue,
        AgeUnit ageUnit,
        string? nationalId,
        string? address,
        string? title,
        bool isVip,
        AccountType accountType,
        string? notes,
        bool isFastingIndicated,
        int? fastingHours,
        bool recentContrastImaging)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("FullName is required.", nameof(fullName));
        }

        if (ageValue < 0)
        {
            throw new ArgumentException("AgeValue must be >= 0.", nameof(ageValue));
        }

        if (!isFastingIndicated && fastingHours is not null)
        {
            throw new ArgumentException("FastingHours requires IsFastingIndicated.", nameof(fastingHours));
        }

        FullName = fullName.Trim();
        Sex = sex;
        AgeValue = ageValue;
        AgeUnit = ageUnit;
        NationalId = nationalId;
        Address = address;
        Title = title;
        IsVip = isVip;
        AccountType = accountType;
        Notes = notes;
        IsFastingIndicated = isFastingIndicated;
        FastingHours = fastingHours;
        RecentContrastImaging = recentContrastImaging;
    }

    public void AssignLabId(string labId)
    {
        if (string.IsNullOrWhiteSpace(labId))
        {
            throw new ArgumentException("LabId is required.", nameof(labId));
        }

        LabId = labId.Trim();
    }

    public void SetTreatingDoctor(ExternalEntityId? treatingDoctorId)
    {
        TreatingDoctorId = treatingDoctorId;
    }

    public void SetReferralEntity(ExternalEntityId? referralEntityId)
    {
        ReferralEntityId = referralEntityId;
    }
}
