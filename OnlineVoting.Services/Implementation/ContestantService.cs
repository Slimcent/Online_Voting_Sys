using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Caching.Interfaces;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Interfaces;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Caching.Keys;
using OnlineVoting.Services.Caching.Policies;
using OnlineVoting.Services.Caching.Tags;
using OnlineVoting.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using VotingSystem.Data.Extensions;
using VotingSystem.Data.Implementation;
using VotingSystem.Logger;

namespace OnlineVoting.Services.Implementation
{
    public class ContestantService : IContestantService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Contestant> _contestantRepo;
        private readonly IMapper _mapper;
        private readonly ILoggerMessage _loggerMessage;
        private readonly ICacheService _cacheService;

        public ContestantService(IServiceFactory serviceFactory)
        {
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _contestantRepo = _unitOfWork.GetRepository<Contestant>();
            _mapper = serviceFactory.GetService<IMapper>();
            _loggerMessage = serviceFactory.GetService<ILoggerMessage>();
            _cacheService = serviceFactory.GetService<ICacheService>();
        }

        public async Task<Result<PagedResponse<ContestantResponse>>> GetContestants(ContestantRequest request)
        {
            string cacheKey = ContestantCacheKeys.GetContestants(request);

            Func<CancellationToken, ValueTask<PagedResponse<ContestantResponse>>> cacheFactory = async _ =>
            {
                IQueryable<Contestant> query = _contestantRepo.GetQueryable(include: query => query.Include(x => x.PositionApplication)
                    .ThenInclude(x => x.Student).ThenInclude(x => x.User).Include(x => x.PositionApplication).ThenInclude(x => x.ElectionPosition)
                        .ThenInclude(x => x.Election).Include(x => x.PositionApplication).ThenInclude(x => x.ElectionPosition)
                            .ThenInclude(x => x.Position)).AsNoTracking();

                if (!string.IsNullOrWhiteSpace(request.ElectionId))
                    query = query.Where(x => x.PositionApplication.ElectionPosition.ElectionId == request.ElectionId);

                if (!string.IsNullOrWhiteSpace(request.PositionId))
                    query = query.Where(x => x.PositionApplication.ElectionPosition.PositionId == request.PositionId);

                if (!string.IsNullOrWhiteSpace(request.ElectionPositionId))
                    query = query.Where(x => x.PositionApplication.ElectionPositionId == request.ElectionPositionId);

                if (request.Active.HasValue)
                    query = query.Where(x => x.Active == request.Active.Value);

                if (request.Active.HasValue)
                    query = query.Where(x => x.Active == request.Active.Value);

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    string searchTerm = request.SearchTerm.Trim();

                    query = query.Where(x => x.PositionApplication.ElectionPosition.Election.Name.Contains(searchTerm)
                        || x.PositionApplication.ElectionPosition.Position.Name.Contains(searchTerm)
                        || (x.PositionApplication.Student.RegNumber != null && x.PositionApplication.Student.RegNumber.Contains(searchTerm))
                        || (x.PositionApplication.Student.User != null
                            && ((x.PositionApplication.Student.User.FirstName != null && x.PositionApplication.Student.User.FirstName.Contains(searchTerm))
                                || (x.PositionApplication.Student.User.LastName != null && x.PositionApplication.Student.User.LastName.Contains(searchTerm))
                                || (x.PositionApplication.Student.User.Email != null && x.PositionApplication.Student.User.Email.Contains(searchTerm)))));
                }

                query = query.OrderBy(x => x.PositionApplication.ElectionPosition.Position.Name)
                    .ThenBy(x => x.PositionApplication.Student.User!.FirstName);

                PagedList<Contestant> contestants = await query.GetPagedItems(request);

                PagedResponse<ContestantResponse> response = _mapper.Map<PagedResponse<ContestantResponse>>(contestants);

                _loggerMessage.LogInfo($"{contestants.MetaData.TotalCount} contestants found.");

                return response;
            };

            PagedResponse<ContestantResponse> response = await _cacheService.GetOrCreate(cacheKey, cacheFactory, CachePolicies.Contestant);

            return Result<PagedResponse<ContestantResponse>>.Success(response);
        }

        public async Task<Result<ContestantResponse>> GetContestant(string contestantId)
        {
            string normalizedContestantId = contestantId.Trim();
            string cacheKey = ContestantCacheKeys.GetContestant(normalizedContestantId);

            Func<CancellationToken, ValueTask<ContestantResponse?>> cacheFactory = async _ =>
            {
                Contestant? contestant = await _contestantRepo.GetSingleByAsync(x => x.Id == normalizedContestantId,
                    include: query => query.Include(x => x.PositionApplication).ThenInclude(x => x.Student).ThenInclude(x => x.User)
                        .Include(x => x.PositionApplication).ThenInclude(x => x.ElectionPosition).ThenInclude(x => x.Election)
                        .Include(x => x.PositionApplication).ThenInclude(x => x.ElectionPosition).ThenInclude(x => x.Position),
                    tracking: false);

                if (contestant is null)
                    return null;

                return _mapper.Map<ContestantResponse>(contestant);
            };

            ContestantResponse? response = await _cacheService.GetOrCreate(cacheKey, cacheFactory, CachePolicies.Contestant);

            if (response is null)
            {
                _loggerMessage.LogWarn($"Contestant with id {normalizedContestantId} was not found.");

                return Result<ContestantResponse>.NotFound($"Contestant with id {normalizedContestantId} was not found.");
            }

            _loggerMessage.LogInfo($"Contestant with id {normalizedContestantId} found.");

            return Result<ContestantResponse>.Success(response);
        }

        public async Task<Result<string>> ToggleContestantActivation(string contestantId)
        {
            string normalizedContestantId = contestantId.Trim();

            Contestant? contestant = await _contestantRepo.GetSingleByAsync(x => x.Id == normalizedContestantId);
            if (contestant is null)
            {
                _loggerMessage.LogWarn($"Contestant with id {normalizedContestantId} was not found.");
                return Result<string>.NotFound($"Contestant with id {normalizedContestantId} was not found.");
            }

            contestant.Active = !contestant.Active;
            contestant.UpdatedAt = DateTime.UtcNow;

            await _contestantRepo.UpdateAsync(contestant);

            await _cacheService.RemoveByTag(CacheTags.Contestant);

            string status = contestant.Active ? "activated" : "deactivated";

            _loggerMessage.LogInfo($"Contestant with id {normalizedContestantId} was {status} successfully.");

            return Result<string>.Success($"Contestant {status} successfully.");
        }
    }
}