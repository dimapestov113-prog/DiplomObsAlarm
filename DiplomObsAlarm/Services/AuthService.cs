using Microsoft.Maui.Storage;

namespace DiplomObsAlarm.Services;

public static class AuthService
{
    private const string UserNameKey = "username";
    private const string UserRoleKey = "userrole";
    private const string IsLoggedInKey = "isloggedin";

    public enum UserRole
    {
        None,
        Admin,
        User
    }

    // Вход админа
    public static void LoginAdmin(string username)
    {
        SecureStorage.SetAsync(UserNameKey, username);
        SecureStorage.SetAsync(UserRoleKey, "admin");
        SecureStorage.SetAsync(IsLoggedInKey, "true");
    }

    // Вход пользователя
    public static void LoginUser(string username)
    {
        SecureStorage.SetAsync(UserNameKey, username);
        SecureStorage.SetAsync(UserRoleKey, "user");
        SecureStorage.SetAsync(IsLoggedInKey, "true");
    }

    public static void Logout()
    {
        SecureStorage.Remove(UserNameKey);
        SecureStorage.Remove(UserRoleKey);
        SecureStorage.Remove(IsLoggedInKey);
    }

    public static bool IsLoggedIn()
    {
        var loggedIn = SecureStorage.GetAsync(IsLoggedInKey).Result;
        return loggedIn == "true";
    }

    public static string GetUserName()
    {
        return SecureStorage.GetAsync(UserNameKey).Result ?? string.Empty;
    }

    public static UserRole GetUserRole()
    {
        var role = SecureStorage.GetAsync(UserRoleKey).Result;
        return role switch
        {
            "admin" => UserRole.Admin,
            "user" => UserRole.User,
            _ => UserRole.None
        };
    }
}