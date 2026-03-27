using DiplomObsAlarm.Services;

namespace DiplomObsAlarm;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        FlyoutBehavior = FlyoutBehavior.Disabled;
        Routing.RegisterRoute(nameof(AdminEnterPage), typeof(AdminEnterPage));
        Routing.RegisterRoute(nameof(AdminPanelPage), typeof(AdminPanelPage));
        Routing.RegisterRoute(nameof(UserPanelPage), typeof(UserPanelPage));

    }

    // Переопределяем метод появления страницы
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Проверяем авторизацию только после появления Shell
        await CheckAuth();
    }

    private async Task CheckAuth()
    {
        // Даём время на полную инициализацию UI
        await Task.Delay(100);

        if (!AuthService.IsLoggedIn())
        {
            await GoToAsync("//AdminEnterPage");
            return;
        }

        var role = AuthService.GetUserRole();

        switch (role)
        {
            case AuthService.UserRole.Admin:
                await GoToAsync("//AdminPanelPage");
                break;
            case AuthService.UserRole.User:
                await GoToAsync("//UserPanelPage");
                break;
            default:
                await GoToAsync("//AdminEnterPage");
                break;
        }
    }
}