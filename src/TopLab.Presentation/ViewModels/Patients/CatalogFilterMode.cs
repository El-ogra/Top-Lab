namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// The three mutually exclusive modes of the registration screen's test-catalogue filter bar
/// (canonical reference 1.2.2.5.1.2, owner decision D-02 / interpretation A of D-03).
/// <para>
/// This is a VIEW filter only: it selects which of the already-loaded catalogue rows are shown.
/// It never writes to the database, never changes the catalogue query, and never hides a test
/// that the unfiltered catalogue shows (mode <see cref="Tm"/> is exactly that set).
/// </para>
/// </summary>
public enum CatalogFilterMode
{
    /// <summary>TM «كل التحاليل»: every test the catalogue already shows (the base set).</summary>
    Tm = 0,

    /// <summary>TG «بحث بالاسم أو الكود»: only the rows matching the search box (all rows when empty).</summary>
    Tg = 1,

    /// <summary>CG «مجموعاتي المخصصة»: only tests belonging to at least one custom group.</summary>
    Cg = 2
}
