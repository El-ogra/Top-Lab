using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.CreateAntibiotic;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.UpdateAntibiotic;
using TopLab.Application.Features.CultureAndAntibiotics.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.ViewModels.Lab;
using Xunit;

namespace TopLab.Presentation.Tests.Lab;

/// <summary>Phase 1 REF-092: the editor VM round-trips CommercialName through init and save.</summary>
public class AntibioticEditorCommercialNameTests
{
    private sealed class RecordingSender : ISender
    {
        public object? LastRequest { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            object response = request switch
            {
                CreateAntibioticCommand => Result<int>.Success(7),
                UpdateAntibioticCommand => Result.Success(),
                _ => throw new NotSupportedException($"No canned response for {request.GetType().Name}.")
            };
            return Task.FromResult((TResponse)response);
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Non-generic Send is not supported.");

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest =>
            throw new NotSupportedException("Void Send is not supported.");

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Streams are not supported.");

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Streams are not supported.");
    }

    private static AntibioticEditorViewModel CreateVm(RecordingSender sender)
        => new(sender, new ResultErrorPresenter());

    [Fact]
    public void InitializeNew_ClearsCommercialName()
    {
        var vm = CreateVm(new RecordingSender());
        vm.CommercialName = "Amoxil";

        vm.InitializeNew();

        Assert.Null(vm.CommercialName);
    }

    [Fact]
    public void InitializeEdit_PopulatesCommercialName()
    {
        var vm = CreateVm(new RecordingSender());

        vm.InitializeEdit(new AntibioticDto(3, "Amoxicillin", false, false, "AMX", "Amoxicillin trihydrate", "Amoxil"));

        Assert.Equal("Amoxil", vm.CommercialName);
    }

    [Fact]
    public async Task SaveAsync_Create_ForwardsCommercialName()
    {
        var sender = new RecordingSender();
        var vm = CreateVm(sender);
        vm.InitializeNew();
        vm.Name = "Amoxicillin";
        vm.CommercialName = "  Amoxil  ";

        Assert.True(await vm.SaveAsync());

        var sent = Assert.IsType<CreateAntibioticCommand>(sender.LastRequest);
        Assert.Equal("Amoxil", sent.CommercialName);
    }

    [Fact]
    public async Task SaveAsync_Update_ForwardsCommercialName()
    {
        var sender = new RecordingSender();
        var vm = CreateVm(sender);
        vm.InitializeEdit(new AntibioticDto(3, "Amoxicillin", false, false));
        vm.CommercialName = "Amoxil";

        Assert.True(await vm.SaveAsync());

        var sent = Assert.IsType<UpdateAntibioticCommand>(sender.LastRequest);
        Assert.Equal("Amoxil", sent.CommercialName);
    }
}
