using DiplomObsAlarm.Services;

namespace DiplomObsAlarm;

public partial class UserPanelPage : ContentPage
{
    public UserPanelPage()
    {
        InitializeComponent();
        UserSetting.Razmetka(40);
    }

    private async void OnExitClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Выход", "Выйти из аккаунта?", "Да", "Нет");

        if (confirm)
        {
            AuthService.Logout();
            await Shell.Current.GoToAsync("//AdminEnterPage");
        }
    }
}