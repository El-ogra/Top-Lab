using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.UsersAndPermissions.Common;
using TopLab.Application.Features.UsersAndPermissions.Queries.GetUsers;
using TopLab.Application.Features.AccessAndNavigation.Commands.LockWorkstation;
using TopLab.Application.Features.ExternalEntities.Common;
using TopLab.Application.Features.ExternalEntities.Queries.SearchExternalEntities;
using TopLab.Application.Features.SentOutSamples.Common;
using TopLab.Application.Features.SentOutSamples.Queries.GetSentOutSamples;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.Tests.Common;

/// <summary>Explicit-state fake; never rely on defaults for IsAuthenticated/IsAbsolutePermission.</summary>
public sealed class FakeCurrentUserService : ICurrentUserService
{
    public bool IsAuthenticated { get; set; } = true;
    public int UserId { get; set; } = 1;
    public string UserName { get; set; } = "testuser";
    public bool IsAbsolutePermission { get; set; }
    public HashSet<string> GrantedPermissions { get; init; } = new();

    public bool HasPermission(string code) => GrantedPermissions.Contains(code);

    public void SetSession(int userId, string userName, bool isAbsolutePermission, IEnumerable<string> grantedPermissions)
    {
        UserId = userId;
        UserName = userName;
        IsAbsolutePermission = isAbsolutePermission;
        GrantedPermissions.Clear();
        foreach (var code in grantedPermissions)
        {
            GrantedPermissions.Add(code);
        }
        IsAuthenticated = true;
    }

    public void ClearSession()
    {
        UserId = 0;
        UserName = string.Empty;
        IsAbsolutePermission = false;
        GrantedPermissions.Clear();
        IsAuthenticated = false;
    }
}

public sealed class FakeDialogService : IDialogService
{
    public List<string> Errors { get; } = new();
    public bool SecondaryPasswordResult { get; set; } = true;

    public Task<bool> ShowConfirmationAsync(string title, string message) => Task.FromResult(true);
    public Task ShowErrorAsync(string message)
    {
        Errors.Add(message);
        return Task.CompletedTask;
    }
    public Task<bool> ShowSecondaryPasswordDialogAsync() => Task.FromResult(SecondaryPasswordResult);
    public Task<string?> PickBackupFolderAsync(string initialDirectory) => Task.FromResult<string?>(null);
    public Task<string?> PickBackupFileAsync() => Task.FromResult<string?>(null);
    public Task<string?> PickPdfSavePathAsync(string? suggestedFileName = null) => Task.FromResult<string?>(null);
}

public sealed class FakeNavigationService : INavigationService
{
    public ViewModelBase? CurrentViewModel { get; private set; }
    public event Action<ViewModelBase?>? Navigated;
    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase => throw new NotSupportedException();
    public void NavigateTo(ViewModelBase viewModel)
    {
        CurrentViewModel = viewModel;
        Navigated?.Invoke(viewModel);
    }
}

public sealed class FakeSender : ISender
{
    private readonly Dictionary<Type, object> _responses = new();
    public int GetSentOutSamplesCallCount { get; private set; }
    public List<(int PatientTestId, string TestName)> SetupCalls { get; } = new();

    public FakeSender WithResponse<TResponse>(IRequest<TResponse> request, TResponse response)
    {
        _responses[request.GetType()] = response!;
        return this;
    }

    private Result? _lockResult;
    private Result<IReadOnlyList<ExternalEntityListItemDto>>? _searchLabs;
    private Result<IReadOnlyList<SentOutSampleDto>>? _sentOut;

    public FakeSender WithLockResult(Result result)
    {
        _lockResult = result;
        return this;
    }

    public FakeSender WithSearchLabsSuccess()
    {
        var labs = new List<ExternalEntityListItemDto>
        {
            new(1, EntityType.PartnerLab, "Lab A", null, null, null, null, null, null)
        };
        _searchLabs = Result<IReadOnlyList<ExternalEntityListItemDto>>.Success(labs);
        return this;
    }

    public FakeSender WithSearchLabsFailure(string message = "labs failed")
    {
        _searchLabs = Result<IReadOnlyList<ExternalEntityListItemDto>>.Failure(Error.Unexpected(message));
        return this;
    }

    public FakeSender WithSentOutSamples(params SentOutSampleDto[] items)
    {
        GetSentOutSamplesCallCount = 0;
        _sentOut = Result<IReadOnlyList<SentOutSampleDto>>.Success(items.ToList());
        return this;
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request is GetSentOutSamplesQuery)
        {
            GetSentOutSamplesCallCount++;
            var sent = _sentOut ?? Result<IReadOnlyList<SentOutSampleDto>>.Success(Array.Empty<SentOutSampleDto>());
            return Task.FromResult((TResponse)(object)sent);
        }
        if (request is SearchExternalEntitiesQuery)
        {
            var labs = _searchLabs ?? Result<IReadOnlyList<ExternalEntityListItemDto>>.Success(Array.Empty<ExternalEntityListItemDto>());
            return Task.FromResult((TResponse)(object)labs);
        }

        if (_responses.TryGetValue(request.GetType(), out var cached))
        {
            return Task.FromResult((TResponse)cached);
        }
        if (request is GetUsersQuery)
        {
            return Task.FromResult((TResponse)(object)Result<IReadOnlyList<UserSummaryDto>>.Success(Array.Empty<UserSummaryDto>()));
        }
        if (request is LockWorkstationCommand)
        {
            return Task.FromResult((TResponse)(object)(_lockResult ?? Result.Success()));
        }
        throw new NotSupportedException($"FakeSender has no canned response for {request.GetType().Name}.");
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("FakeSender does not support non-generic Send.");

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest =>
        throw new NotSupportedException("FakeSender does not support void Send.");

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("FakeSender does not support streams.");

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("FakeSender does not support streams.");
}
public sealed class FakeDateTimeProvider : TopLab.Application.Common.Interfaces.IDateTimeProvider
{
    public DateTime UtcNow { get; set; } = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
}