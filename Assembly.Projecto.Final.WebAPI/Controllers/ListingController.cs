using Assembly.Projecto.Final.Services.Dtos.IServiceDtos.EmployeeUserDtos;
using Assembly.Projecto.Final.Services.Dtos.IServiceDtos.OtherModelsDtos;
using Assembly.Projecto.Final.Services.Interfaces;
using Assembly.Projecto.Final.Services.Pagination;
using Assembly.Projecto.Final.Services.Services;
using Assembly.Projecto.Final.WebAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assembly.Projecto.Final.WebAPI.Controllers
{
    public class ListingController:BaseController
    {
        private readonly IListingService _listingService;

        public ListingController(IListingService listingService) 
        {
            _listingService = listingService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IEnumerable<ListingDto> GetAll() 
        {
            return _listingService.GetAll();
        }

        [AllowAnonymous]
        [HttpGet("GetAllPagination/{pageNumber:int}/{pageSize:int}")]
        public Pagination<ListingDto> GetAllPagination([FromRoute] int pageNumber, [FromRoute] int pageSize,
                                             [FromQuery] string? search)
        {
            return _listingService.GetAllPagination(pageNumber, pageSize, search);
        }

        [AllowAnonymous]
        [HttpGet("{id:int}")] 
        public ActionResult<ListingDto> GetById(int id) 
        {
            return Ok(_listingService.GetById(id));
        }

        [Authorize(Roles = "Agent,Manager,Broker,Admin")]
        [HttpPost]
        public ActionResult<ListingDto> Add([FromBody] CreateListingDto createListingDto) 
        {
            string? id = User.GetId();

            if (id == null)
            {
                return BadRequest("Não está autenticado como empregado.");
            }

            int agentId = int.Parse(id);

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
                MainImageFileName = createListingDto.MainImageFileName,
                OtherImagesFileNames = createListingDto.OtherImagesFileNames,
                AgentId = agentId
            };

            return Ok(_listingService.Add(createListingServiceDto));
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
