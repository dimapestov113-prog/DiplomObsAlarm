namespace DiplomObsAlarm;

public partial class AdminEnterPage : ContentPage
{
	public AdminEnterPage()
    {
        InitializeComponent();
        GeneralSetting.Razmetka();//вызов в autorc 
    }
    private async void OnExitClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
    private async void OnEnterClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new AdminPanelPage());
    }
}