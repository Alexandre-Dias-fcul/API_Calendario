using Assembly.Projecto.Final.Domain.Core.Repositories;
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
    public class StaffController:BaseController
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService staffService) 
        {
            _staffService = staffService;
        }

        [Authorize(Roles = "Manager,Broker,Admin")]
        [HttpGet]
        public IEnumerable<StaffDto> GetAll()
        {
            return _staffService.GetAll();
        }

        [Authorize(Roles = "Manager,Broker,Admin")]
        [HttpGet("GetAllPagination/{pageNumber:int}/{pageSize:int}")]
        public Pagination<StaffDto> GetAllPagination([FromRoute] int pageNumber, [FromRoute] int pageSize,
                                             [FromQuery] string? search)
        {
            return _staffService.GetAllPagination(pageNumber, pageSize, search);
        }

        [Authorize(Roles = "Staff,Agent,Manager,Broker,Admin")]
        [HttpGet("{id:int}")]
        public ActionResult<StaffDto> GetById(int id)
        {
            return Ok(_staffService.GetById(id));
        }

        [Authorize(Roles = "Staff,Agent,Manager,Broker,Admin")]
        [HttpGet("GetByEmail")]
        public ActionResult<AgentDto> GetByEmail(string email)
        {
            return Ok(_staffService.GetByEmail(email));
        }

        [Authorize(Roles = "Staff,Agent,Manager,Broker,Admin")]
        [HttpGet("GetByIdWithAll/{id:int}")]
        public ActionResult<AgentAllDto> GetByIdWithAll(int id)
        {
            return Ok(_staffService.GetByIdWithAll(id));
        }
        [Authorize(Roles = "Staff,Admin")]
        [HttpGet("GetByIdWithPersonalContacs/{id:int}")]
        public ActionResult<StaffWithPersonalContactsDto> GetByIdWithPersonalContacts(int id)
        {
            return Ok(_staffService.GetByIdWithPersonalContacts(id));
        }
        [Authorize(Roles = "Staff,Agent,Manager,Broker,Admin")]
        [HttpGet("GetByIdWithParticipants/{id:int}")]
        public ActionResult<StaffWithParticipantsDto> GetByIdWithParticipants(int id)
        {
            return Ok(_staffService.GetByIdWithParticipants(id));
        }


        [Authorize(Roles = "Staff,Manager,Broker,Admin")]
        [HttpPost("AddAddress/{staffId:int}")]
        public ActionResult<AddressDto> AddAdress(int staffId, [FromBody] CreateAddressDto createAddressDto)
        {
            var addressDto = _staffService.AddressAdd(staffId, createAddressDto);

            return Ok(addressDto);
        }


        [Authorize(Roles = "Staff,Manager,Broker,Admin")]
        [HttpPost("AddContact/{staffId:int}")]
        public ActionResult<ContactDto> AddContact(int staffId, [FromBody] CreateContactDto createContactDto)
        {
            var contactDto = _staffService.ContactAdd(staffId, createContactDto);

            return Ok(contactDto);
        }


        [Authorize(Roles = "Staff,Manager,Broker,Admin")]
        [HttpPost("AddAccount/{staffId:int}")]
        public ActionResult<AccountDto> AddAccount(int staffId, [FromBody] CreateAccountDto createAccountDto)
        {
            var accountDto = _staffService.AccountAdd(staffId, createAccountDto);

            return Ok(accountDto);
        }

        [Authorize(Roles = "Manager,Broker,Admin")]
        [HttpPost]
        public ActionResult<StaffDto> Add([FromBody] CreateStaffDto createStaffDto)
        {
            var staffDto = _staffService.Add(createStaffDto);

            return Ok(staffDto);
        }

        [Authorize(Roles = "Manager,Broker,Admin")]
        [HttpPut("{id:int}")]
        public ActionResult<StaffDto> Update([FromRoute] int id, [FromBody] StaffDto staffDto)
        {
            if(id != staffDto.Id) 
            {
                return BadRequest("Os ids do staff não coincidem.");
            }

            var updatedStaffDto = _staffService.Update(staffDto);

            return Ok(updatedStaffDto);
        }

        [Authorize(Roles = "Manager,Broker,Admin")]
        [HttpDelete("{id:int}")]
        public ActionResult<StaffDto> Delete([FromRoute] int id)
        {
            return Ok(_staffService.Delete(id));
        }

        [Authorize(Roles = "Staff,Manager,Broker,Admin")]
        [HttpPut("UpdateAddress/{staffId:int}/{addressId:int}")]
        public ActionResult<AddressDto> UpdateAddress([FromRoute] int staffId, [FromRoute] int addressId,
           [FromBody] AddressDto addressDto)
        {
            if (addressId != addressDto.Id)
            {
                return BadRequest("Os ids do address não coincidem.");
            }

            var updatedAddressDto = _staffService.AddressUpdate(staffId, addressDto);

            return Ok(updatedAddressDto);
        }


        [Authorize(Roles = "Staff,Manager,Broker,Admin")]
        [HttpPut("UpdateContact/{staffId:int}/{contactId:int}")]
        public ActionResult<ContactDto> UpdateContact([FromRoute] int staffId, [FromRoute] int contactId,
            [FromBody] ContactDto contactDto)
        {
            if (contactId != contactDto.Id)
            {
                return BadRequest("Os ids do contact não coincidem.");
            }

            var updatedContactDto = _staffService.ContactUpdate(staffId, contactDto);

            return Ok(updatedContactDto);
        }

        [Authorize(Roles = "Staff,Manager,Broker,Admin")]
        [HttpPut("UpdateAccount/{staffId:int}")]
        public ActionResult<AccountDto> UpdateAccount([FromRoute] int staffId, [FromBody] UpdateAccountDto updateAccountDto)
        {
            var updatedAccount = _staffService.AccountUpdate(staffId, updateAccountDto);

            return updatedAccount;
        }


        [Authorize(Roles = "Manager,Broker,Admin")]
        [HttpDelete("DeleteAccount/{staffId:int}")]
        public ActionResult DeleteAccount([FromRoute] int staffId)
        {
            var deletedAccount = _staffService.AccountDelete(staffId);

            return Ok(deletedAccount);
        }

        [Authorize(Roles = "Staff,Manager,Broker,Admin")]
        [HttpDelete("DeleteContact/{staffId:int}/{contactId:int}")]
        public ActionResult DeleteContact([FromRoute] int staffId, [FromRoute] int contactId)
        {
            var deletedContact = _staffService.ContactDelete(staffId, contactId);

            return Ok(deletedContact);
        }


        [Authorize(Roles = "Staff,Manager,Broker,Admin")]
        [HttpDelete("DeleteAddress/{staffId:int}/{addressId:int}")]
        public ActionResult DeleteAddress([FromRoute] int staffId, [FromRoute] int addressId)
        {
            var deletedAddress = _staffService.AddressDelete(staffId, addressId);

            return Ok(deletedAddress);
        }

    }
}
