using Assembly.Projecto.Final.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembly.Projecto.Final.Domain.Core.Repositories
{
    public interface IUserRepository: IRepository<User, int>
    {
        public User? GetByIdWithAddresses(int id);

        public User? GetByIdWithAccount(int id);

        public User? GetByIdWithContacts(int id);

        public User? GetByIdWithAll(int id);

        public User? GetByIdWithEverything(int id);

        public List<User> GetAllPagination(int pageNumber, int pageSize, string search);

        public int GetTotalCount(string search);
    }
}
