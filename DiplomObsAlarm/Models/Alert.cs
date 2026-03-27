using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DiplomObsAlarm.Models
{// Models/Alert.cs
    public class Alert
    {
        public string Key { get; set; } // 🔥 Важно: ключ записи в Firebase

        public string Status { get; set; }
        public string StartedBy { get; set; }
        public string StartedAt { get; set; }
        public string? StoppedBy { get; set; }
        public string? StoppedAt { get; set; }
        public string Type { get; set; }

        // 🔥 Вспомогательное свойство для отображения
        [JsonIgnore]
        public string Duration
        {
            get
            {
                if (string.IsNullOrEmpty(StartedAt) || string.IsNullOrEmpty(StoppedAt))
                    return "—";

                try
                {
                    var start = DateTime.Parse(StartedAt);
                    var stop = DateTime.Parse(StoppedAt);
                    var diff = stop - start;
                    return $"{diff.Minutes} мин {diff.Seconds} сек";
                }
                catch { return "—"; }
            }
        }
    }
}
