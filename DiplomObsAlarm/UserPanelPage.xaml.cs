using DiplomObsAlarm.Models;
using Firebase.Database;
using Firebase.Database.Query;
using Plugin.SimpleAudioPlayer;

namespace DiplomObsAlarm;

public partial class UserPanelPage : ContentPage
{
    private readonly string _userKey;
    private User? _currentUser;
    private IDisposable? _alertSubscription;
    private bool _isAlertActive = false;
    private bool _isDismissedLocally = false;
    private string? _currentAlertKey;
    private ISimpleAudioPlayer? _audioPlayer;

    public UserPanelPage(string userKey)
    {
        InitializeComponent();
        _userKey = userKey;
        LoadUserDataAndStartListening();
    }

    private async void LoadUserDataAndStartListening()
    {
        try
        {
            var firebaseUsers = await App.Firebase.Child("users").OnceAsync<User>();
            var firebaseUser = firebaseUsers.FirstOrDefault(u => u.Key == _userKey);

            if (firebaseUser?.Object != null)
            {
                _currentUser = firebaseUser.Object;
                LblLogin.Text = _currentUser.Login;
                LblPhone.Text = string.IsNullOrEmpty(_currentUser.Phone) ? "Телефон не указан" : _currentUser.Phone;
                LblRoom.Text = string.IsNullOrEmpty(_currentUser.RoomNumber) ? "Комната не указана" : $"Комната {_currentUser.RoomNumber}";
            }
            StartListeningForAlerts();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось загрузить данные: {ex.Message}", "OK");
        }
    }

    private async void StartListeningForAlerts()
    {
        try
        {
            _alertSubscription = App.Firebase
                .Child("alerts")
                .AsObservable<Alert>()
                .Subscribe(async alert =>
                {
                    if (alert.Object != null)
                    {
                        if (alert.Object.Status == "active" && alert.Object.StartedBy != _userKey)
                        {
                            await MainThread.InvokeOnMainThreadAsync(() => ShowAlert(alert.Key, alert.Object));
                        }
                        else if (alert.Object.Status == "inactive")
                        {
                            await MainThread.InvokeOnMainThreadAsync(() => ClearAlert());
                        }
                    }
                });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось подключиться: {ex.Message}", "OK");
        }
    }

    private async void ShowAlert(string alertKey, Alert alert)
    {
        if (_isAlertActive) return;

        _isAlertActive = true;
        _isDismissedLocally = false;
        _currentAlertKey = alertKey;

        ActivityIndicator.IsVisible = false;
        ActivityIndicator.IsRunning = false;
        LblStatus.IsVisible = false;

        AlertCard.IsVisible = true;
        LblAlertType.Text = $"ТРЕВОГА: {alert.Type}";
        LblAlertTime.Text = $"Начало: {FormatDate(alert.StartedAt)}";

        LblAlertStatus.Text = "АКТИВНА";
        LblAlertStatus.TextColor = Colors.Red;
        LblAlertStatus.IsVisible = true;
        BtnDismiss.IsVisible = true;

        PlayAlertSound();
        await SendPushNotification(alert.Type);
        await ShowAlertModal(alert);
    }

    /// <summary>
    /// Воспроизведение звука (ИСПРАВЛЕНО: зацикливание через событие)
    /// </summary>
    private async void PlayAlertSound()
    {
        try
        {
            StopAlertSound();

            _audioPlayer = CrossSimpleAudioPlayer.Current;

            // 🔁 Зацикливание через событие PlaybackEnded
            _audioPlayer.PlaybackEnded += OnPlaybackEnded;

            using var stream = await FileSystem.OpenAppPackageFileAsync("siren.mp3");
            _audioPlayer.Load(stream);
            _audioPlayer.Play();

            System.Diagnostics.Debug.WriteLine("🔊 Звук запущен");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка звука: {ex.Message}");
        }
    }

