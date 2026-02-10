using DiplomObsAlarm.Services;

namespace DiplomObsAlarm;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }
}