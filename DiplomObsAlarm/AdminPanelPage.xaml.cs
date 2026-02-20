using DiplomObsAlarm.Services;
using DiplomObsAlarm.Models;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Controls.Shapes;
using System.Text.Json;      
using System.Text;

namespace DiplomObsAlarm;

public partial class AdminPanelPage : ContentPage
{
    private readonly FirebaseClient _firebase;
    private List<User> _allUsers = new(); 
    private readonly HttpClient _httpClient;
    private string _currentFilterRole = "admin";

    // Фиксированные ширины столбцов
    private readonly double _loginWidth = 60;
    private readonly double _numWidth = 80;      // Увеличил для больших чисел
    private readonly double _passwordWidth = 53;
    private readonly double _roleWidth = 45;
    private readonly double _activityWidth = 50;
    private readonly double _roomWidth = 50;
    private readonly double _editWidth = 50;

    public AdminPanelPage()
    {
        InitializeComponent();
        // ИСПРАВЛЕНО: убран лишний пробел в URL
        _firebase = new FirebaseClient("https://obsalarm-23222-default-rtdb.europe-west1.firebasedatabase.app/");
        _httpClient = new HttpClient();
        SettingAdmin.Razmetka(40);
        SettingAdmin2.Razmetka(10);
        SettingUsers.Razmetka(40);
        SettingHistory.Razmetka(40);
    }

