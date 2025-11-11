using Assembly.Projecto.Final.Services.Dtos.GetDtos;
using Assembly.Projecto.Final.Services.Dtos.IServiceDtos.EmployeeUserDtos;
using Assembly.Projecto.Final.Services.Dtos.IServiceDtos.OtherModelsDtos;
using Assembly.Projecto.Final.Services.Interfaces;
using Assembly.Projecto.Final.Services.Pagination;
using Assembly.Projecto.Final.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assembly.Projecto.Final.WebAPI.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IEnumerable<UserDto> GetAll()
        {
            return _userService.GetAll();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllPagination/{pageNumber:int}/{pageSize:int}")]
        public Pagination<UserDto> GetAllPagination([FromRoute] int pageNumber, [FromRoute] int pageSize,
                                             [FromQuery] string? search)
        {
            return _userService.GetAllPagination(pageNumber, pageSize, search);
        }

        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public ActionResult<UserDto> GetById(int id)
        {
            return Ok(_userService.GetById(id));
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("GetByIdWithAll/{id:int}")]
        public ActionResult<UserAllDto> GetByIdWithAll(int id)
        {
            return Ok(_userService.GetByIdWithAll(id));
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("GetByEmail")]
        public ActionResult<AgentDto> GetByEmail(string email)
        {
            return Ok(_userService.GetByEmail(email));
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult<UserDto> Add(CreateUserDto createUserDto)
        {
            var userDto = _userService.Add(createUserDto);

            return Ok(userDto);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost("AddAddress/{userId:int}")]
        public ActionResult<AddressDto> AddAdress(int userId, [FromBody] CreateAddressDto createAddressDto)
        {
            var addressDto = _userService.AddressAdd(userId, createAddressDto);

            return Ok(addressDto);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost("AddContact/{userId:int}")]
        public ActionResult<ContactDto> AddContact(int userId, [FromBody] CreateContactDto createContactDto)
        {

            var contactDto = _userService.ContactAdd(userId, createContactDto);

            return Ok(contactDto);
        }

        [AllowAnonymous]
        [HttpPost("AddAccount/{userId:int}")]
        public ActionResult<AccountDto> AddAccount(int userId, [FromBody] CreateAccountDto createAccountDto)
        {
            var accountDto = _userService.AccountAdd(userId, createAccountDto);

            return Ok(accountDto);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPut("UpdateAddress/{userId:int}/{addressId:int}")]
        public ActionResult<AddressDto> UpdateAddress([FromRoute] int userId, [FromRoute] int addressId,
           [FromBody] AddressDto addressDto)
        {
            if (addressId != addressDto.Id)
            {
                return BadRequest("Os ids do address não coincidem.");
            }

            var updatedAddressDto = _userService.AddressUpdate(userId, addressDto);

            return Ok(updatedAddressDto);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPut("UpdateContact/{userId:int}/{contactId:int}")]
        public ActionResult<ContactDto> UpdateContact([FromRoute] int userId, [FromRoute] int contactId,
            [FromBody] ContactDto contactDto)
        {
            if (contactId != contactDto.Id)
            {
                return BadRequest("Os ids do contact não coincidem.");
            }

            var updatedContactDto = _userService.ContactUpdate(userId, contactDto);

            return Ok(updatedContactDto);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPut("UpdateAccount/{userId:int}")]
        public ActionResult<AccountDto> UpdateAccount([FromRoute] int userId, [FromBody] UpdateAccountDto updateAccountDto)
        {
            var updatedAccount = _userService.AccountUpdate(userId, updateAccountDto);

            return updatedAccount;
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPut("{id:int}")]
        public ActionResult<UserDto> Update([FromRoute] int id, [FromBody] UserDto userDto)
        {
            if (id != userDto.Id)
            {
                return BadRequest("Os Ids do user não coincidem");
            }

            var updatedUserDto = _userService.Update(userDto);

            return Ok(updatedUserDto);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public ActionResult<UserDto> Delete([FromRoute] int id)
        {
            return Ok(_userService.Delete(id));
        }

        [Authorize(Roles = "User,Admin")]
        [HttpDelete("DeleteAccount/{userId:int}")]
        public ActionResult<AccountDto> DeleteAccount([FromRoute] int userId)
        {
            var deletedAccountDto = _userService.AccountDelete(userId);

            return Ok(deletedAccountDto);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpDelete("DeleteContact/{userId:int}/{contactId:int}")]
        public ActionResult<ContactDto> DeleteContact([FromRoute] int userId, [FromRoute] int contactId)
        {
            var deletedContactDto = _userService.ContactDelete(userId, contactId);

            return Ok(deletedContactDto);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpDelete("DeleteAddress/{userId:int}/{addressId:int}")]
        public ActionResult<AddressDto> DeleteAddress([FromRoute] int userId, [FromRoute] int addressId)
        {
            var deletedAddressDto = _userService.AddressDelete(userId, addressId);

            return Ok(deletedAddressDto);
        }
    }
}
