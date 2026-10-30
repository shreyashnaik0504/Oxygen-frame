using System.Windows;
using OxygenFrame.Services;
using OxygenFrame.Views;

namespace OxygenFrame;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        SettingsStore.Load();
        var widget = new WidgetWindow();
        MainWindow = widget;
        widget.Show();
        var center = new ControlCenterWindow();
        center.Show();
    }

    private void Application_Startup(object sender, StartupEventArgs e) { }
}
