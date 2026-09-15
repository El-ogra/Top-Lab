namespace TopLab.Application.Features.ResultDelivery.Common;

/// <summary>
/// Owner-settled period semantics for the undelivered-results list (M-11 precedent):
/// both bounds default to the current day in UTC when unspecified; <c>To ??= From</c>;
/// bounds are inclusive on UTC calendar days.
/// </summary>
internal static class ResultDeliveryPeriod
{
    public static (DateOnly From, DateOnly To) Resolve(DateOnly? from, DateOnly? to)
    {
        var resolvedFrom = from ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var resolvedTo = to ?? resolvedFrom;
        return (resolvedFrom, resolvedTo);
    }

    public static bool IsValid(DateOnly? from, DateOnly? to)
    {
        var (resolvedFrom, resolvedTo) = Resolve(from, to);
        return resolvedFrom <= resolvedTo;
    }
}
