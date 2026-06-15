using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpiceNet.UWP.Views;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;

namespace SpiceNet.UWP.ViewModels;

public partial class MainViewModel : ObservableObject
{

    [ObservableProperty]
    private string _address = string.Empty;
    [ObservableProperty]
    private int _port = 5900;

    [RelayCommand]
    private async Task OpenRemoteDisplay()
    {
        if (string.IsNullOrEmpty(Address))
            return;

        var newView = CoreApplication.CreateNewView();
        int viewId = 0;
        await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        {
            SynchronizationContext.SetSynchronizationContext(new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread()));

            var CoreView = CoreApplication.GetCurrentView();
            var AppView = ApplicationView.GetForCurrentView();
            var coreTitleBar = CoreView.TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            var titleBar = AppView.TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            Window.Current.Content = new RemoteDisplay(Address, Port);
            Window.Current.Activate();

            viewId = ApplicationView.GetForCurrentView().Id;
        });
        await ApplicationViewSwitcher.TryShowAsStandaloneAsync(viewId);

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, false, false);
    }
}
