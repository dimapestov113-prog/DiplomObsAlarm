using System;
using System.Collections.Generic;
using System.Text;

namespace DiplomObsAlarm.Services
{
    public static class AuthService
    {
        private const string IsLoggedInKey = "IsLoggedIn";
        private const string AdminNameKey = "AdminName";

        /// <summary>
        /// Сохраняет вход админа
        /// </summary>
        public static void Login(string adminName)
        {
            Preferences.Set(IsLoggedInKey, true);
            Preferences.Set(AdminNameKey, adminName);
        }

        /// <summary>
        /// Выход админа
        /// </summary>
        public static void Logout()
        {
            Preferences.Remove(IsLoggedInKey);
            Preferences.Remove(AdminNameKey);
        }

        /// <summary>
        /// Проверяет, вошёл ли админ
        /// </summary>
        public static bool IsLoggedIn()
        {
            return Preferences.Get(IsLoggedInKey, false);
        }

        /// <summary>
        /// Получает имя админа
        /// </summary>
        public static string GetAdminName()
        {
            return Preferences.Get(AdminNameKey, string.Empty);
        }
    }
}
