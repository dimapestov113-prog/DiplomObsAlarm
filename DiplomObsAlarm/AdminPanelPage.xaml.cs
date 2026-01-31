namespace DiplomObsAlarm;

public partial class AdminPanelPage : ContentPage
{
	public AdminPanelPage()
	{
		InitializeComponent();
		SettingAdmin.Razmetka(40);
		SettingAdmin2.Razmetka(40);

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

    private void Button_Clicked(object sender, EventArgs e)
    {

    }
}