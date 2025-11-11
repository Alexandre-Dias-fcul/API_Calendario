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
    public class PersonalContactRepository : Repository<PersonalContact, int>, IPersonalContactRepository
    {
        public PersonalContactRepository(ApplicationDbContext context) : base(context)
        {
        }

        public PersonalContact? GetByIdWithDetail(int id)
        {
            return DbSet.Include(p => p.PersonalContactDetails).FirstOrDefault(p => p.Id == id);
        }

        public List<PersonalContact> GetPersonalContactPaginationByEmployeeId(int employeeId, int pageNumber, int pageSize,
            string search)
        {
            return DbSet
                   .Where(p => p.EmployeeId == employeeId && (string.IsNullOrEmpty(search) || p.Name.Contains(search)))
                   .Skip((pageNumber - 1) * pageSize)
                   .Take(pageSize)
                   .ToList();
        }

        public int GetTotalCount(int employeeId, string search)
        {
            return DbSet
                   .Where(p => p.EmployeeId == employeeId && (string.IsNullOrEmpty(search) || p.Name.Contains(search)))
                   .Count();
        }
    }
}
