namespace TopLab.Application.Features.AccessAndNavigation.Common;

/// <summary>
/// Permission codes referenced by <c>AccessAndNavigation</c> features. Mirrors
/// the implicit "magic string" pattern that M-12/M-14/M-22 use elsewhere in
/// the codebase. Centralising them here keeps the gate strings out of command
/// files and gives them a single place to change if the catalog evolves.
/// </summary>
public static class AccessAndNavigationAccessPolicy
{
    public const string EditSystemSettings = "EDIT_SYSTEM_SETTINGS";
}