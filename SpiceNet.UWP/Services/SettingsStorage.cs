using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SpiceNet.UWP.Services;

public interface ISettingsStorageService
{
    T? GetObjectLocal<T>([CallerMemberName] string? key = null);

    T? GetObjectRoamed<T>([CallerMemberName] string? key = null);

    void DeleteObjectLocal(string key);

    void DeleteObjectRoamed(string key);

    void StoreObjectLocal(object obj, [CallerMemberName] string? key = null);

    void StoreObjectRoamed(object obj, [CallerMemberName] string? key = null);

    [return: NotNullIfNotNull(nameof(def))]
    T? GetObjectLocal<T>(T? def, [CallerMemberName] string? key = null);

    [return: NotNullIfNotNull(nameof(def))]
    T? GetObjectRoamed<T>(T? def, [CallerMemberName] string? key = null);

    bool ExistLocal(string key);
    bool ExistRoamed(string key);

}

public class SettingsStorageService : ISettingsStorageService
{
    private readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
    private readonly ApplicationDataContainer roamedSettings = ApplicationData.Current.RoamingSettings;

    private readonly Dictionary<string, object?> Cache = [];

    public T? GetObjectLocal<T>([CallerMemberName] string? key = null) => GetObjectLocal<T>(default, key);

    [return: NotNullIfNotNull(nameof(def))]
    public T? GetObjectLocal<T>(T? def, [CallerMemberName] string? key = null)
    {
        if (Cache.TryGetValue(key!, out var obj))
            return (T?)obj;
        var val = localSettings.Values[key];
        var t = val != null ? (T)val : def;
        Cache[key!] = t;
        return t;
    }

    public T? GetObjectRoamed<T>([CallerMemberName] string? key = null) => GetObjectRoamed<T>(default, key);

    [return: NotNullIfNotNull(nameof(def))]
    public T? GetObjectRoamed<T>(T? def, [CallerMemberName] string? key = null)
    {
        if (Cache.TryGetValue(key!, out var obj))
            return (T?)obj;
        var val = roamedSettings.Values[key];
        var t = val != null ? (T)val : def;
        Cache[key!] = t;
        return t;
    }

    public void StoreObjectLocal(object obj, [CallerMemberName] string? key = null) => Cache[key!] = localSettings.Values[key] = obj;

    public void StoreObjectRoamed(object obj, [CallerMemberName] string? key = null) => Cache[key!] = roamedSettings.Values[key] = obj;

    public void DeleteObjectLocal(string key)
    {
        Cache.Remove(key);
        localSettings.Values.Remove(key);
    }

    public void DeleteObjectRoamed(string key)
    {
        Cache.Remove(key);
        roamedSettings.Values.Remove(key);
    }

    public bool ExistLocal(string key) => localSettings.Values.ContainsKey(key);

    public bool ExistRoamed(string key) => roamedSettings.Values.ContainsKey(key);
}
