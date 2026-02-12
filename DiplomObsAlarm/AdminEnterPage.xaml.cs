using DiplomObsAlarm.Services;
using System.Text.Json;

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

    public class User
    {
        public string Name { get; set; }
        public string Login { get; set; }

        // Принимаем любой тип (число или строка)
        public JsonElement Num { get; set; }
        public JsonElement Password { get; set; }

        public string Role { get; set; }

        // Методы для получения строк
        public string GetNum()
        {
            if (Num.ValueKind == JsonValueKind.Number)
                return Num.GetInt32().ToString();
            if (Num.ValueKind == JsonValueKind.String)
                return Num.GetString();
            return Num.ToString();
        }

        public string GetPassword()
        {
            if (Password.ValueKind == JsonValueKind.Number)
                return Password.GetInt32().ToString();
            if (Password.ValueKind == JsonValueKind.String)
                return Password.GetString();
            return Password.ToString();
        }
    }

    // Кнопка НАЗАД — закрываем приложение
    private void OnExitClicked(object? sender, EventArgs e)
    {
        Application.Current?.Quit();
    }

    private async void OnEnterClicked(object? sender, EventArgs e)
    {
        string login = AdminLogin.Text?.Trim();

        if (string.IsNullOrEmpty(login))
        {
            await DisplayAlert("Ошибка", "Введите логин", "OK");
            return;
        }

        if (string.IsNullOrEmpty(AdminPassword.Text))
        {
            await DisplayAlert("Ошибка", "Введите пароль", "OK");
            return;
        }

        string password = AdminPassword.Text;

        try
        {
            var response = await _httpClient.GetStringAsync(FirebaseUrl);
            var users = JsonSerializer.Deserialize<Dictionary<string, User>>(response);

            if (users == null)
            {
                await DisplayAlert("Ошибка", "Не удалось загрузить данные", "OK");
                return;
            }

            // Ищем по логину и паролю (сравниваем как строки)
            var foundUser = users.FirstOrDefault(u =>
                u.Value.Login == login && u.Value.GetPassword() == password);

            if (foundUser.Value == null)
            {
                await DisplayAlert("Ошибка", "Неверный логин или пароль", "OK");
                return;
            }

            var user = foundUser.Value;
            var userId = foundUser.Key;

            // Сохраняем сессию
            AuthService.Login(userId, user.Name, user.Role);

            // Переход по роли
            if (user.Role == "admin")
            {
                await Shell.Current.GoToAsync("//AdminPanelPage");
            }
            else if (user.Role == "user")
            {
                await Shell.Current.GoToAsync("//UserPanelPage");
            }
            else
            {
                await DisplayAlert("Ошибка", "Неизвестная роль пользователя", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось подключиться: {ex.Message}", "OK");
        }
    }
}