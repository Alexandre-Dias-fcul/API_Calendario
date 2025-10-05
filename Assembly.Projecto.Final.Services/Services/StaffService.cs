using Assembly.Projecto.Final.Domain.Common;
using Assembly.Projecto.Final.Domain.Core.Repositories;
using Assembly.Projecto.Final.Domain.Enums;
using Assembly.Projecto.Final.Domain.Models;
using Assembly.Projecto.Final.Services.Dtos.GetDtos;
using Assembly.Projecto.Final.Services.Dtos.IServiceDtos.EmployeeUserDtos;
using Assembly.Projecto.Final.Services.Dtos.IServiceDtos.OtherModelsDtos;
using Assembly.Projecto.Final.Services.Exceptions;
using Assembly.Projecto.Final.Services.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Assembly.Projecto.Final.Services.Services
{
    public class StaffService : IStaffService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IMapper _mapper;

        public StaffService(IUnitOfWork unitOfWork,IMapper mapper) 
        {
            _unitOfWork = unitOfWork;

            _mapper = mapper;
        }

        public StaffDto Add(CreateStaffDto createStaffDto)
        {
            Staff addedStaff;

            using(_unitOfWork) 
            {
                var name = Name.Create(createStaffDto.Name.FirstName, string.Join(" ",createStaffDto.Name.MiddleNames),
                    createStaffDto.Name.LastName);

                var staff = Staff.Create(name, createStaffDto.DateOfBirth, createStaffDto.Gender,
                    createStaffDto.PhotoFileName, createStaffDto.IsActive,createStaffDto.HiredDate,
                    createStaffDto.DateOfTermination);

                addedStaff = _unitOfWork.StaffRepository.Add(staff);

                _unitOfWork.Commit();
            }

            return _mapper.Map<StaffDto>(addedStaff);
        }

        public AccountDto AccountAdd(int staffId, CreateAccountDto createAccountDto)
        {
            using (_unitOfWork)
            {
                var emailExists = _unitOfWork.AccountRepository.EmailExistsEmployee(createAccountDto.Email);

                CustomApplicationException.When(emailExists is true, "O email já existe.");

                var staff = _unitOfWork.StaffRepository.GetByIdWithAccount(staffId);

                NotFoundException.When(staff is null, $"{nameof(staff)} não foi encontrado.");

                CustomApplicationException.When(staff.EntityLink is not null && staff.EntityLink.Account is not null,
                    "A account já existe");

                byte[] passwordHash;
                byte[] passwordSalt;

                using (var hmac = new HMACSHA512())
                {
                    passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(createAccountDto.Password));
                    passwordSalt = hmac.Key;
                }

                var account = Account.Create(passwordHash, passwordSalt, createAccountDto.Email);
               
                if (staff.EntityLink is null)
                {
                    var entityLink = EntityLink.Create(EntityType.Employee, staff.Id);

                    staff.SetEntityLink(entityLink);
                }

                staff.EntityLink.SetAccount(account);

                _unitOfWork.StaffRepository.Update(staff);

                var accountDto = _mapper.Map<AccountDto>(staff.EntityLink.Account);

                _unitOfWork.Commit();

                return accountDto;
            }
        }

        public AddressDto AddressAdd(int staffId, CreateAddressDto createAddressDto)
        {
            using (_unitOfWork)
            {
                var staff = _unitOfWork.StaffRepository.GetByIdWithAddresses(staffId);

                NotFoundException.When(staff is null, $"{nameof(staff)} não foi encontrado.");

                if (staff.EntityLink is null)
                {
                    var entityLink = EntityLink.Create(EntityType.Employee, staff.Id);

                    staff.SetEntityLink(entityLink);
                }

                var exists = staff.EntityLink.Addresses.Any(address => address.Street == createAddressDto.Street &&
                             address.City == createAddressDto.City && address.Country == createAddressDto.Country &&
                             address.PostalCode == createAddressDto.PostalCode);

                NotFoundException.When(exists, "Este endereço já existe.");

                var address = Address.Create(createAddressDto.Street, createAddressDto.City, createAddressDto.Country,
                        createAddressDto.PostalCode);

                staff.EntityLink.AddAddress(address);

                _unitOfWork.StaffRepository.Update(staff);

                var foundedAddress = staff.EntityLink.Addresses.FirstOrDefault(a => a.Street == address.Street &&
                          a.City == address.City && a.Country == address.Country && a.PostalCode == address.PostalCode);

                _unitOfWork.Commit();

                var addressDto = _mapper.Map<AddressDto>(foundedAddress);

                return addressDto;
            }
        }

        public ContactDto ContactAdd(int staffId, CreateContactDto createContactDto)
        {
            using (_unitOfWork)
            {
                var staff = _unitOfWork.StaffRepository.GetByIdWithContacts(staffId);

                NotFoundException.When(staff is null, $"{nameof(staff)} não foi encontrado.");

                if (staff.EntityLink is null)
                {
                    var entityLink = EntityLink.Create(EntityType.Employee, staff.Id);

                    staff.SetEntityLink(entityLink);
                }

                var exists = staff.EntityLink.Contacts.Any(contact =>
                    contact.ContactType == createContactDto.ContactType && contact.Value == createContactDto.Value);

                CustomApplicationException.When(exists, "Este contacto já existe.");

                var contact = Contact.Create(createContactDto.ContactType, createContactDto.Value);

                staff.EntityLink.AddContact(contact);

                _unitOfWork.StaffRepository.Update(staff);

                var foundedContact = staff.EntityLink.Contacts.FirstOrDefault(c => c.ContactType == contact.ContactType
                                && c.Value == contact.Value);

                _unitOfWork.Commit();

                var contactDto = _mapper.Map<ContactDto>(foundedContact);

                return contactDto;
            }
        }
        public AccountDto AccountUpdate(int staffId, UpdateAccountDto updateAccountDto)
        {
            using (_unitOfWork)
            {
                var staff = _unitOfWork.StaffRepository.GetByIdWithAccount(staffId);

                NotFoundException.When(staff is null, $"{nameof(staff)} não foi encontrado.");

                NotFoundException.When(staff.EntityLink is null, "A account não existe.");

                NotFoundException.When(staff.EntityLink.Account is null,"A account não existe.");

                bool isSamePassword;
                byte[] passwordHash = staff.EntityLink.Account.PasswordHash;
                byte[] passwordSalt = staff.EntityLink.Account.PasswordSalt;

                using (var hmac = new HMACSHA512(passwordSalt))
                {
                    var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(updateAccountDto.Password));

                    isSamePassword = computedHash.SequenceEqual(passwordHash);
                }

                if (!isSamePassword)
                {
                    using (var hmac = new HMACSHA512())
                    {
                        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(updateAccountDto.Password));

                        passwordSalt = hmac.Key;
                    }
                }

                if (updateAccountDto.Email != staff.EntityLink.Account.Email)
                {
                    var emailExists = _unitOfWork.AccountRepository.EmailExistsEmployee(updateAccountDto.Email);

                    CustomApplicationException.When(emailExists is true, "O email já existe.");
                }


                if (!isSamePassword || updateAccountDto.Email != staff.EntityLink.Account.Email)
                {
                    staff.EntityLink.Account.Update(passwordHash, passwordSalt, updateAccountDto.Email);

                    _unitOfWork.StaffRepository.Update(staff);

                    _unitOfWork.Commit();
                }

                var updatedAccountDto = _mapper.Map<AccountDto>(staff.EntityLink.Account);

                return updatedAccountDto;
            }
        }

        public ContactDto ContactUpdate(int staffId, ContactDto contactDto)
        {
            using (_unitOfWork)
            {
                var staff = _unitOfWork.StaffRepository.GetByIdWithContacts(staffId);

                NotFoundException.When(staff is null, $"{nameof(staff)} não foi encontado.");

                NotFoundException.When(staff.EntityLink is null, "O contacto não existe.");

                NotFoundException.When(staff.EntityLink.Contacts is null, "O contacto não existe.");

                var contacto = staff.EntityLink.Contacts.FirstOrDefault(c => c.Id == contactDto.Id);

                NotFoundException.When(contacto is null, "O contacto não existe.");

                if (contacto.ContactType != contactDto.ContactType || contacto.Value != contactDto.Value)
                {
                    staff.EntityLink.Contacts.FirstOrDefault(c => c.Id == contactDto.Id)
                        .Update(contactDto.ContactType, contactDto.Value);

                    _unitOfWork.StaffRepository.Update(staff);

                    _unitOfWork.Commit();
                }

                var foundedContact = staff.EntityLink.Contacts.FirstOrDefault(c => c.Id == contactDto.Id);

                var updatedContactDto = _mapper.Map<ContactDto>(foundedContact);

                return updatedContactDto;

            }
        }

        public AddressDto AddressUpdate(int staffId, AddressDto addressDto)
        {
            using (_unitOfWork)
            {
                var staff = _unitOfWork.StaffRepository.GetByIdWithAddresses(staffId);

                NotFoundException.When(staff is null, $"{nameof(staff)} não foi encontado.");

                NotFoundException.When(staff.EntityLink is null, "O address não existe.");

                NotFoundException.When(staff.EntityLink.Addresses is null, "O address não existe.");

                var address = staff.EntityLink.Addresses.FirstOrDefault(a => a.Id == addressDto.Id);

                NotFoundException.When(address is null, "O address não existe.");

                if (address.Street != addressDto.Street || address.City != addressDto.City
                     || address.Country != addressDto.Country || address.PostalCode != addressDto.PostalCode)
                {
                    staff.EntityLink.Addresses.FirstOrDefault(a => a.Id == addressDto.Id).
                        Update(addressDto.Street, addressDto.City, addressDto.Country, addressDto.PostalCode);

                    _unitOfWork.StaffRepository.Update(staff);

                    _unitOfWork.Commit();
                }

                var foundedAddress = staff.EntityLink.Addresses.FirstOrDefault(a => a.Id == addressDto.Id);

                var updatedAddressDto = _mapper.Map<AddressDto>(foundedAddress);

                return updatedAddressDto;
            }
        }

        public AccountDto AccountDelete(int staffId)
        {
            using (_unitOfWork)
            {
                var staff = _unitOfWork.StaffRepository.GetByIdWithAccount(staffId);

                NotFoundException.When(staff is null, $"{nameof(staff)} não foi encontrado.");

                NotFoundException.When(staff.EntityLink is null, "A account não existe.");

                NotFoundException.When(staff.EntityLink.Account is null, "A account não existe.");

                var account = staff.EntityLink.Account;

                staff.EntityLink.RemoveAccount();

                _unitOfWork.StaffRepository.Update(staff);

                _unitOfWork.Commit();

                var deletedAccountDto = _mapper.Map<AccountDto>(account);

                return deletedAccountDto;
            }
        }

        public ContactDto ContactDelete(int staffId, int contactId)
        {
            using (_unitOfWork)
            {

                var staff = _unitOfWork.StaffRepository.GetByIdWithContacts(staffId);

                NotFoundException.When(staff is null, $"{nameof(staff)} não foi encontado.");

                NotFoundException.When(staff.EntityLink is null, "O contacto não existe.");

                NotFoundException.When(staff.EntityLink.Contacts is null, "O contacto não existe.");

                var contacto = staff.EntityLink.Contacts.FirstOrDefault(c => c.Id == contactId);

                NotFoundException.When(contacto is null, "O contacto não existe.");

                staff.EntityLink.RemoveContact(contacto);

                _unitOfWork.ContactRepository.Delete(contacto);

                _unitOfWork.StaffRepository.Update(staff);

                _unitOfWork.Commit();

                var deletedContactDto = _mapper.Map<ContactDto>(contacto);

                return deletedContactDto;
            }
        }

        public AddressDto AddressDelete(int staffId, int addressId)
        {
            var staff = _unitOfWork.StaffRepository.GetByIdWithAddresses(staffId);

            NotFoundException.When(staff is null, $"{nameof(staff)} não foi encontado.");

            NotFoundException.When(staff.EntityLink is null, "O address não existe.");

            NotFoundException.When(staff.EntityLink.Addresses is null, "O address não existe.");

            var address = staff.EntityLink.Addresses.FirstOrDefault(a => a.Id == addressId);

            NotFoundException.When(address is null, "O address não existe.");

            staff.EntityLink.RemoveAddress(address);

            _unitOfWork.AddressRepository.Delete(address);

            _unitOfWork.StaffRepository.Update(staff);

            _unitOfWork.Commit();

            var deletedAddressDto = _mapper.Map<AddressDto>(address);

            return deletedAddressDto;
        }

        public StaffDto Delete(StaffDto staffDto)
        {
            Staff deletedStaff;

            using (_unitOfWork) 
            {
                _unitOfWork.BeginTransaction();

                var foundedStaff = _unitOfWork.StaffRepository.GetByIdWithEverything(staffDto.Id);

                NotFoundException.When(foundedStaff is null, $"{nameof(foundedStaff)} não foi encontrado.");

                if (foundedStaff.EntityLink is not null)
                {
                    if (foundedStaff.EntityLink.Account is not null)
                    {
                        _unitOfWork.AccountRepository.Delete(foundedStaff.EntityLink.Account);
                    }

                    foreach (var contact in foundedStaff.EntityLink.Contacts)
                    {
                        _unitOfWork.ContactRepository.Delete(contact);
                    }

                    foreach (var address in foundedStaff.EntityLink.Addresses)
                    {
                        _unitOfWork.AddressRepository.Delete(address);
                    }

                    _unitOfWork.EntityLinkRepository.Delete(foundedStaff.EntityLink);
                }

                foreach (var participant in foundedStaff.Participants)
                {
                    if (participant.Role == ParticipantType.Organizer)
                    {
                        var appointment = _unitOfWork.AppointmentRepository
                            .GetByIdWithParticipants(participant.AppointmentId);

                        foreach (var par in appointment.Participants)
                        {
                            _unitOfWork.ParticipantRepository.Delete(par);
                        }

                        _unitOfWork.AppointmentRepository.Delete(appointment);
                    }
                    else if (participant.Role == ParticipantType.Participant)
                    {
                        _unitOfWork.ParticipantRepository.Delete(participant);
                    }
                }

                foreach (var personalContact in foundedStaff.PersonalContacts)
                {
                    foreach (var parsonalDetail in personalContact.PersonalContactDetails)
                    {
                        _unitOfWork.PersonalContactDetailRepository.Delete(parsonalDetail);
                    }

                    _unitOfWork.PersonalContactRepository.Delete(personalContact);
                }

                deletedStaff = _unitOfWork.StaffRepository.Delete(foundedStaff);

                _unitOfWork.Commit();
            }

            return _mapper.Map<StaffDto>(deletedStaff);
        }

        public StaffDto Delete(int id)
        {
            Staff deletedStaff;

            using (_unitOfWork)
            {
                _unitOfWork.BeginTransaction();

                var foundedStaff = _unitOfWork.StaffRepository.GetByIdWithEverything(id);

                NotFoundException.When(foundedStaff is null, $"{nameof(foundedStaff)} não foi encontrado.");

                if (foundedStaff.EntityLink is not null)
                {
                    if (foundedStaff.EntityLink.Account is not null)
                    {
                        _unitOfWork.AccountRepository.Delete(foundedStaff.EntityLink.Account);
                    }

                    foreach (var contact in foundedStaff.EntityLink.Contacts)
                    {
                        _unitOfWork.ContactRepository.Delete(contact);
                    }

                    foreach (var address in foundedStaff.EntityLink.Addresses)
                    {
                        _unitOfWork.AddressRepository.Delete(address);
                    }

                    _unitOfWork.EntityLinkRepository.Delete(foundedStaff.EntityLink);
                }

                foreach (var participant in foundedStaff.Participants)
                {
                    if (participant.Role == ParticipantType.Organizer)
                    {
                        var appointment = _unitOfWork.AppointmentRepository
                            .GetByIdWithParticipants(participant.AppointmentId);

                        foreach (var par in appointment.Participants)
                        {
                            _unitOfWork.ParticipantRepository.Delete(par);
                        }

                        _unitOfWork.AppointmentRepository.Delete(appointment);
                    }
                    else if (participant.Role == ParticipantType.Participant)
                    {
                        _unitOfWork.ParticipantRepository.Delete(participant);
                    }
                }

                foreach (var personalContact in foundedStaff.PersonalContacts) 
                {
                    foreach (var parsonalDetail in personalContact.PersonalContactDetails) 
                    { 
                        _unitOfWork.PersonalContactDetailRepository.Delete(parsonalDetail);
                    }

                    _unitOfWork.PersonalContactRepository.Delete(personalContact);
                }

                deletedStaff = _unitOfWork.StaffRepository.Delete(foundedStaff);

                _unitOfWork.Commit();
            }

            return _mapper.Map<StaffDto>(deletedStaff);
        }

        public List<StaffDto> GetAll()
        {
            var list = new List<StaffDto>();

            foreach(var staff in _unitOfWork.StaffRepository.GetAll()) 
            {
                var staffDto = _mapper.Map<StaffDto>(staff);

                list.Add(staffDto);
            }

            return list;
        }

        public StaffDto GetById(int id)
        {
            var staff = _unitOfWork.StaffRepository.GetById(id);

            return _mapper.Map<StaffDto>(staff);
        }

        public StaffAllDto GetByIdWithAll(int id)
        {
            var staff = _unitOfWork.StaffRepository.GetByIdWithAll(id);

            return _mapper.Map<StaffAllDto>(staff);
        }

        public StaffDto Update(StaffDto staffDto)
        {

            Staff updatedStaff;

            using (_unitOfWork)
            {
                var foundedStaff = _unitOfWork.StaffRepository.GetById(staffDto.Id);

                NotFoundException.When(foundedStaff is null, $"{nameof(foundedStaff)} não foi encontrado.");

                var name = Name.Create(staffDto.Name.FirstName,string.Join(" ",staffDto.Name.MiddleNames),
                   staffDto.Name.LastName);

                foundedStaff.Update(name, staffDto.DateOfBirth, staffDto.Gender,
                    staffDto.PhotoFileName, staffDto.IsActive, staffDto.HiredDate,
                    staffDto.DateOfTermination);

                updatedStaff = _unitOfWork.StaffRepository.Update(foundedStaff);

                _unitOfWork.Commit();
            }

            return _mapper.Map<StaffDto>(updatedStaff);
        }

        public StaffWithPersonalContactsDto GetByIdWithPersonalContacts(int id)
        {
            var staff = _unitOfWork.StaffRepository.GetByIdWithPersonalContacts(id);

            return _mapper.Map<StaffWithPersonalContactsDto>(staff);
        }

        public StaffWithParticipantsDto GetByIdWithParticipants(int id)
        {
            var staff = _unitOfWork.StaffRepository.GetByIdWithParticipants(id);

            return _mapper.Map<StaffWithParticipantsDto>(staff);
        }

        public StaffDto GetByEmail(string email)
        {
            var account = _unitOfWork.AccountRepository.GetByEmailWithEmployee(email);

            NotFoundException.When(account is null, $"{nameof(account)} não foi encontrada.");

            CustomApplicationException.When(account.EntityLink.EntityType is not EntityType.Employee, " Não é empregado.");

            var employeeId = account.EntityLink.Employee.Id;

            var staff = _unitOfWork.StaffRepository.GetById(employeeId);

            return _mapper.Map<StaffDto>(staff);
        }
    }
}
