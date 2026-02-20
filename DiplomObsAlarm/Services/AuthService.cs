using Microsoft.Maui.Storage;

namespace DiplomObsAlarm.Services;

public static class AuthService
{
    private const string UserIdKey = "userid";
    private const string UserLoginKey = "userlogin";
    private const string UserRoleKey = "userrole";
    private const string IsLoggedInKey = "isloggedin";

    public enum UserRole
    {
        None,
        Admin,
        User
    }

    public static void Login(string userId, string login, string role)
    {
        SecureStorage.Default.SetAsync(UserIdKey, userId);
        SecureStorage.Default.SetAsync(UserLoginKey, login);
        SecureStorage.Default.SetAsync(UserRoleKey, role);
        SecureStorage.Default.SetAsync(IsLoggedInKey, "true");
    }

    public static void Logout()
    {
        SecureStorage.Default.Remove(UserIdKey);
        SecureStorage.Default.Remove(UserLoginKey);
        SecureStorage.Default.Remove(UserRoleKey);
        SecureStorage.Default.Remove(IsLoggedInKey);
    }

    public static bool IsLoggedIn()
    {
        var loggedIn = SecureStorage.Default.GetAsync(IsLoggedInKey).Result;
        return loggedIn == "true";
    }

    public static string GetUserId()
    {
        return SecureStorage.Default.GetAsync(UserIdKey).Result ?? string.Empty;
    }

    public static UserRole GetUserRole()
    {
        var role = SecureStorage.Default.GetAsync(UserRoleKey).Result?.ToLower();
        return role switch
        {
            "admin" => UserRole.Admin,
            "user" => UserRole.User,
            _ => UserRole.None
        };
    }
}