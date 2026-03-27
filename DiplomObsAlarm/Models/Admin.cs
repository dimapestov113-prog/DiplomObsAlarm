using System;
using System.Collections.Generic;
using System.Text;

namespace DiplomObsAlarm.Models
{
    public class User
    {
        public string Key { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int activity { get; set; }
        public string role { get; set; }
        public string Phone { get; set; }
        public string RoomNumber { get; set; }
    }
}