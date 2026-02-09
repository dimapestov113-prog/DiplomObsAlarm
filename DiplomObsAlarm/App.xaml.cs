using DiplomObsAlarm.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DiplomObsAlarm
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            //MainPage = new NavigationPage(new MainPage());
            if (AuthService.IsLoggedIn())
            {
                // Сразу на панель админа
                MainPage = new NavigationPage(new AdminPanelPage());
            }
            else
            {
                // На страницу входа
                MainPage = new NavigationPage(new MainPage());
            }
        }

    }
}