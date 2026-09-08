namespace TopLab.Application.Features.ResultsEntry.Commands.BulkPrint;

public static class BulkPrintMessages
{
    public const string ReprintConfirmationMessage =
        "لقد تم طباعه هذا التقرير لهذا المريض من قبل هل ترغب في اعاده الطباعه";
}

public sealed record BulkPrintPreflightDto(
    int PatientId,
    string PatientFullName,
    bool RequiresReprintConfirmation,
    int VerifiedCount,
    int TotalCount);

public sealed record BulkPrintDecision(int PatientId, bool ConfirmReprint);

public sealed record BulkPrintOutcomeDto(int PatientId, string Outcome, int PrintedCount);

public static class BulkPrintOutcomes
{
    public const string Printed = "Printed";
    public const string Skipped = "Skipped";
    public const string BlockedByBalance = "BlockedByBalance";
    public const string NoVerifiedResults = "NoVerifiedResults";
    public const string PatientNotFound = "PatientNotFound";
}
