using Assembly.Projecto.Final.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembly.Projecto.Final.Domain.Core.Repositories
{
    public interface IListingRepository:IRepository<Listing,int>
    {
       public Listing? GetByIdWithAll(int id);

       public List<Listing> GetAllPagination(int pageNumber, int pageSize, string search);

       public int GetTotalCount(string search);

    }
}
