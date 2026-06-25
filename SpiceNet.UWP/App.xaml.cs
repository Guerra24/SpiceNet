using SpiceNet.UWP.Services;
using SpiceNet.UWP.Util;
using SpiceNet.UWP.Views;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;

namespace SpiceNet.UWP;

public sealed partial class App : Application
{

    private bool standalone;

    public App()
    {
        InitializeComponent();
        Service.BuildServices();
        Suspending += OnSuspending;
    }

    [DynamicWindowsRuntimeCast(typeof(Frame))]
    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
        Main? root = Window.Current.Content as Main;

        if (root == null)
        {
            SetupView();

            root = new Main();

            Window.Current.Content = root;
        }
        //CoreApplication.EnablePrelaunch(true);
        Window.Current.Activate();
    }

    [DynamicWindowsRuntimeCast(typeof(StorageFile))]
    protected override async void OnFileActivated(FileActivatedEventArgs args)
    {
        standalone = true;

        var file = (StorageFile)args.Files[0];

        var ini = new IniFile();
        await ini.LoadFileAsync(file);

        var title = ini["virt-viewer", "title"];
        var proxy = ini["virt-viewer", "proxy"];
        var host = ini["virt-viewer", "host"]!;
        var password = ini["virt-viewer", "password"] ?? "";
        var ca = ini["virt-viewer", "ca"];
        var port = ini["virt-viewer", "tls-port"] ?? ini["virt-viewer", "port"]!;

        if (int.TryParse(ini["virt-viewer", "delete-this-file"], out var res) && res == 1)
            await file.DeleteAsync();

        RemoteDisplay? root = Window.Current.Content as RemoteDisplay;

        if (root == null)
        {
            SetupView(title);

            root = new RemoteDisplay(host, int.Parse(port), password, true, true, Models.FitMode.Center, proxy, ca);

            Window.Current.Content = root;
        }

        Window.Current.Activate();
    }

    private async void OnSuspending(object sender, SuspendingEventArgs e)
    {
        var deferral = e.SuspendingOperation.GetDeferral();

        if (!standalone)
            await Service.Settings.SaveProfiles();

        deferral.Complete();
    }

    private void SetupView(string? title = null)
    {
        var coreView = CoreApplication.GetCurrentView();
        var appView = ApplicationView.GetForCurrentView();
        if (title != null)
            appView.Title = title;
        var coreTitleBar = coreView.TitleBar;
        coreTitleBar.ExtendViewIntoTitleBar = true;

        var titleBar = appView.TitleBar;
        titleBar.ButtonBackgroundColor = Colors.Transparent;
        titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
    }
}
