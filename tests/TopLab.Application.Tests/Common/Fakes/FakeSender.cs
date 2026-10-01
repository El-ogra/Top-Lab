using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.CultureResults.Common;
using TopLab.Application.Features.CultureResults.Queries.GetCultureReport;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroupById;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetPriceListById;
using TopLab.Application.Features.ProfileResults.Common;
using TopLab.Application.Features.ProfileResults.Queries.GetProfileReport;
using TopLab.Application.Features.ReportProduction.Common;

namespace TopLab.Application.Tests.Common.Fakes;

/// <summary>
/// Hand-rolled <see cref="ISender"/> shim that returns canned M-13 query results.
/// Used by M-02 handler tests to exercise the M-02 → M-13 MediatR integration
/// without standing up a real <c>IApplicationDbContext</c> for M-13 read paths.
/// </summary>
public sealed class FakeSender : ISender
{
    private readonly Dictionary<int, Result<PriceListDetailDto>> _priceLists = new();
    private readonly Dictionary<int, Result<CustomGroupDetailDto>> _customGroups = new();
    private readonly Dictionary<int, Result<CultureReportDto>> _cultureReports = new();
    private readonly Dictionary<int, Result<ProfileReportDto>> _profileReports = new();
    private readonly Dictionary<Type, object> _otherResponses = new();

    public FakeSender WithPriceList(int id, PriceListDetailDto dto)
    {
        _priceLists[id] = Result<PriceListDetailDto>.Success(dto);
        return this;
    }

    public FakeSender WithPriceListNotFound(int id)
    {
        _priceLists[id] = Result<PriceListDetailDto>.Failure(Error.NotFound("قائمة الأسعار غير موجودة."));
        return this;
    }

    public FakeSender WithCustomGroup(int id, CustomGroupDetailDto dto)
    {
        _customGroups[id] = Result<CustomGroupDetailDto>.Success(dto);
        return this;
    }

    public FakeSender WithCustomGroupNotFound(int id)
    {
        _customGroups[id] = Result<CustomGroupDetailDto>.Failure(Error.NotFound("المجموعة غير موجودة."));
        return this;
    }

    public FakeSender WithResponse<TResponse>(IRequest<TResponse> request, TResponse response)
    {
        _otherResponses[request.GetType()] = response!;
        return this;
    }

    // ---- W-02 S4: helpers for the print coordinator ----

    /// <summary>Answers <see cref="GetCultureReportQuery"/> so the coordinator can resolve a patient id.</summary>
    public FakeSender WithCultureReport(int patientTestId, int patientId)
    {
        _cultureReports[patientTestId] = Result<CultureReportDto>.Success(BuildCultureReport(patientTestId, patientId));
        return this;
    }

    public FakeSender WithCultureFailure(int patientTestId)
    {
        _cultureReports[patientTestId] = Result<CultureReportDto>.Failure(Error.NotFound("التحليل غير موجود"));
        return this;
    }

    public FakeSender WithProfileReport(int patientTestId, int patientId)
    {
        _profileReports[patientTestId] = Result<ProfileReportDto>.Success(BuildProfileReport(patientTestId, patientId));
        return this;
    }

    public FakeSender WithProfileFailure(int patientTestId)
    {
        _profileReports[patientTestId] = Result<ProfileReportDto>.Failure(Error.NotFound("التحليل غير موجود"));
        return this;
    }

    // ---- W-02 S4: the build commands the coordinator sends ----

    public FakeSender WithCombinedReport(int patientId, int patientTestId, CombinedReportDto dto)
    {
        _otherResponses[typeof(TopLab.Application.Features.ReportProduction.Commands.BuildCombinedReport.BuildCombinedReportCommand)] =
            Result<CombinedReportDto>.Success(dto);
        return this;
    }

    public FakeSender WithBlankReport(BlankReportDto dto)
    {
        _otherResponses[typeof(TopLab.Application.Features.ReportProduction.Commands.BuildBlankReport.BuildBlankReportCommand)] =
            Result<BlankReportDto>.Success(dto);
        return this;
    }

    private static CultureReportDto BuildCultureReport(int patientTestId, int patientId) => new(
        patientTestId, patientId, "Patient", null, "Male", 30, "Year", "Culture", "Culture report",
        DateTime.UtcNow, null, null, null, null, null, null,
        [], false, false, false, 0);

    private static ProfileReportDto BuildProfileReport(int patientTestId, int patientId) => new(
        patientTestId, patientId, "Patient", null, "CBC", null, false, false, false, []);

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request is GetCultureReportQuery crq)
        {
            if (_cultureReports.TryGetValue(crq.PatientTestId, out var cr))
            {
                return Task.FromResult((TResponse)(object)cr);
            }
        }

        if (request is GetProfileReportQuery prq)
        {
            if (_profileReports.TryGetValue(prq.PatientTestId, out var pr))
            {
                return Task.FromResult((TResponse)(object)pr);
            }
        }

        if (request is GetPriceListByIdQuery plq)
        {
            if (_priceLists.TryGetValue(plq.Id, out var plr))
            {
                return Task.FromResult((TResponse)(object)plr);
            }
            var nf = Result<PriceListDetailDto>.Failure(Error.NotFound("قائمة الأسعار غير موجودة."));
            return Task.FromResult((TResponse)(object)nf);
        }

        if (request is GetCustomGroupByIdQuery cgq)
        {
            if (_customGroups.TryGetValue(cgq.Id, out var cgr))
            {
                return Task.FromResult((TResponse)(object)cgr);
            }
            var nf = Result<CustomGroupDetailDto>.Failure(Error.NotFound("المجموعة غير موجودة."));
            return Task.FromResult((TResponse)(object)nf);
        }

        if (_otherResponses.TryGetValue(request.GetType(), out var cached))
        {
            return Task.FromResult((TResponse)cached);
        }

        throw new NotSupportedException($"FakeSender has no canned response for {request.GetType().Name}.");
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("FakeSender does not support non-generic Send.");
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        throw new NotSupportedException("FakeSender does not support void Send.");
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("FakeSender does not support streams.");
    }

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("FakeSender does not support streams.");
    }
}