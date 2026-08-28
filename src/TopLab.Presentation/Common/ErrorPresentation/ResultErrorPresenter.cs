using TopLab.Application.Common.Results;

namespace TopLab.Presentation.Common.ErrorPresentation;

public sealed class ResultErrorPresenter
{
    public const string PermissionDeniedMessage = "أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام";

    public string Present(Error error)
    {
        return error.Type switch
        {
            ErrorType.Validation => error.Message,
            ErrorType.NotFound => error.Message,
            ErrorType.Conflict => error.Message,
            ErrorType.Forbidden => PermissionDeniedMessage,
            ErrorType.Unexpected => "حدث خطأ غير متوقع. حاول مرة أخرى.",
            _ => error.Message
        };
    }

    public IReadOnlyList<string> PresentAll(IReadOnlyList<Error> errors)
    {
        return errors.Select(Present).ToList();
    }
}
