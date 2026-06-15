using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpiceNet.UWP.Models;
using System.Net;

namespace SpiceNet.UWP.ViewModels;

public partial class RemoteDisplayViewModel : ObservableObject
{
    public MainChannel Channel { get; private set; }

    [ObservableProperty]
    private bool _autoResizeGuest = true;

    [ObservableProperty]
    private bool _autoResizeViewer = true;

    [ObservableProperty]
    private FitMode _fitMode = FitMode.OneToOne;

    public event EventHandler? FitModeChanged;

    public RemoteDisplayViewModel(string address, int port)
    {
        var host = Dns.GetHostEntry(address);
        Channel = new MainChannel(new IPEndPoint(host.AddressList[0], port));
    }

    [RelayCommand]
    private void ChangeFitMode(int fitMode)
    {
        FitMode = (FitMode)fitMode;
        FitModeChanged?.Invoke(this, new EventArgs());
    }
}
