using Assembly.Projecto.Final.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembly.Projecto.Final.Domain.Core.Repositories
{
    public interface IAppointmentRepository:IRepository<Appointment, int>
    {
        public List<Appointment> GetAllWithParticipants();
        public Appointment? GetByIdWithParticipants(int id);
        public List<Appointment> GetBetweenToDates(DateTime startDate, DateTime endDate);
        public List<Appointment> GetAppointmentIntersections(DateTime date, TimeOnly hourStart, TimeOnly hourEnd);
        public List<Appointment> GetAppointmentsPaginationByEmployeeId(int employeeId, int pageNumber, int pageSize,
            string search);
        public int GetTotalCount(int employeeId, string search);
    }
}
