using SpiceNet.UWP.Services;
using SpiceNet.UWP.Views;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;

namespace SpiceNet.UWP;

public sealed partial class App : Application
{

    public App()
    {
        InitializeComponent();
        Service.BuildServices();
        Suspending += OnSuspending;
    }

    /// <inheritdoc/>
    [DynamicWindowsRuntimeCast(typeof(Frame))]
    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
        Main? root = Window.Current.Content as Main;

        if (root == null)
        {
            var CoreView = CoreApplication.GetCurrentView();
            var AppView = ApplicationView.GetForCurrentView();
            var coreTitleBar = CoreView.TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            var titleBar = AppView.TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            root = new Main();

            Window.Current.Content = root;
        }
        //CoreApplication.EnablePrelaunch(true);
        Window.Current.Activate();
    }

    private async void OnSuspending(object sender, SuspendingEventArgs e)
    {
        var deferral = e.SuspendingOperation.GetDeferral();

        await Service.Settings.SaveProfiles();

        deferral.Complete();
    }
}
