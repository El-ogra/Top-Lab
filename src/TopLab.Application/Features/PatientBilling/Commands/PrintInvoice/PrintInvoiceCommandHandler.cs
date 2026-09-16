using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.PatientBilling.Commands.PrintInvoice;

public sealed class PrintInvoiceCommandHandler : IRequestHandler<PrintInvoiceCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;
    private readonly IInvoicePrintingService _printingService;

    public PrintInvoiceCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock,
        IInvoicePrintingService printingService)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _printingService = printingService;
    }

    public async Task<Result<int>> Handle(PrintInvoiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
            if (patient is null || patient.IsDeleted)
            {
                return Result<int>.Failure(Error.NotFound("المريض غير موجود."));
            }

            var account = PatientBillingReader.ReadAccount(_db, patient);

            var settings = _db.Set<ReceiptSettings>().SingleOrDefault(s => s.Id == 1);
            if (settings is null)
            {
                return Result<int>.Failure(Error.Unexpected("سجل إعدادات الإيصال مفقود."));
            }

            var issue = await AllocateIssueAsync(
                patient.Id,
                account.TotalCharged,
                account.TotalDiscount,
                account.ChargedTests.Count,
                cancellationToken);

            var invoice = new InvoiceDto(
                account.PatientId,
                account.PatientFullName,
                account.LabId,
                issue.InvoiceNumber,
                issue.IssuedAtUtc,
                account.ChargedTests,
                account.TotalCharged,
                account.TotalDiscount,
                account.TotalPaid,
                account.Balance,
                settings.Currency);

            var print = await _printingService.PrintInvoiceAsync(
                InvoicePrintEnvelope.CreateToken(invoice),
                cancellationToken);
            if (!print.IsSuccess)
            {
                return Result<int>.Failure(print.Error!);
            }

            return Result<int>.Success(issue.InvoiceNumber);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return Result<int>.Failure(Error.Unexpected("تعذر إصدار الفاتورة."));
        }
    }

    private async Task<InvoiceIssue> AllocateIssueAsync(
        PatientId patientId,
        decimal totalCharged,
        decimal totalDiscount,
        int itemCount,
        CancellationToken cancellationToken)
    {
        // Gapless numbering: single SELECT MAX inside the same unit of work as
        // the insert; the unique index is the backstop. Retry once on a lost
        // race, mirroring the M-12 residual unique-violation idiom (Application
        // has no EF Core reference, so detection is message-based).
        for (var attempt = 0; ; attempt++)
        {
            var number = (_db.Set<InvoiceIssue>().Select(i => (int?)i.InvoiceNumber).Max() ?? 0) + 1;
            var issue = InvoiceIssue.Create(
                InvoiceIssueId.Create(0),
                patientId,
                number,
                _clock.UtcNow,
                _currentUser.UserId,
                totalCharged,
                totalDiscount,
                itemCount);

            _db.Add(issue);
            try
            {
                await _db.SaveChangesAsync(cancellationToken);
                return issue;
            }
            catch (Exception ex) when (attempt == 0 && IsUniqueViolation(ex))
            {
                _db.Remove(issue);
            }
        }
    }

    private static bool IsUniqueViolation(Exception ex)
    {
        var msg = ex.Message;
        return msg.Contains("IX_InvoiceIssues_InvoiceNumber")
            || msg.Contains("duplicate")
            || msg.Contains("UNIQUE");
    }
}