    private void CreateTableHeader()
    {
        var headerGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(new GridLength(_loginWidth)),
                new ColumnDefinition(new GridLength(_numWidth)),
                new ColumnDefinition(new GridLength(_passwordWidth)),
                new ColumnDefinition(new GridLength(_roleWidth)),
                new ColumnDefinition(new GridLength(_activityWidth)),
                new ColumnDefinition(new GridLength(_roomWidth)),
                new ColumnDefinition(new GridLength(_editWidth))
            },
            BackgroundColor = Color.FromArgb("#E0E0E0"),
            Padding = new Thickness(5, 3),
            HeightRequest = 35
        };

        AddHeaderCell(headerGrid, "ЛОГИН", 0);
        AddHeaderCell(headerGrid, "НОМЕР", 1);      // Было "ТЕЛЕФОН"
        AddHeaderCell(headerGrid, "ПАРОЛЬ", 2);
        AddHeaderCell(headerGrid, "РОЛЬ", 3);
        AddHeaderCell(headerGrid, "АКТИВЕН", 4);
        AddHeaderCell(headerGrid, "КОМНАТА", 5);
        AddHeaderCell(headerGrid, "", 6);

        UsersStack.Children.Add(headerGrid);
    }

    private void AddHeaderCell(Grid grid, string text, int column)
    {
        grid.Add(new Label
        {
            Text = text,
            FontAttributes = FontAttributes.Bold,
            FontSize = 9,
            TextColor = Colors.DarkGray,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start
        }, column, 0);
    }

    private async Task LoadAllUsers()
    {
        try
        {
            var users = await _firebase.Child("users").OnceAsync<User>();
            _allUsers = users.Select(u =>
            {
                u.Object.Id = u.Key;
                return u.Object;
            }).ToList();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось загрузить данные: {ex.Message}", "OK");
        }
    }

    private void DisplayUsers(string filterRole)
    {
        _currentFilterRole = filterRole;
        UsersStack.Children.Clear();
        CreateTableHeader();

        var filteredUsers = _allUsers.Where(u =>
            u.Role?.ToLower() == filterRole.ToLower()).ToList();

        if (!filteredUsers.Any())
        {
            UsersStack.Children.Add(new Label
            {
                Text = $"Нет пользователей с ролью '{filterRole}'",
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Colors.Gray,
                Margin = new Thickness(0, 20)
            });
            return;
        }

        foreach (var user in filteredUsers)
        {
            var row = CreateUserRow(user);
            UsersStack.Children.Add(row);
        }
    }

    private Border CreateUserRow(User user)
    {
        // Основной Grid: данные слева (растягиваются), кнопка справа (фиксировано)
        var mainGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition(GridLength.Star),      // Данные занимают всё доступное место
            new ColumnDefinition(new GridLength(_editWidth))  // Кнопка фиксированной ширины
        },
            Padding = new Thickness(5, 3)
        };

        // Grid для данных (внутри левой колонки)
        var dataGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition(new GridLength(_loginWidth)),
            new ColumnDefinition(new GridLength(_numWidth)),
            new ColumnDefinition(new GridLength(_passwordWidth)),
            new ColumnDefinition(new GridLength(_roleWidth)),
            new ColumnDefinition(new GridLength(_activityWidth)),
            new ColumnDefinition(new GridLength(_roomWidth))
        }
        };

        // Данные
        AddDataCell(dataGrid, TruncateText(user.Login ?? "-", 10), 0, true);
        AddDataCell(dataGrid, user.Num.ToString(), 1, false);
        AddDataCell(dataGrid, user.Password.ToString(), 2, false);
        AddDataCell(dataGrid, user.Role ?? "-", 3, false);
        AddDataCell(dataGrid, user.ActivityText, 4, false);
        AddDataCell(dataGrid, user.Room.ToString(), 5, false);

        // Кнопка редактирования — ПРИЖАТА К ПРАВОМУ КРАЮ
        var editBtn = new Button
        {
            Text = "✏️",
            FontSize = 10,
            WidthRequest = 36,
            HeightRequest = 36,
            BackgroundColor = Colors.LightBlue,
            CornerRadius = 18,
            HorizontalOptions = LayoutOptions.End,      // ← ПРИЖАТА ВПРАВО!
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 5, 0)          // Отступ справа 5px
        };
        editBtn.Clicked += (s, e) => EditUser(user);

        // Добавляем в основной Grid
        mainGrid.Add(dataGrid, 0, 0);   // Данные в колонку 0 (левая, растягивается)
        mainGrid.Add(editBtn, 1, 0);    // Кнопка в колонку 1 (правая, фиксировано)

        bool isEven = UsersStack.Children.Count % 2 == 0;

        return new Border
        {
            Content = mainGrid,
            BackgroundColor = isEven ? Colors.White : Color.FromArgb("#F8F8F8"),
            Stroke = Colors.LightGray,
            StrokeThickness = 0.5,
            Padding = new Thickness(0),
            Margin = new Thickness(0, 0, 0, 1)
        };
    }

    private void AddDataCell(Grid grid, string text, int column, bool isBold)
    {
        grid.Add(new Label
        {
            Text = text,
            FontSize = isBold ? 10 : 9,
            TextColor = isBold ? Colors.Black : Colors.Gray,
            FontAttributes = isBold ? FontAttributes.Bold : FontAttributes.None,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start,
            LineBreakMode = LineBreakMode.TailTruncation,
            MaxLines = 1
        }, column, 0);
    }

    private string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return "-";
        if (text.Length <= maxLength) return text;
        return text.Substring(0, maxLength - 2) + "..";
    }

    private async Task DeleteUser(User user)
    {
        bool confirm = await DisplayAlert("⚠️ УДАЛЕНИЕ", $"Удалить '{user.Login}'?", "Да", "Нет");
        if (!confirm) return;

        try
        {
            await _firebase.Child("users").Child(user.Id).DeleteAsync();
            _allUsers.Remove(user);
            DisplayUsers(_currentFilterRole);
            await DisplayAlert("Успех", "Пользователь удалён", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async void EditUser(User user)
    {
        var action = await DisplayActionSheet($"Пользователь: {user.Login}", "Отмена", "🗑️ УДАЛИТЬ",
            "Изменить логин", "Изменить пароль", "Изменить номер",
            "Изменить комнату", "Изменить активность", "Изменить роль");

        if (action == "Отмена") return;
        if (action == "🗑️ УДАЛИТЬ") { await DeleteUser(user); return; }

        try
        {
            switch (action)
            {
                case "Изменить логин":
                    string newLogin = await DisplayPromptAsync("Новый логин", "Введите логин:", initialValue: user.Login, maxLength: 10);
                    if (!string.IsNullOrWhiteSpace(newLogin) && newLogin != user.Login)
                    {
                        if (_allUsers.Any(u => u.Login?.ToLower() == newLogin.ToLower() && u.Id != user.Id))
                        {
                            await DisplayAlert("Ошибка", "Логин занят", "OK");
                            return;
                        }
                        user.Login = newLogin;
                        await UpdateUser(user);
                    }
                    break;

                case "Изменить пароль":
                    string pass = await DisplayPromptAsync("Новый пароль", "Цифры:", initialValue: user.Password.ToString());
                    if (int.TryParse(pass, out int newPass) && newPass != user.Password)
                    {
                        user.Password = newPass;
                        await UpdateUser(user);
                    }
                    break;

                // ИСПРАВЛЕНО: "Изменить телефон" → "Изменить номер", убрано форматирование
                case "Изменить номер":
                    string numStr = await DisplayPromptAsync("Новый номер", "Введите число:", initialValue: user.Num.ToString());
                    if (long.TryParse(numStr, out long newNum) && newNum != user.Num)
                    {
                        user.Num = newNum;
                        await UpdateUser(user);
                    }
                    break;

                case "Изменить комнату":
                    string room = await DisplayPromptAsync("Новая комната", "Номер:", initialValue: user.Room.ToString());
                    if (int.TryParse(room, out int newRoom) && newRoom != user.Room)
                    {
                        user.Room = newRoom;
                        await UpdateUser(user);
                    }
                    break;

                case "Изменить активность":
                    user.activity = user.activity == 1 ? 0 : 1;
                    await UpdateUser(user);
                    break;

                case "Изменить роль":
                    string role = await DisplayActionSheet("Новая роль", "Отмена", null, "admin", "user");
                    if (role != "Отмена" && !string.IsNullOrEmpty(role) && role != user.Role)
                    {
                        user.Role = role;
                        await UpdateUser(user);
                        await LoadAllUsers();
                        DisplayUsers(_currentFilterRole);
                        return;
                    }
                    break;
            }
            DisplayUsers(_currentFilterRole);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async Task UpdateUser(User user)
    {
        await _firebase.Child("users").Child(user.Id).PutAsync(user);
    }

    private async void AddUser_Clicked(object sender, EventArgs e)
    {
        string role = await DisplayActionSheet("Роль", "Отмена", null, "admin", "user");
        if (role == "Отмена" || string.IsNullOrEmpty(role)) return;

        string login = await DisplayPromptAsync("Логин", "Введите:", maxLength: 10);
        if (string.IsNullOrWhiteSpace(login) || _allUsers.Any(u => u.Login?.ToLower() == login.ToLower()))
        {
            await DisplayAlert("Ошибка", "Логин пуст или занят", "OK");
            return;
        }

        string passStr = await DisplayPromptAsync("Пароль", "Цифры:", maxLength: 20);
        if (!int.TryParse(passStr, out int password))
        {
            await DisplayAlert("Ошибка", "Только цифры", "OK");
            return;
        }

        // ИСПРАВЛЕНО: убрано форматирование телефона, Num теперь long
        string numStr = await DisplayPromptAsync("Номер", "Введите число:", maxLength: 15);
        if (!long.TryParse(numStr, out long num))
        {
            await DisplayAlert("Ошибка", "Только цифры", "OK");
            return;
        }

        string roomStr = await DisplayPromptAsync("Комната", "Номер:", maxLength: 3);
        if (!int.TryParse(roomStr, out int room))
        {
            await DisplayAlert("Ошибка", "Только цифры", "OK");
            return;
        }

        if (!await DisplayAlert("Подтверждение", $"{role}: {login}\nДобавить?", "Да", "Нет")) return;

        // ИСПРАВЛЕНО: Num = num (long), не строка
        var newUser = new User
        {
            Login = login,
            Password = password,
            Num = num,
            Role = role,
            activity = 0,
            Room = room
        };

        try
        {
            var result = await _firebase.Child("users").PostAsync(newUser);
            newUser.Id = result.Key;
            _allUsers.Add(newUser);
            DisplayUsers(role);
            await DisplayAlert("Успех", "Добавлен", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async void AdminUser_Clicked(object sender, EventArgs e)
    {
        opovesheniya.IsVisible = false;
        AdminPanel.IsVisible = true;
        AdminHistory.IsVisible = false;
        if (!_allUsers.Any()) await LoadAllUsers();
        DisplayUsers("admin");
    }

    private async void UserUser_Clicked(object sender, EventArgs e)
    {
        opovesheniya.IsVisible = false;
        AdminPanel.IsVisible = true;
        AdminHistory.IsVisible = false;
        if (!_allUsers.Any()) await LoadAllUsers();
        DisplayUsers("user");
    }

    private async void OnExitClicked(object? sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Подтверждение", "Вы действительно хотите выйти?", "Да", "Нет");
        if (!confirm) return;

        // Получаем ID до выхода
        var userId = AuthService.GetUserId();

        // Обновляем activity на 0 (вышел)
        if (!string.IsNullOrEmpty(userId))
        {
            await UpdateUserActivity(userId, 0);
        }

        // Выходим
        AuthService.Logout();
        await Shell.Current.GoToAsync("//AdminEnterPage");
    }

    // Тот же метод что и в AdminEnterPage
    private async Task UpdateUserActivity(string userId, int activity)
    {
        try
        {
            await _firebase.Child("users").Child(userId).Child("activity").PutAsync(activity);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка обновления activity: {ex.Message}");
        }
    }

    private void TerrorSwitch_Toggled(object sender, ToggledEventArgs e) { }
    private void FireSwitch_Toggled(object sender, ToggledEventArgs e) { }
    private void BplaSwitch_Toggled(object sender, ToggledEventArgs e) { }

    private void AdminPanel_Clicked(object sender, EventArgs e)
    {
        opovesheniya.IsVisible = true;
        AdminPanel.IsVisible = false;
        AdminHistory.IsVisible = false;
    }

    private void AdminHistory_Clicked(object sender, EventArgs e)
    {
        opovesheniya.IsVisible = false;
        AdminPanel.IsVisible = false;
        AdminHistory.IsVisible = true;
    }
}