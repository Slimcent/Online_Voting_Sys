using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Extensions;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;
using OnlineVoting.Data.Interfaces;

namespace OnlineVoting.Services.Implementation
{
    public class StaffService : IStaffService
    {
        private readonly IRepository<Staff> _staffRepo;
        private readonly IRepository<Address> _addressRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IMapper _mapper;
        private readonly IServiceFactory _serviceFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string userId;
        private readonly IHttpContextAccessor _contextAccessor;

        public StaffService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _unitOfWork = _serviceFactory.GetService<IUnitOfWork>();
            _staffRepo = _unitOfWork.GetRepository<Staff>();
            _addressRepo = _unitOfWork.GetRepository<Address>();
            _userRepo = _unitOfWork.GetRepository<User>();
            _mapper = _serviceFactory.GetService<IMapper>();
            _contextAccessor = _serviceFactory.GetService<IHttpContextAccessor>();
            userId = _contextAccessor.HttpContext.User.GetUserId();
        }

        public async Task<Result<string>> CreateStaff(CreateStaffRequest request)
        {
            //CreateUserRequest user = new()
            //{
            //   
            //    FirstName = request.FirstName,
            //    Role = request.Role,
            //};
            CreateUserRequest user = _mapper.Map<CreateUserRequest>(request);

            Result<string> userIdResult = await _serviceFactory.GetService<IUserService>().CreateUser(user);
            if (!userIdResult.IsSuccess)
                return Result<string>.FromFailure(userIdResult);

            Staff staff = new()
            {
                UserId = userIdResult.Value!,
                PhoneNumber = request.PhoneNumber,
                LastName = request.LastName,
                FirstName = request.FirstName,
                GenderId = request.GenderId
            };

            await _staffRepo.AddAsync(staff);

            await CreateStaffAddress(staff);

            return Result<string>.Created($"Staff with email {request.Email} was created successfully");
        }

        private async Task CreateStaffAddress(Staff staff)
        {
            Address address = new() { StaffId = staff.Id };
            await _addressRepo.AddAsync(address);
        }

        //public async Task<IEnumerable<StaffResponseDto>> GetAllStaff()
        //{
        //    IEnumerable<Staff> allStaff = await _staffRepo.GetAllAndInclude(x => x.Address, x => x.User);

        //    return _mapper.Map<IEnumerable<StaffResponseDto>>(allStaff);
        //}

        public async Task<Result<string>> UpdateStaffAddress(Guid staffId, UpdateAddressRequest model)
        {
            Address staffAddress = await _addressRepo.GetSingleByAsync(x => x.StaffId == staffId);
            if (staffAddress == null)
                return Result<string>.NotFound($"Staff with id {staffId} does not exist");

            Address update = _mapper.Map(model, staffAddress);
            await _addressRepo.UpdateAsync(update);
            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success("Address updated successfully");
        }

        public async Task<Result<string>> UpdateStaff(Guid id, JsonPatchDocument<UpdateStaffRequest> request)
        {
            Staff staff = await _staffRepo.GetSingleByAsync(s => s.Id == id,
                include: s => s.Include(u => u.User));

            if (staff == null)
                return Result<string>.NotFound($"Staff with id {id} does not exist");

            UpdateStaffRequest updateStaff = new()
            {
                LastName = staff.LastName,
                FirstName = staff.FirstName,
                Email = staff.User.Email,
                PhoneNumber = staff.PhoneNumber
            };

            request.ApplyTo(updateStaff);

            _mapper.Map(updateStaff, staff);

            _staffRepo.Update(staff);

            staff.User.NormalizedEmail = staff.User.Email.ToUpper();

            _userRepo.Update(staff.User);

            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success($"Staff with email {staff.User.Email} updated successfully");
        }

        public async Task<Result<StaffResponse>> GetStaff(Guid id)
        {
            Staff staff = await _staffRepo.GetSingleByAsync(x => x.Id == id, include: x => x.Include(x => x.Address).Include(x => x.User));

            if (staff == null)
                return Result<StaffResponse>.NotFound("Staff not found");

            StaffResponse response = _mapper.Map<StaffResponse>(staff);

            return Result<StaffResponse>.Success(response);
        }

        public Result<int> GetTotalNumberOfStaff()
        {
            int staffCount = _staffRepo.GetAll().Count();

            return Result<int>.Success(staffCount);
        }

        public async Task<Result<string>> DeleteStaffById(Guid id)
        {
            Staff staff = await _staffRepo.GetByIdAsync(id);

            if (staff == null)
                return Result<string>.NotFound($"Staff with id {id} does not exist");

            await _staffRepo.DeleteAsync(staff);

            return Result<string>.Success("Staff deleted successfully");
        }

        public async Task<Result<StaffResponse>> GetStaffByEmail(string email)
        {
            User user = await _userRepo.GetSingleByAsync(u => u.Email == email,
                include: u => u.Include(s => s.Staff).ThenInclude(a => a.Address));

            if (user == null)
                return Result<StaffResponse>.NotFound("User not found");

            StaffResponse response = _mapper.Map<StaffResponse>(user);

            return Result<StaffResponse>.Success(response);
        }

