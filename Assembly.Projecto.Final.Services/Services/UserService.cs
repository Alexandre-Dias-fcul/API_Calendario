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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Assembly.Projecto.Final.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;

            _mapper = mapper;
        }

        public UserDto Add(CreateUserDto createUserDto)
        {
            User addedUser;

            using(_unitOfWork) 
            {
                var name = Name.Create(createUserDto.Name.FirstName,string.Join(" ",createUserDto.Name.MiddleNames),
                   createUserDto.Name.LastName);

                var user = User.Create(name, createUserDto.DateOfBirth, createUserDto.Gender,
                    createUserDto.PhotoFileName, createUserDto.IsActive);

                addedUser = _unitOfWork.UserRepository.Add(user);

                _unitOfWork.Commit();
            }

            return _mapper.Map<UserDto>(addedUser);
        }

        public AccountDto AccountAdd(int userId, CreateAccountDto createAccountDto)
        {
            using (_unitOfWork)
            {
                var emailExists = _unitOfWork.AccountRepository.EmailExistsUser(createAccountDto.Email);

                CustomApplicationException.When(emailExists is true, "O email já existe.");

                var user = _unitOfWork.UserRepository.GetByIdWithAccount(userId);

                NotFoundException.When(user is null, $"{nameof(user)} não foi encontrado.");

                CustomApplicationException.When(user.EntityLink is not null && user.EntityLink.Account is not null,
                    "A account já existe");

                byte[] passwordHash;
                byte[] passwordSalt;

                using (var hmac = new HMACSHA512())
                {
                    passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(createAccountDto.Password));
                    passwordSalt = hmac.Key;
                }

                var account = Account.Create(passwordHash, passwordSalt, createAccountDto.Email);

                if (user.EntityLink is null)
                {
                    var entityLink = EntityLink.Create(EntityType.User, user.Id);

                    user.SetEntityLink(entityLink);
                }

                user.EntityLink.SetAccount(account);

                _unitOfWork.UserRepository.Update(user);

                var accountDto = _mapper.Map<AccountDto>(user.EntityLink.Account);

                _unitOfWork.Commit();

                return accountDto;
            }
        }

        public AddressDto AddressAdd(int userId, CreateAddressDto createAddressDto)
        {

            using (_unitOfWork)
            {
                var user = _unitOfWork.UserRepository.GetByIdWithAddresses(userId);

                NotFoundException.When(user is null, $"{nameof(user)} não foi encontrado.");

                if (user.EntityLink is null)
                {
                    var entityLink = EntityLink.Create(EntityType.User, user.Id);

                    user.SetEntityLink(entityLink);
                }

                var exists = user.EntityLink.Addresses.Any(address => address.Street == createAddressDto.Street &&
                             address.City == createAddressDto.City && address.Country == createAddressDto.Country &&
                             address.PostalCode == createAddressDto.PostalCode);

                NotFoundException.When(exists, "Este endereço já existe.");

                var address = Address.Create(createAddressDto.Street, createAddressDto.City, createAddressDto.Country,
                        createAddressDto.PostalCode);

                user.EntityLink.AddAddress(address);

                _unitOfWork.UserRepository.Update(user);

                var foundedAddress = user.EntityLink.Addresses.FirstOrDefault(a => a.Street == address.Street &&
                          a.City == address.City && a.Country == address.Country && a.PostalCode == address.PostalCode);

                _unitOfWork.Commit();

                var addressDto = _mapper.Map<AddressDto>(foundedAddress);

                return addressDto;
            }
        }

        public ContactDto ContactAdd(int userId, CreateContactDto createContactDto)
        {
            using (_unitOfWork)
            {
                var user = _unitOfWork.UserRepository.GetByIdWithContacts(userId);

                NotFoundException.When(user is null, $"{nameof(user)} não foi encontrado.");

                if (user.EntityLink is null)
                {
                    var entityLink = EntityLink.Create(EntityType.User, user.Id);

                    user.SetEntityLink(entityLink);
                }

                var exists = user.EntityLink.Contacts.Any(contact =>
                    contact.ContactType == createContactDto.ContactType && contact.Value == createContactDto.Value);

                CustomApplicationException.When(exists, "Este contacto já existe.");

                var contact = Contact.Create(createContactDto.ContactType, createContactDto.Value);

                user.EntityLink.AddContact(contact);

                _unitOfWork.UserRepository.Update(user);

                var foundedContact = user.EntityLink.Contacts.FirstOrDefault(c => c.ContactType == contact.ContactType
                                && c.Value == contact.Value);

                _unitOfWork.Commit();

                var contactDto = _mapper.Map<ContactDto>(foundedContact);

                return contactDto;
            }
        }

        public AccountDto AccountUpdate(int userId, UpdateAccountDto updateAccountDto)
        {
            using (_unitOfWork)
            {
                var user = _unitOfWork.UserRepository.GetByIdWithAccount(userId);

                NotFoundException.When(user is null, $"{nameof(user)} não foi encontrado.");

                NotFoundException.When(user.EntityLink is null, "A account não existe.");

                NotFoundException.When(user.EntityLink.Account is null,"A account não existe.");

                bool isSamePassword;
                byte[] passwordHash = user.EntityLink.Account.PasswordHash;
                byte[] passwordSalt = user.EntityLink.Account.PasswordSalt;

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

                if(updateAccountDto.Email != user.EntityLink.Account.Email) 
                {
                    var emailExists = _unitOfWork.AccountRepository.EmailExistsUser(updateAccountDto.Email);

                    CustomApplicationException.When(emailExists is true, "O email já existe.");
                }

                if (!isSamePassword || updateAccountDto.Email != user.EntityLink.Account.Email)
                {
                    user.EntityLink.Account.Update(passwordHash, passwordSalt, updateAccountDto.Email);

                    _unitOfWork.UserRepository.Update(user);

                    _unitOfWork.Commit();
                }

                var updatedAccountDto = _mapper.Map<AccountDto>(user.EntityLink.Account);

                return updatedAccountDto;
            }
        }

        public ContactDto ContactUpdate(int userId, ContactDto contactDto)
        {
            using (_unitOfWork)
            {
                var user = _unitOfWork.UserRepository.GetByIdWithContacts(userId);

                NotFoundException.When(user is null, $"{nameof(user)} não foi encontado.");

                NotFoundException.When(user.EntityLink is null, "O contacto não existe.");

                NotFoundException.When(user.EntityLink.Contacts is null, "O contacto não existe.");

                var contacto = user.EntityLink.Contacts.FirstOrDefault(c => c.Id == contactDto.Id);

                NotFoundException.When(contacto is null, "O contacto não existe.");

                if (contacto.ContactType != contactDto.ContactType || contacto.Value != contactDto.Value)
                {
                    user.EntityLink.Contacts.FirstOrDefault(c => c.Id == contactDto.Id)
                        .Update(contactDto.ContactType, contactDto.Value);

                    _unitOfWork.UserRepository.Update(user);

                    _unitOfWork.Commit();
                }

                var foundedContact = user.EntityLink.Contacts.FirstOrDefault(c => c.Id == contactDto.Id);

                var updatedContactDto = _mapper.Map<ContactDto>(foundedContact);

                return updatedContactDto;

            }
        }

        public AddressDto AddressUpdate(int userId, AddressDto addressDto)
        {
            using (_unitOfWork)
            {
                var user = _unitOfWork.UserRepository.GetByIdWithAddresses(userId);

                NotFoundException.When(user is null, $"{nameof(user)} não foi encontado.");

                NotFoundException.When(user.EntityLink is null, "O address não existe.");

                NotFoundException.When(user.EntityLink.Addresses is null, "O address não existe.");

                var address = user.EntityLink.Addresses.FirstOrDefault(a => a.Id == addressDto.Id);

                NotFoundException.When(address is null, "O address não existe.");

                if (address.Street != addressDto.Street || address.City != addressDto.City
                     || address.Country != addressDto.Country || address.PostalCode != addressDto.PostalCode)
                {
                    user.EntityLink.Addresses.FirstOrDefault(a => a.Id == addressDto.Id).
                        Update(addressDto.Street, addressDto.City, addressDto.Country, addressDto.PostalCode);

                    _unitOfWork.UserRepository.Update(user);

                    _unitOfWork.Commit();
                }

                var foundedAddress = user.EntityLink.Addresses.FirstOrDefault(a => a.Id == addressDto.Id);

                var updatedAddressDto = _mapper.Map<AddressDto>(foundedAddress);

                return updatedAddressDto;
            }
        }
        public AccountDto AccountDelete(int userId)
        {
            using (_unitOfWork)
            {
                var user = _unitOfWork.UserRepository.GetByIdWithAccount(userId);

                NotFoundException.When(user is null, $"{nameof(user)} não foi encontrado.");

                NotFoundException.When(user.EntityLink is null, "A account não existe.");

                NotFoundException.When(user.EntityLink.Account is null, "A account não existe.");

                var account = user.EntityLink.Account;

                user.EntityLink.RemoveAccount();

                _unitOfWork.UserRepository.Update(user);

                _unitOfWork.Commit();

                var deletedAccountDto = _mapper.Map<AccountDto>(account);

                return deletedAccountDto;
            }
        }

        public ContactDto ContactDelete(int userId, int contactId)
        {
            var user = _unitOfWork.UserRepository.GetByIdWithContacts(userId);

            NotFoundException.When(user is null, $"{nameof(user)} não foi encontado.");

            NotFoundException.When(user.EntityLink is null, "O contacto não existe.");

            NotFoundException.When(user.EntityLink.Contacts is null, "O contacto não existe.");

            var contacto = user.EntityLink.Contacts.FirstOrDefault(c => c.Id == contactId);

            NotFoundException.When(contacto is null, "O contacto não existe.");

            user.EntityLink.RemoveContact(contacto);

            _unitOfWork.ContactRepository.Delete(contacto);

            _unitOfWork.UserRepository.Update(user);

            _unitOfWork.Commit();

            var deletedContactDto = _mapper.Map<ContactDto>(contacto);

            return deletedContactDto;
        }

        public AddressDto AddressDelete(int userId, int addressId)
        {
            var user = _unitOfWork.UserRepository.GetByIdWithAddresses(userId);

            NotFoundException.When(user is null, $"{nameof(user)} não foi encontado.");

            NotFoundException.When(user.EntityLink is null, "O address não existe.");

            NotFoundException.When(user.EntityLink.Addresses is null, "O address não existe.");

            var address = user.EntityLink.Addresses.FirstOrDefault(a => a.Id == addressId);

            NotFoundException.When(address is null, "O address não existe.");

            user.EntityLink.RemoveAddress(address);

            _unitOfWork.AddressRepository.Delete(address);

            _unitOfWork.UserRepository.Update(user);

            _unitOfWork.Commit();

            var deletedAddressDto = _mapper.Map<AddressDto>(address);

            return deletedAddressDto;
        }

        public UserDto Delete(UserDto userDto)
        {
            User deletedUser;

            using (_unitOfWork) 
            {
                _unitOfWork.BeginTransaction();

                var foundedUser = _unitOfWork.UserRepository.GetByIdWithEverything(userDto.Id);

                NotFoundException.When(foundedUser is null, $"{nameof(foundedUser)} não foi encontrado.");

                if(foundedUser.EntityLink is not null) 
                {
                    if (foundedUser.EntityLink.Account is not null)
                    {
                        _unitOfWork.AccountRepository.Delete(foundedUser.EntityLink.Account);
                    }

                    foreach (var contact in foundedUser.EntityLink.Contacts)
                    {
                        _unitOfWork.ContactRepository.Delete(contact);
                    }

                    foreach (var address in foundedUser.EntityLink.Addresses)
                    {
                        _unitOfWork.AddressRepository.Delete(address);
                    }

                    _unitOfWork.EntityLinkRepository.Delete(foundedUser.EntityLink);
                }

                foreach(var favorite in foundedUser.Favorites) 
                {
                    _unitOfWork.FavoriteRepository.Delete(favorite);
                }

                foreach (var feedBack in foundedUser.FeedBacks)
                {
                    _unitOfWork.FeedBackRepository.Delete(feedBack);
                }

                deletedUser = _unitOfWork.UserRepository.Delete(foundedUser);

                _unitOfWork.Commit();
            }

            return _mapper.Map<UserDto>(deletedUser);
        }

        public UserDto Delete(int id)
        {
            User deletedUser;

            using (_unitOfWork)
            {
                _unitOfWork.BeginTransaction();

                var foundedUser = _unitOfWork.UserRepository.GetByIdWithEverything(id);

                NotFoundException.When(foundedUser is null, $"{nameof(foundedUser)} não foi encontrado.");

                if (foundedUser.EntityLink is not null)
                {
                    if (foundedUser.EntityLink.Account is not null)
                    {
                        _unitOfWork.AccountRepository.Delete(foundedUser.EntityLink.Account);
                    }

                    foreach (var contact in foundedUser.EntityLink.Contacts)
                    {
                        _unitOfWork.ContactRepository.Delete(contact);
                    }

                    foreach (var address in foundedUser.EntityLink.Addresses)
                    {
                        _unitOfWork.AddressRepository.Delete(address);
                    }

                    _unitOfWork.EntityLinkRepository.Delete(foundedUser.EntityLink);
                }

                foreach (var favorite in foundedUser.Favorites)
                {
                    _unitOfWork.FavoriteRepository.Delete(favorite);
                }

                foreach (var feedBack in foundedUser.FeedBacks)
                {
                    _unitOfWork.FeedBackRepository.Delete(feedBack);
                }

                deletedUser = _unitOfWork.UserRepository.Delete(foundedUser);

                _unitOfWork.Commit();
            }

            return _mapper.Map<UserDto>(deletedUser);
        }

        public List<UserDto> GetAll()
        {
            var list = new List<UserDto>();

            foreach (var user in _unitOfWork.UserRepository.GetAll()) 
            {
                var userDto = _mapper.Map<UserDto>(user);

                list.Add(userDto);
            }

            return list;
        }

        public UserDto GetById(int id)
        {
            var user = _unitOfWork.UserRepository.GetById(id);

            return _mapper.Map<UserDto>(user);
        }

        public UserAllDto GetByIdWithAll(int id)
        {
            var user = _unitOfWork.UserRepository.GetByIdWithAll(id);

            return _mapper.Map<UserAllDto>(user);
        }

        public UserDto Update(UserDto userDto)
        {
            User updatedUser;

            using (_unitOfWork)
            {
                var foundedUser = _unitOfWork.UserRepository.GetById(userDto.Id);

                NotFoundException.When(foundedUser is null, $"{nameof(foundedUser)} não foi encontrado.");

                var name = Name.Create(userDto.Name.FirstName, string.Join(" ", userDto.Name.MiddleNames),
                   userDto.Name.LastName);

                foundedUser.Update(name, userDto.DateOfBirth, userDto.Gender,
                    userDto.PhotoFileName, userDto.IsActive);

                updatedUser = _unitOfWork.UserRepository.Update(foundedUser);

                _unitOfWork.Commit();
            }

            return _mapper.Map<UserDto>(updatedUser);
        }

        public UserDto GetByEmail(string email)
        {
            var account = _unitOfWork.AccountRepository.GetByEmailWithUser(email);

            NotFoundException.When(account is null, $"{nameof(account)} não foi encontrada.");

            CustomApplicationException.When(account.EntityLink.EntityType is not EntityType.User, "Não é usuário.");

            var userId = account.EntityLink.User.Id;

            var user = _unitOfWork.UserRepository.GetById(userId);

            return _mapper.Map<UserDto>(user);
        }
    }
}
