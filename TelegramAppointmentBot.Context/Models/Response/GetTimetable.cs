using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramAppointmentBot.Context.Models.Response
{
    public class GetTimetable : GorzdravResponse
    {
        public List<TimetableDayResult> result { get; set; }

    }
}
