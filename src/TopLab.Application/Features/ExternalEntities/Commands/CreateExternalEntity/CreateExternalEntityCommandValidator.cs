using FluentValidation;
using TopLab.Domain.ExternalEntities;

namespace TopLab.Application.Features.ExternalEntities.Commands.CreateExternalEntity;

public sealed class CreateExternalEntityCommandValidator : AbstractValidator<CreateExternalEntityCommand>
{
    public CreateExternalEntityCommandValidator()
    {
        RuleFor(x => x.EntityType)
            .IsInEnum().WithMessage("نوع الجهة غير صالح.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم الجهة الخارجية مطلوب.")
            .MaximumLength(ExternalEntity.MaxNameLength).WithMessage("اسم الجهة الخارجية يجب ألا يتجاوز 200 حرفًا.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("المدينة يجب ألا تتجاوز 100 حرف.");

        RuleFor(x => x.Address)
            .MaximumLength(300).WithMessage("العنوان يجب ألا يتجاوز 300 حرفًا.");

        RuleFor(x => x.Phone)
            .MaximumLength(30).WithMessage("الهاتف يجب ألا يتجاوز 30 حرفًا.");

        RuleFor(x => x.Fax)
            .MaximumLength(30).WithMessage("الفاكس يجب ألا يتجاوز 30 حرفًا.");

        RuleFor(x => x.ResponsiblePersonName)
            .MaximumLength(150).WithMessage("اسم المسؤول يجب ألا يتجاوز 150 حرفًا.");

        RuleFor(x => x.ResponsiblePersonPhone)
            .MaximumLength(30).WithMessage("هاتف المسؤول يجب ألا يتجاوز 30 حرفًا.");

        When(x => x.PriceListId.HasValue, () =>
        {
            RuleFor(x => x.PriceListId!.Value)
                .GreaterThan(0).WithMessage("قائمة الأسعار المحددة غير موجودة.");
        });

        RuleFor(x => x.PriceListId)
            .Must((command, priceListId) => command.EntityType != Domain.Common.Enums.EntityType.TreatingDoctor || priceListId is null)
            .WithMessage("الطبيب المعالج لا يرتبط بقائمة أسعار.");

        RuleFor(x => x.PriceListId)
            .Must((command, priceListId) => command.EntityType != Domain.Common.Enums.EntityType.ReferralOrContract || priceListId is not null)
            .WithMessage("جهة الإحالة / التعاقد تتطلب قائمة أسعار.");

        When(x => x.DiscountOrCommissionPercent.HasValue, () =>
        {
            RuleFor(x => x.DiscountOrCommissionPercent!.Value)
                .InclusiveBetween(0m, 100m).WithMessage("نسبة الخصم / العمولة يجب أن تكون بين 0 و 100.");
        });
    }
}
