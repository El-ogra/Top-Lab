using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.ExternalEntities.Common;

public static class ReferralNameResolver
{
    public static string Resolve(string? referralName, Sex patientSex)
    {
        if (!string.IsNullOrWhiteSpace(referralName))
        {
            return referralName.Trim();
        }

        return patientSex == Sex.Female ? "Herself" : "Himself";
    }
}