        public async Task<Result<string>> PatchStaffAddress(Guid staffId, JsonPatchDocument<UpdateAddressRequest> request)
        {
            Address staffAddress = await _addressRepo.GetSingleByAsync(x => x.StaffId == staffId);

            if (staffAddress == null)
                return Result<string>.NotFound($"Staff with id {staffId} does not exist");

            UpdateAddressRequest updateAddress = new()
            {
                PlotNo = staffAddress.PlotNo ?? 0,
                StreetName = staffAddress.StreetName,
                City = staffAddress.City,
                State = staffAddress.State,
                Nationality = staffAddress.Nationality,
            };

            request.ApplyTo(updateAddress);

            _mapper.Map(updateAddress, staffAddress);

            await _addressRepo.UpdateAsync(staffAddress);

            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success("Staff updated successfully");
        }

        public async Task<Result<string>> EditStaff(Guid staffId, UpdateStaffRequest request)
        {
            Staff staffExists = await _staffRepo.GetSingleByAsync(x => x.Id == staffId);
            if (staffExists == null)
                return Result<string>.NotFound("Staff does not exist");

            Staff updateStaff = _mapper.Map(request, staffExists);

            _staffRepo.Update(updateStaff);

            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success("Update successful");
        }

        public async Task<Result<IEnumerable<StaffResponse>>> GetAllDeletedStaff()
        {
            IEnumerable<Staff> allDeletedStaff = await _staffRepo.GetByAsync(x => x.Active == true);

            if (!allDeletedStaff.Any())
            {
                return Result<IEnumerable<StaffResponse>>.Success(new List<StaffResponse>());
            }

            IEnumerable<StaffResponse> response = _mapper.Map<IEnumerable<StaffResponse>>(allDeletedStaff);

            return Result<IEnumerable<StaffResponse>>.Success(response);
        }

        public async Task<Result<IEnumerable<StaffResponse>>> GetAllActiveStaff()
        {
            IEnumerable<Staff> allActiveStaff = await _staffRepo.GetByAsync(x => x.Active == false);

            if (!allActiveStaff.Any())
            {
                return Result<IEnumerable<StaffResponse>>.Success(new List<StaffResponse>());
            }

            IEnumerable<StaffResponse> response = _mapper.Map<IEnumerable<StaffResponse>>(allActiveStaff);

            return Result<IEnumerable<StaffResponse>>.Success(response);
        }

        public async Task<Result<PagedResponse<StaffResponse>>> AllStaff(StaffRequest request)
        {
            PagedList<Staff> staff = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _staffRepo.GetPagedItems(request)
                : await _staffRepo.GetPagedItems(request, x => x.FirstName.Contains(request.SearchTerm.ToLower().Trim())
                            || x.LastName.Contains(request.SearchTerm.ToLower().Trim()));

            PagedResponse<StaffResponse> response = _mapper.Map<PagedResponse<StaffResponse>>(staff);

            return Result<PagedResponse<StaffResponse>>.Success(response);
        }

        public async Task<Result<PagedResponse<StaffResponse>>> AllActiveStaff(StaffRequest request)
        {
            PagedList<Staff> staff = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _staffRepo.GetPagedItems(request, x => x.Active == false)
                : await _staffRepo.GetPagedItems(request, x => (x.FirstName.Contains(request.SearchTerm.ToLower().Trim())
                            || x.LastName.Contains(request.SearchTerm.ToLower().Trim())) && x.Active == false);

            PagedResponse<StaffResponse> response = _mapper.Map<PagedResponse<StaffResponse>>(staff);

            return Result<PagedResponse<StaffResponse>>.Success(response);
        }

        public async Task<Result<PagedResponse<StaffResponse>>> AllDeletedStaff(StaffRequest request)
        {
            PagedList<Staff> staff = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _staffRepo.GetPagedItems(request, x => x.Active == true)
                : await _staffRepo.GetPagedItems(request, x => (x.FirstName.Contains(request.SearchTerm.ToLower().Trim())
                            || x.LastName.Contains(request.SearchTerm.ToLower().Trim())) && x.Active == true);

            PagedResponse<StaffResponse> response = _mapper.Map<PagedResponse<StaffResponse>>(staff);

            return Result<PagedResponse<StaffResponse>>.Success(response);
        }

        public async Task<Result<string>> ToggleStaffStatus(Guid id)
        {
            Staff staff = await _staffRepo.GetByIdAsync(id);

            if (staff == null)
                return Result<string>.NotFound($"Staff with id {id} does not exist");

            staff.Active = !staff.Active;

            await _staffRepo.UpdateAsync(staff);

            if (staff.Active == false)
            {
                return Result<string>.Success("Staff activated successfully");
            }
            else
            {
                return Result<string>.Success("Staff deleted successfully");
            }
        }
    }
}