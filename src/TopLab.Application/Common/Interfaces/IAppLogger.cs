namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Minimal structured logger used by <c>LoggingBehavior</c>. Implemented in Infrastructure;
/// tests supply a fake that records calls.
/// </summary>
public interface IAppLogger
{
    void Log(string requestName, string outcome, TimeSpan duration);
}
