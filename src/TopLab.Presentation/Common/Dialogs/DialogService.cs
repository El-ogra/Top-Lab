using System.Windows;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Features.UsersAndPermissions.Queries.VerifySecondaryPassword;
using TopLab.Presentation.Views.Dialogs;

namespace TopLab.Presentation.Common.Dialogs;

public sealed class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;

    public DialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<bool> ShowConfirmationAsync(string title, string message)
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return Task.FromResult(result == MessageBoxResult.Yes);
    }

    public Task ShowErrorAsync(string message)
    {
        MessageBox.Show(message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        return Task.CompletedTask;
    }

    public async Task<bool> ShowSecondaryPasswordDialogAsync()
    {
        var dialog = new SystemMenuPasswordDialog
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };

        bool? result = dialog.ShowDialog();
        if (result != true)
        {
            return false;
        }

        string password = dialog.EnteredPassword;
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
            var verifyResult = await mediator.Send(new VerifySecondaryPasswordQuery(password));
            if (!verifyResult.IsSuccess)
            {
                return false;
            }

            return verifyResult.Value;
        }
        catch
        {
            return false;
        }
    }
}
