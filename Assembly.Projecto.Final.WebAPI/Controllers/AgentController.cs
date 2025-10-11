using Assembly.Projecto.Final.Services.Dtos.GetDtos;
using Assembly.Projecto.Final.Services.Dtos.IServiceDtos.EmployeeUserDtos;
using Assembly.Projecto.Final.Services.Dtos.IServiceDtos.OtherModelsDtos;
using Assembly.Projecto.Final.Services.Interfaces;
using Assembly.Projecto.Final.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;

namespace Assembly.Projecto.Final.WebAPI.Controllers
{
    public class AgentController : BaseController
    {
        private readonly IAgentService _agentService;
        public AgentController(IAgentService agentService)
        {
            _agentService = agentService;
        }

        [Authorize(Roles = "Manager,Broker,Admin")]
        [HttpGet]
        public IEnumerable<AgentDto> GetAll()
        {
            return _agentService.GetAll();
        }

        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public ActionResult<AgentDto> GetById(int id)
        {
            return Ok(_agentService.GetById(id));
        }

        [Authorize(Roles = "Staff,Agent,Manager,Broker,Admin")]
        [HttpGet("GetByEmail")]
        public ActionResult<AgentDto> GetByEmail(string email) 
        {
            return Ok(_agentService.GetByEmail(email));
        }

        [Authorize(Roles = "Staff,Agent,Manager,Broker,Admin")]
        [HttpGet("GetByIdWithAll/{id:int}")]
        public ActionResult<AgentAllDto> GetByIdWithAll(int id)
        {
            return Ok(_agentService.GetByIdWithAll(id));
        }

        [Authorize(Roles = "Agent,Manager,Broker,Admin")]
        [HttpGet("GetByIdWithPersonalContacts/{id:int}")]
        public ActionResult<AgentWithPersonalContactsDto> GetByIdWithPersonalContacts(int id) 
        {
            return Ok(_agentService.GetByIdWithPersonalContacts(id));
        }

        [Authorize(Roles = "Staff,Agent,Manager,Broker,Admin")]
        [HttpGet("GetByIdWithParticipants/{id:int}")]
        public ActionResult<AgentWithParticipantsDto> GetByIdWithParticipants(int id) 
        {
            return Ok(_agentService.GetByIdWithParticipants(id));
        }
        [Authorize(Roles = "Agent,Manager,Broker,Admin")]
        [HttpGet("GetByIdWithListings/{id:int}")]
        public ActionResult<AgentWithListingsDto> GetByIdWithListings(int id)
        {
            return Ok(_agentService.GetByIdWithListings(id));
        }

        [Authorize(Roles = "Manager,Broker,Admin")]
        [HttpGet("GetByIdWithAgents/{id:int}")]
        public ActionResult<AgentWithAgentsDto> GetByIdWithAgents(int id) 
        {
            return Ok(_agentService.GetByIdWithAgents(id));
        }

        [Authorize(Roles = "Manager,Broker,Admin")]
        [HttpPost]
        public ActionResult<AgentDto> Add([FromBody] CreateAgentDto createAgentDto)
        {
            var agentDto = _agentService.Add(createAgentDto);

            return Ok(agentDto);
        }


        [Authorize(Roles = "Agent,Manager,Broker,Admin")]
        [HttpPost("AddAddress/{agentId:int}")]
        public ActionResult<AddressDto> AddAdress(int agentId, [FromBody] CreateAddressDto createAddressDto)
        {
            var addressDto = _agentService.AddressAdd(agentId, createAddressDto);

            return Ok(addressDto);
        }

        [Authorize(Roles = "Agent,Manager,Broker,Admin")]
        [HttpPost("AddContact/{agentId:int}")]
        public ActionResult<ContactDto> AddContact(int agentId, [FromBody] CreateContactDto createContactDto)
        {
            var contactDto = _agentService.ContactAdd(agentId, createContactDto);

            return Ok(contactDto);
        }

        [Authorize(Roles = "Manager,Broker,Admin")]
        [HttpPost("AddAccount/{agentId:int}")]
        public ActionResult<AccountDto> AddAccount(int agentId, [FromBody] CreateAccountDto createAccountDto)
        {
            var accountDto = _agentService.AccountAdd(agentId,createAccountDto);

            return accountDto;
        }

        [Authorize(Roles = "Agent,Manager,Broker,Admin")]
        [HttpPut("{id:int}")]
        public ActionResult<AgentDto> Update([FromRoute] int id, [FromBody] AgentDto agentDto)
        {
            if (id != agentDto.Id)
            {
                return BadRequest("Os ids do agent não coincidem.");
            }

            var updatedAgentDto = _agentService.Update(agentDto);

            return Ok(updatedAgentDto);
        }

        [Authorize(Roles = "Manager,Broker,Admin")]
        [HttpDelete("{id:int}")]

        public ActionResult<AgentDto> Delete([FromRoute] int id)
        {
            return Ok(_agentService.Delete(id));
        }

        [Authorize(Roles = "Agent,Manager,Broker,Admin")]
        [HttpPut("UpdateAddress/{agentId:int}/{addressId:int}")]
        public ActionResult<AddressDto> UpdateAddress([FromRoute] int agentId,[FromRoute] int addressId, 
            [FromBody] AddressDto addressDto)
        {
            if(addressId != addressDto.Id) 
            {
                return BadRequest("Os ids do address não coincidem.");
            }

            var updatedAddressDto = _agentService.AddressUpdate(agentId, addressDto);

            return Ok(updatedAddressDto);
        }

        [Authorize(Roles = "Agent,Manager,Broker,Admin")]
        [HttpPut("UpdateContact/{agentId:int}/{contactId:int}")]
        public ActionResult<ContactDto> UpdateContact([FromRoute]int agentId, [FromRoute] int contactId,
            [FromBody] ContactDto contactDto)
        {
            if(contactId != contactDto.Id) 
            {
                return BadRequest("Os ids do contact não coincidem.");
            }

            var updatedContactDto = _agentService.ContactUpdate(agentId, contactDto);

            return Ok(updatedContactDto);
        }

        [Authorize(Roles = "Agent,Manager,Broker,Admin")]
        [HttpPut("UpdateAccount/{agentId:int}")]
        public ActionResult<AccountDto> UpdateAccount([FromRoute] int agentId, [FromBody] UpdateAccountDto updateAccountDto) 
        {
            var updatedAccount = _agentService.AccountUpdate(agentId, updateAccountDto);

            return Ok(updatedAccount);
        }

        [Authorize(Roles = "Manager,Broker,Admin")]
        [HttpDelete("DeleteAccount/{agentId:int}")]
        public ActionResult<AccountDto> DeleteAccount([FromRoute] int agentId)
        {
            var deletedAccount = _agentService.AccountDelete(agentId);

            return Ok(deletedAccount);
        }

        [Authorize(Roles = "Agent,Manager,Broker,Admin")]
        [HttpDelete("DeleteContact/{agentId:int}/{contactId:int}")]
        public ActionResult<ContactDto> DeleteContact([FromRoute] int agentId, [FromRoute] int contactId)
        {
            var deletedContact = _agentService.ContactDelete(agentId, contactId);

            return Ok(deletedContact);
        }

        [Authorize(Roles = "Agent,Manager,Broker,Admin")]
        [HttpDelete("DeleteAddress/{agentId:int}/{addressId:int}")]
        public ActionResult<AddressDto> DeleteAddress([FromRoute] int agentId, [FromRoute] int addressId)
        {
            var deletedAddress = _agentService.AddressDelete(agentId, addressId);

            return Ok(deletedAddress);
        }
    }
}
