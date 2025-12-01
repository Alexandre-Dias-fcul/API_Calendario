
using Assembly.Projecto.Final.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembly.Projecto.Final.Domain.Core.Repositories
{
    public interface IAgentRepository:IRepository<Agent,int>
    {
        public Agent? GetByIdWithAddresses(int id);

        public Agent? GetByIdWithAccount(int id);

        public Agent? GetByIdWithContacts(int id);

        public Agent? GetByIdWithAll(int id);

        public Agent? GetByIdWithPersonalContacts(int id);

        public Agent? GetByIdWithParticipants(int id);

        public Agent? GetByIdWithListings(int id);

        public Agent? GetByIdWithAgents(int id);

        public Agent? GetByIdWithEverything(int id);

        public List<Agent> GetAllAdmins();

        public List<Agent> GetAllSupervised(int id);

        public List<Agent> GetAllPagination(int pageNumber, int pageSize, string search);

        public int GetTotalCount(string search);
    }
}
