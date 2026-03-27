using DiplomObsAlarm.Services;
using Firebase.Database;

namespace DiplomObsAlarm;


public partial class App : Application
{
    public static FirebaseClient Firebase { get; } = new FirebaseClient("https://obsalarm-23222-default-rtdb.europe-west1.firebasedatabase.app/");

    public App()
    {
        InitializeComponent();


        var isLoged = Preferences.Get("IsLoged", 0);
        var userKey = Preferences.Get("UserKey", "");
        var userRole = Preferences.Get("UserRole", "");

        if (isLoged == 1 && !string.IsNullOrEmpty(userKey))
        {
            MainPage = userRole == "admin"
                ? new AdminPanelPage(userKey)
                : new UserPanelPage(userKey);
        }
        else
        {
            MainPage = new AdminEnterPage();
        }
    }
}