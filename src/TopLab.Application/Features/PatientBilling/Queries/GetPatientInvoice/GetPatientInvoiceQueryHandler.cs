using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Domain.Billing;
using TopLab.Domain.Patients;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.PatientBilling.Queries.GetPatientInvoice;

public sealed class GetPatientInvoiceQueryHandler : IRequestHandler<GetPatientInvoiceQuery, Result<InvoiceDto>>
{
    private readonly IApplicationDbContext _db;

    public GetPatientInvoiceQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<InvoiceDto>> Handle(GetPatientInvoiceQuery request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Task.FromResult(Result<InvoiceDto>.Failure(Error.NotFound("المريض غير موجود.")));
        }

        var account = PatientBillingReader.ReadAccount(_db, patient);

        var settings = _db.Set<ReceiptSettings>().SingleOrDefault(s => s.Id == 1);
        if (settings is null)
        {
            return Task.FromResult(Result<InvoiceDto>.Failure(Error.Unexpected("سجل إعدادات الإيصال مفقود.")));
        }

        var latest = _db.Set<InvoiceIssue>()
            .Where(i => i.PatientId.Value == request.PatientId)
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefault();

        var invoice = new InvoiceDto(
            account.PatientId,
            account.PatientFullName,
            account.LabId,
            latest?.InvoiceNumber,
            latest?.IssuedAtUtc,
            account.ChargedTests,
            account.TotalCharged,
            account.TotalDiscount,
            account.TotalPaid,
            account.Balance,
            settings.Currency);

        return Task.FromResult(Result<InvoiceDto>.Success(invoice));
    }
}
