
namespace DiplomObsAlarm
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnAdminClicked(object sender, EventArgs e)
        {
            await DisplayAlertAsync("Кнопка","Админ","Ок");
        }
        private async void OnUserClicked(object? sender, EventArgs e)
        {
            await DisplayAlertAsync("Кнопка", "Пользователь", "Ок");
        }
    }
}
