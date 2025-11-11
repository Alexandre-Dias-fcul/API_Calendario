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
    public class UserRepository :Repository<User,int>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public User? GetByIdWithAccount(int id)
        {
            return DbSet.Include(u => u.EntityLink)
                .ThenInclude(el => el.Account)
                .FirstOrDefault(u => u.Id == id);
        }

        public User? GetByIdWithAddresses(int id)
        {
            return DbSet.Include(u => u.EntityLink)
                 .ThenInclude(el => el.Addresses)
                 .FirstOrDefault(u => u.Id == id);
        }

        public User? GetByIdWithAll(int id)
        {
            return DbSet
                  .Include(u => u.EntityLink)
                  .ThenInclude(el => el.Account)
                  .Include(u => u.EntityLink)
                  .ThenInclude(el => el.Contacts)
                  .Include(u => u.EntityLink)
                  .ThenInclude(el => el.Addresses)
                  .FirstOrDefault(u => u.Id == id);
        }

        public User? GetByIdWithContacts(int id)
        {
            return DbSet.Include(u => u.EntityLink)
                 .ThenInclude(el => el.Contacts)
                 .FirstOrDefault(u => u.Id == id);
        }

        public User? GetByIdWithEverything(int id) 
        {
            return DbSet
                  .Include(u => u.Favorites)
                  .Include(u => u.FeedBacks)
                  .Include(u => u.EntityLink)
                  .ThenInclude(el => el.Account)
                  .Include(u => u.EntityLink)
                  .ThenInclude(el => el.Contacts)
                  .Include(u => u.EntityLink)
                  .ThenInclude(el => el.Addresses)
                  .FirstOrDefault(u => u.Id == id);
        }

        public List<User> GetAllPagination(int pageNumber, int pageSize, string search)
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
