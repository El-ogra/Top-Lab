using TopLab.Application.Common.Results;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Port for barcode generation/printing. Implemented in Infrastructure (Barcode/).
/// </summary>
public interface IBarcodeService
{
    Task<Result> PrintBarcodeAsync(string value, CancellationToken cancellationToken = default);
}