    /// <summary>
    /// Обработчик окончания воспроизведения (для зацикливания)
    /// </summary>
    private void OnPlaybackEnded(object? sender, EventArgs e)
    {
        // 🔁 Проигрываем снова, если тревога ещё активна и не отключена локально
        if (_isAlertActive && !_isDismissedLocally)
        {
            _audioPlayer?.Play();
        }
    }

    /// <summary>
    /// Остановка звука
    /// </summary>
    private void StopAlertSound()
    {
        try
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.PlaybackEnded -= OnPlaybackEnded; // 🔥 Отписываемся от события
                _audioPlayer.Stop();
                _audioPlayer.Dispose();
                _audioPlayer = null;
            }
            System.Diagnostics.Debug.WriteLine("🔇 Звук остановлен");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка остановки: {ex.Message}");
        }
    }

    private async Task ShowAlertModal(Alert alert)
    {
        var modal = new ContentPage
        {
            BackgroundColor = Colors.Black.WithAlpha(0.95f),
            Title = "ТРЕВОГА"
        };

        var layout = new VerticalStackLayout
        {
            Padding = 30,
            Spacing = 20,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label { Text = "ТРЕВОГА", FontSize = 32, FontAttributes = FontAttributes.Bold, TextColor = Colors.Red, HorizontalOptions = LayoutOptions.Center },
                new Label { Text = alert.Type, FontSize = 24, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center },
                new Label { Text = $"Начало: {FormatDate(alert.StartedAt)}", FontSize = 14, TextColor = Colors.LightGray, HorizontalOptions = LayoutOptions.Center },
                new Label { Text = "ВНИМАНИЕ!\n\nПроизошла чрезвычайная ситуация!\n\nСледуйте инструкциям администрации!", FontSize = 16, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 20, 0, 20) },
                new Button { Text = "ОТКЛЮЧИТЬ", BackgroundColor = Colors.Orange, TextColor = Colors.White, FontSize = 18, FontAttributes = FontAttributes.Bold, CornerRadius = 12, WidthRequest = 250, HeightRequest = 55, HorizontalOptions = LayoutOptions.Center }
            }
        };

        var btn = (Button)layout.Children.Last();
        btn.Clicked += async (s, e) => { await modal.Navigation.PopModalAsync(); DismissAlertLocally(); };

        modal.Content = layout;
        await Navigation.PushModalAsync(modal);
    }

    private void DismissAlertLocally()
    {
        _isDismissedLocally = true;
        BtnDismiss.IsVisible = false;
        LblAlertStatus.Text = "АКТИВНА (уведомление отключено)";
        LblAlertStatus.TextColor = Colors.Orange;
    }

    private void ClearAlert()
    {
        _isAlertActive = false;
        _isDismissedLocally = false;
        _currentAlertKey = null;

        AlertCard.IsVisible = false;
        BtnDismiss.IsVisible = false;
        LblAlertStatus.IsVisible = false;

        ActivityIndicator.IsVisible = true;
        ActivityIndicator.IsRunning = true;
        LblStatus.IsVisible = true;
        LblStatus.Text = "Ожидание тревог...";
        LblStatus.TextColor = Colors.Gray;

        StopAlertSound();
    }

    private async Task SendPushNotification(string alertType)
    {
        try { System.Diagnostics.Debug.WriteLine($"📱 Push: {alertType}"); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"❌ Push ошибка: {ex.Message}"); }
    }

    private async void OnExitClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert("Выход", "Выйти из приложения?", "Да", "Отмена");
        if (!confirm) return;

        try { await App.Firebase.Child("users").Child(_userKey).Child("activity").PutAsync(0); }
        catch { }

        StopAlertSound();
        Preferences.Clear();
        Application.Current.MainPage = new AdminEnterPage();
    }

    private string FormatDate(string dateTimeStr)
    {
        if (string.IsNullOrEmpty(dateTimeStr)) return "—";
        try { return DateTime.Parse(dateTimeStr).ToString("dd.MM.yyyy HH:mm"); }
        catch { return dateTimeStr; }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _alertSubscription?.Dispose();
        StopAlertSound();
    }
}