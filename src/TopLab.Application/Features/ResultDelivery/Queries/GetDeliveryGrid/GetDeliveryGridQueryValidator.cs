using FluentValidation;

namespace TopLab.Application.Features.ResultDelivery.Queries.GetDeliveryGrid;

public sealed class GetDeliveryGridQueryValidator : AbstractValidator<GetDeliveryGridQuery>
{
    public GetDeliveryGridQueryValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
    }
}
