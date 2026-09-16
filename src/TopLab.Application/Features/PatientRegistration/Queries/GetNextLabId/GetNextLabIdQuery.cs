using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.PatientRegistration.Queries.GetNextLabId;

/// <summary>
/// Computes the next LabId suggestion for the registration desk (S-01 SD-4):
/// MAX(numeric LabId)+1, zero-padded to the longest numeric width; "1" when no
/// numeric LabId exists. Non-numeric LabIds are skipped deterministically.
/// Ungated read, like the registration catalog. The desk may override the
/// suggestion; the backend rejects duplicates on save.
/// </summary>
public sealed record GetNextLabIdQuery : IRequest<Result<string>>;
