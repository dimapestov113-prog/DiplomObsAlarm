namespace DiplomObsAlarm;

public partial class AdminPanelPage : ContentPage
{
	public AdminPanelPage()
	{
		InitializeComponent();
		SettingAdmin.Razmetka(40);
		SettingAdmin2.Razmetka(10);
		SettingUsers.Razmetka(40);
		SettingHistory.Razmetka(40);

    }

	private async void OnExitClicked(object? sender, EventArgs e)
	{
		//await DisplayAlertAsync("нет", "нет", "ок00");
		await Navigation.PopModalAsync();
	}
	private void TerrorSwitch_Toggled()
	{

	}
	private void FireSwitch_Toggled()
	{

	}
	private void BplaSwitch_Toggled()
	{

	}

    private void AdminPanel_Clicked(object sender, EventArgs e)
    {
		opovesheniya.IsVisible = true;
        AdminPanel.IsVisible = false;
		AdminHistory.IsVisible = false;
    }
    private void AdminUser_Clicked(object sender, EventArgs e)
    {
        opovesheniya.IsVisible = false;
		AdminPanel.IsVisible = true;
        AdminHistory.IsVisible = false;
    }
    private void AdminHistory_Clicked(object sender, EventArgs e)
    {
        opovesheniya.IsVisible = false;
        AdminPanel.IsVisible = false;
        AdminHistory.IsVisible = true;
    }
}