using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramAppointmentBot.Context.Enums
{
    public enum InlineMode
    {
        AppointmentProfileId,
        AppointmentLPUs,
        AppointmentDoctor,
        AppointmentDay,
        AppointmentDates,
        AppointmentSpecialities,
        VisitsShow,
        DeleteVisit,
        ProfileInfo,
        ChangeProfile,
        DeleteProfile,

        ChangeTitle,
        ChangeOMS,
        ChangeSurname,
        ChangeName,
        ChangePatronomyc,
        ChangeEmail,
        ChangeBirthdate,


        HuntersShow,

        DeleteHunter,

        LpuVisitsShow
    }
}
