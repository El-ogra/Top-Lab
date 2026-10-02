using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientSearch.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;

namespace TopLab.Application.Features.PatientSearch.Queries.SearchPatientsGlobal;

/// <summary>
/// P-01 F4 (age) band. Carries the <see cref="AgeUnit"/> explicitly so the handler
/// compares like with like and performs no unit conversion (AS-5, BR-04).
/// A null <see cref="From"/> or <see cref="To"/> leaves that bound open; the unit
/// equality always applies, so supplying a band always narrows.
/// </summary>
public sealed record AgeValueBand(AgeUnit Unit, int? From = null, int? To = null);

/// <summary>
/// Global patient search. The positional shape <c>Text, Page, PageSize</c> is part of the
/// public surface and must not be reordered; every P-01 filter is appended at the tail with
/// a default (C-1, SD-5, SD-7).
/// <para>
/// <see cref="PageSize"/> is the display cap and its default of 50 is unchanged (SD-7).
/// </para>
/// </summary>
public sealed record SearchPatientsGlobalQuery(
    string? Text,
    int Page = 1,
    int PageSize = 50,
    // --- P-01 filters, appended at the tail, all defaulted and all inert when null ---
    // F1 — two separate parameters over two separate columns (SD-6). Never collapse these.
    ExternalEntityId? TreatingDoctorId = null,
    ExternalEntityId? ReferralEntityId = null,
    // F2
    int? TestId = null,
    // F3
    Sex? Sex = null,
    // F4
    AgeValueBand? Age = null,
    // F5 — one function, a From/To pair
    DateOnly? From = null,
    DateOnly? To = null) : IRequest<Result<IReadOnlyList<PatientSearchHitDto>>>;