using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Common;
using TopLab.Application.Features.ExternalEntities.Queries.SearchExternalEntities;
using TopLab.Application.Features.SentOutSamples.Commands.SendSampleOut;
using TopLab.Application.Features.SentOutSamples.Commands.SetSentOutDefaultLab;
using TopLab.Application.Features.SentOutSamples.Common;
using TopLab.Application.Features.SentOutSamples.Queries.GetSentOutSamples;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetSystemSettings;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;

namespace TopLab.Presentation.Tests.SentOutSamples;

public class Gap20SendSampleOutDialogTests
{
    private class DialogSender : ISender
    {
        public int SetDefaultCallCount { get; private set; }
        public int? LastSetDefaultLabId { get; private set; }
        public int SendCallCount { get; private set; }
        public int? LastSendLabId { get; private set; }
        public int GetSystemSettingsCallCount { get; protected set; }

        public virtual Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is SearchExternalEntitiesQuery)
            {
                var labs = new List<ExternalEntityListItemDto>
                {
                    new(1, EntityType.PartnerLab, "مختبر أ", null, null, null, null, null, null),
                    new(2, EntityType.PartnerLab, "مختبر ب", null, null, null, null, null, null)
                };
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<ExternalEntityListItemDto>>.Success(labs));
            }

            if (request is GetSystemSettingsQuery)
            {
                GetSystemSettingsCallCount++;
                return Task.FromResult((TResponse)(object)Result<TopLab.Application.Features.SystemAndPrintSettings.Common.SystemSettingsDto>
                    .Success(new TopLab.Application.Features.SystemAndPrintSettings.Common.SystemSettingsDto(
                        TopLab.Domain.Common.Enums.AccountType.Individual, false, false, false, false, false, false, false, false,
                        TopLab.Domain.Common.Enums.ResultScreenAccountDisplayMode.Hidden, false, null, 1, false, 2)));
            }

            if (request is SetSentOutDefaultLabCommand cmd)
            {
                SetDefaultCallCount++;
                LastSetDefaultLabId = cmd.ExternalLabEntityId;
                return Task.FromResult((TResponse)(object)Result.Success());
            }

            if (request is SendSampleOutCommand send)
            {
                SendCallCount++;
                LastSendLabId = send.ExternalLabEntityId;
                return Task.FromResult((TResponse)(object)Result<int>.Success(1));
            }

            throw new NotSupportedException(request.GetType().Name);
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private class PreselectionSender : DialogSender
    {
        private readonly int? _defaultLabId;
        public PreselectionSender(int? defaultLabId) => _defaultLabId = defaultLabId;

        public override Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetSystemSettingsQuery)
            {
                GetSystemSettingsCallCount++;
                return Task.FromResult((TResponse)(object)Result<TopLab.Application.Features.SystemAndPrintSettings.Common.SystemSettingsDto>
                    .Success(new TopLab.Application.Features.SystemAndPrintSettings.Common.SystemSettingsDto(
                        TopLab.Domain.Common.Enums.AccountType.Individual, false, false, false, false, false, false, false, false,
                        TopLab.Domain.Common.Enums.ResultScreenAccountDisplayMode.Hidden, false, null, 1, false, _defaultLabId)));
            }
            return base.Send(request, cancellationToken);
        }
    }

    private static (SendSampleOutDialogViewModel Vm, DialogSender Sender, FakeDialogService Dialogs) Build()
    {
        var sender = new DialogSender();
        var presenter = new ResultErrorPresenter();
        var dialogs = new FakeDialogService();
        var vm = new SendSampleOutDialogViewModel(sender, presenter, dialogs);
        return (vm, sender, dialogs);
    }

    [Fact]
    public void Ignore_SetsWasIgnored_AndDoesNotCallSender()
    {
        var (vm, sender, _) = Build();
        vm.SelectedLabId = 1;
        vm.IgnoreCommand.Execute(null);
        Assert.True(vm.WasIgnored);
        Assert.Equal(0, sender.SetDefaultCallCount);
        Assert.Equal(0, sender.SendCallCount);
        Assert.Equal(string.Empty, vm.ErrorMessage);
    }

    [Fact]
    public async Task AlwaysTo_WithoutLab_ShowsArabicError_DoesNotSend()
    {
        var (vm, sender, _) = Build();
        vm.SelectedLabId = null;
        vm.AlwaysToCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(50);
        Assert.Equal("اختر الجهة الخارجية أولاً.", vm.ErrorMessage);
        Assert.Equal(0, sender.SetDefaultCallCount);
        Assert.Equal(0, sender.SendCallCount);
        Assert.False(vm.IsBusy);
    }

    [Fact]
    public async Task AlwaysTo_WithLab_PersistsDefault_ThenSends()
    {
        var (vm, sender, _) = Build();
        vm.SelectedLabId = 2;
        vm.AlwaysToCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(100);
        Assert.Equal(1, sender.SetDefaultCallCount);
        Assert.Equal(2, sender.LastSetDefaultLabId);
        Assert.Equal(1, sender.SendCallCount);
        Assert.Equal(2, sender.LastSendLabId);
        Assert.Equal(string.Empty, vm.ErrorMessage);
    }

    [Fact]
    public async Task Setup_PreselectsPersistedDefault()
    {
        var sender = new PreselectionSender(defaultLabId: 2);
        var presenter = new ResultErrorPresenter();
        var dialogs = new FakeDialogService();
        var vm = new SendSampleOutDialogViewModel(sender, presenter, dialogs);
        await vm.SetupAsync(42, "تحليل");
        Assert.Equal(2, vm.SelectedLabId);
        Assert.True(sender.GetSystemSettingsCallCount > 0);
    }

    [Fact]
    public async Task Setup_IgnoresStaleDefault()
    {
        var sender = new PreselectionSender(defaultLabId: 99);
        var presenter = new ResultErrorPresenter();
        var dialogs = new FakeDialogService();
        var vm = new SendSampleOutDialogViewModel(sender, presenter, dialogs);
        await vm.SetupAsync(42, "تحليل");
        Assert.Null(vm.SelectedLabId);
    }
}
