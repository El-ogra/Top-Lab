using TopLab.Domain.Common;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Tests;

/// <summary>
/// Many-to-many link between a <see cref="Profile"/> and an <see cref="Analyte"/>. A
/// profile may contain many analytes and an analyte may belong to many profiles. The
/// (ProfileId, AnalyteId) pair is unique.
/// </summary>
public sealed class ProfileAnalyte : Entity<ProfileAnalyteId>
{
    public ProfileId ProfileId { get; private set; } = default!;

    public AnalyteId AnalyteId { get; private set; } = default!;

    private ProfileAnalyte()
    {
    }

    private ProfileAnalyte(ProfileAnalyteId id, ProfileId profileId, AnalyteId analyteId)
        : base(id)
    {
        ProfileId = profileId;
        AnalyteId = analyteId;
    }

    public static ProfileAnalyte Create(ProfileAnalyteId id, ProfileId profileId, AnalyteId analyteId)
    {
        ArgumentNullException.ThrowIfNull(profileId);
        ArgumentNullException.ThrowIfNull(analyteId);

        return new ProfileAnalyte(id, profileId, analyteId);
    }
}