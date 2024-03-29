﻿using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using PipManager.Resources.Library;
using PipManager.Languages;
using PipManager.Models.Action;
using PipManager.Models.Pages;
using PipManager.Services.Action;
using PipManager.Services.Environment;
using PipManager.Services.Mask;
using PipManager.Services.Toast;
using PipManager.Views.Pages.Action;
using System.Collections.ObjectModel;
using System.IO;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace PipManager.ViewModels.Pages.Library;

public partial class LibraryInstallViewModel : ObservableObject, INavigationAware
{
    private bool _isInitialized;

    public record InstalledPackagesMessage(List<LibraryListItem> InstalledPackages);

    private readonly IActionService _actionService;
    private readonly IMaskService _maskService;
    private readonly IContentDialogService _contentDialogService;
    private readonly IEnvironmentService _environmentService;
    private readonly IToastService _toastService;
    private readonly INavigationService _navigationService;

    public LibraryInstallViewModel(IActionService actionService, IMaskService maskService, IContentDialogService contentDialogService, IEnvironmentService environmentService, IToastService toastService, INavigationService navigationService)
    {
        _actionService = actionService;
        _maskService = maskService;
        _contentDialogService = contentDialogService;
        _environmentService = environmentService;
        _toastService = toastService;
        _navigationService = navigationService;
        WeakReferenceMessenger.Default.Register<InstalledPackagesMessage>(this, Receive);
    }

    public void OnNavigatedTo()
    {
        if (!_isInitialized)
            InitializeViewModel();
    }

    public void OnNavigatedFrom()
    {
        PreInstallPackages.Clear();
    }

    private void InitializeViewModel()
    {
        _isInitialized = true;
    }

    public void Receive(object recipient, InstalledPackagesMessage message)
    {
        _installedPackages = message.InstalledPackages;
    }

    #region Install

    [ObservableProperty] private ObservableCollection<LibraryInstallPackageItem> _preInstallPackages = [];
    private List<LibraryListItem> _installedPackages = [];

    [RelayCommand]
    private async Task AddDefaultTask()
    {
        var custom = new InstallAddContentDialog(_contentDialogService.GetContentPresenter());
        var packageName = await custom.ShowAsync();
        if (packageName == "")
        {
            return;
        }
        if (PreInstallPackages.Any(package => package.PackageName == packageName))
        {
            _toastService.Error(Lang.LibraryInstall_Add_AlreadyInList);
            return;
        }
        if (_installedPackages.Any(item => item.PackageName == packageName))
        {
            _toastService.Error(Lang.LibraryInstall_Add_AlreadyInstalled);
            return;
        }
        _maskService.Show(Lang.LibraryInstall_Add_Verifying);
        var packageVersions = await _environmentService.GetVersions(packageName);
        _maskService.Hide();
        switch (packageVersions.Status)
        {
            case 1:
                _toastService.Error(Lang.LibraryInstall_Add_PackageNotFound);
                return;

            case 2:
                _toastService.Error(Lang.LibraryInstall_Add_InvalidPackageName);
                return;

            default:
                PreInstallPackages.Add(new LibraryInstallPackageItem
                {
                    PackageName = packageName,
                    AvailableVersions = new List<string>(packageVersions.Versions!.Reverse())
                });
                break;
        }
    }

    [RelayCommand]
    private void AddDefaultToAction()
    {
        List<string> operationCommand = [];
        operationCommand.AddRange(PreInstallPackages.Select(preInstallPackage => preInstallPackage.VersionSpecified
            ? $"{preInstallPackage.PackageName}=={preInstallPackage.TargetVersion}"
            : $"{preInstallPackage.PackageName}"));
        _actionService.AddOperation(new ActionListItem
        (
            ActionType.Install,
            string.Join(' ', operationCommand),
            totalSubTaskNumber: operationCommand.Count
        ));
        PreInstallPackages.Clear();
    }

    [RelayCommand]
    private void DeleteDefaultTask(object? parameter)
    {
        var target = -1;
        for (int index = 0; index < PreInstallPackages.Count; index++)
        {
            if (ReferenceEquals(PreInstallPackages[index].PackageName, parameter))
            {
                target = index;
            }
        }

        if (target != -1)
        {
            PreInstallPackages.RemoveAt(target);
        }
    }

