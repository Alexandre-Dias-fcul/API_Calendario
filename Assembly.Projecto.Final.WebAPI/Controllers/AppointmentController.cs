using Assembly.Projecto.Final.Domain.Enums;
using Assembly.Projecto.Final.Services.Dtos.GetDtos;
using Assembly.Projecto.Final.Services.Dtos.IServiceDtos.OtherModelsDtos;
using Assembly.Projecto.Final.Services.Interfaces;
using Assembly.Projecto.Final.WebAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assembly.Projecto.Final.WebAPI.Controllers
{
    public class AppointmentController : BaseController
    {
        private readonly IAppointmentService _appointmentService;
        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [Authorize(Roles = "Staff,Agent,Manager,Broker,Admin")]
        [HttpGet]
        public IEnumerable<AppointmentDto> GetAll()
        {
            return _appointmentService.GetAll();
        }

        [Authorize(Roles = "Staff,Agent,Manager,Broker,Admin")]
        [HttpGet("GetAllWithParticipants")]

        public IEnumerable<AppointmentAllDto> GetAllWithParticipants()
        {
            return _appointmentService.GetAllWithParticipants();
        }

        [Authorize(Roles = "Staff,Agent,Manager,Broker,Admin")]
        [HttpGet("{id:int}")]
        public ActionResult<AppointmentDto> GetById(int id)
        {
            return Ok(_appointmentService.GetById(id));
        }

        [Authorize(Roles = "Staff,Agent,Manager,Broker,Admin")]
        [HttpGet("GetByIdWithParticipants/{id:int}")]
        public ActionResult<AppointmentAllDto> GetByIdWithParticipants(int id)
        {
            return Ok(_appointmentService.GetByIdWithParticipants(id));
        }

        [Authorize(Roles = "Staff,Agent,Manager,Broker,Admin")]
        [HttpGet("GetBetweenToDates/{startDate:DateTime}/{endDate:DateTime}")]
        public IEnumerable<AppointmentAllDto> GetBetWeenToDates(DateTime startDate, DateTime endDate)
        {
            return _appointmentService.GetBetweenToDates(startDate, endDate);
        }

        [Authorize(Roles = "Staff,Agent,Manager,Broker,Admin")]
        [HttpGet("GetAppointmentIntersections/{date:datetime}/{hourStart}/{hourEnd}")]
        public IEnumerable<AppointmentAllDto> GetAppointmentIntersections( DateTime date,TimeOnly hourStart,TimeOnly hourEnd)
        {
            return _appointmentService.GetAppointmentIntersections(date,hourStart, hourEnd);
        }

        [Authorize(Roles = "Staff,Agent,Manager,Broker,Admin")]
        [HttpPost]
        public ActionResult<AppointmentDto> Add(CreateAppointmentDto createAppointmentDto)
        {
            string? id = User.GetId();

            if (id == null)
            {
                return BadRequest("Não está autenticado como empregado.");
            }

            int employeeId = int.Parse(id);

            string? role = User.GetRole();

            if (role == null)
            {
                return BadRequest("Não está autenticado como empregado.");
            }

            bool isStaff = false;

            if (role == "Staff")
            {
                isStaff = true;
            }

            CreateAppointmentServiceDto createAppointmentServiceDto = new()
            {
                Title = createAppointmentDto.Title,
                Description = createAppointmentDto.Description,
                Date = createAppointmentDto.Date,
                HourStart = createAppointmentDto.HourStart,
                HourEnd = createAppointmentDto.HourEnd,
                Status = createAppointmentDto.Status,
                IsStaff = isStaff,
                EmployeeId = employeeId
            };


            return Ok(_appointmentService.Add(createAppointmentServiceDto));
        }

        [Authorize(Roles = "Staff,Agent,Manager,Broker,Admin")]
        [HttpPost("AddParticipant/{appointmentId:int}/{employeeId:int}")]
        public ActionResult<ParticipantDto> AddParticipant([FromRoute] int appointmentId, [FromRoute] int employeeId)
        {
            return Ok(_appointmentService.AddParticipant(appointmentId, employeeId));
        }

        [Authorize(Roles = "Staff,Agent,Manager,Broker,Admin")]
        [HttpPut("{id:int}")]
        public ActionResult<ParticipantDto> Update([FromRoute] int id, [FromBody] AppointmentDto appointmentDto) 
        {
            if (id != appointmentDto.Id) 
            {
                return BadRequest("Os ids do appointment não coincidem.");
            }

            string? employeeId = User.GetId();

            if (employeeId == null)
            {
                return BadRequest("Não está autenticado como empregado.");
            }

            int employeeIdInt = int.Parse(employeeId);

            return Ok(_appointmentService.Update(appointmentDto,employeeIdInt));
        }

        [Authorize(Roles = "Staff,Agent,Manager,Broker,Admin")]
        [HttpDelete("DeleteParticipant/{appointmentId:int}/{participantId:int}")]
        public ActionResult<ParticipantDto> DeleteParticipant([FromRoute] int appointmentId,[FromRoute] int participantId) 
        {
            return Ok(_appointmentService.DeleteParticipant(appointmentId, participantId));
        }

        [Authorize(Roles = "Staff,Agent,Manager,Broker,Admin")]
        [HttpDelete("{id:int}")]
        public ActionResult<AppointmentDto> Delete(int id) 
        {
            return Ok(_appointmentService.Delete(id));
        }

    }
}
