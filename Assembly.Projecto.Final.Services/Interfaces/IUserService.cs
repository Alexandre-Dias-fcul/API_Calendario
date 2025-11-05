using Assembly.Projecto.Final.Domain.Models;
using Assembly.Projecto.Final.Services.Dtos.GetDtos;
using Assembly.Projecto.Final.Services.Dtos.IServiceDtos.EmployeeUserDtos;
using Assembly.Projecto.Final.Services.Dtos.IServiceDtos.OtherModelsDtos;
using Assembly.Projecto.Final.Services.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembly.Projecto.Final.Services.Interfaces
{
    public interface IUserService:IService<CreateUserDto,UserDto,int>
    {
        public ContactDto ContactAdd(int userId, CreateContactDto createContactDto);
        public AddressDto AddressAdd(int userId, CreateAddressDto createAddressDto);
        public AccountDto AccountAdd(int userId, CreateAccountDto createAccountDto);
        public AccountDto AccountUpdate(int userId, UpdateAccountDto updateAccountDto);
        public ContactDto ContactUpdate(int userId, ContactDto contactDto);
        public AddressDto AddressUpdate(int userId, AddressDto addressDto);
        public AccountDto AccountDelete(int userId);
        public ContactDto ContactDelete(int userId, int contactId);
        public AddressDto AddressDelete(int userId, int addressId);
        public UserAllDto GetByIdWithAll(int id);
        public UserDto GetByEmail(string email);
        public Pagination<UserDto> GetAllPagination(int pageNumber, int pageSize, string search);
    }
}
