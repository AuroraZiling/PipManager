﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PipManager.Views.Pages.Library;
using PipManager.Views.Windows;

namespace PipManager.Services;

/// <summary>
/// Managed host of the application.
/// </summary>
public class ApplicationHostService(IServiceProvider serviceProvider) : IHostedService
{
    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await HandleActivationAsync();
    }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// Creates main window during activation.
    /// </summary>
    private async Task HandleActivationAsync()
    {
        await Task.CompletedTask;

        if (!Application.Current.Windows.OfType<MainWindow>().Any())
        {
            var navigationWindow = serviceProvider.GetRequiredService<MainWindow>();
            navigationWindow.Loaded += OnNavigationWindowLoaded;
            navigationWindow.Show();
        }
    }

    private void OnNavigationWindowLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not MainWindow navigationWindow)
        {
            return;
        }

        navigationWindow.NavigationView.Navigate(typeof(LibraryPage));
    }
}