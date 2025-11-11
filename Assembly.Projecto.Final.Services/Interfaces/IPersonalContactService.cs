
using Assembly.Projecto.Final.Domain.Models;
using Assembly.Projecto.Final.Services.Dtos.GetDtos;
using Assembly.Projecto.Final.Services.Dtos.IServiceDtos.OtherModelsDtos;
using Assembly.Projecto.Final.Services.Pagination;
using Assembly.Projecto.Final.Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembly.Projecto.Final.Services.Interfaces
{
     public interface IPersonalContactService:IService<CreatePersonalContactServiceDto,PersonalContactDto,int>
    {
       public PersonalContactDetailDto AddDetail(int personalContactId, 
               CreatePersonalContactDetailDto createPersonalContactDetailDto);
       public PersonalContactDetailDto UpdateDetail(int personalContactId,
               PersonalContactDetailDto personalContactDetailDto);

       public PersonalContactDetailDto DeleteDetail(int personalContactId, int personalContactDetailId);

       public PersonalContactAllDto GetByIdWithDetail(int id);

        public Pagination<PersonalContact> GetPersonalContactPaginationByEmployeeId(int employeeId, int pageNumber, int pageSize,
            string search);
    }
}
