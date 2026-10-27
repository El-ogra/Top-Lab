using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SentOutSamples.Commands.SendSampleOut;
using TopLab.Application.Features.ExternalEntities.Common;
using TopLab.Application.Features.ExternalEntities.Queries.SearchExternalEntities;
using TopLab.Application.Features.SentOutSamples.Common;
using TopLab.Application.Features.SentOutSamples.Queries.GetSentOutSamples;
using TopLab.Presentation.Tests.Common;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.ViewModels.Patients;

namespace TopLab.Presentation.Tests.SentOutSamples;

/// <summary>
/// GAP-20 (6.3.1.6.4): SentOutSamplesViewModel ignore — session-only, no database write.
/// Hand-rolled fakes, no mocking library.
/// </summary>
public class Gap20SentOutSamplesViewModelTests
{
    private sealed class NoOpSender : ISender
    {
        public int SendCallCount { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetSentOutSamplesQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<SentOutSampleDto>>.Success(
                    new List<SentOutSampleDto>
                    {
                        new(1, 42, "أحمد", "CBC", 1, "مختبر أ", 10m, 15m,
                            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), 0m, 10m, false)
                    }));
            }
            if (request is SearchExternalEntitiesQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<ExternalEntityListItemDto>>.Success(
                    new List<ExternalEntityListItemDto>()));
            }
            throw new NotSupportedException(request.GetType().Name);
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private static (SentOutSamplesViewModel Vm, NoOpSender Sender) Build()
    {
        var sender = new NoOpSender();
        var presenter = new ResultErrorPresenter();
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        var dialogVm = new SendSampleOutDialogViewModel(sender, presenter, new FakeDialogService());
        services.AddSingleton(dialogVm);
        var provider = services.BuildServiceProvider();

        var vm = new SentOutSamplesViewModel(sender, presenter, new FakeNavigationService(), provider);
        return (vm, sender);
    }

    [Fact]
    public async Task Ignore_RemovesSelectedRow_FromCurrentView()
    {
        var (vm, sender) = Build();
        await vm.LoadAsync();

        vm.SelectedItem = vm.Items[0];
        vm.IgnoreCommand.Execute(null);

        Assert.Empty(vm.Items);
        Assert.Null(vm.SelectedItem);
        // No database write: no SendSampleOut command dispatched
        Assert.Equal(0, sender.SendCallCount);
    }

    [Fact]
    public async Task Ignore_EmptySelection_IsNoOp()
    {
        var (vm, sender) = Build();
        await vm.LoadAsync();

        vm.IgnoreCommand.Execute(null);

        Assert.Single(vm.Items);
        Assert.Equal(string.Empty, vm.ErrorMessage);
        Assert.Equal(0, sender.SendCallCount);
    }

    [Fact]
    public async Task Reload_RestoresIgnoredRow()
    {
        var (vm, sender) = Build();
        await vm.LoadAsync();
        var before = vm.Items.Count;

        vm.SelectedItem = vm.Items[0];
        vm.IgnoreCommand.Execute(null);
        Assert.Equal(before - 1, vm.Items.Count);

        await vm.LoadAsync();
        Assert.Equal(before, vm.Items.Count);
    }
}
