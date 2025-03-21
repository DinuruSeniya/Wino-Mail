﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.ApplicationModel;
using Windows.System;
using Wino.Core.Domain.Interfaces;

namespace Wino.Server;

public partial class ServerViewModel : ObservableObject, IInitializeAsync
{
    private readonly INotificationBuilder _notificationBuilder;

    public ServerContext Context { get; }

    public ServerViewModel(ServerContext serverContext, INotificationBuilder notificationBuilder)
    {
        Context = serverContext;
        _notificationBuilder = notificationBuilder;
    }

    [RelayCommand]
    public Task LaunchWinoAsync()
    {
        return Launcher.LaunchUriAsync(new Uri($"{App.WinoMailLaunchProtocol}:")).AsTask();
    }

    /// <summary>
    /// Shuts down the application.
    /// </summary>
    [RelayCommand]
    public async Task ExitApplication()
    {
        // Find the running UWP app by AppDiagnosticInfo API and terminate it if possible.
        var appDiagnosticInfos = await AppDiagnosticInfo.RequestInfoForPackageAsync(Package.Current.Id.FamilyName);

        var clientDiagnosticInfo = appDiagnosticInfos.FirstOrDefault();

        if (clientDiagnosticInfo == null)
        {
            Debug.WriteLine($"Wino Mail client is not running. Termination is skipped.");
        }
        else
        {
            var appResourceGroupInfo = clientDiagnosticInfo.GetResourceGroups().FirstOrDefault();

            if (appResourceGroupInfo != null)
            {
                await appResourceGroupInfo.StartTerminateAsync();

                Debug.WriteLine($"Wino Mail client is terminated succesfully.");
            }
        }

        Application.Current.Shutdown();
    }

    public async Task ReconnectAsync() => await Context.InitializeAppServiceConnectionAsync();

    public Task InitializeAsync() => Context.InitializeAppServiceConnectionAsync();
}
