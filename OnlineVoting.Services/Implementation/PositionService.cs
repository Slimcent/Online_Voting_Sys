using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
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

        public async Task<Result<string>> CreatePosition(CreateWithNameRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<string>.ValidationError("Position name cannot be empty");

            Position positionExists = await _positionRepo.GetSingleByAsync(x => x.Name == request.Name);
            if (positionExists != null)
                return Result<string>.Conflict("Position already exists");

            Position addPosition = _mapper.Map<Position>(request);

            await _positionRepo.AddAsync(addPosition);

            return Result<string>.Created($"Position with name {addPosition.Name} created successfully");
        }

        public async Task<Result<string>> DeletePosition(Guid id)
        {
            Position positionExists = await _positionRepo.GetByIdAsync(id);
            if (positionExists == null)
                return Result<string>.NotFound("Position does not exist");

            positionExists.Active = !positionExists.Active;

            _positionRepo.Update(positionExists);

            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success("Toggle successful");
        }

        public async Task<Result<IEnumerable<PositionResponse>>> GetAllPositions()
        {
            IEnumerable<Position> allPositions = await _positionRepo.GetAllAsync();

            if (!allPositions.Any())
            {
                return Result<IEnumerable<PositionResponse>>.Success(new List<PositionResponse>());
            }

            IEnumerable<PositionResponse> positions = _mapper.Map<IEnumerable<PositionResponse>>(allPositions);

            return Result<IEnumerable<PositionResponse>>.Success(positions);
        }

        public async Task<Result<IEnumerable<PositionResponse>>> GetAllDeletedPositions()
        {
            IEnumerable<Position> allDeletedPositions = await _positionRepo.GetByAsync(x => x.Active == true);

            if (!allDeletedPositions.Any())
            {
                return Result<IEnumerable<PositionResponse>>.Success(new List<PositionResponse>());
            }

            IEnumerable<PositionResponse> positions = _mapper.Map<IEnumerable<PositionResponse>>(allDeletedPositions);

            return Result<IEnumerable<PositionResponse>>.Success(positions);
        }

        public async Task<Result<IEnumerable<PositionResponse>>> GetAllActivePositions()
        {
            IEnumerable<Position> allActivePositions = await _positionRepo.GetByAsync(x => x.Active == false);

            if (!allActivePositions.Any())
            {
                return Result<IEnumerable<PositionResponse>>.Success(new List<PositionResponse>());
            }

            IEnumerable<PositionResponse> positions = _mapper.Map<IEnumerable<PositionResponse>>(allActivePositions);

            return Result<IEnumerable<PositionResponse>>.Success(positions);
        }

        public async Task<Result<PositionResponse>> GetAPosition(Guid positionId)
        {
            Position positionExists = await _positionRepo.GetByIdAsync(positionId);
            if (positionExists == null)
                return Result<PositionResponse>.NotFound("Position does not exist");

            PositionResponse position = _mapper.Map<PositionResponse>(positionExists);

            return Result<PositionResponse>.Success(position);
        }

        public async Task<Result<string>> PatchPosition(Guid positionId, JsonPatchDocument<CreateWithNameRequest> request)
        {
            Position positionExists = await _positionRepo.GetByIdAsync(positionId);
            if (positionExists == null)
                return Result<string>.NotFound("Position does not exist");

            CreateWithNameRequest updatePosition = _mapper.Map<CreateWithNameRequest>(positionExists);

            request.ApplyTo(updatePosition);

            if (string.IsNullOrWhiteSpace(updatePosition.Name))
                return Result<string>.ValidationError("Position name cannot be empty");

            _mapper.Map(updatePosition, positionExists);

            _positionRepo.Update(positionExists);

            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success("Position updated successfully");
        }

        public async Task<Result<PagedResponse<PositionResponse>>> AllPositions(PositionRequest request)
        {
            PagedList<Position> position = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _positionRepo.GetPagedItems(request)
                : await _positionRepo.GetPagedItems(request, x => x.Name.Contains(request.SearchTerm.ToLower().Trim()));

            PagedResponse<PositionResponse> response = _mapper.Map<PagedResponse<PositionResponse>>(position);

            return Result<PagedResponse<PositionResponse>>.Success(response);
        }

        public async Task<Result<PagedResponse<PositionResponse>>> AllActivePositions(PositionRequest request)
        {
            PagedList<Position> position = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _positionRepo.GetPagedItems(request, x => x.Active == false)
                : await _positionRepo.GetPagedItems(request, x => x.Name.Contains(request.SearchTerm.ToLower().Trim()));

            PagedResponse<PositionResponse> response = _mapper.Map<PagedResponse<PositionResponse>>(position);

            return Result<PagedResponse<PositionResponse>>.Success(response);
        }

        public async Task<Result<PagedResponse<PositionResponse>>> AllDeletedPositions(PositionRequest request)
        {
            PagedList<Position> position = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _positionRepo.GetPagedItems(request, x => x.Active == true)
                : await _positionRepo.GetPagedItems(request, x => x.Name.Contains(request.SearchTerm.ToLower().Trim()));

            PagedResponse<PositionResponse> response = _mapper.Map<PagedResponse<PositionResponse>>(position);

            return Result<PagedResponse<PositionResponse>>.Success(response);
        }

        public async Task<Result<string>> UpdatePosition(Guid positionId, CreateWithNameRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<string>.ValidationError("Position name cannot be empty");

            Position positionExists = await _positionRepo.GetSingleByAsync(x => x.Id == positionId);
            if (positionExists == null)
                return Result<string>.NotFound("Position does not exist");

            Position updatePosition = _mapper.Map(request, positionExists);

            _positionRepo.Update(updatePosition);

            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success("Update successful");
        }
    }
}