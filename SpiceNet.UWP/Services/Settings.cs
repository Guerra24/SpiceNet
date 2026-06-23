using CommunityToolkit.Mvvm.ComponentModel;
using SpiceNet.UWP.Models;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace SpiceNet.UWP.Services;

public partial class SettingsService : ObservableObject
{
    private readonly ISettingsStorageService SettingsStorage;
    private readonly IFilesService Files;
    //private readonly PlatformService Platform;

    public ObservableCollection<ServerProfile> Profiles { get; private set; } = new();

    public AppTheme Theme
    {
        get => (AppTheme)SettingsStorage.GetObjectLocal((int)AppTheme.System);
        set
        {
            SettingsStorage.StoreObjectLocal((int)value);
            //Platform.ChangeTheme(value);
        }
    }

    public static readonly int CurrentLocalVersion = 1;
    public int SettingsVersionLocal
    {
        get => SettingsStorage.GetObjectLocal(CurrentLocalVersion);
        set => SettingsStorage.StoreObjectLocal(value);
    }
    public static readonly int CurrentRoamedVersion = 1;
    public int SettingsVersionRoamed
    {
        get => SettingsStorage.GetObjectRoamed(CurrentRoamedVersion);
        set => SettingsStorage.StoreObjectRoamed(value);
    }

    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026")]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050")]
    public SettingsService(ISettingsStorageService settingsStorage, IFilesService files/*, PlatformService platform*/)
    {
        SettingsStorage = settingsStorage;
        Files = files;
        //Platform = platform;

        var profilesFile = Path.Combine(Files.Local, "Profiles.json");

        var content = File.Exists(profilesFile) ? File.ReadAllText(profilesFile) : "";
        Profiles = !string.IsNullOrEmpty(content) ? JsonSerializer.Deserialize<ObservableCollection<ServerProfile>>(content, JsonSettings.Options)! : [];
    }

    public ServerProfile AddProfile(string name, string address, int port, string password, bool autoResizeGuest, bool autoResizeViewer, FitMode fitMode)
    {
        var profile = new ServerProfile(name, address, port, password, autoResizeGuest, autoResizeViewer, fitMode);
        Profiles.Add(profile);
        return profile;
    }

    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026")]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050")]
    public async Task SaveProfiles()
    {
        await Files.StoreFileSafe(Path.Combine(Files.Local, "Profiles.json"), JsonSerializer.Serialize(Profiles, JsonSettings.Options)).ConfigureAwait(false);
    }

}

