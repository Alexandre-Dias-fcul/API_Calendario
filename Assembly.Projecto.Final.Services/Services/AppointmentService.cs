using Assembly.Projecto.Final.Domain.Common;
using Assembly.Projecto.Final.Domain.Core.Repositories;
using Assembly.Projecto.Final.Domain.Enums;
using Assembly.Projecto.Final.Domain.Models;
using Assembly.Projecto.Final.Services.Dtos.GetDtos;
using Assembly.Projecto.Final.Services.Dtos.IServiceDtos.EmployeeUserDtos;
using Assembly.Projecto.Final.Services.Dtos.IServiceDtos.OtherModelsDtos;
using Assembly.Projecto.Final.Services.Exceptions;
using Assembly.Projecto.Final.Services.Interfaces;
using Assembly.Projecto.Final.Services.Pagination;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembly.Projecto.Final.Services.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        public AppointmentService(IUnitOfWork unitOfWork,IMapper mapper) 
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public AppointmentDto Add(CreateAppointmentServiceDto createAppointmentServiceDto)
        {
            Appointment addedAppointment;

            using (_unitOfWork) 
            {
                _unitOfWork.BeginTransaction();

                Employee employee;

                if (createAppointmentServiceDto.IsStaff)
                {
                    employee = _unitOfWork.StaffRepository.GetById(createAppointmentServiceDto.EmployeeId);
                }
                else
                {
                    employee = _unitOfWork.AgentRepository.GetById(createAppointmentServiceDto.EmployeeId);
                }

                NotFoundException.When(employee is null, $"{nameof(employee)} não foi encontrado.");

                var appointmentsConflict = _unitOfWork.AppointmentRepository
                    .GetAppointmentIntersections(createAppointmentServiceDto.Date,
                    createAppointmentServiceDto.HourStart, createAppointmentServiceDto.HourEnd);

                foreach(var appointmentConflict in appointmentsConflict) 
                {
                    CustomApplicationException.When(appointmentConflict.Participants
                        .Any(p => p.EmployeeId == employee.Id),
                        $"O {nameof(employee)} já tem um appointment marcado para este horário.");
                }
                
                var appointment = Appointment.Create(createAppointmentServiceDto.Title,
                    createAppointmentServiceDto.Description,
                    createAppointmentServiceDto.Date,
                    createAppointmentServiceDto.HourStart,createAppointmentServiceDto.HourEnd,
                    createAppointmentServiceDto.Status);

                addedAppointment = _unitOfWork.AppointmentRepository.Add(appointment);

                var participant = Participant.Create(ParticipantType.Organizer,addedAppointment,employee);

                _unitOfWork.ParticipantRepository.Add(participant);

                _unitOfWork.Commit();

            }

            return _mapper.Map<AppointmentDto>(addedAppointment);
        }

        public ParticipantDto AddParticipant(int appointmentId, int employeeId)
        {
            using (_unitOfWork) 
            {
                var appointment = _unitOfWork.AppointmentRepository.GetByIdWithParticipants(appointmentId);

                NotFoundException.When(appointment is null,$"{nameof(appointment)} não foi encontrado.");

                Employee employee = _unitOfWork.StaffRepository.GetById(employeeId);

                if (employee == null) 
                {
                    employee = _unitOfWork.AgentRepository.GetById(employeeId);
                }

                NotFoundException.When(employee is null, $"{nameof(employee)} não foi encontrado.");

                NotFoundException.When(appointment.Participants.Any(a => a.EmployeeId == employeeId),
                    $"O {nameof(employee)} já é participante deste appointment.");

                var appointmentsConflict = _unitOfWork.AppointmentRepository
                        .GetAppointmentIntersections(appointment.Date,appointment.HourStart, appointment.HourEnd);

                foreach (var appointmentConflict in appointmentsConflict)
                {
                    CustomApplicationException.When(appointmentConflict.Participants
                        .Any(p => p.EmployeeId == employee.Id),
                         $"O {nameof(employee)} já tem um appointment marcado para este horário.");
                }

                var participant = Participant.Create(ParticipantType.Participant, appointment, employee);

                var addedParticipant =_unitOfWork.ParticipantRepository.Add(participant);

                _unitOfWork.Commit();

                return _mapper.Map<ParticipantDto>(addedParticipant);
            }
        }

        public ParticipantDto DeleteParticipant(int appointmentId, int participantId)
        {
            using (_unitOfWork)
            {
                var appointment = _unitOfWork.AppointmentRepository.GetByIdWithParticipants(appointmentId);

                NotFoundException.When(appointment is null, $"{nameof(appointment)} não foi encontrado.");

                var participant = appointment.Participants.FirstOrDefault(a => a.Id == participantId);

                NotFoundException.When(participant is null, $"{nameof(participant)} não foi encontrado.");

                CustomApplicationException.When(participant.Role == ParticipantType.Organizer,
                    $" Não pode apagar o organizador do appointment.");

                var deletedParticipant =_unitOfWork.ParticipantRepository.Delete(participant);

                _unitOfWork.Commit();

                return _mapper.Map<ParticipantDto>(deletedParticipant);
              
            }
        }

        public AppointmentDto Delete(AppointmentDto appointmentDto)
        {
            Appointment deletedAppointment;

            using (_unitOfWork) 
            {
                _unitOfWork.BeginTransaction();

                var foundedAppointment = _unitOfWork.AppointmentRepository.GetByIdWithParticipants(appointmentDto.Id);

                NotFoundException.When(foundedAppointment is null, $"{nameof(foundedAppointment)} não foi encontrado.");

                foreach(var participant in foundedAppointment.Participants) 
                {
                    _unitOfWork.ParticipantRepository.Delete(participant);
                }

                deletedAppointment =_unitOfWork.AppointmentRepository.Delete(foundedAppointment);

                _unitOfWork.Commit();
            }

            return _mapper.Map<AppointmentDto>(deletedAppointment);
        }

        public AppointmentDto Delete(int id)
        {
            Appointment deletedAppointment;

            using( _unitOfWork) 
            {
                _unitOfWork.BeginTransaction();

                var foundedAppointment = _unitOfWork.AppointmentRepository.GetByIdWithParticipants(id);

                NotFoundException.When(foundedAppointment is null, $"{nameof(foundedAppointment)} não foi encontrado.");

                foreach (var participant in foundedAppointment.Participants)
                {
                    _unitOfWork.ParticipantRepository.Delete(participant);
                }

                deletedAppointment = _unitOfWork.AppointmentRepository.Delete(foundedAppointment);

                _unitOfWork.Commit();
            }

            return _mapper.Map<AppointmentDto>(deletedAppointment);
        }

        public List<AppointmentDto> GetAll()
        {
            var list = new List<AppointmentDto>();

            foreach(var appointment in _unitOfWork.AppointmentRepository.GetAll()) 
            {
                var appointmentDto = _mapper.Map<AppointmentDto>(appointment);

                list.Add(appointmentDto);
            }

            return list;
        }

        public AppointmentDto GetById(int id)
        {
            var appointment = _unitOfWork.AppointmentRepository.GetById(id);

            return _mapper.Map<AppointmentDto>(appointment);
        }

        public AppointmentDto Update(AppointmentDto appointmentDto) 
        {
            throw new NotImplementedException();
        }
        public AppointmentDto Update(AppointmentDto appointmentDto,int employeeId)
        {
            Appointment updatedAppointment;

            using (_unitOfWork)
            {
                var foundedAppointment = _unitOfWork.AppointmentRepository.GetById(appointmentDto.Id);

                NotFoundException.When(foundedAppointment is null, $"{nameof(foundedAppointment)} não foi encontrado.");

                Employee employee = _unitOfWork.StaffRepository.GetById(employeeId);

                if (employee == null)
                {
                    employee = _unitOfWork.AgentRepository.GetById(employeeId);
                }

                NotFoundException.When(employee is null, $"{nameof(employee)} não foi encontrado.");

                var appointmentsConflict = _unitOfWork.AppointmentRepository
                       .GetAppointmentIntersections(appointmentDto.Date, appointmentDto.HourStart,
                       appointmentDto.HourEnd);

                foreach (var appointmentConflict in appointmentsConflict)
                {
                    if (appointmentConflict.Id != foundedAppointment.Id)
                    {
                        CustomApplicationException.When(appointmentConflict.Participants
                        .Any(p => p.EmployeeId == employee.Id),
                         $"O {nameof(employee)} já tem um appointment marcado para este horário.");
                    }
                }

                foundedAppointment.Update(appointmentDto.Title, appointmentDto.Description,
                    appointmentDto.Date, appointmentDto.HourStart,appointmentDto.HourEnd,
                    appointmentDto.Status);

                updatedAppointment = _unitOfWork.AppointmentRepository.Update(foundedAppointment);

                _unitOfWork.Commit();
            }

            return _mapper.Map<AppointmentDto>(updatedAppointment);
        }
        public List<AppointmentAllDto> GetAllWithParticipants()
        {
            var appointments = _unitOfWork.AppointmentRepository.GetAllWithParticipants();

            return _mapper.Map<List<AppointmentAllDto>>(appointments);
        }

        public AppointmentAllDto GetByIdWithParticipants(int id)
        {
            var appointment =_unitOfWork.AppointmentRepository.GetByIdWithParticipants(id);

            return _mapper.Map<AppointmentAllDto>(appointment);
        }

        public List<AppointmentAllDto> GetBetweenToDates(DateTime startDate, DateTime endDate)
        {
            var appointments = _unitOfWork.AppointmentRepository.GetBetweenToDates(startDate,endDate);

            return _mapper.Map<List<AppointmentAllDto>>(appointments);
        }

        public List<AppointmentAllDto> GetAppointmentIntersections(DateTime date, TimeOnly hourStart, TimeOnly hourEnd)
        {
            var appointments = _unitOfWork.AppointmentRepository.GetAppointmentIntersections(date,hourStart,hourEnd);

            return _mapper.Map<List<AppointmentAllDto>>(appointments);
        }

        public Pagination<AppointmentAllDto> GetAppointmentsPaginationByEmployeeId(int employeeId, int pageNumber, int pageSize,
            string search)
        {
            var totalCount = _unitOfWork.AppointmentRepository.GetTotalCount(employeeId, search);

            var appointments = _unitOfWork.AppointmentRepository
                   .GetAppointmentsPaginationByEmployeeId(employeeId, pageNumber, pageSize, search);

            var pagination = Pagination<AppointmentAllDto>.Create(_mapper.Map<List<AppointmentAllDto>>(appointments),
                pageNumber, pageSize, totalCount);

            return pagination;
        }
    }
}