    #endregion Install

    #region Requirements Import

    [ObservableProperty] private string _requirements = "";

    [RelayCommand]
    private void AddRequirementsTask()
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "requirements.txt",
            FileName = "requirements.txt",
            DefaultExt = ".txt",
            Filter = "requirements|*.txt",
            RestoreDirectory = true
        };
        var result = openFileDialog.ShowDialog();
        if (result == true)
        {
            Requirements = File.ReadAllText(openFileDialog.FileName);
        }
    }

    [RelayCommand]
    private void AddRequirementsToAction()
    {
        _actionService.AddOperation(new ActionListItem
        (
            ActionType.InstallByRequirements,
            Requirements,
            displayCommand: "requirements.txt"
        ));
        Requirements = "";
        _navigationService.Navigate(typeof(ActionPage));
    }

    #endregion Requirements Import

    #region Download Wheel File

    [ObservableProperty] private ObservableCollection<LibraryInstallPackageItem> _preDownloadPackages = [];
    [ObservableProperty] private string _downloadDistributionsFolderPath = "";
    [ObservableProperty] private bool _downloadDistributionsEnabled = false;
    [ObservableProperty] private bool _downloadDependencies = false;

    [RelayCommand]
    private async Task DownloadDistributionsTask()
    {
        var custom = new InstallAddContentDialog(_contentDialogService.GetContentPresenter());
        var packageName = await custom.ShowAsync();
        if (packageName == "")
        {
            return;
        }
        if (PreDownloadPackages.Any(package => package.PackageName == packageName))
        {
            _toastService.Error(Lang.LibraryInstall_Add_AlreadyInList);
            return;
        }
        _maskService.Show(Lang.LibraryInstall_Add_Verifying);
        var packageVersions = await _environmentService.GetVersions(packageName);
        _maskService.Hide();
        switch (packageVersions.Status)
        {
            case 1:
                _toastService.Error(Lang.LibraryInstall_Add_PackageNotFound);
                return;

            case 2:
                _toastService.Error(Lang.LibraryInstall_Add_InvalidPackageName);
                return;

            default:
                PreDownloadPackages.Add(new LibraryInstallPackageItem
                {
                    PackageName = packageName,
                    AvailableVersions = new List<string>(packageVersions.Versions!.Reverse())
                });
                DownloadDistributionsEnabled = DownloadDistributionsFolderPath.Length > 0;
                break;
        }
    }

    [RelayCommand]
    private void BrowseDownloadDistributionsFolderTask()
    {
        var openFolderDialog = new OpenFolderDialog
        {
            Title = "Download Folder (for wheel files)"
        };
        var result = openFolderDialog.ShowDialog();
        if (result == true)
        {
            DownloadDistributionsFolderPath = openFolderDialog.FolderName;
            DownloadDistributionsEnabled = PreDownloadPackages.Count > 0;
        }
    }

    [RelayCommand]
    private void DownloadDistributionsToAction()
    {
        List<string> operationCommand = [];
        operationCommand.AddRange(PreDownloadPackages.Select(preDownloadPackage => preDownloadPackage.VersionSpecified
            ? $"{preDownloadPackage.PackageName}=={preDownloadPackage.TargetVersion}"
            : $"{preDownloadPackage.PackageName}"));
        _actionService.AddOperation(new ActionListItem
        (
            ActionType.Download,
            string.Join(' ', operationCommand),
            path: DownloadDistributionsFolderPath,
            extraParameters: DownloadDependencies ? null : ["--no-deps"],
            totalSubTaskNumber: operationCommand.Count
        ));
        PreDownloadPackages.Clear();
        _navigationService.Navigate(typeof(ActionPage));
    }

    [RelayCommand]
    private void DeleteDownloadDistributionsTask(object? parameter)
    {
        var target = -1;
        for (int index = 0; index < PreDownloadPackages.Count; index++)
        {
            if (ReferenceEquals(PreDownloadPackages[index].PackageName, parameter))
            {
                target = index;
            }
        }

        if (target != -1)
        {
            PreDownloadPackages.RemoveAt(target);
        }
        DownloadDistributionsEnabled = PreDownloadPackages.Count > 0;
    }

    #endregion Download Wheel File
}