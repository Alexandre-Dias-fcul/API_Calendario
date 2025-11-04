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
    public class ListingRepository : Repository<Listing, int>, IListingRepository
    {
        public ListingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public List<Agent> GetAllPagination(int pageNumber, int pageSize, string search)
        {
            throw new NotImplementedException();
        }

        public Listing? GetByIdWithAll(int id)
        {
            return DbSet
                .Include(x => x.Favorites)
                .Include(x => x.FeedBacks)
                .Include(x => x.Reassigns)
                .FirstOrDefault(x => x.Id == id);
        }

        List<Listing> IListingRepository.GetAllPagination(int pageNumber, int pageSize, string search)
        {
            return DbSet
                .Where(l => string.IsNullOrEmpty(search) ||
                            l.Type.Contains(search) ||
                            l.Type.Contains(search))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public int GetTotalCount(string search)
        {
            return DbSet.Where(l => string.IsNullOrEmpty(search) ||
                            l.Type.Contains(search) ||
                            l.Type.Contains(search)).Count();
        }
    }
}
