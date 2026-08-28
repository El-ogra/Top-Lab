namespace TopLab.Domain.Common.Enums;

/// <summary>Patient account type (tinyint). Default from SystemSettings.</summary>
public enum AccountType
{
    Individual = 0,
    LabToLab = 1,
    Contracts = 2,
    Vip = 3,
    Free = 4
}
