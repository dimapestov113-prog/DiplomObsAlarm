using Firebase.Database;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Storage;
namespace DiplomObsAlarm;

public partial class AdminPanelPage : ContentPage
{
	private readonly FirebaseClient _firebase;
	public AdminPanelPage()
	{
		InitializeComponent();
		_firebase = new FirebaseClient("https://obsalarm-23222-default-rtdb.europe-west1.firebasedatabase.app/");
    SettingAdmin.Razmetka(40);
		SettingAdmin2.Razmetka(10);
		SettingUsers.Razmetka(40);
		SettingHistory.Razmetka(40);
		LoadUsers();
    }
    private async void LoadUsers()
    {
        try
        {
            var users = await _firebase
                .Child("users")
                .OnceAsync<User>();

            UsersStack.Children.Clear();

            foreach (var user in users)
            {
                System.Diagnostics.Debug.WriteLine($"Key: {user.Key}");
                System.Diagnostics.Debug.WriteLine($"Name: '{user.Object?.Name}'");
                System.Diagnostics.Debug.WriteLine($"Num: '{user.Object?.Num}'");

                var nameText = user.Object?.Name ?? "NULL";
                var numText = user.Object?.Num ?? "NULL";

                var panel = new Border
                {
                    Content = new HorizontalStackLayout
                    {
                        Spacing = 20,
                        VerticalOptions = LayoutOptions.Center,
                        Children =
                    {
                        new Label
                        {
                            Text = nameText,
                            FontSize = 16,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Colors.Black
                        },
                        new Label
                        {
                            Text = numText,
                            FontSize = 14,
                            TextColor = Colors.Gray
                        }
                    }
                    },
                    Padding = new Thickness(15),
                    Margin = new Thickness(5),
                    Background = Colors.White,
                    Stroke = Colors.LightGray,
                    StrokeThickness = 1,
                    StrokeShape = new RoundRectangle { CornerRadius = 5 }
                };

                UsersStack.Children.Add(panel);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }
    public class User
    {
        public string Name { get; set; }
        public string Num { get; set; }
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
        LoadUsers();
    }
    private void AdminHistory_Clicked(object sender, EventArgs e)
    {
        opovesheniya.IsVisible = false;
        AdminPanel.IsVisible = false;
        AdminHistory.IsVisible = true;
    }
}