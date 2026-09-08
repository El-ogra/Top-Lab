using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Domain.Billing;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;

/// <summary>
/// Shared read logic for the patient-billing queries. Loads the patient's
/// ordered tests (joined to the catalog for the receipt-facing names), all of
/// the patient's payment operations including voided rows (history must show
/// them, flagged), and resolves the receiving-user display names.
/// </summary>
internal static class PatientBillingReader
{
    internal static PatientAccountDto ReadAccount(IApplicationDbContext db, Patient patient)
    {
        var chargedTests = ReadChargedTests(db, patient.Id.Value);
        var operations = ReadOperations(db, patient.Id.Value);

        var prices = chargedTests.Select(t => t.PriceAtOrderTime).ToList();
        var entities = db.Set<PaymentOperation>()
            .Where(o => o.PatientId.Value == patient.Id.Value)
            .ToList();

        var totalCharged = PatientAccountCalculator.TotalCharged(prices, entities);
        var totalPaid = PatientAccountCalculator.TotalPaid(entities);
        var totalDiscount = entities
            .Where(o => !o.IsVoided && !o.IsExtraCharge)
            .Sum(o => o.DiscountAmount ?? 0m);

        return new PatientAccountDto(
            patient.Id.Value,
            patient.FullName,
            patient.LabId == null ? null : patient.LabId.Value,
            patient.AccountType.ToString(),
            totalCharged,
            totalPaid,
            totalDiscount,
            totalCharged - totalPaid,
            chargedTests,
            operations);
    }

    internal static IReadOnlyList<ChargedTestDto> ReadChargedTests(IApplicationDbContext db, int patientId)
    {
        var catalog = db.Set<Test>().ToDictionary(t => t.Id.Value);

        return db.Set<PatientTest>()
            .Where(pt => pt.PatientId.Value == patientId)
            .OrderBy(pt => pt.Id.Value)
            .ToList()
            .Select(pt => new ChargedTestDto(
                pt.Id.Value,
                catalog.TryGetValue(pt.TestId.Value, out var test) ? test.Name : string.Empty,
                catalog.TryGetValue(pt.TestId.Value, out var byCode) ? byCode.TestCode : string.Empty,
                catalog.TryGetValue(pt.TestId.Value, out var byReceipt) ? byReceipt.ReceiptName : string.Empty,
                pt.PriceAtOrderTime))
            .ToList();
    }

    internal static IReadOnlyList<PaymentOperationDto> ReadOperations(IApplicationDbContext db, int patientId)
    {
        var userNames = db.Set<User>().ToDictionary(u => u.Id.Value, u => u.UserName);

        return db.Set<PaymentOperation>()
            .Where(o => o.PatientId.Value == patientId)
            .OrderByDescending(o => o.OperationAtUtc)
            .ThenByDescending(o => o.Id.Value)
            .ToList()
            .Select(o => new PaymentOperationDto(
                o.Id.Value,
                o.Amount,
                o.DiscountAmount,
                o.IsExtraCharge,
                // Deliberately the enum name string (cashier-readable display output),
                // not the int precedent used by TestDetailDto.ResultKind.
                o.OperationType.ToString(),
                o.ReceivedByUserId,
                // Deleted-user fallback (stated rule): echo the raw id when the user row is gone.
                userNames.TryGetValue(o.ReceivedByUserId, out var name) ? name : o.ReceivedByUserId.ToString(),
                o.OperationAtUtc,
                o.IsVoided))
            .ToList();
    }
}
