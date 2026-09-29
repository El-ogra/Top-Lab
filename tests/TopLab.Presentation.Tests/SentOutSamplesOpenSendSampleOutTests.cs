using Microsoft.Extensions.DependencyInjection;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Common;
using TopLab.Application.Features.ExternalEntities.Queries.SearchExternalEntities;
using TopLab.Application.Features.SentOutSamples.Common;
using TopLab.Application.Features.SentOutSamples.Queries.GetSentOutSamples;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;

namespace TopLab.Presentation.Tests;

/// <summary>
/// FIX-M1: OpenSendSampleOut lifecycle — await setup, open dialog only on setup
/// success, reload the sent-out list only when the dialog returns true.
/// </summary>
public sealed class SentOutSamplesOpenSendSampleOutTests
{
    private static readonly SentOutSampleDto Sample = new(
        Id: 1,
        PatientTestId: 42,
        PatientName: "Ali",
        TestName: "CBC",
        ExternalLabEntityId: 1,
        ExternalLabName: "Lab A",
        CostPrice: 10m,
        PatientPrice: 15m,
        SentAtUtc: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        TotalPaid: 0m,
        Remaining: 10m,
        IsFullySettled: false);

    private static (SentOutSamplesViewModel Vm, FakeSender Sender) Build(FakeSender sender)
    {
        var services = new ServiceCollection();
        var dialogs = new FakeDialogService();
        var presenter = new ResultErrorPresenter();
        var dialogVm = new SendSampleOutDialogViewModel(sender, presenter, dialogs);
        services.AddSingleton(dialogVm);
        var provider = services.BuildServiceProvider();

        var vm = new SentOutSamplesViewModel(sender, presenter, new FakeNavigationService(), provider)
        {
            SelectedItem = Sample
        };
        return (vm, sender);
    }

    private static (SentOutSamplesViewModel Vm, ThrowingSetupSender Sender) BuildThrowing()
    {
        var sender = new ThrowingSetupSender();
        var services = new ServiceCollection();
        var presenter = new ResultErrorPresenter();
        var dialogVm = new SendSampleOutDialogViewModel(sender, presenter, new FakeDialogService());
        services.AddSingleton(dialogVm);
        var provider = services.BuildServiceProvider();

        var vm = new SentOutSamplesViewModel(sender, presenter, new FakeNavigationService(), provider)
        {
            SelectedItem = Sample
        };
        return (vm, sender);
    }

    [Fact]
    public async Task SetupFailure_DoesNotOpenDialog_AndDoesNotReload()
    {
        var sender = new FakeSender()
            .WithSearchLabsFailure("lab lookup failed")
            .WithSentOutSamples(Sample);
        var (vm, s) = Build(sender);

        var opened = false;
        await vm.OpenSendSampleOutAsync(_ =>
        {
            opened = true;
            return true;
        });

        Assert.False(opened);
        Assert.Equal(0, s.GetSentOutSamplesCallCount);
        Assert.False(string.IsNullOrEmpty(vm.ErrorMessage));
    }

    [Fact]
    public async Task SetupException_DoesNotOpenDialog_AndSurfacesError()
    {
        var (vm, s) = BuildThrowing();

        var opened = false;
        await vm.OpenSendSampleOutAsync(_ =>
        {
            opened = true;
            return true;
        });

        Assert.False(opened);
        Assert.Equal(0, s.GetSentOutSamplesCallCount);
        Assert.False(string.IsNullOrEmpty(vm.ErrorMessage));
    }

    [Fact]
    public async Task Cancel_DoesNotReload()
    {
        var sender = new FakeSender()
            .WithSearchLabsSuccess()
            .WithSentOutSamples(Sample);
        var (vm, s) = Build(sender);

        var opened = false;
        await vm.OpenSendSampleOutAsync(_ =>
        {
            opened = true;
            return false;
        });

        Assert.True(opened);
        Assert.Equal(0, s.GetSentOutSamplesCallCount);
    }

    [Fact]
    public async Task NullDialogResult_DoesNotReload()
    {
        var sender = new FakeSender()
            .WithSearchLabsSuccess()
            .WithSentOutSamples(Sample);
        var (vm, s) = Build(sender);

        var opened = false;
        await vm.OpenSendSampleOutAsync(_ =>
        {
            opened = true;
            return null;
        });

        Assert.True(opened);
        Assert.Equal(0, s.GetSentOutSamplesCallCount);
    }

    [Fact]
    public async Task Success_ReloadsSentOutList()
    {
        var sender = new FakeSender()
            .WithSearchLabsSuccess()
            .WithSentOutSamples(Sample);
        var (vm, s) = Build(sender);

        var opened = false;
        await vm.OpenSendSampleOutAsync(_ =>
        {
            opened = true;
            return true;
        });

        Assert.True(opened);
        Assert.True(s.GetSentOutSamplesCallCount >= 1);
    }

    private sealed class ThrowingSetupSender : ISender
    {
        public int GetSentOutSamplesCallCount { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is SearchExternalEntitiesQuery)
            {
                throw new InvalidOperationException("setup boom");
            }
            if (request is GetSentOutSamplesQuery)
            {
                GetSentOutSamplesCallCount++;
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<SentOutSampleDto>>.Success(Array.Empty<SentOutSampleDto>()));
            }
            throw new NotSupportedException(request.GetType().Name);
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest =>
            throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}