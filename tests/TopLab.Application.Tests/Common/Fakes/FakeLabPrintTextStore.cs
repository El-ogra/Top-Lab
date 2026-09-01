using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Tests.Common.Fakes;

public sealed class FakeLabPrintTextStore : ILabPrintTextStore
{
    public Dictionary<LabPrintTextScope, LabPrintTextDto> Store { get; } = new();

    public int SaveCount { get; private set; }

    public Task<Result<LabPrintTextDto>> GetAsync(LabPrintTextScope scope, CancellationToken cancellationToken = default)
    {
        if (Store.TryGetValue(scope, out var content))
        {
            return Task.FromResult(Result<LabPrintTextDto>.Success(content));
        }

        var defaults = new LabPrintTextDto(string.Empty, string.Empty, string.Empty, string.Empty, 0);
        return Task.FromResult(Result<LabPrintTextDto>.Success(defaults));
    }

    public Task<Result> SaveAsync(LabPrintTextScope scope, LabPrintTextDto content, CancellationToken cancellationToken = default)
    {
        Store[scope] = content;
        SaveCount++;
        return Task.FromResult(Result.Success());
    }
}