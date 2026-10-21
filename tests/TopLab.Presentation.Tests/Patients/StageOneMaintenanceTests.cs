using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Common;
using TopLab.Application.Features.ExternalEntities.Queries.GetExternalEntityById;
using TopLab.Application.Features.PatientRegistration.Commands.AddMedicalConditionType;
using TopLab.Application.Features.PatientRegistration.Commands.RenameMedicalConditionType;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Application.Features.PatientRegistration.Queries.GetMedicalConditionTypes;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.ViewModels.External;
using TopLab.Presentation.ViewModels.Lab;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

/// <summary>
/// STAGE-01: the catalogue's maintenance tab and the read-only additional-
/// information window of the referring entity and the treating doctor.
/// </summary>
public class StageOneMaintenanceTests
{
    private sealed class CatalogueSender : ISender
    {
        public List<AddMedicalConditionTypeCommand> AddCommands { get; } = new();

        public List<RenameMedicalConditionTypeCommand> RenameCommands { get; } = new();

        public IReadOnlyList<MedicalConditionTypeDto> Rows { get; set; } = new[]
        {
            new MedicalConditionTypeDto(1, "حمل", MedicalConditionCategory.Pregnancy),
            new MedicalConditionTypeDto(2, "هل يؤخذ علاج للسكر", MedicalConditionCategory.Medication),
            new MedicalConditionTypeDto(12, "مريض بالذئبة الحمراء", MedicalConditionCategory.Condition),
            new MedicalConditionTypeDto(30, "علاج مخصص", MedicalConditionCategory.Medication)
        };

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetMedicalConditionTypesQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<MedicalConditionTypeDto>>.Success(Rows));
            }

            if (request is AddMedicalConditionTypeCommand add)
            {
                AddCommands.Add(add);
                return Task.FromResult((TResponse)(object)Result<int>.Success(31));
            }

            if (request is RenameMedicalConditionTypeCommand rename)
            {
                RenameCommands.Add(rename);
                return Task.FromResult((TResponse)(object)Result.Success());
            }

            throw new NotSupportedException($"CatalogueSender has no canned response for {request.GetType().Name}.");
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
            => throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class EntityDetailsSender : ISender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetExternalEntityByIdQuery)
            {
                var dto = new ExternalEntityDetailDto(
                    7, EntityType.TreatingDoctor, "د. أحمد", "المنصورة", "شارع الجمهورية", "01012345678",
                    "01098765432", "منى", "01112223333", 3, "قائمة الأسعار الافتراضية", 5m, "DOC-7", "doc@example.com");
                return Task.FromResult((TResponse)(object)Result<ExternalEntityDetailDto>.Success(dto));
            }

            throw new NotSupportedException($"EntityDetailsSender has no canned response for {request.GetType().Name}.");
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
            => throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    [Fact]
    public async Task MaintenanceTab_ListsTheCatalogueAndMarksTheSeededRows()
    {
        var sender = new CatalogueSender();
        var vm = new MedicalConditionTypesViewModel(sender, new ResultErrorPresenter());

        await vm.LoadAsync();

        Assert.Equal(4, vm.Rows.Count);
        Assert.True(vm.Rows.Single(r => r.Id == 1).IsSeeded);
        Assert.False(vm.Rows.Single(r => r.Id == 30).IsSeeded);
        Assert.Equal("حالة مرضية", vm.Rows.Single(r => r.Id == 12).CategoryLabel);
    }

    [Fact]
    public async Task MaintenanceTab_ASeededRowCannotBeRenamed()
    {
        var sender = new CatalogueSender();
        var vm = new MedicalConditionTypesViewModel(sender, new ResultErrorPresenter());
        await vm.LoadAsync();

        vm.SelectedRow = vm.Rows.Single(r => r.Id == 1);

        Assert.False(vm.CanRenameSelected);
    }

    [Fact]
    public async Task MaintenanceTab_AddDispatchesTheCommand()
    {
        var sender = new CatalogueSender();
        var vm = new MedicalConditionTypesViewModel(sender, new ResultErrorPresenter());
        await vm.LoadAsync();

        vm.Name = "علاج مخصص";
        vm.SelectedCategory = vm.CategoryOptions.Single(o => o.Value == MedicalConditionCategory.Medication);
        vm.AddCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(100);

        var command = Assert.Single(sender.AddCommands);
        Assert.Equal("علاج مخصص", command.Name);
        Assert.Equal(MedicalConditionCategory.Medication, command.Category);
    }

    [Fact]
    public async Task MaintenanceTab_RenameDispatchesTheCommand()
    {
        var sender = new CatalogueSender();
        var vm = new MedicalConditionTypesViewModel(sender, new ResultErrorPresenter());
        await vm.LoadAsync();

        vm.SelectedRow = vm.Rows.Single(r => r.Id == 30);
        vm.Name = "اسم جديد";
        vm.RenameCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(100);

        var command = Assert.Single(sender.RenameCommands);
        Assert.Equal(30, command.MedicalConditionTypeId);
        Assert.Equal("اسم جديد", command.Name);
    }

    [Fact]
    public async Task EntityDetailsWindow_ShowsTheAdditionalInformationReadOnly()
    {
        var vm = new ExternalEntityDetailsViewModel(new EntityDetailsSender(), new ResultErrorPresenter());

        await vm.LoadAsync(7);

        Assert.Equal("د. أحمد", vm.Name);
        Assert.Equal("طبيب معالج", vm.Type);
        Assert.Equal("DOC-7", vm.Code);
        Assert.Equal("المنصورة", vm.City);
        Assert.Equal("شارع الجمهورية", vm.Address);
        Assert.Equal("01012345678", vm.Phone);
        Assert.Equal("01098765432", vm.Fax);
        Assert.Equal("منى", vm.ResponsiblePersonName);
        Assert.Equal("01112223333", vm.ResponsiblePersonPhone);
        Assert.Equal("doc@example.com", vm.Email);
        Assert.Equal("قائمة الأسعار الافتراضية", vm.PriceListName);
        Assert.Equal(5m, vm.DiscountOrCommissionPercent);
        Assert.Empty(vm.ErrorMessage);
    }

    [Fact]
    public void EntityDetailsWindow_ExposesNoPublicEditingEntryPoints()
    {
        var properties = typeof(ExternalEntityDetailsViewModel).GetProperties();

        Assert.All(properties, p => Assert.Null(p.GetSetMethod()));
    }
}
