namespace TopLab.Domain.Common;

/// <summary>
/// WP-15 / SD-2: branch scope is System + User only (no Patient.BranchNumber).
/// </summary>
public sealed record BranchScope(int? Requested, int SystemDefault, int? UserAssigned)
{
    public int Resolve() => Requested ?? UserAssigned ?? SystemDefault;

    public bool IsAllBranches => Requested is null && UserAssigned is null;
}
