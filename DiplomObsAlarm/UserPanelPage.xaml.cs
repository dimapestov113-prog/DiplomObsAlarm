using DiplomObsAlarm.Services;
using System.Text.Json;

namespace DiplomObsAlarm;

public partial class UserPanelPage : ContentPage
{
    private readonly HttpClient _httpClient;

    public UserPanelPage()
    {
        InitializeComponent();
        _httpClient = new HttpClient();
    }

    private async void OnExitClicked(object? sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Подтверждение", "Вы действительно хотите выйти?", "Да", "Нет");
        if (confirm)
        {
            // Обновляем activity на 0 (вышел)
            await UpdateUserActivity(AuthService.GetUserId(), 0);

            AuthService.Logout();
            await Shell.Current.GoToAsync("//AdminEnterPage");
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