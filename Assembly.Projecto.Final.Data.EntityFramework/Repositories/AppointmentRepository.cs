using Assembly.Projecto.Final.Data.EntityFramework.Context;
using Assembly.Projecto.Final.Domain.Core.Repositories;
using Assembly.Projecto.Final.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembly.Projecto.Final.Data.EntityFramework.Repositories
{
    public class AppointmentRepository : Repository<Appointment, int>, IAppointmentRepository
    {
        public AppointmentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Appointment? GetByIdWithParticipants(int id)
        {
            return DbSet.Include(a => a.Participants).FirstOrDefault(a => a.Id == id);
        }

        public List<Appointment> GetAllWithParticipants()
        {
            return DbSet.Include(a => a.Participants).ToList();
        }

        public List<Appointment> GetBetweenToDates(DateTime startDate, DateTime endDate)
        {
            return DbSet.Where(a => a.Date >= startDate && a.Date <= endDate)
                        .Include(a => a.Participants).ToList();
        }

        public List<Appointment> GetAppointmentIntersections(DateTime date, TimeOnly hourStart, TimeOnly hourEnd)
        {
            return DbSet.Where(a => a.Date == date && a.HourStart < hourEnd && a.HourEnd > hourStart)
                        .Include(a => a.Participants).ToList();
        }

        public List<Appointment> GetAppointmentsPaginationByEmployeeId(int employeeId, int pageNumber, int pageSize, string search)
        {
            return DbSet
                   .Where(a => a.Participants.Any(p => p.EmployeeId == employeeId) && 
                    (string.IsNullOrEmpty(search) || a.Title.Contains(search)))
                   .Skip((pageNumber - 1) * pageSize)
                   .Take(pageSize)
                   .Include(a => a.Participants).ToList();
        }

        public int GetTotalCount(int employeeId, string search)
        {
            return DbSet
                   .Where(a => a.Participants.Any(p => p.EmployeeId == employeeId) &&
                    (string.IsNullOrEmpty(search) || a.Title.Contains(search)))
                   .Count();
        }
    }
}
