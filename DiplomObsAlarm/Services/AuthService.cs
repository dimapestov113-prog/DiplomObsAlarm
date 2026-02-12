using Microsoft.Maui.Storage;

namespace DiplomObsAlarm.Services;

public static class AuthService
{
    private const string UserIdKey = "userid";
    private const string UserNameKey = "username";
    private const string UserRoleKey = "userrole";
    private const string IsLoggedInKey = "isloggedin";

    public enum UserRole
    {
        None,
        Admin,
        User
    }

    // Универсальный вход
    public static void Login(string userId, string name, string role)
    {
        SecureStorage.SetAsync(UserIdKey, userId);
        SecureStorage.SetAsync(UserNameKey, name);
        SecureStorage.SetAsync(UserRoleKey, role);
        SecureStorage.SetAsync(IsLoggedInKey, "true");
    }

    public static void Logout()
    {
        SecureStorage.Remove(UserIdKey);
        SecureStorage.Remove(UserNameKey);
        SecureStorage.Remove(UserRoleKey);
        SecureStorage.Remove(IsLoggedInKey);
    }

    public static bool IsLoggedIn()
    {
        var loggedIn = SecureStorage.GetAsync(IsLoggedInKey).Result;
        return loggedIn == "true";
    }

    public static string GetUserId() =>
        SecureStorage.GetAsync(UserIdKey).Result ?? string.Empty;

    public static string GetUserName() =>
        SecureStorage.GetAsync(UserNameKey).Result ?? string.Empty;

    public static UserRole GetUserRole()
    {
        var role = SecureStorage.GetAsync(UserRoleKey).Result?.ToLower();
        return role switch
        {
            "admin" => UserRole.Admin,
            "user" => UserRole.User,
            _ => UserRole.None
        };
    }
}