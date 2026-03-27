using DiplomObsAlarm.Services;
using Firebase.Database.Query;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using DiplomObsAlarm.Models;


namespace DiplomObsAlarm;

public partial class AdminEnterPage : ContentPage
{
    private const string FirebaseUrl = "https://obsalarm-23222-default-rtdb.europe-west1.firebasedatabase.app/users.json";
    private readonly HttpClient _httpClient;

    public AdminEnterPage()
    {
        InitializeComponent();
        GeneralSetting.Razmetka(40);
        _httpClient = new HttpClient();
    }



    private async void OnEnterClicked(object sender, EventArgs e)
    {
        string login = AdminLogin.Text;
        string password = AdminPassword.Text;

        var users = await App.Firebase.Child("users").OnceAsync<User>();

        var user = users.FirstOrDefault(u =>
            u.Object.Login == login &&
            u.Object.Password == password);

        if (user == null)
        {
            await DisplayAlert("Ошибка", "Неверный логин или пароль", "OK");
            return;
        }

        // Меняем activity на 1 в БД
        await App.Firebase.Child("users").Child(user.Key).Child("activity").PutAsync(1);

        // Сохраняем сессию
        Preferences.Set("IsLoged", 1);
        Preferences.Set("UserKey", user.Key);
        Preferences.Set("UserRole", user.Object.role);

        // Переход на панель по роли
        if (user.Object.role == "admin")
            Application.Current.MainPage = new AdminPanelPage(user.Key);
        else
            Application.Current.MainPage = new UserPanelPage(user.Key);
    }
}