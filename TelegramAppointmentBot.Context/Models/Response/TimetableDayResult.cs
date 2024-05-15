using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramAppointmentBot.Context.Models.Response
{
    public class TimetableDayResult
    {
        public List<TimetableResult> appointments { get; set; }
        public string? denyCause { get; set; }
        public bool recordableDay { get; set; }
        public DateTime visitStart { get; set; }
        public DateTime visitEnd { get; set; }
    }
}
