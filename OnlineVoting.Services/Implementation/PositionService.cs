using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VotingSystem.Data.Interfaces;

namespace OnlineVoting.Services.Implementation
{
    public class PositionService : IPositionService
    {
        private readonly IRepository<Position> _positionRepo;
        private readonly IMapper _mapper;
        private readonly IServiceFactory _serviceFactory;
        private readonly IUnitOfWork _unitOfWork;

        public PositionService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _positionRepo = _unitOfWork.GetRepository<Position>();
            _mapper = _serviceFactory.GetService<IMapper>();
        }

        public async Task<string> CreatePosition(PositionDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new InvalidOperationException("Name cannot be empty");

            Position positionExists = await _positionRepo.GetSingleByAsync(x => x.Name == request.Name);
            if(positionExists != null)
                throw new InvalidOperationException("Position already exists");

            Position addPosition = _mapper.Map<Position>(request);

            await _positionRepo.AddAsync(addPosition);

            return $"Position with name {addPosition.Name} created successfully";
        }

        public async Task<string> DeletePosition(Guid id)
        {
            Position positionExists = await _positionRepo.GetByIdAsync(id);
            if (positionExists == null)
                throw new InvalidOperationException("Position does not exists");

            positionExists.IsDeleted = !positionExists.IsDeleted;

            _positionRepo.Update(positionExists);

            await _unitOfWork.SaveChangesAsync();

            return "Toggle successful";
        }

        public async Task<IEnumerable<PositionResponseDto>> GetAllPositions()
        {
            IEnumerable<Position> allPositions = await _positionRepo.GetAllAsync();

            if (!allPositions.Any())
            {
               return new List<PositionResponseDto>();
            }

            return _mapper.Map<IEnumerable<PositionResponseDto>>(allPositions);

        }

        public async Task<IEnumerable<PositionResponseDto>> GetAllDeletedPositions()
        {
            IEnumerable<Position> allDeletedPositions = await _positionRepo.GetByAsync(x => x.IsDeleted == true);

            if (!allDeletedPositions.Any())
            {
                return new List<PositionResponseDto>();
            }

            return _mapper.Map<IEnumerable<PositionResponseDto>>(allDeletedPositions);
        }

        public async Task<IEnumerable<PositionResponseDto>> GetAllActivePositions()
        {
            IEnumerable<Position> allActivePositions = await _positionRepo.GetByAsync(x => x.IsDeleted == false);

            if (!allActivePositions.Any())
            {
                return new List<PositionResponseDto>();
            }

            return _mapper.Map<IEnumerable<PositionResponseDto>>(allActivePositions);
        }

        public async Task<PositionResponseDto> GetAPosition(Guid positionId)
        {
            Position positionExists = await _positionRepo.GetByIdAsync(positionId);
            if (positionExists == null)
                throw new InvalidOperationException("Position does not exists");

            return _mapper.Map<PositionResponseDto>(positionExists);
        }

        public async Task<string> PatchPosition(Guid positionId, JsonPatchDocument<PositionDto> request)
        {
            Position positionExists = await _positionRepo.GetByIdAsync(positionId);
            if (positionExists == null)
                throw new InvalidOperationException("Position does not exists");

            PositionDto updatePosition = _mapper.Map<PositionDto>(positionExists);

            request.ApplyTo(updatePosition);

            _mapper.Map(updatePosition, positionExists);

            _positionRepo.Update(positionExists);

            await _unitOfWork.SaveChangesAsync();

            return $"Position updated successfully";
        }

        public async Task<PagedResponse<PositionResponseDto>> AllPositions(PositionRequestDto request)
        {
            PagedList<Position> position = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _positionRepo.GetPagedItems(request)
                : await _positionRepo.GetPagedItems(request, x => x.Name.Contains(request.SearchTerm.ToLower().Trim()));

            return _mapper.Map<PagedResponse<PositionResponseDto>>(position);
        }

        public async Task<PagedResponse<PositionResponseDto>> AllActivePositions(PositionRequestDto request)
        {
            PagedList<Position> position = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _positionRepo.GetPagedItems(request, x => x.IsDeleted == false)
                : await _positionRepo.GetPagedItems(request, x => x.Name.Contains(request.SearchTerm.ToLower().Trim()));

            return _mapper.Map<PagedResponse<PositionResponseDto>>(position);
        }

        public async Task<PagedResponse<PositionResponseDto>> AllDeletedPositions(PositionRequestDto request)
        {
            PagedList<Position> position = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _positionRepo.GetPagedItems(request, x => x.IsDeleted == true)
                : await _positionRepo.GetPagedItems(request, x => x.Name.Contains(request.SearchTerm.ToLower().Trim()));

            return _mapper.Map<PagedResponse<PositionResponseDto>>(position);
        }

        public async Task<string> UpdatePosition(Guid positionId, PositionDto request)
        {
            Position positionExists = await _positionRepo.GetSingleByAsync(x => x.Id == positionId);
            if (positionExists == null)
                throw new InvalidOperationException("Position does not exists");

           Position updatePosition = _mapper.Map(request, positionExists);

            _positionRepo.Update(updatePosition);

            await _unitOfWork.SaveChangesAsync();

            return "Update successful";
        }
    }
}
