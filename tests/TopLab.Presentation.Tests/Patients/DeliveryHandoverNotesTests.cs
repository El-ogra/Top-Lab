using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Common;
using TopLab.Application.Features.ResultDelivery.Queries.GetDeliveryAccount;
using TopLab.Application.Features.ResultDelivery.Queries.GetDeliveryGrid;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

public class DeliveryHandoverNotesTests
{
    private sealed class FakeSender : ISender
    {
        public Result<DeliveryAccountDto>? AccountResult { get; set; }
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetDeliveryAccountQuery q)
            {
                var r = AccountResult ?? Result<DeliveryAccountDto>.Success(new DeliveryAccountDto(1, 100m, 80m, 20m, 20m, 0m, "note text"));
                return Task.FromResult((TResponse)(object)r);
            }
            if (request is GetDeliveryGridQuery q2)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<DeliveryGridRowDto>>.Success(Array.Empty<DeliveryGridRowDto>()));
            }
            throw new NotSupportedException(request.GetType().Name);
        }
        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class FakeNav : INavigationService
    {
        public ViewModelBase? CurrentViewModel { get; set; }
        public event Action<ViewModelBase?>? Navigated
        {
            add { }
            remove { }
        }
        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase { }
        public void NavigateTo(ViewModelBase viewModel) { CurrentViewModel = viewModel; }
    }

    [Fact]
    public async Task PatientNotes_Loaded_WhenPresent()
    {
        var sender = new FakeSender { AccountResult = Result<DeliveryAccountDto>.Success(new DeliveryAccountDto(1, 100m, 80m, 20m, 20m, 0m, "note text")) };
        var vm = new DeliveryHandoverViewModel(sender, new ResultErrorPresenter(), new FakeDialogService(), new FakeNav());
        await vm.LoadAsync(1);
        Assert.Equal("note text", vm.PatientNotes);
    }

    [Fact]
    public async Task PatientNotes_Empty_WhenNull()
    {
        var sender = new FakeSender { AccountResult = Result<DeliveryAccountDto>.Success(new DeliveryAccountDto(1, 100m, 80m, 20m, 20m, 0m, null)) };
        var vm = new DeliveryHandoverViewModel(sender, new ResultErrorPresenter(), new FakeDialogService(), new FakeNav());
        await vm.LoadAsync(1);
        Assert.Equal(string.Empty, vm.PatientNotes);
    }
}
