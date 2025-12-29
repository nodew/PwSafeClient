using PwSafeClient.AppCore.Configuration;

namespace PwSafeClient.Maui.Services;

public sealed class MauiAppPaths : IAppPaths
{
    public string AppDataDirectory => FileSystem.AppDataDirectory;
}
