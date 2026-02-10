using DiplomObsAlarm.Services;
using System.Text.Json;

namespace DiplomObsAlarm;

public partial class AdminEnterPage : ContentPage
{
    private const string FirebaseUrl = "https://obsalarm-23222-default-rtdb.europe-west1.firebasedatabase.app/Admin.json";
    private readonly HttpClient _httpClient;

    public AdminEnterPage()
    {
        InitializeComponent();
        GeneralSetting.Razmetka(40);
        _httpClient = new HttpClient();
    }

    public class Admin
    {
        public string Name { get; set; }
        public int Password { get; set; }
    }

    // Кнопка НАЗАД — возврат на MainPage
    private async void OnExitClicked(object? sender, EventArgs e)
    {

        await Shell.Current.GoToAsync("//MainPage");
    }

    private async void OnEnterClicked(object? sender, EventArgs e)
    {
        string name = AdminLogin.Text?.Trim();

        if (string.IsNullOrEmpty(name))
        {
            await DisplayAlert("Ошибка", "Введите логин", "OK");
            return;
        }

        if (string.IsNullOrEmpty(AdminPassword.Text))
        {
            await DisplayAlert("Ошибка", "Введите пароль", "OK");
            return;
        }

        if (!int.TryParse(AdminPassword.Text, out int password))
        {
            await DisplayAlert("Ошибка", "Пароль должен быть числом", "OK");
            return;
        }

        try
        {
            var response = await _httpClient.GetStringAsync(FirebaseUrl);
            var admins = JsonSerializer.Deserialize<Dictionary<string, Admin>>(response);

            if (admins == null)
            {
                await DisplayAlert("Ошибка", "Не удалось загрузить данные", "OK");
                return;
            }

            bool found = admins.Values.Any(admin =>
                admin.Name == name && admin.Password == password);

            if (found)
            {
                AuthService.LoginAdmin(name);
                // Переход на панель админа (сбрасываем стек)
                await Shell.Current.GoToAsync("//AdminPanelPage");
            }
            else
            {
                await DisplayAlert("Ошибка", "Неверный логин или пароль", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось подключиться: {ex.Message}", "OK");
        }
    }
}