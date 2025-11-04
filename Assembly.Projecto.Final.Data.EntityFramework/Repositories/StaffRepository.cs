using Assembly.Projecto.Final.Data.EntityFramework.Context;
using Assembly.Projecto.Final.Domain.Core.Repositories;
using Assembly.Projecto.Final.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembly.Projecto.Final.Data.EntityFramework.Repositories
{
    public class StaffRepository : Repository<Staff, int>, IStaffRepository
    {
        public StaffRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Staff? GetByIdWithAccount(int id)
        {
            return DbSet.Include(s => s.EntityLink)
                .ThenInclude(el => el.Account)
                .FirstOrDefault(s => s.Id == id);
        }

        public Staff? GetByIdWithAddresses(int id)
        {
            return DbSet.Include(s => s.EntityLink)
                 .ThenInclude(el => el.Addresses)
                 .FirstOrDefault(s => s.Id == id);
        }

        public Staff? GetByIdWithAll(int id)
        {
            return DbSet
                   .Include(s => s.EntityLink)
                   .ThenInclude(el => el.Account)
                   .Include(s => s.EntityLink)
                   .ThenInclude(el => el.Contacts)
                   .Include(s => s.EntityLink)
                   .ThenInclude(el => el.Addresses)
                   .FirstOrDefault(s => s.Id == id);
        }

        public Staff? GetByIdWithContacts(int id)
        {
            return DbSet.Include(s => s.EntityLink)
                 .ThenInclude(el => el.Contacts)
                 .FirstOrDefault(s => s.Id == id);
        }

        public Staff? GetByIdWithPersonalContacts(int id)
        {
            return DbSet
                .Include(a => a.PersonalContacts)
                .ThenInclude(c => c.PersonalContactDetails)
                .FirstOrDefault(a => a.Id == id);
        }

        public Staff? GetByIdWithParticipants(int id)
        {
            return DbSet
                  .Include(a => a.Participants)
                  .ThenInclude(p => p.Appointment)
                  .FirstOrDefault(a => a.Id == id);
        }

        public Staff? GetByIdWithEverything(int id)
        {
            return DbSet
                   .Include(s => s.PersonalContacts)
                   .ThenInclude(c => c.PersonalContactDetails)
                   .Include(s => s.Participants)
                   .ThenInclude(p => p.Appointment)
                   .Include(s => s.EntityLink)
                   .ThenInclude(el => el.Account)
                   .Include(s => s.EntityLink)
                   .ThenInclude(el => el.Contacts)
                   .Include(s => s.EntityLink)
                   .ThenInclude(el => el.Addresses)
                   .FirstOrDefault(s => s.Id == id);
        }

        public List<Staff> GetAllPagination(int pageNumber, int pageSize, string search)
        {
            return DbSet
                .Where(a => string.IsNullOrEmpty(search) ||
                            a.Name.FirstName.Contains(search) ||
                            a.Name.LastName.Contains(search))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public int GetTotalCount(string search)
        {
            return DbSet
                 .Where(a => string.IsNullOrEmpty(search) ||
                             a.Name.FirstName.Contains(search) ||
                             a.Name.LastName.Contains(search))
                 .Count();
        }
    }
}
