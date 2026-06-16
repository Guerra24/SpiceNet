using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpiceNet.UWP.Models;
using System.Net;

namespace SpiceNet.UWP.ViewModels;

public partial class RemoteDisplayViewModel : ObservableObject
{
    private DispatcherQueue dispatcherQueue;

    public MainChannel? Channel { get; private set; }

    [ObservableProperty]
    private bool _autoResizeGuest = true;

    [ObservableProperty]
    private bool _autoResizeViewer = true;

    [ObservableProperty]
    private FitMode _fitMode = FitMode.OneToOne;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public event EventHandler? FitModeChanged;

    public RemoteDisplayViewModel(string address, int port)
    {
        dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        try
        {
            var host = Dns.GetHostEntry(address);
            Channel = new MainChannel(new IPEndPoint(host.AddressList[0], port));
            Channel.OnDisconnected += Channel_OnDisconnected;
            Channel.OnError += Channel_OnError;
        }
        catch (Exception e)
        {
            dispatcherQueue.TryEnqueue(() =>
            {
                ErrorMessage = e.Message;
            });
        }
    }

    [RelayCommand]
    private void ChangeFitMode(int fitMode)
    {
        FitMode = (FitMode)fitMode;
        FitModeChanged?.Invoke(this, new EventArgs());
    }

    private void Channel_OnDisconnected(object? sender, EventArgs e)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            ErrorMessage = "Disconnected...";
        });
    }

    private void Channel_OnError(object? sender, Exception e)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            ErrorMessage = e.Message;
        });
    }
}
