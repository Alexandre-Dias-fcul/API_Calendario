using Assembly.Projecto.Final.Domain.Models;
using Assembly.Projecto.Final.Services.Dtos;
using Assembly.Projecto.Final.Services.Dtos.IServiceDtos.OtherModelsDtos;
using Assembly.Projecto.Final.Services.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembly.Projecto.Final.Services.Interfaces
{
    public interface IListingService:IService<CreateListingServiceDto,ListingDto, int>
    {
        public ReassignDto SelfReassignTo(int listingId, int newAgentId);
        public ReassignDto SelfReassign(int listingId,int newAgentId);
        public ReassignDto BetweenReassign(int listingId, int newAgentId);
        public Pagination<ListingDto> GetAllPagination(int pageNumber, int pageSize, string search);
    }
}
