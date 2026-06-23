using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace SpiceNet.UWP.Models;

public partial class ServerProfile : ObservableObject
{
    public int Version { get; set; }
    public string UID { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public int Port { get; set; }
    public string Password { get; set; }
    public bool AutoResizeGuest { get; set; }
    public bool AutoResizeViewer { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter<FitMode>))]
    public FitMode FitMode { get; set; }

    [JsonConstructor]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public ServerProfile() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public ServerProfile(string name, string address, int port, string password, bool autoResizeGuest, bool autoResizeViewer, FitMode fitMode)
    {
        Name = name;
        Address = address;
        Port = port;
        Password = password;
        UID = Guid.NewGuid().ToString();
        Version = 1;
        AutoResizeGuest = autoResizeGuest;
        AutoResizeViewer = autoResizeViewer;
        FitMode = fitMode;
    }

    public void Update()
    {
        OnPropertyChanged(string.Empty);
    }

    public override string ToString()
    {
        return Name;
    }

    public override bool Equals(object? obj)
    {
        return obj is ServerProfile profile && UID.Equals(profile.UID);
    }

    public override int GetHashCode()
    {
        return UID.GetHashCode();
    }
}
