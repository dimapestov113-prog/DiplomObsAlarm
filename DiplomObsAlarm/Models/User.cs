using System.Text.Json.Serialization;

namespace DiplomObsAlarm.Models
{
    public class User
    {
        // Не сохраняется в БД, только для работы в коде
        public string Id { get; set; }

        // Поля как в Firebase (с большой буквы кроме activity и room)
        [JsonPropertyName("Login")]
        public string Login { get; set; }

        [JsonPropertyName("Num")]
        public long Num { get; set; }

        [JsonPropertyName("Password")]
        public int Password { get; set; }

        [JsonPropertyName("Role")]
        public string Role { get; set; }

        [JsonPropertyName("activity")]
        public int Activity { get; set; }

        [JsonPropertyName("room")]
        public int Room { get; set; }

        // Только для отображения, не сохраняется в БД
        public string ActivityText => Activity == 1 ? "Да" : "Нет";
    }
}