using DiplomObsAlarm.Services;

namespace DiplomObsAlarm
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();


            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(UserEnterPage), typeof(UserEnterPage));
            Routing.RegisterRoute(nameof(AdminEnterPage), typeof(AdminEnterPage));
            Routing.RegisterRoute(nameof(AdminPanelPage), typeof(AdminPanelPage));
        }
        public static void SetStartPage()
        {
            if (AuthService.IsLoggedIn())
            {
                Current.GoToAsync("//AdminPanelPage");
            }
            else
            {
                Current.GoToAsync("//MainPage");
            }
        }
    }
}