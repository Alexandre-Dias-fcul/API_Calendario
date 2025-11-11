using Assembly.Projecto.Final.Domain.Common;
using Assembly.Projecto.Final.Domain.Core.Repositories;
using Assembly.Projecto.Final.Domain.Enums;
using Assembly.Projecto.Final.Domain.Models;
using Assembly.Projecto.Final.Services.Dtos.GetDtos;
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
    public class PersonalContactService : IPersonalContactService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IMapper _mapper;
        public PersonalContactService(IUnitOfWork unitOfWork,IMapper mapper) 
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public PersonalContactDto Add(CreatePersonalContactServiceDto createPersonalContactServiceDto)
        {
            PersonalContact addedPersonalContact;

            using (_unitOfWork) 
            {
                var personalContact = PersonalContact.Create(createPersonalContactServiceDto.Name,
                    createPersonalContactServiceDto.IsPrimary,createPersonalContactServiceDto.Notes);

                Employee employee;

                if(createPersonalContactServiceDto.IsStaff) 
                {
                    employee = _unitOfWork.StaffRepository.GetById(createPersonalContactServiceDto.EmployeeId);
                }
                else 
                {
                    employee = _unitOfWork.AgentRepository.GetById(createPersonalContactServiceDto.EmployeeId);
                }

                NotFoundException.When(employee is null,$"{nameof(employee)} não foi encontrado.");

                personalContact.SetEmployee(employee);

                addedPersonalContact = _unitOfWork.PersonalContactRepository.Add(personalContact);

                _unitOfWork.Commit();
            }

            return _mapper.Map<PersonalContactDto>(addedPersonalContact);
        }

        public PersonalContactDetailDto AddDetail(int personalContactId, 
                                                   CreatePersonalContactDetailDto createPersonalContactDetailDto)
        {
            using (_unitOfWork) 
            {
                var personalContact = _unitOfWork.PersonalContactRepository.GetByIdWithDetail(personalContactId);

                NotFoundException.When(personalContact is null,
                    $"{nameof(personalContact)} não foi encontrado.");

                var exists = personalContact.PersonalContactDetails
                    .Any(p => p.ContactType == createPersonalContactDetailDto.ContactType 
                         && p.Value == createPersonalContactDetailDto.Value);

                CustomApplicationException.When(exists, " O contacto já existe.");

                var personalContactDetail = PersonalContactDetail.Create(createPersonalContactDetailDto.ContactType,
                    createPersonalContactDetailDto.Value);

                personalContact.AddPersonalContactDetail(personalContactDetail);

                _unitOfWork.PersonalContactRepository.Update(personalContact);

                var foundedPersonalContactDetail = personalContact.PersonalContactDetails
                    .FirstOrDefault(p => p.ContactType == personalContactDetail.ContactType &&
                     p.Value == personalContactDetail.Value);

                _unitOfWork.Commit();

                var personalContactDetailDto = _mapper.Map<PersonalContactDetailDto>(foundedPersonalContactDetail);

                return personalContactDetailDto;

            }
        }

        public PersonalContactDetailDto UpdateDetail(int personalContactId,  
            PersonalContactDetailDto personalContactDetailDto)
        {
            using (_unitOfWork) 
            {
                var personalContact = _unitOfWork.PersonalContactRepository.GetByIdWithDetail(personalContactId);

                NotFoundException.When(personalContact is null, $"{nameof(personalContact)} não foi encontrado.");

                NotFoundException.When(personalContact.PersonalContactDetails is null, "O contacto não existe.");

                var personalContactDetail = personalContact.PersonalContactDetails
                    .FirstOrDefault(p =>p.Id == personalContactDetailDto.Id);

                NotFoundException.When(personalContactDetail is null,"O contacto não existe.");

                if (personalContactDetail.ContactType != personalContactDetailDto.ContactType ||
                    personalContactDetail.Value != personalContactDetailDto.Value) 
                {
                    personalContact.PersonalContactDetails.FirstOrDefault(p => p.Id == personalContactDetailDto.Id)
                        .Update(personalContactDetailDto.ContactType, personalContactDetailDto.Value);

                    _unitOfWork.PersonalContactRepository.Update(personalContact);

                    _unitOfWork.Commit();
                }

                var foundedPersonalContactDetail = personalContact.PersonalContactDetails
                    .FirstOrDefault(p => p.Id == personalContactDetailDto.Id);

                var foundedPersonalContactDetailDto = _mapper.Map<PersonalContactDetailDto>(foundedPersonalContactDetail);

                return foundedPersonalContactDetailDto;
            }
        }

        public PersonalContactDetailDto DeleteDetail(int personalContactId, int personalContactDetailId)
        {
            using (_unitOfWork) 
            {
                var personalContact = _unitOfWork.PersonalContactRepository.GetByIdWithDetail(personalContactId);

                NotFoundException.When(personalContact is null,$"{nameof(personalContact)} não foi encontrado.");

                NotFoundException.When(personalContact.PersonalContactDetails is null,"O contacto não existe.");

                var personalContactDetail = personalContact.PersonalContactDetails
                    .FirstOrDefault(p => p.Id == personalContactDetailId);

                NotFoundException.When(personalContactDetail is null, "O contacto não existe.");

                var deletedPersonalContactDetail=_unitOfWork.PersonalContactDetailRepository.Delete(personalContactDetail);

                _unitOfWork.Commit();

                return _mapper.Map<PersonalContactDetailDto>(deletedPersonalContactDetail);
            }
        }

        public PersonalContactDto Delete(PersonalContactDto personalContactDto)
        {
            PersonalContact deletedPersonalContact;

            using (_unitOfWork) 
            {
                _unitOfWork.BeginTransaction();

                var foundedPersonalContact = _unitOfWork.PersonalContactRepository.GetByIdWithDetail(personalContactDto.Id);

                NotFoundException.When(foundedPersonalContact is null, 
                    $"{nameof(foundedPersonalContact)} não foi encontrado.");

                foreach(var detail in foundedPersonalContact.PersonalContactDetails) 
                {
                    _unitOfWork.PersonalContactDetailRepository.Delete(detail);
                }

                deletedPersonalContact =_unitOfWork.PersonalContactRepository.Delete(foundedPersonalContact);

                _unitOfWork.Commit();
            }

            return _mapper.Map<PersonalContactDto>(deletedPersonalContact);
        }

        public PersonalContactDto Delete(int id)
        {
            PersonalContact deletedPersonalContact;

            using (_unitOfWork)
            {
                _unitOfWork.BeginTransaction();

                var foundedPersonalContact = _unitOfWork.PersonalContactRepository.GetByIdWithDetail(id);

                NotFoundException.When(foundedPersonalContact is null,
                    $"{nameof(foundedPersonalContact)} não foi encontrado.");

                foreach (var detail in foundedPersonalContact.PersonalContactDetails)
                {
                    _unitOfWork.PersonalContactDetailRepository.Delete(detail);
                }

                deletedPersonalContact = _unitOfWork.PersonalContactRepository.Delete(foundedPersonalContact);

                _unitOfWork.Commit();
            }

            return _mapper.Map<PersonalContactDto>(deletedPersonalContact);
        }

        public List<PersonalContactDto> GetAll()
        {
            var list = new List<PersonalContactDto>();

            foreach (var personalContact in _unitOfWork.PersonalContactRepository.GetAll()) 
            {
                var personalContactDto = _mapper.Map<PersonalContactDto>(personalContact);

                list.Add(personalContactDto);
            }

            return list;
        }

        public PersonalContactDto GetById(int id)
        {
            var personalContact = _unitOfWork.PersonalContactRepository.GetById(id);

            return _mapper.Map<PersonalContactDto>(personalContact);
        }

        public PersonalContactAllDto GetByIdWithDetail(int id)
        {
            var personalContactWithDetail = _unitOfWork.PersonalContactRepository.GetByIdWithDetail(id);

            return _mapper.Map<PersonalContactAllDto>(personalContactWithDetail);
        }

        public PersonalContactDto Update(PersonalContactDto personalContactDto)
        {
            PersonalContact updatedPersonalContact;

            using (_unitOfWork)
            {

                var foundedPersonalContact = _unitOfWork.PersonalContactRepository.GetById(personalContactDto.Id);

                NotFoundException.When(foundedPersonalContact is null,
                    $"{nameof(foundedPersonalContact)} não foi encontrado.");

                foundedPersonalContact.Update(personalContactDto.Name,personalContactDto.IsPrimary,
                    personalContactDto.Notes);

                updatedPersonalContact = _unitOfWork.PersonalContactRepository.Update(foundedPersonalContact);

                _unitOfWork.Commit();
            }

            return _mapper.Map<PersonalContactDto>(updatedPersonalContact);
        }

        public Pagination<PersonalContact> GetPersonalContactPaginationByEmployeeId(int employeeId, int pageNumber, int pageSize,
            string search)
        {
            var totalCount = _unitOfWork.PersonalContactRepository.GetTotalCount(employeeId, search);

            var appointments = _unitOfWork.PersonalContactRepository
                   .GetPersonalContactPaginationByEmployeeId(employeeId, pageNumber, pageSize, search);

            var pagination = Pagination<PersonalContact>.Create(_mapper.Map<List<PersonalContact>>(appointments),
                pageNumber, pageSize, totalCount);

            return pagination;
        }
    }
}
