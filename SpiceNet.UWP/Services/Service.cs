using Microsoft.Extensions.DependencyInjection;
using SpiceNet.UWP.ViewModels;

namespace SpiceNet.UWP.Services;

public static class Service
{
    public static IServiceProvider Services { get; private set; } = null!;

    public static void BuildServices()
    {
        var collection = new ServiceCollection();

        collection.AddSingleton<SettingsService>();
        collection.AddSingleton<MainViewModel>();
        collection.AddTransient<RemoteDisplayViewModel>();

        collection.AddSingleton<ISettingsStorageService, SettingsStorageService>();
        collection.AddSingleton<IFilesService, FilesService>();

        Services = collection.BuildServiceProvider();
    }

    public static SettingsService Settings => Services.GetRequiredService<SettingsService>();
    public static MainViewModel MainPage => Services.GetRequiredService<MainViewModel>();
}
