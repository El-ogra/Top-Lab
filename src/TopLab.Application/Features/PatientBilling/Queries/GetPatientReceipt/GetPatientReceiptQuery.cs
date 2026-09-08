using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;

namespace TopLab.Application.Features.PatientBilling.Queries.GetPatientReceipt;

public sealed record GetPatientReceiptQuery(
    int PatientId) : IRequest<Result<ReceiptDto>>;
