namespace TopLab.Domain.Attendance;

/// <summary>
/// Pure static Domain service implementing the settled attendance time math
/// (SD-18-4/SD-18-7): lateness at check-in vs the user's configured
/// <c>WorkStartTime</c>, overtime at check-out vs <c>WorkEndTime</c>, and
/// worked minutes excluding the recorded break span.
/// Local wall-clock conversion via <c>TimeOnly.FromDateTime(atUtc.ToLocalTime())</c>
/// — single-site LAN product. Missing configuration yields null (never an error).
/// Single source of the formula — never restated in Application (M-16 D39 precedent).
/// </summary>
public static class AttendanceCalculator
{
    public static int? LatenessMinutes(TimeOnly? workStart, DateTime checkInUtc)
    {
        if (workStart is null)
        {
            return null;
        }

        var local = TimeOnly.FromDateTime(checkInUtc.ToLocalTime());
        var diffMinutes = (local.ToTimeSpan() - workStart.Value.ToTimeSpan()).TotalMinutes;
        return diffMinutes <= 0 ? 0 : (int)diffMinutes;
    }

    public static int? OvertimeMinutes(TimeOnly? workEnd, DateTime checkOutUtc)
    {
        if (workEnd is null)
        {
            return null;
        }

        var local = TimeOnly.FromDateTime(checkOutUtc.ToLocalTime());
        var diffMinutes = (local.ToTimeSpan() - workEnd.Value.ToTimeSpan()).TotalMinutes;
        return diffMinutes <= 0 ? 0 : (int)diffMinutes;
    }

    public static int WorkedMinutes(
        DateTime checkInUtc,
        DateTime checkOutUtc,
        DateTime? breakStartUtc,
        DateTime? breakEndUtc)
    {
        var totalMinutes = (checkOutUtc - checkInUtc).TotalMinutes;

        double breakMinutes = 0;
        if (breakStartUtc is not null && breakEndUtc is not null)
        {
            breakMinutes = (breakEndUtc.Value - breakStartUtc.Value).TotalMinutes;
        }

        var worked = totalMinutes - breakMinutes;
        return worked <= 0 ? 0 : (int)worked;
    }
}
