using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;

namespace TopLab.Application.Features.PatientBilling.Queries.GetPatientInvoice;

/// <summary>
/// Builds the invoice view for a patient: live charges plus the latest
/// numbered issue, or a preview (null number/date) when no issue exists yet.
/// </summary>
public sealed record GetPatientInvoiceQuery(
    int PatientId) : IRequest<Result<InvoiceDto>>;
