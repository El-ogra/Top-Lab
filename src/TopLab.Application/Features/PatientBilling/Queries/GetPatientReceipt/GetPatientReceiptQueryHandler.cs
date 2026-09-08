using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Domain.Patients;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.PatientBilling.Queries.GetPatientReceipt;

public sealed class GetPatientReceiptQueryHandler
    : IRequestHandler<GetPatientReceiptQuery, Result<ReceiptDto>>
{
    private readonly IApplicationDbContext _db;

    public GetPatientReceiptQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<ReceiptDto>> Handle(
        GetPatientReceiptQuery request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Task.FromResult(Result<ReceiptDto>.Failure(Error.NotFound("المريض غير موجود.")));
        }

        var settings = _db.Set<ReceiptSettings>().SingleOrDefault(s => s.Id == 1);
        if (settings is null)
        {
            return Task.FromResult(Result<ReceiptDto>.Failure(Error.Unexpected("سجل إعدادات الإيصال مفقود.")));
        }

        var account = PatientBillingReader.ReadAccount(_db, patient);

        var receipt = new ReceiptDto(
            account.PatientId,
            account.PatientFullName,
            account.LabId,
            account.ChargedTests,
            account.TotalCharged,
            account.TotalDiscount,
            account.TotalPaid,
            account.Balance,
            settings.Currency);

        return Task.FromResult(Result<ReceiptDto>.Success(receipt));
    }
}
