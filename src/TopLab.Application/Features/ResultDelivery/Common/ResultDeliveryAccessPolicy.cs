namespace TopLab.Application.Features.ResultDelivery.Common;

/// <summary>
/// Single operational permission gate for the delivery-handover surface (OD-09-A).
/// Reuses the seeded <c>DELIVER_RESULTS</c> code (id=6); no catalog change.
/// Mirrors <c>ResultsEntryAccessPolicy.DeliverResults</c> without cross-feature coupling.
/// </summary>
public static class ResultDeliveryAccessPolicy
{
    public const string DeliverResults = "DELIVER_RESULTS";
}
