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

            if (createListingDto.SecondaryImage == null)
            {
                return BadRequest("A segunda imagem é obrigatória.");
            }

            try 
            { 
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images/listings");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var originalName = Path.GetFileNameWithoutExtension(createListingDto.Image.FileName);

                var extension = Path.GetExtension(createListingDto.Image.FileName);

                var uniqueFileName = $"{Guid.NewGuid()}_{originalName}{extension}";

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await createListingDto.Image.CopyToAsync(fileStream);
                }

                var secondaryOriginalName = Path.GetFileNameWithoutExtension(createListingDto.SecondaryImage.FileName);

                var secondaryExtension = Path.GetExtension(createListingDto.SecondaryImage.FileName);

                var secondaryUniqueFileName = $"{Guid.NewGuid()}_{secondaryOriginalName}{secondaryExtension}";

                var secondaryImagePath = Path.Combine(uploadsFolder, secondaryUniqueFileName);

                using (var fileStream = new FileStream(secondaryImagePath, FileMode.Create))
                {
                    await createListingDto.SecondaryImage.CopyToAsync(fileStream);
                }

                CreateListingServiceDto createListingServiceDto = new()
                {
                    Type =
                    createListingDto.Type,
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
                    OtherImagesFileNames = $"images/listings/{secondaryUniqueFileName}",
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
        public async Task<ActionResult<ListingDto>> Update([FromRoute] int id, [FromForm] CreateListingDto createListingDto) 
        {
            try 
            {
                var listing = _listingService.GetById(id);

                if (listing == null) 
                {
                    return NotFound("Listing não encontrada.");
                }

                string uploadsFolder = Path.Combine(_env.WebRootPath, "images/listings");

                if (!Directory.Exists(uploadsFolder)) 
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                if (createListingDto.Image != null)
                {
      
                    if (!string.IsNullOrEmpty(listing.MainImageFileName))
                    {
                        string oldPath = Path.Combine(_env.WebRootPath, listing.MainImageFileName);

                        if (System.IO.File.Exists(oldPath)) 
                        {
                            System.IO.File.Delete(oldPath);
                        }
                            
                    }

                    var originalName = Path.GetFileNameWithoutExtension(createListingDto.Image.FileName);

                    var extension = Path.GetExtension(createListingDto.Image.FileName);

                    var uniqueFileName = $"{Guid.NewGuid()}_{originalName}{extension}";

                    string newPath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(newPath, FileMode.Create))
                    {
                        await createListingDto.Image.CopyToAsync(stream);
                    }

                    listing.MainImageFileName = $"images/listings/{uniqueFileName}";
                }

                if (createListingDto.SecondaryImage != null)
                {
                    if (!string.IsNullOrEmpty(listing.OtherImagesFileNames))
                    {
                        string oldPath = Path.Combine(_env.WebRootPath, listing.OtherImagesFileNames);

                        if (System.IO.File.Exists(oldPath)) 
                        {
                            System.IO.File.Delete(oldPath);
                        }
                            
                    }

                    var originalName = Path.GetFileNameWithoutExtension(createListingDto.SecondaryImage.FileName);

                    var extension = Path.GetExtension(createListingDto.SecondaryImage.FileName);

                    var uniqueFileName = $"{Guid.NewGuid()}_{originalName}{extension}";

                    string newPath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(newPath, FileMode.Create))
                    {
                        await createListingDto.SecondaryImage.CopyToAsync(stream);
                    }

                    listing.OtherImagesFileNames = $"images/listings/{uniqueFileName}";
                }

                ListingDto listingDto = new()
                {
                    Id = id,
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
                    MainImageFileName = listing.MainImageFileName,
                    OtherImagesFileNames = listing.OtherImagesFileNames
                };

                return Ok(_listingService.Update(listingDto));

            }
            catch(Exception ex)
            {
                return BadRequest("Erro ao atualizar listing: " + ex.Message);
            }
        }

        [Authorize(Roles = "Agent,Manager,Broker,Admin")]
        [HttpDelete("{id:int}")]
        public ActionResult<ListingDto> Delete(int id)
        {
            try
            {
                var listing = _listingService.GetById(id);

                if (listing == null)
                {
                    return NotFound("Listing não encontrada.");
                }

                if (!string.IsNullOrEmpty(listing.MainImageFileName))
                {
                    string mainImagePath = Path.Combine(_env.WebRootPath, listing.MainImageFileName);

                    if (System.IO.File.Exists(mainImagePath))
                    {
                        System.IO.File.Delete(mainImagePath);
                    }
                }

                if (!string.IsNullOrEmpty(listing.OtherImagesFileNames))
                {
                    string secondaryImagePath = Path.Combine(_env.WebRootPath, listing.OtherImagesFileNames);

                    if (System.IO.File.Exists(secondaryImagePath))
                    {
                        System.IO.File.Delete(secondaryImagePath);
                    }
                }

                return Ok(_listingService.Delete(id));

            }
            catch (Exception ex)
            {
                return BadRequest("Erro ao deletar listing: " + ex.Message);
            }
            
        }
    }
}
