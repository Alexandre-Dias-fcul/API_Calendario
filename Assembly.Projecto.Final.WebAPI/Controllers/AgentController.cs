using Assembly.Projecto.Final.Domain.Common;
using Assembly.Projecto.Final.Services.Dtos.GetDtos;
using Assembly.Projecto.Final.Services.Dtos.IServiceDtos.EmployeeUserDtos;
using Assembly.Projecto.Final.Services.Dtos.IServiceDtos.OtherModelsDtos;
using Assembly.Projecto.Final.Services.Interfaces;
using Assembly.Projecto.Final.Services.Pagination;
using Assembly.Projecto.Final.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Reflection.Metadata.Ecma335;

namespace Assembly.Projecto.Final.WebAPI.Controllers
{
    public class AgentController : BaseController
    {
        private readonly IAgentService _agentService;
        private readonly IWebHostEnvironment _env;
        public AgentController(IAgentService agentService, IWebHostEnvironment env)
        {
            _agentService = agentService;
            _env = env;
        }

        [Authorize(Roles = "Staff,Manager,Broker,Admin")]
        [HttpGet]
        public IEnumerable<AgentDto> GetAll()
        {
            return _agentService.GetAll();
        }

        [Authorize(Roles = "Staff,Manager,Broker,Admin")]
        [HttpGet("GetAllPagination/{pageNumber:int}/{pageSize:int}")]
        public Pagination<AgentDto> GetAllPagination([FromRoute]int pageNumber, [FromRoute]int pageSize,
                                             [FromQuery] string? search)
        {
            return _agentService.GetAllPagination(pageNumber, pageSize, search);
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
        public async Task<ActionResult<AgentDto>> Add([FromForm] AgentControllerDto agentControllerDto)
        {
            if(agentControllerDto.PhotoFileName == null)
            {
                return BadRequest("A foto do agente é obrigatória.");
            }

            try
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images/agents");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var originalName = Path.GetFileNameWithoutExtension(agentControllerDto.PhotoFileName.FileName);

                var extension = Path.GetExtension(agentControllerDto.PhotoFileName.FileName);

                var uniqueFileName = $"{Guid.NewGuid()}_{originalName}{extension}";

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await agentControllerDto.PhotoFileName.CopyToAsync(fileStream);
                }

                CreateAgentDto createAgentDto = new CreateAgentDto
                {
                    Name = new NameDto
                    {
                        FirstName = agentControllerDto.Name.FirstName,
                        MiddleNames = agentControllerDto.Name.MiddleNames,
                        LastName = agentControllerDto.Name.LastName
                    },
                    DateOfBirth = agentControllerDto.DateOfBirth,
                    Gender = agentControllerDto.Gender,
                    PhotoFileName = $"images/agents/{uniqueFileName}",
                    IsActive = agentControllerDto.IsActive,
                    HiredDate = agentControllerDto.HiredDate,
                    DateOfTermination = agentControllerDto.DateOfTermination,
                    Role = agentControllerDto.Role,
                    SupervisorId = agentControllerDto.SupervisorId,

                };

                var agentDto = _agentService.Add(createAgentDto);

                return Ok(agentDto);

            }
            catch (Exception ex)
            {
                return BadRequest("Erro ao criar agente: " + ex.Message);
            }
           
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
        public async Task<ActionResult<AgentDto>> Update([FromRoute] int id, [FromForm] AgentControllerDto agentControllerDto)
        {
        
            try
            {
                var agent = _agentService.GetById(id);

                if (agent == null)
                {
                    return NotFound("= agente não foi encontrado.");
                }

                string uploadsFolder = Path.Combine(_env.WebRootPath, "images/agents");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                if (agentControllerDto.PhotoFileName != null)
                {

                    if (!string.IsNullOrEmpty(agent.PhotoFileName))
                    {
                        string oldPath = Path.Combine(_env.WebRootPath, agent.PhotoFileName);

                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }

                    }

                    var originalName = Path.GetFileNameWithoutExtension(agentControllerDto.PhotoFileName.FileName);

                    var extension = Path.GetExtension(agentControllerDto.PhotoFileName.FileName);

                    var uniqueFileName = $"{Guid.NewGuid()}_{originalName}{extension}";

                    string newPath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(newPath, FileMode.Create))
                    {
                        await agentControllerDto.PhotoFileName.CopyToAsync(stream);
                    }

                    agent.PhotoFileName = $"images/agents/{uniqueFileName}";
                }

                AgentDto agentDto = new AgentDto
                {
                    Id = id,
                    Name = new NameDto
                    {
                        FirstName = agentControllerDto.Name.FirstName,
                        MiddleNames = agentControllerDto.Name.MiddleNames,
                        LastName = agentControllerDto.Name.LastName
                    },
                    DateOfBirth = agentControllerDto.DateOfBirth,
                    Gender = agentControllerDto.Gender,
                    PhotoFileName = agent.PhotoFileName,
                    IsActive = agentControllerDto.IsActive,
                    HiredDate = agentControllerDto.HiredDate,
                    DateOfTermination = agentControllerDto.DateOfTermination,
                    Role = agentControllerDto.Role,
                    SupervisorId = agentControllerDto.SupervisorId,

                };

                var updatedAgentDto = _agentService.Update(agentDto);

                return Ok(updatedAgentDto);

            }
            catch (Exception ex)
            {
                return BadRequest("Erro ao atualizar agente: " + ex.Message);
            }
            
        }

        [Authorize(Roles = "Manager,Broker,Admin")]
        [HttpDelete("{id:int}")]

        public ActionResult<AgentDto> Delete([FromRoute] int id)
        {
            try
            {
                var agent = _agentService.GetById(id);

                if (agent == null)
                {
                    return NotFound("O agente não foi encontrado.");
                }

                if (!string.IsNullOrEmpty(agent.PhotoFileName))
                {
                    string imagePath = Path.Combine(_env.WebRootPath, agent.PhotoFileName);

                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                return Ok(_agentService.Delete(id));

            }
            catch (Exception ex)
            {
                return BadRequest("Erro ao deletar agente: " + ex.Message);
            }
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
