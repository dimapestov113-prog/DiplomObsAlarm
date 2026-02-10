namespace DiplomObsAlarm;

public partial class UserEnterPage : ContentPage
{
	public UserEnterPage()
	{
		InitializeComponent();
		UserSetting.Razmetka(40);
    }
	public async void OnExitClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}
//AuthService.LoginUser(phoneNumber);  // <-- LoginUser
//await Shell.Current.GoToAsync("//UserPanelPage");