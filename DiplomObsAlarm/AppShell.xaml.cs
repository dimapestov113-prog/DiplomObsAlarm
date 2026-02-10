using DiplomObsAlarm.Services;

namespace DiplomObsAlarm
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(AdminEnterPage), typeof(AdminEnterPage));
            Routing.RegisterRoute(nameof(AdminPanelPage), typeof(AdminPanelPage));
            Routing.RegisterRoute(nameof(UserEnterPage), typeof(UserEnterPage));

        }
        protected override async void OnNavigated(ShellNavigatedEventArgs args)
        {
            base.OnNavigated(args);

            // Только при первом запуске
            if (args.Current.Location.OriginalString == "//MainPage")
            {
                await CheckAuthAndRedirect();
            }
        }

        private async Task CheckAuthAndRedirect()
        {
            try
            {
                if (!AuthService.IsLoggedIn())
                    return;

                var role = AuthService.GetUserRole();

                // Небольшая задержка для инициализации
                await Task.Delay(100);

                switch (role)
                {
                    case AuthService.UserRole.Admin:
                        await GoToAsync("//AdminPanelPage");
                        break;
                    case AuthService.UserRole.User:
                        await GoToAsync("//UserPanelPage");
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auth error: {ex}");
            }
        }
    }
}