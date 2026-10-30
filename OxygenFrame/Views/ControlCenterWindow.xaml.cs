using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OxygenFrame.Services;

namespace OxygenFrame.Views;

public partial class ControlCenterWindow : Window
{
    private bool _loaded;
    public ControlCenterWindow() => InitializeComponent();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var s = SettingsStore.Current;
        _loaded = false;
        IntervalSlider.Value = s.IntervalSeconds; CornerSlider.Value = s.CornerRadius;
        BorderSlider.Value = s.BorderThickness; BlurSlider.Value = s.ShadowBlur;
        OpacitySlider.Value = s.ShadowOpacity; DirectionSlider.Value = s.LightDirection;
        WidgetOpacitySlider.Value = s.WidgetOpacity * 100;
        CycleCheck.IsChecked = s.CycleEnabled; PauseHoverCheck.IsChecked = s.PauseOnHover;
        FilenameCheck.IsChecked = s.ShowFileName; TopmostCheck.IsChecked = s.AlwaysOnTop;
        BorderCheck.IsChecked = s.ShowBorder; NeonCheck.IsChecked = s.CyberpunkNeon;
        PolaroidCheck.IsChecked = s.RetroPolaroid; GlassCheck.IsChecked = s.Glassmorphism;
        ShadowCheck.IsChecked = s.EnableDropShadow; HoverCheck.IsChecked = s.EnableHoverEffect;
        ShuffleCheck.IsChecked = s.RandomOrder; SubfoldersCheck.IsChecked = s.IncludeSubfolders;
        PauseButton.Content = s.CycleEnabled ? "Pause slideshow" : "Resume slideshow";
        SelectCombo(StrokeCombo, s.BorderStyle); SelectCombo(FitCombo, s.FitMode);
        FolderText.Text = s.FolderPath;
        UpdateStatus();
        _loaded = true;
    }

    private static void SelectCombo(ComboBox box, string text)
    {
        foreach (ComboBoxItem item in box.Items)
            if (string.Equals(item.Content?.ToString(), text, StringComparison.OrdinalIgnoreCase)) { box.SelectedItem = item; return; }
    }

    private void UpdateStatus()
    {
        StatusText.Text = Directory.Exists(SettingsStore.Current.FolderPath) ? "Your frame is live." : "Choose a photo folder to begin.";
        FolderText.Text = SettingsStore.Current.FolderPath;
    }
    private void Folder_Click(object sender, RoutedEventArgs e)
    {
        // FolderBrowserDialog from WinForms avoids a package dependency.
        using var dialog = new System.Windows.Forms.FolderBrowserDialog { Description = "Choose the folder for your photo rotation", UseDescriptionForTitle = true };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            SettingsStore.Update(s => s.FolderPath = dialog.SelectedPath);
            FolderText.Text = dialog.SelectedPath; UpdateStatus();
        }
    }
    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        var widget = Application.Current.Windows.OfType<WidgetWindow>().FirstOrDefault();
        widget?.RefreshPhotos();
        UpdateStatus();
    }
    private void Previous_Click(object sender, RoutedEventArgs e) => Application.Current.Windows.OfType<WidgetWindow>().FirstOrDefault()?.PreviousPhoto();
    private void Next_Click(object sender, RoutedEventArgs e) => Application.Current.Windows.OfType<WidgetWindow>().FirstOrDefault()?.NextPhoto();
    private void Pause_Click(object sender, RoutedEventArgs e)
    {
        SettingsStore.Update(s => s.CycleEnabled = !s.CycleEnabled);
        PauseButton.Content = SettingsStore.Current.CycleEnabled ? "Pause slideshow" : "Resume slideshow";
    }
    private void IntervalSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_loaded) return;
        var v = (int)Math.Round(IntervalSlider.Value); IntervalValue.Text = $"{v} seconds";
        SettingsStore.Update(s => s.IntervalSeconds = v);
    }
    private void CornerSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_loaded) return;
        var v = Math.Round(CornerSlider.Value); CornerValue.Text = $"{v:0} px";
        SettingsStore.Update(s => s.CornerRadius = v);
    }
    private void BorderSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_loaded) return;
        var v = Math.Round(BorderSlider.Value); BorderValue.Text = $"{v:0} px";
        SettingsStore.Update(s => s.BorderThickness = v);
    }
    private void StrokeCombo_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!_loaded || StrokeCombo.SelectedItem is not ComboBoxItem item) return;
        SettingsStore.Update(s => s.BorderStyle = item.Content?.ToString() ?? "Solid");
    }
    private void FitCombo_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!_loaded || FitCombo.SelectedItem is not ComboBoxItem item) return;
        SettingsStore.Update(s => s.FitMode = item.Content?.ToString() ?? "UniformToFill");
    }
    private void Effects_Changed(object sender, RoutedEventArgs e)
    {
        if (!_loaded) return;
        SettingsStore.Update(s =>
        {
            s.ShowBorder = BorderCheck.IsChecked == true;
            s.CyberpunkNeon = NeonCheck.IsChecked == true;
            s.RetroPolaroid = PolaroidCheck.IsChecked == true;
            s.Glassmorphism = GlassCheck.IsChecked == true;
            s.EnableDropShadow = ShadowCheck.IsChecked == true;
            s.EnableHoverEffect = HoverCheck.IsChecked == true;
        });
    }
    private void WidgetOpacity_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_loaded) return;
        var value = Math.Clamp(WidgetOpacitySlider.Value / 100.0, 0.2, 1.0);
        WidgetOpacityValue.Text = $"{value:P0}";
        SettingsStore.Update(s => s.WidgetOpacity = value);
    }
    private void Behavior_Changed(object sender, RoutedEventArgs e)
    {
        if (!_loaded) return;
        SettingsStore.Update(s =>
        {
            s.CycleEnabled = CycleCheck.IsChecked == true;
            s.PauseOnHover = PauseHoverCheck.IsChecked == true;
            s.ShowFileName = FilenameCheck.IsChecked == true;
            s.AlwaysOnTop = TopmostCheck.IsChecked == true;
        });
    }
    private void AdvancedBehavior_Changed(object sender, RoutedEventArgs e)
    {
        if (!_loaded) return;
        SettingsStore.Update(s => { s.RandomOrder = ShuffleCheck.IsChecked == true; s.IncludeSubfolders = SubfoldersCheck.IsChecked == true; });
        Application.Current.Windows.OfType<WidgetWindow>().FirstOrDefault()?.RefreshPhotos();
    }
    private void Lighting_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_loaded) return;
        SettingsStore.Update(s =>
        {
            s.ShadowBlur = BlurSlider.Value; s.ShadowOpacity = OpacitySlider.Value; s.LightDirection = DirectionSlider.Value;
        });
    }
    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Close_Click(object sender, RoutedEventArgs e) => Close();
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) Close();
        else if (e.Key == Key.N) Next_Click(this, new RoutedEventArgs());
        else if (e.Key == Key.P) Previous_Click(this, new RoutedEventArgs());
        else if (e.Key == Key.Space) Pause_Click(this, new RoutedEventArgs());
    }
}
