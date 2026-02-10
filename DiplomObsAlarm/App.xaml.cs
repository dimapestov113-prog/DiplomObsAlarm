using DiplomObsAlarm.Services;

namespace DiplomObsAlarm;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();

        // Запускаем на главном потоке после инициализации
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Task.Delay(100); // Небольшая задержка для инициализации Shell

            if (AuthService.IsLoggedIn())
            {
                await Shell.Current.GoToAsync("//AdminPanelPage");
            }
            else
            {
                await Shell.Current.GoToAsync("//MainPage");
            }
        });
    }
}