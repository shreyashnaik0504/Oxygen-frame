using System.IO;
using System.Text.Json;
using OxygenFrame.Models;

namespace OxygenFrame.Services;

public static class SettingsStore
{
    private static readonly object Gate = new();
    private static readonly string DirectoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OxygenFrame");
    public static readonly string FilePath = Path.Combine(DirectoryPath, "settings.json");
    public static WidgetSettings Current { get; private set; } = new();
    public static event EventHandler<WidgetSettings>? Changed;

    public static void Load()
    {
        lock (Gate)
        {
            try
            {
                Directory.CreateDirectory(DirectoryPath);
                if (File.Exists(FilePath))
                    Current = JsonSerializer.Deserialize<WidgetSettings>(File.ReadAllText(FilePath)) ?? new();
                else SaveCore();
            }
            catch { Current = new WidgetSettings(); }
        }
    }

    public static void Update(Action<WidgetSettings> change)
    {
        lock (Gate)
        {
            change(Current);
            try { SaveCore(); } catch { /* UI remains responsive; next change retries persistence. */ }
        }
        Changed?.Invoke(null, Current);
    }

    private static void SaveCore()
    {
        Directory.CreateDirectory(DirectoryPath);
        var temp = FilePath + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temp, FilePath, true);
    }
}
