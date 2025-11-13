using Assembly.Projecto.Final.Services.Dtos.IServiceDtos.EmployeeUserDtos;
using Assembly.Projecto.Final.Services.Dtos.IServiceDtos.OtherModelsDtos;
using Assembly.Projecto.Final.Services.Interfaces;
using Assembly.Projecto.Final.Services.Pagination;
using Assembly.Projecto.Final.Services.Services;
using Assembly.Projecto.Final.WebAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Runtime.CompilerServices;

namespace Assembly.Projecto.Final.WebAPI.Controllers
{
    public class ListingController:BaseController
    {
        private readonly IListingService _listingService;
        private readonly IWebHostEnvironment _env;

        public ListingController(IListingService listingService, IWebHostEnvironment env) 
        {
            _listingService = listingService;
            _env = env;
        }

        [AllowAnonymous]
        [HttpGet]
        public IEnumerable<ListingDto> GetAll() 
        {
            return _listingService.GetAll();
        }

        [AllowAnonymous]
        [HttpGet("GetAllSearch")]
        public IEnumerable<ListingDto> GetAllSearch([FromQuery] string? search)
        {
            return _listingService.GetAllSearch(search);
        }

        [Authorize(Roles = "Agent,Manager,Broker,Admin")]
        [HttpGet("GetAllPagination/{pageNumber:int}/{pageSize:int}")]
        public Pagination<ListingDto> GetAllPagination([FromRoute] int pageNumber, [FromRoute] int pageSize,
                                             [FromQuery] string? search)
        {
            return _listingService.GetAllPagination(pageNumber, pageSize, search);
        }

        [Authorize(Roles = "Agent,Manager,Broker,Admin")]
        [HttpGet("GetListingsPaginationByAgentId/{agentId:int}/{pageNumber:int}/{pageSize:int}")]
        public Pagination<ListingDto> GetListingsPaginationByAgentId([FromRoute]int agentId,[FromRoute] int pageNumber, 
            [FromRoute] int pageSize,[FromQuery] string? search)
        {
            return _listingService.GetListingsPaginationByAgentId(agentId,pageNumber, pageSize, search);
        }

        [AllowAnonymous]
        [HttpGet("{id:int}")] 
        public ActionResult<ListingDto> GetById(int id) 
        {
            return Ok(_listingService.GetById(id));
        }

        [Authorize(Roles = "Agent,Manager,Broker,Admin")]
        [HttpPost]
        public async Task<ActionResult<ListingDto>> Add([FromForm] CreateListingDto createListingDto) 
        {
            string? id = User.GetId();

            if (id == null)
            {
                return BadRequest("Não está autenticado como empregado.");
            }

            int agentId = int.Parse(id);

            if(createListingDto.Image == null) 
            { 
                return BadRequest("Imagem principal é obrigatória.");
            }

            try 
            { 
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images/listings");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(createListingDto.Image.FileName)}";

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await createListingDto.Image.CopyToAsync(fileStream);
                }

                CreateListingServiceDto createListingServiceDto = new()
                {
                    Type = createListingDto.Type,
                    Status = createListingDto.Status,
                    NumberOfRooms = createListingDto.NumberOfRooms,
                    NumberOfKitchens = createListingDto.NumberOfKitchens,
                    NumberOfBathrooms = createListingDto.NumberOfBathrooms,
                    Price = createListingDto.Price,
                    Location = createListingDto.Location,
                    Area = createListingDto.Area,
                    Parking = createListingDto.Parking,
                    Description = createListingDto.Description,
                    MainImageFileName = $"images/listings/{uniqueFileName}",
                    OtherImagesFileNames = createListingDto.OtherImagesFileNames,
                    AgentId = agentId
                };

                return Ok(_listingService.Add(createListingServiceDto));
            }
            catch(Exception ex) 
            {
                return BadRequest("Erro ao carregar a imagem: " + ex.Message);
            }
        }

        [Authorize(Roles = "Manager,Broker,Admin")]
        [HttpPost("SelfReassign/{listingId:int}")] 
        public ActionResult<ReassignDto> SelfReassign(int listingId) 
        {
            var agentIdString = User.GetId();

            if (agentIdString == null)
            {
                return NotFound("Não está autenticado.");
            }

            var agentId = int.Parse(agentIdString);

            return Ok(_listingService.SelfReassign(listingId, agentId));
        }

        [Authorize(Roles = "Manager,Broker,Admin")]
        [HttpPost("SelfReassignTo/{listingId:int}/{agentId:int}")]
        public ActionResult<ReassignDto> SelfReassignTo(int listingId,int agentId)
        {
            return Ok(_listingService.SelfReassignTo(listingId, agentId));
        }

        [Authorize(Roles = "Manager,Broker,Admin")]
        [HttpPost("BetweenReassign/{listingId:int}/{agentId:int}")]
        public ActionResult<ReassignDto> BetweenReassign(int listingId,int agentId)
        {
            return Ok(_listingService.BetweenReassign(listingId, agentId));
        }

        [Authorize(Roles = "Agent,Manager,Broker,Admin")]
        [HttpPut("{id:int}")]
        public ActionResult<ListingDto> Update([FromRoute] int id,[FromBody] ListingDto listingDto) 
        {
            if(id != listingDto.Id) 
            {
                return BadRequest("Os ids da listing não coincidem.");   
            }

            return Ok(_listingService.Update(listingDto));
        }

        [Authorize(Roles = "Agent,Manager,Broker,Admin")]
        [HttpDelete("{id:int}")]
        public ActionResult<ListingDto> Delete(int id)
        {
            return Ok(_listingService.Delete(id));
        }
    }
}
