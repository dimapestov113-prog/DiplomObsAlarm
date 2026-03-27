using DiplomObsAlarm.Models;
using DiplomObsAlarm.Services;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Controls;

namespace DiplomObsAlarm;

public partial class AdminPanelPage : ContentPage
{
    private readonly string _userKey;
    private string? _alertKey;
    private bool _isActive = false;


    private IDisposable? _alertStatusSubscription;
    // Флаги свитчей
    private bool _isTerrorOn = false;
    private bool _isFireOn = false;
    private bool _isBplaOn = false;
    private bool _isSmsEnabled = false;

    // 🔹 Для управления пользователями
    private string _currentFilter = "admin";
    private Dictionary<string, User> _usersCache = new();

    // 🔹 Для кэширования имён (история тревог)
    private Dictionary<string, string> _userNamesCache = new();
    private DateTime _cacheTimestamp = DateTime.MinValue;
    private const int CACHE_DURATION_MINUTES = 10;

    public AdminPanelPage(string userKey)
    {
        InitializeComponent();

        SettingAdmin.Razmetka(40);
        SettingAdmin2.Razmetka(10);
        SettingUsers.Razmetka(40);
        SettingHistory.Razmetka(40);

        _userKey = userKey;

        SetTabVisibility(TabType.Alert);
        CheckAlert();
        StartListeningForAlertStatus();
    }


