using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramAppointmentBot.Context.Models;

namespace TelegramAppointmentBot.Service.Contract.Interfaces
{
    public interface IVisitService
    {
        public Task<Guid> AddVisit(Visit visit, CancellationToken cancellationToken);
        public Task<Visit> GetVisit(Guid id, CancellationToken cancellationToken);

    }
}
