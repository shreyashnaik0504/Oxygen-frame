using System.IO;
using System.Windows.Media.Imaging;
using OxygenFrame.Services;

namespace OxygenFrame.Services;

/// <summary>Stores paths, not decoded bitmaps. Each decoded image is frozen and released after transition.</summary>
public sealed class ImageRotationService : IDisposable
{
    private readonly object _gate = new();
    private FileSystemWatcher? _watcher;
    private Timer? _debounce;
    private List<string> _paths = new();
    private int _index = -1;
    private string _folder = "";
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp" };

    public event EventHandler? QueueChanged;
    public int Count { get { lock (_gate) return _paths.Count; } }

    public void SetFolder(string folder)
    {
        folder = Environment.ExpandEnvironmentVariables(folder ?? "");
        lock (_gate)
        {
            _folder = folder;
            _watcher?.Dispose();
            _watcher = null;
            if (Directory.Exists(folder))
            {
                _watcher = new FileSystemWatcher(folder)
                {
                    IncludeSubdirectories = SettingsStore.Current.IncludeSubfolders,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                    EnableRaisingEvents = true
                };
                _watcher.Created += OnFsChanged; _watcher.Deleted += OnFsChanged;
                _watcher.Renamed += OnRenamed; _watcher.Changed += OnFsChanged;
            }
        }
        Refresh();
    }

    private void OnFsChanged(object? s, FileSystemEventArgs e) => Debounce();
    private void OnRenamed(object? s, RenamedEventArgs e) => Debounce();
    private void Debounce()
    {
        _debounce?.Dispose();
        _debounce = new Timer(_ => Refresh(), null,
            Math.Clamp(SettingsStore.Current.WatcherDebounceMilliseconds, 100, 3000), Timeout.Infinite);
    }

    public void Refresh()
    {
        List<string> found;
        string folder;
        lock (_gate) folder = _folder;
        try
        {
            if (!Directory.Exists(folder)) found = new();
            else
            {
                var option = SettingsStore.Current.IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                found = Directory.EnumerateFiles(folder, "*", option)
                    .Where(p => Extensions.Contains(Path.GetExtension(p)))
                    .OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
                if (SettingsStore.Current.ReverseOrder) found.Reverse();
                if (SettingsStore.Current.RandomOrder)
                    found = found.OrderBy(_ => Random.Shared.Next()).ToList();
            }
        }
        catch (IOException) { return; }
        catch (UnauthorizedAccessException) { return; }

        lock (_gate)
        {
            var current = _index >= 0 && _index < _paths.Count ? _paths[_index] : null;
            _paths = found;
            _index = current is null ? -1 : _paths.IndexOf(current);
            if (_index < 0 && _paths.Count > 0) _index = 0;
        }
        QueueChanged?.Invoke(this, EventArgs.Empty);
    }

    public string? NextPath()
    {
        lock (_gate)
        {
            if (_paths.Count == 0) return null;
            _index = (_index + 1) % _paths.Count;
            return _paths[_index];
        }
    }

    public string? PreviousPath()
    {
        lock (_gate)
        {
            if (_paths.Count == 0) return null;
            _index = (_index - 1 + _paths.Count) % _paths.Count;
            return _paths[_index];
        }
    }

    public string? CurrentPath()
    {
        lock (_gate) return _index >= 0 && _index < _paths.Count ? _paths[_index] : null;
    }

    public static BitmapSource? LoadBitmap(string path)
    {
        try
        {
            // OnLoad closes the file immediately; decode at display size where possible.
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.OnLoad);
            var frame = decoder.Frames.FirstOrDefault();
            if (frame is null) return null;
            var bmp = new WriteableBitmap(frame);
            bmp.Freeze();
            return bmp;
        }
        catch { return null; }
    }

    public void Dispose()
    {
        lock (_gate) { _watcher?.Dispose(); _debounce?.Dispose(); }
    }
}
