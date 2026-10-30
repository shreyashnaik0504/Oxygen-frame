using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using OxygenFrame.Interop;
using OxygenFrame.Models;
using OxygenFrame.Services;

namespace OxygenFrame.Views;

public partial class WidgetWindow : Window
{
    private readonly ImageRotationService _rotation = new();
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(10) };
    private bool _showA = true;
    private bool _attached;
    private string _watchedFolder = "";
    private WidgetSettings S => SettingsStore.Current;

    public WidgetWindow()
    {
        InitializeComponent();
        _rotation.QueueChanged += (_, _) => Dispatcher.Invoke(ShowCurrentOrEmpty);
        _timer.Tick += (_, _) => Advance();
        SettingsStore.Changed += SettingsChanged;
        _watchedFolder = S.FolderPath;
        _rotation.SetFolder(_watchedFolder);
        ApplySettings();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var hwnd = new WindowInteropHelper(this).EnsureHandle();
        DesktopInterop.MakeToolWindow(this);
        _attached = DesktopInterop.AttachToWallpaper(this);
        ApplySettings();
        ShowCurrentOrEmpty();
        if (S.CycleEnabled) _timer.Start();
    }

    private void SettingsChanged(object? sender, WidgetSettings e) => Dispatcher.BeginInvoke(() =>
    {
        ApplySettings();
        if (!string.Equals(_watchedFolder, S.FolderPath, StringComparison.OrdinalIgnoreCase))
        {
            _watchedFolder = S.FolderPath;
            _rotation.SetFolder(_watchedFolder);
        }
        _timer.Interval = TimeSpan.FromSeconds(Math.Clamp(S.IntervalSeconds, 2, 3600));
        if (S.CycleEnabled && !_timer.IsEnabled) _timer.Start();
        if (!S.CycleEnabled) _timer.Stop();
    });

    private void ApplySettings()
    {
        Topmost = S.AlwaysOnTop;
        Width = Math.Clamp(S.WidgetWidth, 120, 1200);
        Height = Math.Clamp(S.WidgetHeight, 100, 1000);
        Left = S.WidgetLeft; Top = S.WidgetTop; Opacity = Math.Clamp(S.WidgetOpacity, 0.2, 1);
        Frame.CornerRadius = new CornerRadius(Math.Clamp(S.CornerRadius, 0, Math.Min(Width, Height) / 2));
        ShadowBorder.CornerRadius = Frame.CornerRadius;
        Frame.BorderThickness = new Thickness(S.ShowBorder ? Math.Clamp(S.BorderThickness, 0, 30) : 0);
        try { Frame.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(S.BorderColor1)); } catch { }
        DropShadow.BlurRadius = Math.Clamp(S.ShadowBlur, 0, 100);
        DropShadow.Opacity = Math.Clamp(S.ShadowOpacity, 0, 1);
        DropShadow.Direction = S.LightDirection;
        DropShadow.ShadowDepth = Math.Clamp(S.ShadowSpread * 20, 0, 50);
        DropShadow.Color = Colors.Black;
        DropShadow.Opacity = S.EnableDropShadow ? Math.Clamp(S.ShadowOpacity, 0, 1) : 0;
        ImageA.Stretch = ImageB.Stretch = S.FitMode == "Uniform" ? Stretch.Uniform :
            S.FitMode == "Fill" ? Stretch.Fill : Stretch.UniformToFill;
        FileLabel.Visibility = S.ShowFileName ? Visibility.Visible : Visibility.Collapsed;
        _timer.Interval = TimeSpan.FromSeconds(Math.Clamp(S.IntervalSeconds, 2, 3600));
        if (S.RetroPolaroid) Frame.Padding = new Thickness(2, 2, 2, S.PolaroidChin); else Frame.Padding = new Thickness(0);
        if (S.CyberpunkNeon) DropShadow.Color = ParseColor(S.BorderColor1);
        ShadowBorder.Background = S.Glassmorphism ? new SolidColorBrush(Color.FromArgb((byte)(Math.Clamp(S.GlassOpacity,0,1)*255), 30,35,45)) : new SolidColorBrush(Color.FromRgb(23,27,34));
    }

    private static Color ParseColor(string s) { try { return (Color)ColorConverter.ConvertFromString(s); } catch { return Colors.Lime; } }

    private void ShowCurrentOrEmpty()
    {
        var path = _rotation.CurrentPath();
        if (path is null || !File.Exists(path)) { EmptyState.Visibility = Visibility.Visible; return; }
        var bmp = ImageRotationService.LoadBitmap(path);
        if (bmp is null) { EmptyState.Visibility = Visibility.Visible; return; }
        EmptyState.Visibility = Visibility.Collapsed;
        var active = _showA ? ImageA : ImageB;
        active.Source = bmp;
        active.Opacity = 1;
        (_showA ? ImageB : ImageA).Opacity = 0;
        FileLabel.Text = System.IO.Path.GetFileName(path);
    }

    public void RefreshPhotos() => _rotation.Refresh();
    public void NextPhoto() => Advance();
    public void PreviousPhoto()
    {
        var path = _rotation.PreviousPath();
        ShowPath(path);
    }

    private void Advance()
    {
        if (S.PauseOnHover && IsMouseOver) return;
        var path = _rotation.NextPath();
        if (path is null) { ShowCurrentOrEmpty(); return; }
        ShowPath(path);
    }

    private void ShowPath(string? path)
    {
        if (path is null || !File.Exists(path)) { ShowCurrentOrEmpty(); return; }
        var bmp = ImageRotationService.LoadBitmap(path);
        if (bmp is null) { ShowCurrentOrEmpty(); return; }
        var incoming = _showA ? ImageB : ImageA;
        var outgoing = _showA ? ImageA : ImageB;
        incoming.Source = bmp;
        incoming.Opacity = 0;
        FileLabel.Text = System.IO.Path.GetFileName(path);
        var duration = TimeSpan.FromMilliseconds(S.AnimateTransitions ? Math.Clamp(S.TransitionMilliseconds, 100, 3000) : 1);
        incoming.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, duration) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
        outgoing.BeginAnimation(OpacityProperty, new DoubleAnimation(outgoing.Opacity, 0, duration));
        var old = outgoing.Source;
        var cleanup = new DispatcherTimer { Interval = duration };
        cleanup.Tick += (_, _) => { cleanup.Stop(); outgoing.Source = null; };
        cleanup.Start();
        _showA = !_showA;
    }

    private void Window_MouseEnter(object sender, MouseEventArgs e)
    {
        if (!S.EnableHoverEffect) return;
        var scale = new ScaleTransform(1, 1);
        Root.RenderTransformOrigin = new Point(.5, .5);
        Root.RenderTransform = scale;
        scale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1, S.HoverScale, TimeSpan.FromMilliseconds(180)) { EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut } });
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1, S.HoverScale, TimeSpan.FromMilliseconds(180)) { EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut } });
    }
    private void Window_MouseLeave(object sender, MouseEventArgs e)
    {
        if (Root.RenderTransform is ScaleTransform scale)
        {
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(220)));
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(220)));
        }
    }

    // Resize hit testing: eight edges/corners, in physical WPF device-independent pixels.
    private void Root_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        // Drag anywhere in the image. Edge resizing is initiated on mouse-down below.
    }
    private void Root_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var p = e.GetPosition(this);
        const double grip = 9;
        int hit = p.X < grip ? (p.Y < grip ? 13 : p.Y > ActualHeight-grip ? 16 : 10)
                : p.X > ActualWidth-grip ? (p.Y < grip ? 14 : p.Y > ActualHeight-grip ? 17 : 11)
                : p.Y < grip ? 12 : p.Y > ActualHeight-grip ? 15 : 2;
        if (hit != 2)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            NativeResize(hwnd, hit);
            e.Handled = true;
        }
        else if (S.KeepWidgetInteractive)
        {
            try { DragMove(); } catch (InvalidOperationException) { }
        }
    }

    private static void NativeResize(IntPtr hwnd, int hit)
    {
        const int WM_NCLBUTTONDOWN = 0x00A1;
        const int HTCAPTION = 2, HTLEFT = 10, HTRIGHT = 11, HTTOP = 12, HTTOPLEFT = 13,
            HTTOPRIGHT = 14, HTBOTTOM = 15, HTBOTTOMLEFT = 16, HTBOTTOMRIGHT = 17;
        int mapped = hit switch { 10=>HTLEFT, 11=>HTRIGHT, 12=>HTTOP, 13=>HTTOPLEFT, 14=>HTTOPRIGHT, 15=>HTBOTTOM, 16=>HTBOTTOMLEFT, 17=>HTBOTTOMRIGHT, _=>HTCAPTION };
        var p = new Point((int)Mouse.GetPosition(Application.Current.MainWindow).X, (int)Mouse.GetPosition(Application.Current.MainWindow).Y);
        // Release capture before requesting standard non-client resize.
        ReleaseCapture();
        SendMessage(hwnd, WM_NCLBUTTONDOWN, new IntPtr(mapped), IntPtr.Zero);
    }
    [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern bool ReleaseCapture();
    [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr h, int msg, IntPtr wp, IntPtr lp);

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        SettingsStore.Update(s => { s.WidgetLeft = Left; s.WidgetTop = Top; s.WidgetWidth = Width; s.WidgetHeight = Height; });
        _timer.Stop(); _rotation.Dispose(); SettingsStore.Changed -= SettingsChanged;
    }
}
