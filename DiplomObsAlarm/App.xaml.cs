using DiplomObsAlarm.Services;

namespace DiplomObsAlarm;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        //SecureStorage.Remove("userid");
        //SecureStorage.Remove("username");
        //SecureStorage.Remove("userrole");
        //SecureStorage.Remove("isloggedin");

        MainPage = new AppShell();
    }
}