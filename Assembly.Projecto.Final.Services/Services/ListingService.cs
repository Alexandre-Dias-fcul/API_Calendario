using Assembly.Projecto.Final.Domain.Core.Repositories;
using Assembly.Projecto.Final.Domain.Enums;
using Assembly.Projecto.Final.Domain.Models;
using Assembly.Projecto.Final.Services.Dtos;
using Assembly.Projecto.Final.Services.Dtos.IServiceDtos.EmployeeUserDtos;
using Assembly.Projecto.Final.Services.Dtos.IServiceDtos.OtherModelsDtos;
using Assembly.Projecto.Final.Services.Exceptions;
using Assembly.Projecto.Final.Services.Interfaces;
using Assembly.Projecto.Final.Services.Pagination;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Assembly.Projecto.Final.Services.Services
{
    public class ListingService : IListingService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IMapper _mapper;
        public ListingService(IUnitOfWork unitOfWork, IMapper mapper) 
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public ListingDto Add(CreateListingServiceDto createListingServiceDto)
        {
            Listing addedListing;

            using (_unitOfWork) 
            {
                var listing = Listing.Create(createListingServiceDto.Type,createListingServiceDto.Status,
                    createListingServiceDto.NumberOfBathrooms,createListingServiceDto.NumberOfBathrooms,
                    createListingServiceDto.NumberOfKitchens,createListingServiceDto.Price,createListingServiceDto.Location,
                    createListingServiceDto.Area,createListingServiceDto.Parking,createListingServiceDto.Description,
                    createListingServiceDto.MainImageFileName,createListingServiceDto.OtherImagesFileNames);

                var agent = _unitOfWork.AgentRepository.GetById(createListingServiceDto.AgentId);

                NotFoundException.When(agent is null, $"{nameof(agent)} não foi encontrado.");

                listing.SetAgent(agent);

                addedListing = _unitOfWork.ListingRepository.Add(listing);

                _unitOfWork.Commit();
            }

            return _mapper.Map<ListingDto>(addedListing);
        }

        public ReassignDto SelfReassign(int listingId, int newAgentId)
        {
            using (_unitOfWork) 
            {
                _unitOfWork.BeginTransaction();

                var listing = _unitOfWork.ListingRepository.GetById(listingId);

                NotFoundException.When(listing is null, $" {nameof(listing)} não foi encontrada.");

                var newAgent = _unitOfWork.AgentRepository.GetById(newAgentId);

                NotFoundException.When(newAgent is null, $"{nameof(newAgent)} não foi encontrado.");

                CustomApplicationException.When(listing.AgentId == newAgent.Id, "Esta listing já é deste agente.");

                var previousAgent = _unitOfWork.AgentRepository.GetById(listing.AgentId);

                NotFoundException.When(previousAgent is null,$"{nameof(previousAgent)} não foi encontrado.");

                int? supervisorId = null;

                if (previousAgent.SupervisorId is not null && (previousAgent.SupervisorId == newAgent.Id))
                {
                    supervisorId = (int)previousAgent.SupervisorId;
                }

                NotFoundException.When(supervisorId is null, "Não foi possivel determinar o supervisor.");

                listing.SetAgent(newAgent);

                _unitOfWork.ListingRepository.Update(listing);

                var reassign = Reassign.Create(previousAgent.Id,listing.AgentId,(int)supervisorId, DateTime.UtcNow);

                reassign.SetListing(listing);

                var addedReassign = _unitOfWork.ReassignRepository.Add(reassign);

                _unitOfWork.Commit();

                return _mapper.Map<ReassignDto>(addedReassign);
            }      
        }

        public ReassignDto SelfReassignTo(int listingId, int newAgentId)
        {
            using (_unitOfWork) 
            {
                _unitOfWork.BeginTransaction();

                var listing = _unitOfWork.ListingRepository.GetById(listingId);

                NotFoundException.When(listing is null, $" {nameof(listing)} não foi encontrada.");

                var newAgent = _unitOfWork.AgentRepository.GetById(newAgentId);

                NotFoundException.When(newAgent is null, $"{nameof(newAgent)} não foi encontrado.");

                CustomApplicationException.When(listing.AgentId == newAgent.Id, "Esta listing já é deste agente.");

                var previousAgent = _unitOfWork.AgentRepository.GetById(listing.AgentId);

                NotFoundException.When(previousAgent is null,$"{nameof(previousAgent)} não foi encontrado.");

                int? supervisorId = null;

                if (newAgent.SupervisorId is not null && (newAgent.SupervisorId == previousAgent.Id))
                {
                    supervisorId = (int)newAgent.SupervisorId;
                }

                NotFoundException.When(supervisorId is null, "Não foi possivel determinar o supervisor.");

                listing.SetAgent(newAgent);

                _unitOfWork.ListingRepository.Update(listing);

                var reassign = Reassign.Create(previousAgent.Id,listing.AgentId,(int)supervisorId, DateTime.UtcNow);

                reassign.SetListing(listing);

                var addedReassign = _unitOfWork.ReassignRepository.Add(reassign);

                _unitOfWork.Commit();

                return _mapper.Map<ReassignDto>(addedReassign);
            }      
        }

        public ReassignDto BetweenReassign(int listingId, int newAgentId)
        {
            using (_unitOfWork)
            {
                _unitOfWork.BeginTransaction();

                var listing = _unitOfWork.ListingRepository.GetById(listingId);

                NotFoundException.When(listing is null, $" {nameof(listing)} não foi encontrada.");

                var newAgent = _unitOfWork.AgentRepository.GetById(newAgentId);

                NotFoundException.When(newAgent is null, $"{nameof(newAgent)} não foi encontrado.");

                CustomApplicationException.When(listing.AgentId == newAgent.Id, "Esta listing já é deste agente.");

                var previousAgent = _unitOfWork.AgentRepository.GetById(listing.AgentId);

                NotFoundException.When(previousAgent is null, $"{nameof(previousAgent)} não foi encontrado.");

                int? supervisorId = null;

                if (previousAgent.SupervisorId is not null && (previousAgent.SupervisorId == newAgent.SupervisorId))
                {
                    supervisorId = (int)newAgent.SupervisorId;
                }

                NotFoundException.When(supervisorId is null, "Não foi possivel determinar o supervisor.");

                listing.SetAgent(newAgent);

                _unitOfWork.ListingRepository.Update(listing);

                var reassign = Reassign.Create(previousAgent.Id, listing.AgentId, (int)supervisorId, DateTime.UtcNow);

                reassign.SetListing(listing);

                var addedReassign = _unitOfWork.ReassignRepository.Add(reassign);

                _unitOfWork.Commit();

                return _mapper.Map<ReassignDto>(addedReassign);
            }
        }
        public ListingDto Delete(ListingDto listingDto)
        {
            Listing deletedListing;

            using (_unitOfWork) 
            {
                _unitOfWork.BeginTransaction();

                var foundedListing = _unitOfWork.ListingRepository.GetByIdWithAll(listingDto.Id);

                NotFoundException.When(foundedListing is null,$" { nameof(foundedListing) } não foi encontrado.");

                foreach (var favorite in foundedListing.Favorites)
                {
                    _unitOfWork.FavoriteRepository.Delete(favorite);
                }

                foreach (var feedBack in foundedListing.FeedBacks)
                {
                    _unitOfWork.FeedBackRepository.Delete(feedBack);
                }

                foreach(var reassign in foundedListing.Reassigns) 
                {
                    _unitOfWork.ReassignRepository.Delete(reassign);
                }

                deletedListing = _unitOfWork.ListingRepository.Delete(foundedListing);

                _unitOfWork.Commit();
            }

            return _mapper.Map<ListingDto>(deletedListing);
        }

        public ListingDto Delete(int id)
        {
            Listing deletedListing;

            using (_unitOfWork)
            {
                _unitOfWork.BeginTransaction();

                var foundedListing = _unitOfWork.ListingRepository.GetByIdWithAll(id);

                NotFoundException.When(foundedListing is null, $" {nameof(foundedListing)} não foi encontrado.");

                foreach (var favorite in foundedListing.Favorites)
                {
                    _unitOfWork.FavoriteRepository.Delete(favorite);
                }

                foreach (var feedBack in foundedListing.FeedBacks)
                {
                    _unitOfWork.FeedBackRepository.Delete(feedBack);
                }

                foreach (var reassign in foundedListing.Reassigns)
                {
                    _unitOfWork.ReassignRepository.Delete(reassign);
                }

                deletedListing = _unitOfWork.ListingRepository.Delete(foundedListing);

                _unitOfWork.Commit();
            }

            return _mapper.Map<ListingDto>(deletedListing);
        }

        public List<ListingDto> GetAll()
        {
            var list = new List<ListingDto>();

            foreach(var listing in _unitOfWork.ListingRepository.GetAll()) 
            {
                var listingDto = _mapper.Map<ListingDto>(listing);

                list.Add(listingDto);
            }

            return list;
        }

        public ListingDto GetById(int id)
        {
            var listing = _unitOfWork.ListingRepository.GetById(id);

            return _mapper.Map<ListingDto>(listing);
        }

        public ListingDto Update(ListingDto listingDto)
        {
            Listing updatedListing;

            using (_unitOfWork)
            {
                var foundedListing = _unitOfWork.ListingRepository.GetById(listingDto.Id);

                NotFoundException.When(foundedListing is null, $" {nameof(foundedListing)} não foi encontrado.");

                foundedListing.Update(listingDto.Type, listingDto.Status,
                    listingDto.NumberOfBathrooms, listingDto.NumberOfBathrooms,
                    listingDto.NumberOfKitchens,listingDto.Price, listingDto.Location,
                    listingDto.Area, listingDto.Parking, listingDto.Description,
                    listingDto.MainImageFileName, listingDto.OtherImagesFileNames);

                updatedListing = _unitOfWork.ListingRepository.Update(foundedListing);

                _unitOfWork.Commit();
            }

            return _mapper.Map<ListingDto>(updatedListing);
        }

        public Pagination<ListingDto> GetAllPagination(int pageNumber, int pageSize, string search)
        {
            var totalCount = _unitOfWork.ListingRepository.GetTotalCount(search);

            var listings = _unitOfWork.ListingRepository.GetAllPagination(pageNumber, pageSize, search);

            var pagination = Pagination<ListingDto>.Create(_mapper.Map<List<ListingDto>>(listings),
                pageNumber, pageSize, totalCount);

            return pagination;
        }

        public Pagination<ListingDto> GetListingsPaginationByAgentId(int agentId, int pageNumber, int pageSize, string search)
        {
            var totalCount = _unitOfWork.ListingRepository.GetTotalCount(agentId,search);

            var listings = _unitOfWork.ListingRepository.GetListingsPaginationByAgentId(agentId,pageNumber, pageSize, search);

            var pagination = Pagination<ListingDto>.Create(_mapper.Map<List<ListingDto>>(listings),
                pageNumber, pageSize, totalCount);

            return pagination;
        }

        public List<ListingDto> GetAllSearch(string search)
        {
            var listings = _unitOfWork.ListingRepository.GetAllSearch(search);

            return _mapper.Map<List<ListingDto>>(listings);
        }
    }
}
