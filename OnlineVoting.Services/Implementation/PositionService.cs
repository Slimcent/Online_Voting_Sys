using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Services.Interfaces;
using SchMgr_FUTO.Data.Interfaces;
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

        public async Task<string> CreatePosition(CreateWithNameRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new InvalidOperationException("Name cannot be empty");

            Position positionExists = await _positionRepo.GetSingleByAsync(x => x.Name == request.Name);
            if (positionExists != null)
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

            positionExists.Active = !positionExists.Active;

            _positionRepo.Update(positionExists);

            await _unitOfWork.SaveChangesAsync();

            return "Toggle successful";
        }

        public async Task<IEnumerable<PositionResponse>> GetAllPositions()
        {
            IEnumerable<Position> allPositions = await _positionRepo.GetAllAsync();

            if (!allPositions.Any())
            {
                return new List<PositionResponse>();
            }

            return _mapper.Map<IEnumerable<PositionResponse>>(allPositions);
        }

        public async Task<IEnumerable<PositionResponse>> GetAllDeletedPositions()
        {
            IEnumerable<Position> allDeletedPositions = await _positionRepo.GetByAsync(x => x.Active == true);

            if (!allDeletedPositions.Any())
            {
                return new List<PositionResponse>();
            }

            return _mapper.Map<IEnumerable<PositionResponse>>(allDeletedPositions);
        }

        public async Task<IEnumerable<PositionResponse>> GetAllActivePositions()
        {
            IEnumerable<Position> allActivePositions = await _positionRepo.GetByAsync(x => x.Active == false);

            if (!allActivePositions.Any())
            {
                return new List<PositionResponse>();
            }

            return _mapper.Map<IEnumerable<PositionResponse>>(allActivePositions);
        }

        public async Task<PositionResponse> GetAPosition(Guid positionId)
        {
            Position positionExists = await _positionRepo.GetByIdAsync(positionId);
            if (positionExists == null)
                throw new InvalidOperationException("Position does not exists");

            return _mapper.Map<PositionResponse>(positionExists);
        }

        public async Task<string> PatchPosition(Guid positionId, JsonPatchDocument<CreateWithNameRequest> request)
        {
            Position positionExists = await _positionRepo.GetByIdAsync(positionId);
            if (positionExists == null)
                throw new InvalidOperationException("Position does not exists");

            CreateWithNameRequest updatePosition = _mapper.Map<CreateWithNameRequest>(positionExists);

            request.ApplyTo(updatePosition);

            _mapper.Map(updatePosition, positionExists);

            _positionRepo.Update(positionExists);

            await _unitOfWork.SaveChangesAsync();

            return $"Position updated successfully";
        }

        public async Task<PagedResponse<PositionResponse>> AllPositions(PositionRequest request)
        {
            PagedList<Position> position = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _positionRepo.GetPagedItems(request)
                : await _positionRepo.GetPagedItems(request, x => x.Name.Contains(request.SearchTerm.ToLower().Trim()));

            return _mapper.Map<PagedResponse<PositionResponse>>(position);
        }

        public async Task<PagedResponse<PositionResponse>> AllActivePositions(PositionRequest request)
        {
            PagedList<Position> position = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _positionRepo.GetPagedItems(request, x => x.Active == false)
                : await _positionRepo.GetPagedItems(request, x => x.Name.Contains(request.SearchTerm.ToLower().Trim()));

            return _mapper.Map<PagedResponse<PositionResponse>>(position);
        }

        public async Task<PagedResponse<PositionResponse>> AllDeletedPositions(PositionRequest request)
        {
            PagedList<Position> position = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _positionRepo.GetPagedItems(request, x => x.Active == true)
                : await _positionRepo.GetPagedItems(request, x => x.Name.Contains(request.SearchTerm.ToLower().Trim()));

            return _mapper.Map<PagedResponse<PositionResponse>>(position);
        }

        public async Task<string> UpdatePosition(Guid positionId, CreateWithNameRequest request)
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