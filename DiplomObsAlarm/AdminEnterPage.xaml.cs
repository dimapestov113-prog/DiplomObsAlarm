using DiplomObsAlarm.Services;
using DiplomObsAlarm.Models;
using System.Text.Json;

namespace DiplomObsAlarm;

public partial class AdminEnterPage : ContentPage
{
    private const string FirebaseUrl = "https://obsalarm-23222-default-rtdb.europe-west1.firebasedatabase.app/users.json";
    private readonly HttpClient _httpClient;

    public AdminEnterPage()
    {
        InitializeComponent();
        _httpClient = new HttpClient();
    }

    private void OnExitClicked(object? sender, EventArgs e)
    {
        Application.Current?.Quit();
    }

    private async void OnEnterClicked(object? sender, EventArgs e)
    {
        string login = AdminLogin.Text?.Trim();
        string password = AdminPassword.Text;

        if (string.IsNullOrEmpty(login))
        {
            await DisplayAlert("Ошибка", "Введите логин", "OK");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Ошибка", "Введите пароль", "OK");
            return;
        }

        if (!int.TryParse(password, out int passwordInt))
        {
            await DisplayAlert("Ошибка", "Пароль должен содержать только цифры", "OK");
            return;
        }

        try
        {
            var response = await _httpClient.GetStringAsync(FirebaseUrl);
            var users = JsonSerializer.Deserialize<Dictionary<string, User>>(response);

            if (users == null)
            {
                await DisplayAlert("Ошибка", "Не удалось загрузить данные", "OK");
                return;
            }

            var foundUser = users.FirstOrDefault(u =>
                u.Value.Login == login && u.Value.Password == passwordInt);

            if (foundUser.Value == null)
            {
                await DisplayAlert("Ошибка", "Неверный логин или пароль", "OK");
                return;
            }

            var user = foundUser.Value;
            var userId = foundUser.Key;

            // Обновляем activity на 1 (вошел)
            await UpdateUserActivity(userId, 1);

            // Сохраняем сессию
            AuthService.Login(userId, user.Login, user.Role);

            // Переход по роли
            if (user.Role == "admin")
                await Shell.Current.GoToAsync("//AdminPanelPage");
            else
                await Shell.Current.GoToAsync("//UserPanelPage");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось подключиться: {ex.Message}", "OK");
        }
    }

    private async Task UpdateUserActivity(string userId, int activity)
    {
        try
        {
            var patchData = new { activity = activity };
            var json = JsonSerializer.Serialize(patchData);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PatchAsync(
                $"https://obsalarm-23222-default-rtdb.europe-west1.firebasedatabase.app/users/{userId}.json",
                content);

            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка обновления activity: {ex.Message}");
        }
    }
}