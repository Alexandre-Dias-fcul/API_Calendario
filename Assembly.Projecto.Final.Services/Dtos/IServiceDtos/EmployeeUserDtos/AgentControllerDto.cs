using Assembly.Projecto.Final.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembly.Projecto.Final.Services.Dtos.IServiceDtos.EmployeeUserDtos
{
    public class AgentControllerDto
    {
        public NameDto Name { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public IFormFile? PhotoFileName { get; set; }
        public bool IsActive { get; set; }
        public DateTime? HiredDate { get; set; }
        public DateTime? DateOfTermination { get; set; }
        public RoleType Role { get; set; }
        public int? SupervisorId { get; set; }
    }
}
