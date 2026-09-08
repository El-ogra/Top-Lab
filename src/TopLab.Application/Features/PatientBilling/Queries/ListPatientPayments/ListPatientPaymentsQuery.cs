using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;

namespace TopLab.Application.Features.PatientBilling.Queries.ListPatientPayments;

public sealed record ListPatientPaymentsQuery(
    int PatientId,
    int Page,
    int PageSize) : IRequest<Result<IReadOnlyList<PaymentOperationDto>>>;
