using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpiceNet.UWP.Extensions;
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
    private FitMode _fitMode = FitMode.Center;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private Guid? _guid;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public event EventHandler? FitModeChanged;

    public RemoteDisplayViewModel(string address, int port, string password)
    {
        dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        try
        {
            var host = Dns.GetHostEntry(address);
            Channel = new MainChannel(new IPEndPoint(host.AddressList[0], port), password);
            Channel.OnDisconnected += Channel_OnDisconnected;
            Channel.OnError += Channel_OnError;
            Channel.Name += GuestName;
            Channel.Guid += GuestGuid;
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

    [RelayCommand]
    public void SendEmulatedKeypress(EmulatedKey emulatedKey)
    {
        if (Channel?.Inputs?.Ready != true)
            return;

        if (emulatedKey.Modifier != VirtualKey.None)
            Channel.Inputs.KeyDown(emulatedKey.Modifier.ToScancode());
        if (emulatedKey.Extra != VirtualKey.None)
            Channel.Inputs.KeyDown(emulatedKey.Extra.ToScancode());
        Channel.Inputs.KeyDown(emulatedKey.Key.ToScancode());

        if (emulatedKey.Modifier != VirtualKey.None)
            Channel.Inputs.KeyUp(emulatedKey.Modifier.ToScancode());
        if (emulatedKey.Extra != VirtualKey.None)
            Channel.Inputs.KeyUp(emulatedKey.Extra.ToScancode());
        Channel.Inputs.KeyUp(emulatedKey.Key.ToScancode());
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


    private void GuestName(object? sender, string e)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            Name = e;
        });
    }

    private void GuestGuid(object? sender, Guid e)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            Guid = e;
        });
    }


}
