using System.Windows.Media;

namespace OxygenFrame.Models;

/// <summary>Persisted settings. Keep this DTO free of WPF-only objects so JSON stays portable.</summary>
public sealed class WidgetSettings
{
    public string FolderPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
    public int IntervalSeconds { get; set; } = 10;
    public double CornerRadius { get; set; } = 28;
    public double BorderThickness { get; set; } = 2;
    public string BorderStyle { get; set; } = "Solid";
    public string BorderColor1 { get; set; } = "#FFB9F36B";
    public string BorderColor2 { get; set; } = "#FF7C8DFF";
    public double GradientAngle { get; set; } = 45;
    public bool RetroPolaroid { get; set; }
    public bool CyberpunkNeon { get; set; }
    public bool Glassmorphism { get; set; }
    public double ShadowBlur { get; set; } = 24;
    public double ShadowSpread { get; set; } = 0.2;
    public double ShadowOpacity { get; set; } = 0.45;
    public double LightDirection { get; set; } = 315;
    public string ViewportMode { get; set; } = "Fill";
    public bool LockAspectRatio { get; set; } = true;
    public bool BlurredBackdrop { get; set; }
    public bool AlwaysOnTop { get; set; }
    public bool StartWithWindows { get; set; }
    public double WidgetOpacity { get; set; } = 1;
    public double HoverScale { get; set; } = 1.025;
    public bool PauseOnHover { get; set; }
    public bool RandomOrder { get; set; }
    public bool IncludeSubfolders { get; set; }
    public bool ShowFileName { get; set; }
    public bool ShowCounter { get; set; }
    public bool RoundedImageClip { get; set; } = true;
    public bool AnimateTransitions { get; set; } = true;
    public int TransitionMilliseconds { get; set; } = 650;
    public double ImageBrightness { get; set; } = 1;
    public double ImageSaturation { get; set; } = 1;
    public double ImageContrast { get; set; } = 1;
    public string FitMode { get; set; } = "UniformToFill";
    public double WidgetWidth { get; set; } = 360;
    public double WidgetHeight { get; set; } = 260;
    public double WidgetLeft { get; set; } = 80;
    public double WidgetTop { get; set; } = 100;
    public string FramePreset { get; set; } = "Custom";
    public bool EnableHoverEffect { get; set; } = true;
    public bool EnableDropShadow { get; set; } = true;
    public bool CycleEnabled { get; set; } = true;
    public bool SkipUnreadableFiles { get; set; } = true;
    public bool UseHardwareRendering { get; set; } = true;
    public string Language { get; set; } = "en";
    public string Theme { get; set; } = "Oxygen";
    public bool MinimizeControlCenterToTray { get; set; }
    public bool ShowWidgetOnAllDesktops { get; set; } = true;
    public string LastSelectedPage { get; set; } = "Appearance";
    public int Padding { get; set; } = 10;
    public double PolaroidChin { get; set; } = 38;
    public double NeonGlow { get; set; } = 18;
    public double GlassOpacity { get; set; } = 0.22;
    public string ShadowColor { get; set; } = "#FF000000";
    public bool ShowBorder { get; set; } = true;
    public bool KeepWidgetInteractive { get; set; } = true;
    public bool RespectReducedMotion { get; set; }
    public int ImageLoadTimeoutMilliseconds { get; set; } = 3000;
    public bool LogDiagnostics { get; set; }
    public int MaxRetryCount { get; set; } = 2;
    public bool RememberWindowPosition { get; set; } = true;
    public bool AutoRefreshFolder { get; set; } = true;
    public string SortOrder { get; set; } = "Name";
    public bool ReverseOrder { get; set; }
    public bool UseExifOrientation { get; set; } = true;
    public bool ShowEmptyFolderHint { get; set; } = true;
    public double ImageCornerRadius { get; set; } = 20;
    public string AccentColor { get; set; } = "#FFB9F36B";
    public bool UseSystemAccent { get; set; }
    public int WatcherDebounceMilliseconds { get; set; } = 350;
    public bool PauseWhenFullscreenAppFocused { get; set; }
    public bool EnableKeyboardShortcuts { get; set; } = true;
    public bool CloseToBackground { get; set; } = true;
    public bool ConfirmReset { get; set; } = true;
    public string SettingsVersion { get; set; } = "1";
}
