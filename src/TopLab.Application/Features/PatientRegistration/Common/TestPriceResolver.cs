using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;

namespace TopLab.Application.Features.PatientRegistration.Common;

/// <summary>
/// Pure pricing resolver shared by <c>AddTestsToVisitCommandHandler</c> and
/// <c>AddCustomGroupToVisitCommandHandler</c>. The algorithm is the single source
/// of truth for M-02 pricing; both commands delegate to it.
/// </summary>
internal static class TestPriceResolver
{
    /// <summary>
    /// Resolves the per-test price at registration time.
    /// Branches (in order):
    /// <list type="number">
    ///   <item>If a referral entity with a <c>PriceListId</c> exists and the price list
    ///         contains the requested test — use the price-list price.</item>
    ///   <item>If the patient's account type is <c>LabToLab</c> — use
    ///         <c>test.LabToLabPrice</c> when set, otherwise fall back to
    ///         <c>test.PatientPrice</c>.</item>
    ///   <item>Otherwise — use <c>test.PatientPrice</c> (cash / VIP / Free — Free is
    ///         overridden to zero at billing time).</item>
    /// </list>
    /// Caller is responsible for verifying that the price list contains the test
    /// when a referral entity supplies a price list; this resolver simply applies
    /// the price once the existence check has passed.
    /// </summary>
    /// <param name="accountType">Patient account type.</param>
    /// <param name="testPatientPrice">The test's <c>PatientPrice</c>.</param>
    /// <param name="testLabToLabPrice">The test's optional <c>LabToLabPrice</c>.</param>
    /// <param name="referralEntity">Optional referral/contract entity.</param>
    /// <param name="priceListItems">Map of test id → price for the active price list
    ///   (empty when no contract price list is in use).</param>
    /// <param name="customGroupItemPrices">Fallback map of test id → price for a
    ///   custom group (used only by <c>AddCustomGroupToVisit</c> when no contract
    ///   price list is present).</param>
    /// <param name="testId">The id of the test being priced.</param>
    public static decimal Resolve(
        AccountType accountType,
        decimal testPatientPrice,
        decimal? testLabToLabPrice,
        ExternalEntity? referralEntity,
        IReadOnlyDictionary<TestId, decimal> priceListItems,
        IReadOnlyDictionary<TestId, decimal> customGroupItemPrices,
        TestId testId)
    {
        if (referralEntity is not null
            && referralEntity.PriceListId is not null
            && priceListItems.TryGetValue(testId, out var contractPrice))
        {
            return contractPrice;
        }

        if (accountType == AccountType.LabToLab && testLabToLabPrice.HasValue)
        {
            return testLabToLabPrice.Value;
        }

        if (customGroupItemPrices.TryGetValue(testId, out var groupPrice))
        {
            return groupPrice;
        }

        return testPatientPrice;
    }
}