    /// <summary>
    /// Слушаем изменения тревог в реальном времени (для обновления кнопок)
    /// </summary>
    private void StartListeningForAlertStatus()
    {
        try
        {
            _alertStatusSubscription = App.Firebase
                .Child("alerts")
                .AsObservable<Alert>()
                .Subscribe(alert =>
                {
                    if (alert.Object != null)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            if (alert.Object.Status == "active" && alert.Key == _alertKey)
                            {
                                // Тревога активна - обновляем кнопку
                                UpdateAlertButton(true);
                            }
                            else if (alert.Object.Status == "inactive" && alert.Key == _alertKey)
                            {
                                // Тревога отключена
                                _isActive = false;
                                _alertKey = null;
                                UpdateAlertButton(false);
                            }
                        });
                    }
                });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка подписки на тревоги: {ex.Message}");
        }
    }

    // В деструкторе или OnDisappearing:
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _alertStatusSubscription?.Dispose();
    }

    #region Управление пользователями

    /// <summary>
    /// Загружает пользователей из Firebase (как история тревог)
    /// </summary>
    private async Task LoadUsersAsync()
    {
        try
        {
            UsersStack.Children.Clear();
            UsersStack.Children.Add(new Label
            {
                Text = "Загрузка...",
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Colors.Gray
            });

            // 🔹 Загрузка как в истории (FirebaseObject<User>)
            var firebaseUsers = await App.Firebase
                .Child("users")
                .OnceAsync<User>();

            // 🔹 Фильтрация и извлечение User (как в истории)
            var filtered = firebaseUsers
                .Where(u => u.Object?.role == _currentFilter)
                .Select(u =>
                {
                    u.Object.Key = u.Key; // Сохраняем ключ Firebase в объект
                    return u.Object;
                })
                .OrderBy(u => u.Login)
                .ToList();

            UsersStack.Children.Clear();

            if (!filtered.Any())
            {
                UsersStack.Children.Add(new Label
                {
                    Text = _currentFilter == "admin" ? "Нет администраторов" : "Нет пользователей",
                    HorizontalOptions = LayoutOptions.Center,
                    TextColor = Colors.Gray,
                    Margin = new Thickness(0, 20)
                });
                return;
            }

            // 🔹 Создаём карточки как в истории
            foreach (var user in filtered)
            {
                var item = CreateUserCard(user);
                UsersStack.Children.Add(item);
            }
        }
        catch (Exception ex)
        {
            UsersStack.Children.Clear();
            UsersStack.Children.Add(new Label
            {
                Text = $"Ошибка: {ex.Message}",
                TextColor = Colors.Red,
                HorizontalOptions = LayoutOptions.Center
            });
        }
    }
    /// <summary>
    /// Создаёт карточку пользователя (с комнатой)
    /// </summary>
    private View CreateUserCard(User user)
    {
        var statusColor = user.activity == 1 ? Colors.Green : Colors.Gray;

        // 🔹 Индикатор активности
        var statusIndicator = new Border
        {
            WidthRequest = 12,
            HeightRequest = 12,
            BackgroundColor = statusColor,
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Margin = new Thickness(0, 4, 12, 4),
            VerticalOptions = LayoutOptions.Center
        };

        // 🔹 Логин
        var loginLabel = new Label
        {
            Text = user.Login,
            FontAttributes = FontAttributes.Bold,
            FontSize = 14,
            TextColor = Colors.Black
        };

        // 🔹 Телефон
        var phoneLabel = new Label
        {
            Text = !string.IsNullOrEmpty(user.Phone) ? $"{user.Phone}" : "Не указан",
            FontSize = 12,
            TextColor = Colors.Gray
        };

        // 🔹 Комната
        var roomLabel = new Label
        {
            Text = !string.IsNullOrEmpty(user.RoomNumber) ? $"Комната {user.RoomNumber}" : "Комната не указана",
            FontSize = 11,
            TextColor = Colors.Gray
        };

        // 🔹 Роль
        var roleLabel = new Label
        {
            Text = user.role == "admin" ? "Админ" : "Пользователь",
            FontSize = 11,
            TextColor = Colors.Gray
        };

        // 🔹 Кнопка редактирования
        var editBtn = new Button
        {
            Text = "✏️",
            FontSize = 18,
            WidthRequest = 50,
            HeightRequest = 40,
            CornerRadius = 8,
            BackgroundColor = Colors.LightBlue,
            TextColor = Colors.Black,
            Padding = new Thickness(5),
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center
        };
        editBtn.Clicked += (s, e) => ShowUserActionsModal(user);

        // 🔹 Сборка Grid (4 строки: логин, телефон, комната, роль)
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition(GridLength.Auto),    // 0: кружок
            new ColumnDefinition(GridLength.Star),    // 1: данные
            new ColumnDefinition(GridLength.Auto)     // 2: кнопка
        },
            RowDefinitions = new RowDefinitionCollection
        {
            new RowDefinition(GridLength.Auto),  // 0: логин
            new RowDefinition(GridLength.Auto),  // 1: телефон
            new RowDefinition(GridLength.Auto),  // 2: комната
            new RowDefinition(GridLength.Auto)   // 3: роль
        }
        };

        Grid.SetRow(statusIndicator, 0);
        Grid.SetColumn(statusIndicator, 0);
        Grid.SetRowSpan(statusIndicator, 4);
        grid.Children.Add(statusIndicator);

        Grid.SetRow(loginLabel, 0);
        Grid.SetColumn(loginLabel, 1);
        grid.Children.Add(loginLabel);

        Grid.SetRow(phoneLabel, 1);
        Grid.SetColumn(phoneLabel, 1);
        grid.Children.Add(phoneLabel);

        Grid.SetRow(roomLabel, 2);
        Grid.SetColumn(roomLabel, 1);
        grid.Children.Add(roomLabel);

        Grid.SetRow(roleLabel, 3);
        Grid.SetColumn(roleLabel, 1);
        grid.Children.Add(roleLabel);

        Grid.SetRow(editBtn, 0);
        Grid.SetColumn(editBtn, 2);
        Grid.SetRowSpan(editBtn, 4);
        grid.Children.Add(editBtn);

        return new Border
        {
            Stroke = Colors.Gray,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Padding = new Thickness(12),
            Margin = new Thickness(8, 4),
            BackgroundColor = Colors.White,
            Content = grid
        };
    }

    /// <summary>
    /// Переключение фильтра Админы/Пользователи
    /// </summary>
    private void FilterUsers_Clicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;

        // Сброс стилей
        if (BtnFilterAdmins != null) BtnFilterAdmins.BackgroundColor = Colors.White;
        if (BtnFilterUsers != null) BtnFilterUsers.BackgroundColor = Colors.White;

        // Применение фильтра
        if (btn == BtnFilterAdmins)
        {
            _currentFilter = "admin";
            btn.BackgroundColor = Colors.LightGray;
        }
        else
        {
            _currentFilter = "user";
            btn.BackgroundColor = Colors.LightGray;
        }

        LoadUsersAsync();
    }

    /// <summary>
    /// Кнопка "Добавить" — открывает модалку с пустой формой
    /// </summary>
    private async void AddUser_Clicked(object sender, EventArgs e)
    {
        await ShowUserActionsModal(null);
    }
    /// <summary>
    /// Модалка со всеми действиями (комната, пароль, телефон)
    /// </summary>
    private async Task ShowUserActionsModal(User? editUser)
    {
        var modalPage = new ContentPage
        {
            BackgroundColor = Colors.White,
            Title = editUser == null ? "Добавить участника" : "Редактировать участника"
        };

        var lblTitle = new Label
        {
            Text = editUser == null ? "Добавить участника" : "Редактировать участника",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 20, 0, 20),
            TextColor = Colors.Black
        };

        // 🔹 Логин
        var lblLogin = new Label { Text = "Логин *", TextColor = Colors.Black, FontSize = 14 };
        var entryLogin = new Entry
        {
            Placeholder = "Введите логин",
            Text = editUser?.Login,
            BackgroundColor = Colors.White,
            TextColor = Colors.Black,
            Margin = new Thickness(0, 0, 0, 15)
        };

        // 🔹 Пароль (всегда показываем, но для редактирования — необязательно)
        var lblPassword = new Label
        {
            Text = editUser == null ? "Пароль * (только цифры)" : "Пароль (оставьте пустым, чтобы не менять)",
            TextColor = Colors.Black,
            FontSize = 14
        };
        var entryPassword = new Entry
        {
            Placeholder = editUser == null ? "Пароль (цифры)" : "Новый пароль (необязательно)",
            BackgroundColor = Colors.White,
            TextColor = Colors.Black,
            IsPassword = true,
            Keyboard = Keyboard.Numeric,
            Margin = new Thickness(0, 0, 0, 15)
        };

        // 🔹 Телефон (простой ввод)
        var lblPhone = new Label { Text = "Телефон", TextColor = Colors.Black, FontSize = 14 };
        var entryPhone = new Entry
        {
            Placeholder = "79001234567",
            Text = editUser?.Phone,
            BackgroundColor = Colors.White,
            TextColor = Colors.Black,
            Keyboard = Keyboard.Telephone,
            MaxLength = 11,
            Margin = new Thickness(0, 0, 0, 15)
        };

        // 🔹 Номер комнаты
        var lblRoom = new Label { Text = "Номер комнаты", TextColor = Colors.Black, FontSize = 14 };
        var entryRoom = new Entry
        {
            Placeholder = "101",
            Text = editUser?.RoomNumber,
            BackgroundColor = Colors.White,
            TextColor = Colors.Black,
            Keyboard = Keyboard.Numeric,
            Margin = new Thickness(0, 0, 0, 15)
        };

        // 🔹 Роль
        var lblRole = new Label { Text = "Роль *", TextColor = Colors.Black, FontSize = 14 };
        var radioAdmin = new RadioButton
        {
            Content = "Администратор",
            Value = "admin",
            GroupName = "role",
            TextColor = Colors.Black,
            IsChecked = editUser?.role != "user"
        };
        var radioUser = new RadioButton
        {
            Content = "Пользователь",
            Value = "user",
            GroupName = "role",
            TextColor = Colors.Black,
            IsChecked = editUser?.role == "user"
        };

        string selectedRole = editUser?.role ?? "admin";
        radioAdmin.CheckedChanged += (s, e) => { if (e.Value) selectedRole = "admin"; };
        radioUser.CheckedChanged += (s, e) => { if (e.Value) selectedRole = "user"; };

        // 🔹 Активность
        var lblActive = new Label { Text = "Активен", TextColor = Colors.Black, FontSize = 14, VerticalOptions = LayoutOptions.Center };
        var switchActive = new Switch { IsToggled = editUser?.activity == 1, HorizontalOptions = LayoutOptions.Start };

        // 🔹 Кнопки
        var btnSave = new Button
        {
            Text = "💾 Сохранить",
            BackgroundColor = Colors.LightGreen,
            TextColor = Colors.Black,
            WidthRequest = 120,
            CornerRadius = 8,
            HeightRequest = 40
        };

        var btnDelete = new Button
        {
            Text = "🗑️ Удалить",
            BackgroundColor = Colors.LightCoral,
            TextColor = Colors.Black,
            WidthRequest = 120,
            CornerRadius = 8,
            HeightRequest = 40,
            IsVisible = editUser != null
        };

        var btnCancel = new Button
        {
            Text = "❌ Отмена",
            BackgroundColor = Colors.LightGray,
            TextColor = Colors.Black,
            WidthRequest = 120,
            CornerRadius = 8,
            HeightRequest = 40
        };

        // 🔹 Обработчик сохранения
        btnSave.Clicked += async (s, e) =>
        {
            // 🔥 Валидация логина
            if (string.IsNullOrWhiteSpace(entryLogin.Text))
            {
                await DisplayAlert("Ошибка", "Введите логин", "OK");
                return;
            }

            // 🔥 Валидация пароля
            if (editUser == null)
            {
                // Для нового пользователя пароль обязателен
                if (string.IsNullOrWhiteSpace(entryPassword.Text))
                {
                    await DisplayAlert("Ошибка", "Введите пароль", "OK");
                    return;
                }

                // Проверка: только цифры
                if (!entryPassword.Text.All(char.IsDigit))
                {
                    await DisplayAlert("Ошибка", "Пароль должен содержать только цифры!", "OK");
                    return;
                }

                // Минимум 4 цифры
                if (entryPassword.Text.Length < 4)
                {
                    await DisplayAlert("Ошибка", "Пароль должен содержать минимум 4 цифры", "OK");
                    return;
                }
            }
            else
            {
                // Для редактирования — если ввели пароль, проверяем
                if (!string.IsNullOrWhiteSpace(entryPassword.Text))
                {
                    if (!entryPassword.Text.All(char.IsDigit))
                    {
                        await DisplayAlert("Ошибка", "Пароль должен содержать только цифры!", "OK");
                        return;
                    }

                    if (entryPassword.Text.Length < 4)
                    {
                        await DisplayAlert("Ошибка", "Пароль должен содержать минимум 4 цифры", "OK");
                        return;
                    }
                }
            }

            // 🔥 Валидация телефона (если введён)
            if (!string.IsNullOrWhiteSpace(entryPhone.Text))
            {
                // Удаляем всё кроме цифр
                var phoneDigits = new string(entryPhone.Text.Where(char.IsDigit).ToArray());

                // Проверка: должно быть 11 цифр
                if (phoneDigits.Length != 11)
                {
                    await DisplayAlert("Ошибка", "Номер телефона должен содержать 11 цифр", "OK");
                    return;
                }

                // Проверка: начинается с 7 или 8
                if (phoneDigits[0] != '7' && phoneDigits[0] != '8')
                {
                    await DisplayAlert("Ошибка", "Номер телефона должен начинаться с 7 или 8", "OK");
                    return;
                }

                // Если начинается с 8, заменяем на 7
                if (phoneDigits[0] == '8')
                {
                    phoneDigits = "7" + phoneDigits.Substring(1);
                    entryPhone.Text = phoneDigits;
                }
            }

            try
            {
                if (editUser == null)
                {
                    // 🔹 Добавление нового
                    await App.Firebase.Child("users").PostAsync(new User
                    {
                        Login = entryLogin.Text.Trim(),
                        Password = entryPassword.Text,
                        Phone = entryPhone.Text?.Trim() ?? "",
                        RoomNumber = entryRoom.Text?.Trim() ?? "",
                        role = selectedRole,
                        activity = switchActive.IsToggled ? 1 : 0
                    });
                    await DisplayAlert("Успех", "Участник добавлен", "OK");
                }
                else
                {
                    // 🔹 Редактирование
                    var updateData = new Dictionary<string, object>
                    {
                        ["Login"] = entryLogin.Text.Trim(),
                        ["Phone"] = entryPhone.Text?.Trim() ?? "",
                        ["RoomNumber"] = entryRoom.Text?.Trim() ?? "",
                        ["role"] = selectedRole,
                        ["activity"] = switchActive.IsToggled ? 1 : 0
                    };

                    // 🔥 Если ввели новый пароль — обновляем
                    if (!string.IsNullOrWhiteSpace(entryPassword.Text))
                    {
                        updateData["Password"] = entryPassword.Text;
                    }

                    await App.Firebase.Child("users").Child(editUser.Key).PatchAsync(updateData);
                    await DisplayAlert("Успех", "Данные обновлены", "OK");
                }
                await modalPage.Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось сохранить: {ex.Message}", "OK");
            }
        };

        // 🔹 Обработчик удаления
        btnDelete.Clicked += async (s, e) =>
        {
            if (editUser == null) return;

            var confirm = await DisplayAlert("Подтверждение", "Удалить участника?", "Да", "Отмена");
            if (!confirm) return;

            try
            {
                await App.Firebase.Child("users").Child(editUser.Key).DeleteAsync();
                await DisplayAlert("Успех", "Участник удалён", "OK");
                await modalPage.Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось удалить: {ex.Message}", "OK");
            }
        };

        // 🔹 Обработчик отмены
        btnCancel.Clicked += async (s, e) => await modalPage.Navigation.PopModalAsync();

        // 🔹 Сборка формы
        var formLayout = new VerticalStackLayout
        {
            Children = {
            lblLogin, entryLogin,
            lblPassword, entryPassword,
            lblPhone, entryPhone,
            lblRoom, entryRoom,
            lblRole,
            new HorizontalStackLayout { Children = { radioAdmin, radioUser }, Spacing = 20, Margin = new Thickness(0, 0, 0, 15) },
            new HorizontalStackLayout { Children = { lblActive, switchActive }, Spacing = 10, Margin = new Thickness(0, 0, 0, 20) },
            new HorizontalStackLayout { Children = { btnSave, btnDelete, btnCancel }, Spacing = 10, HorizontalOptions = LayoutOptions.Center }
        },
            Padding = new Thickness(20),
            Spacing = 5
        };

        var scrollView = new ScrollView { Content = formLayout };

        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitionCollection { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) }
        };
        mainGrid.Children.Add(lblTitle);
        Grid.SetRow(lblTitle, 0);
        mainGrid.Children.Add(scrollView);
        Grid.SetRow(scrollView, 1);

        modalPage.Content = mainGrid;

        await Navigation.PushModalAsync(modalPage);
        modalPage.Disappearing += async (s, e) => await LoadUsersAsync();
    }
    #endregion

    #region Проверка и работа с тревогами

    private async void CheckAlert()
    {
        try
        {
            var alerts = await App.Firebase.Child("alerts").OnceAsync<Alert>();
            var active = alerts.FirstOrDefault(a => a.Object.Status == "active");

            if (active != null)
            {
                _alertKey = active.Key;
                _isActive = true;
                UpdateAlertButton(true);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось проверить тревоги: {ex.Message}", "OK");
        }
    }

    private string GetSelectedType()
    {
        var types = new List<string>();
        if (_isFireOn) types.Add("Пожар");
        if (_isBplaOn) types.Add("БПЛА");
        if (_isTerrorOn) types.Add("Террор");
        return string.Join(", ", types);
    }

    private async void SendAlert_Clicked(object sender, EventArgs e)
    {
        if (_isActive) await DisableAlert();
        else await EnableAlert();
    }
    private async Task EnableAlert()
    {
        var type = GetSelectedType();
        if (string.IsNullOrEmpty(type))
        {
            await DisplayAlert("Ошибка", "Выберите хотя бы один тип тревоги", "OK");
            return;
        }

        var confirm = await DisplayAlert("Подтверждение", $"Отправить тревогу типа: {type}?", "Да", "Нет");
        if (!confirm) return;

        try
        {
            var alert = new Alert
            {
                Status = "active",
                StartedBy = _userKey,
                StartedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Type = type
            };

            var result = await App.Firebase.Child("alerts").PostAsync(alert);
            _alertKey = result.Key;
            _isActive = true;
            UpdateAlertButton(true);

            // Отправляем Push ВСЕМ (пользователям и другим админам)
            await SendPushToAllUsers(type);

            await DisplayAlert("Успех", "Тревога активирована", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось создать тревогу: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Отправка Push-уведомлений ВСЕМ активным пользователям и админам
    /// </summary>
    private async Task SendPushToAllUsers(string alertType)
    {
        try
        {
            var users = await App.Firebase.Child("users").OnceAsync<User>();

            var activeUsers = users.Where(u =>
                u.Object?.activity == 1 &&
                u.Key != _userKey  // Не отправляем тому, кто нажал
            ).ToList();

            // Формируем уведомление
            var notification = new
            {
                title = "ТРЕВОГА",
                body = alertType,
                data = new
                {
                    type = "alert",
                    alertType,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                },
                // Для отправки через FCM нужно собрать токены
                // tokens = activeUsers.Select(u => u.Object.FcmToken).ToList()
            };

            // Здесь отправка через ваш сервер или Firebase Cloud Functions
            System.Diagnostics.Debug.WriteLine($"Push отправлен {activeUsers.Count} пользователям и админам");

            // Пример: await HttpClient.PostAsync("your-server/send-push", notification);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка отправки push: {ex.Message}");
        }
    }
    private async Task DisableAlert()
    {
        if (string.IsNullOrEmpty(_alertKey)) return;

        try
        {
            await App.Firebase.Child("alerts").Child(_alertKey).PatchAsync(new Dictionary<string, object>
            {
                ["Status"] = "inactive",
                ["StoppedBy"] = _userKey,
                ["StoppedAt"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });

            _isActive = false;
            _alertKey = null;
            UpdateAlertButton(false);
            await DisplayAlert("Успех", "Тревога отключена", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось отключить тревогу: {ex.Message}", "OK");
        }
    }

    private void UpdateAlertButton(bool isActive)
    {
        if (AlertButton == null) return;

        AlertButton.Text = isActive ? "Отключить тревогу" : "Отправить тревогу";
        AlertButton.BackgroundColor = isActive ? Colors.LightGray : Colors.White;
    }

    #endregion

    #region Обработчики свитчей

    private void TerrorSwitch_Toggled(object sender, ToggledEventArgs e) => _isTerrorOn = e.Value;
    private void FireSwitch_Toggled(object sender, ToggledEventArgs e) => _isFireOn = e.Value;
    private void BplaSwitch_Toggled(object sender, ToggledEventArgs e) => _isBplaOn = e.Value;
    private void SmsSwitch_Toggled(object sender, ToggledEventArgs e) => _isSmsEnabled = e.Value;

    #endregion

    #region Навигация по вкладкам

    private enum TabType { Alert, Users, History }

    private void SetTabVisibility(TabType activeTab)
    {
        opovesheniya.IsVisible = activeTab == TabType.Alert;
        AdminPanel.IsVisible = activeTab == TabType.Users;
        AdminHistory.IsVisible = activeTab == TabType.History;
    }

    private void AdminPanel_Clicked(object sender, EventArgs e) => SetTabVisibility(TabType.Alert);

    private void AdminUser_Clicked(object sender, EventArgs e)
    {
        SetTabVisibility(TabType.Users);
        LoadUsersAsync();
    }

    private void AdminHistory_Clicked(object sender, EventArgs e)
    {
        SetTabVisibility(TabType.History);
        LoadAlertHistory();
    }

    #endregion

    #region История тревог

    private async Task LoadAlertHistory()
    {
        try
        {
            HistoryStack.Children.Clear();
            HistoryStack.Children.Add(new Label
            {
                Text = "Загрузка...",
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Colors.Gray
            });

            await LoadUserNamesCacheAsync();

            var alerts = await App.Firebase.Child("alerts").OnceAsync<Alert>();

            var history = alerts
                .Where(a => !string.IsNullOrEmpty(a.Object.StartedAt))
                .Select(a =>
                {
                    a.Object.Key = a.Key;
                    return a.Object;
                })
                .OrderByDescending(a => DateTime.Parse(a.StartedAt))
                .ToList();

            HistoryStack.Children.Clear();

            if (!history.Any())
            {
                HistoryStack.Children.Add(new Label
                {
                    Text = "Нет записей за последний месяц",
                    HorizontalOptions = LayoutOptions.Center,
                    TextColor = Colors.Gray,
                    Margin = new Thickness(0, 20)
                });
                return;
            }

            foreach (var alert in history)
            {
                var item = CreateHistoryItem(alert);
                HistoryStack.Children.Add(item);
            }
        }
        catch (Exception ex)
        {
            HistoryStack.Children.Clear();
            HistoryStack.Children.Add(new Label
            {
                Text = $"Ошибка загрузки: {ex.Message}",
                TextColor = Colors.Red,
                HorizontalOptions = LayoutOptions.Center
            });
        }
    }

    private async Task LoadUserNamesCacheAsync()
    {
        if (DateTime.Now - _cacheTimestamp < TimeSpan.FromMinutes(CACHE_DURATION_MINUTES) && _userNamesCache.Any())
            return;

        try
        {
            var users = await App.Firebase.Child("users").OnceAsync<User>();
            _userNamesCache = users
                .Where(u => u.Object != null && !string.IsNullOrEmpty(u.Object.Login))
                .ToDictionary(u => u.Key, u => u.Object.Login);
            _cacheTimestamp = DateTime.Now;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Ошибка загрузки кэша пользователей: {ex.Message}");
        }
    }

    private string GetUserNameFromCache(string userKey)
    {
        if (string.IsNullOrEmpty(userKey)) return "—";
        return _userNamesCache.TryGetValue(userKey, out var login) ? login : userKey;
    }

    private View CreateHistoryItem(Alert alert)
    {
        var statusColor = alert.Status == "active" ? Colors.Orange : Colors.Green;
        var statusText = alert.Status == "active" ? "⚠ Активна" : "✓ Завершена";

        var startedByName = GetUserNameFromCache(alert.StartedBy);
        var stoppedByName = !string.IsNullOrEmpty(alert.StoppedBy) ? GetUserNameFromCache(alert.StoppedBy) : "—";

        var statusIndicator = new Border
        {
            WidthRequest = 12,
            HeightRequest = 12,
            BackgroundColor = statusColor,
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Margin = new Thickness(0, 4, 8, 4),
            VerticalOptions = LayoutOptions.Center
        };

        var typeLabel = new Label
        {
            Text = alert.Type,
            FontAttributes = FontAttributes.Bold,
            FontSize = 14,
            TextColor = Colors.Black
        };

        var dateLabel = new Label
        {
            Text = $"🕐 Начато: {FormatDate(alert.StartedAt)}",
            FontSize = 12,
            TextColor = Colors.DarkGray
        };

        var userLabel = new Label
        {
            Text = $"👤 Вкл: {startedByName} | Выкл: {stoppedByName}",
            FontSize = 11,
            TextColor = Colors.Gray
        };

        var statusBadge = new Label
        {
            Text = statusText,
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            BackgroundColor = statusColor,
            Padding = new Thickness(8, 4),
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)
            }
        };

        Grid.SetRow(statusIndicator, 0);
        Grid.SetColumn(statusIndicator, 0);
        Grid.SetRowSpan(statusIndicator, 3);
        grid.Children.Add(statusIndicator);

        Grid.SetRow(typeLabel, 0);
        Grid.SetColumn(typeLabel, 1);
        grid.Children.Add(typeLabel);

        Grid.SetRow(dateLabel, 1);
        Grid.SetColumn(dateLabel, 1);
        grid.Children.Add(dateLabel);

        Grid.SetRow(userLabel, 2);
        Grid.SetColumn(userLabel, 1);
        grid.Children.Add(userLabel);

        Grid.SetRow(statusBadge, 0);
        Grid.SetColumn(statusBadge, 2);
        Grid.SetRowSpan(statusBadge, 2);
        grid.Children.Add(statusBadge);

        return new Border
        {
            Stroke = Colors.Gray,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Padding = new Thickness(12),
            Margin = new Thickness(8, 4),
            BackgroundColor = Colors.White,
            Content = grid
        };
    }

    private string FormatDate(string dateTimeStr)
    {
        if (string.IsNullOrEmpty(dateTimeStr)) return "—";
        try
        {
            var dt = DateTime.Parse(dateTimeStr);
            return dt.ToString("dd.MM.yyyy HH:mm");
        }
        catch { return dateTimeStr; }
    }

    #endregion

    #region Выход из системы

    private async void OnExitClicked(object? sender, EventArgs e)
    {
        try
        {
            await App.Firebase.Child("users").Child(_userKey).Child("activity").PutAsync(0);
        }
        catch { }

        Preferences.Clear();
        Application.Current.MainPage = new AdminEnterPage();
    }

    #endregion
}