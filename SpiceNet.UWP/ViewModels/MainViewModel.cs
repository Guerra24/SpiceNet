using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpiceNet.UWP.Models;
using SpiceNet.UWP.Services;
using SpiceNet.UWP.Views;
using SpiceNet.UWP.Views.Dialogs;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;

namespace SpiceNet.UWP.ViewModels;

public partial class MainViewModel : ObservableObject
{

    public SettingsService Settings { get; }

    [ObservableProperty]
    private string _address = string.Empty;
    [ObservableProperty]
    private int _port = 5900;
    [ObservableProperty]
    private string _password = string.Empty;

    public MainViewModel(SettingsService settings)
    {
        Settings = settings;
    }

    [RelayCommand]
    private async Task QuickConnect()
    {
        if (string.IsNullOrEmpty(Address))
            return;

        await OpenConnection(Address, Port, Password);
    }

    [RelayCommand]
    private async Task AddProfile()
    {
        var dialog = new ServerProfileDialog(false);
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            Settings.AddProfile(dialog.ProfName, dialog.Address, dialog.Port, dialog.Password, dialog.AutoResizeGuest, dialog.AutoResizeViewer, dialog.FitMode);
            await Settings.SaveProfiles();
        }
    }

    [RelayCommand]
    private async Task EditProfile(ServerProfile profile)
    {
        var dialog = new ServerProfileDialog(true)
        {
            ProfName = profile.Name,
            Address = profile.Address,
            Port = profile.Port,
            Password = profile.Password,
            AutoResizeGuest = profile.AutoResizeGuest,
            AutoResizeViewer = profile.AutoResizeViewer,
            FitMode = profile.FitMode
        };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            profile.Name = dialog.ProfName;
            profile.Address = dialog.Address;
            profile.Port = dialog.Port;
            profile.Password = dialog.Password;
            profile.AutoResizeGuest = dialog.AutoResizeGuest;
            profile.AutoResizeViewer = dialog.AutoResizeViewer;
            profile.FitMode = dialog.FitMode;
            profile.Update();
            await Settings.SaveProfiles();
        }
    }

    [RelayCommand]
    private async Task RemoveProfile(ServerProfile profile)
    {
        var dialog = new GenericDialog { Title = "Remove connection", PrimaryButtonText = "Yes", CloseButtonText = "No", Content = $"Do you want to remove \"{profile.Name}\"?" };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            Settings.Profiles.Remove(profile);
            await Settings.SaveProfiles();
        }
    }

    [RelayCommand]
    private async Task ContinueProfile(ServerProfile profile)
    {
        await OpenConnection(profile.Address, profile.Port, profile.Password, profile.AutoResizeGuest, profile.AutoResizeViewer, profile.FitMode);
    }

    private async Task OpenConnection(string address, int port, string password, bool autoResizeGuest = true, bool autoResizeViewer = true, FitMode fitMode = FitMode.Center)
    {
        var newView = CoreApplication.CreateNewView();
        int viewId = 0;
        await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        {
            SynchronizationContext.SetSynchronizationContext(new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread()));

            var coreView = CoreApplication.GetCurrentView();
            var appView = ApplicationView.GetForCurrentView();
            var coreTitleBar = coreView.TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            var titleBar = appView.TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            Window.Current.Content = new RemoteDisplay(address, port, password, autoResizeGuest, autoResizeViewer, fitMode);
            Window.Current.Activate();

            viewId = ApplicationView.GetForCurrentView().Id;
        });
        await ApplicationViewSwitcher.TryShowAsStandaloneAsync(viewId);

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, true, false);
    }
}
