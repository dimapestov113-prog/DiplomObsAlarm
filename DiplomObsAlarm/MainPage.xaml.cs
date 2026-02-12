namespace DiplomObsAlarm
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
            GeneralSeter.Razmetka(40);
        }

        private async void OnAdminClicked(object? sender, EventArgs e)
        {
            //await DisplayAlertAsync("Кнопка","Админ","Ок");
            await Navigation.PushModalAsync(new AdminEnterPage());
        }
        private async void OnUserClicked(object? sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new UserPanelPage());
            //await DisplayAlertAsync("Кнопка", "Пользователь", "Ок");
        }
    }
}
