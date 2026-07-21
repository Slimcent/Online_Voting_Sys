using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Services.Infrastructures.Extensions;
using OnlineVoting.Services.Interfaces;
using SchMgr_FUTO.Data.Interfaces;
using VotingSystem.Data.Interfaces;

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


        public async Task<string> CreateStaff(CreateStaffRequest request)
        {
            //CreateUserRequest user = new()
            //{
            //    Email = request.Email,
            //    FirstName = request.FirstName,
            //    Role = request.Role,
            //};
            CreateUserRequest user = _mapper.Map<CreateUserRequest>(request);

            string userId = await _serviceFactory.GetService<IUserService>().CreateUser(user);

            Staff staff = new()
            {
                UserId = userId,
                PhoneNumber = request.PhoneNumber,
                LastName = request.LastName,
                FirstName = request.FirstName,
                GenderId = request.GenderId
            };
            await _staffRepo.AddAsync(staff);

            await CreateStaffAddress(staff);

            return $"Staff with email {request.Email} was created successfully";
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

        public async Task<string> UpdateStaffAddress(Guid staffId, UpdateAddressRequest model)
        {
            Address staffAddress = await _addressRepo.GetSingleByAsync(x => x.StaffId == staffId);
            if (staffAddress == null)
                return $"staff with id {staffId} does not exist";

            Address update = _mapper.Map(model, staffAddress);
            await _addressRepo.UpdateAsync(update);
            await _unitOfWork.SaveChangesAsync();

            return "Address updated successfully";
        }

        public async Task<string> UpdateStaff(Guid id, JsonPatchDocument<UpdateStaffRequest> request)
        {
            Staff staff = await _staffRepo.GetSingleByAsync(s => s.Id == id,
                include: s => s.Include(u => u.User));

            if (staff == null)
                return $"staff with id {id} does not exist";

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

            return $"staff with email {staff.User.Email} updated successfully";
        }

        public async Task<StaffResponseDto> GetStaff(Guid id)
        {
            Staff staff = await _staffRepo.GetSingleByAsync(x => x.Id == id, include: x => x.Include(x => x.Address).Include(x => x.User));

            if (staff == null)
                throw new InvalidOperationException("Staff not found");

            return _mapper.Map<StaffResponseDto>(staff);
        }

        public IEnumerable<Staff> GetTotalNumberOfStaff()
        {
            return _staffRepo.GetAll();
        }

        public async Task<string> DeleteStaffById(Guid id)
        {
            Staff staff = await _staffRepo.GetByIdAsync(id);

            if (staff == null)
                return $"Staff with id {id} does not exist";

            await _staffRepo.DeleteAsync(staff);

            return $"Staff deleted successfully";
        }

        public async Task<StaffResponseDto> GetStaffByEmail(string email)
        {
            User user = await _userRepo.GetSingleByAsync(u => u.Email == email,
                include: u => u.Include(s => s.Staff).ThenInclude(a => a.Address));

            if (user == null)
                throw new InvalidOperationException("User not found");

            return _mapper.Map<StaffResponseDto>(user);
        }


        public async Task<string> PatchStaffAddress(Guid staffId, JsonPatchDocument<UpdateAddressRequest> request)
        {
            Address staffAddress = await _addressRepo.GetSingleByAsync(x => x.StaffId == staffId);

            if (staffAddress == null)
                return $"staff with id {staffId} does not exist";

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

            return $"staff updated successfully";
        }

        public async Task<string> EditStaff(Guid staffId, UpdateStaffRequest request)
        {
            Staff staffExists = await _staffRepo.GetSingleByAsync(x => x.Id == staffId);
            if (staffExists == null)
                throw new InvalidOperationException("Staff does not exists");

            Staff updateStaff = _mapper.Map(request, staffExists);

            _staffRepo.Update(updateStaff);

            await _unitOfWork.SaveChangesAsync();

            return "Update successful";
        }

        public async Task<IEnumerable<StaffResponseDto>> GetAllDeletedStaff()
        {
            IEnumerable<Staff> allDeletedStaff = await _staffRepo.GetByAsync(x => x.Active == true);

            if (!allDeletedStaff.Any())
            {
                return new List<StaffResponseDto>();
            }

            return _mapper.Map<IEnumerable<StaffResponseDto>>(allDeletedStaff);
        }

        public async Task<IEnumerable<StaffResponseDto>> GetAllActiveStaff()
        {
            IEnumerable<Staff> allActiveStaff = await _staffRepo.GetByAsync(x => x.Active == false);

            if (!allActiveStaff.Any())
            {
                return new List<StaffResponseDto>();
            }

            return _mapper.Map<IEnumerable<StaffResponseDto>>(allActiveStaff);
        }

        public async Task<PagedResponse<StaffResponseDto>> AllStaff(StaffRequestDto request)
        {
            PagedList<Staff> staff = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _staffRepo.GetPagedItems(request)
                : await _staffRepo.GetPagedItems(request, x => x.FirstName.Contains(request.SearchTerm.ToLower().Trim())
                            || x.LastName.Contains(request.SearchTerm.ToLower().Trim()));

            return _mapper.Map<PagedResponse<StaffResponseDto>>(staff);
        }

        public async Task<PagedResponse<StaffResponseDto>> AllActiveStaff(StaffRequestDto request)
        {
            PagedList<Staff> staff = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _staffRepo.GetPagedItems(request, x => x.Active == false)
                : await _staffRepo.GetPagedItems(request, x => x.FirstName.Contains(request.SearchTerm.ToLower().Trim())
                            || x.LastName.Contains(request.SearchTerm.ToLower().Trim()));

            return _mapper.Map<PagedResponse<StaffResponseDto>>(staff);
        }

        public async Task<PagedResponse<StaffResponseDto>> AllDeletedStaff(StaffRequestDto request)
        {
            PagedList<Staff> staff = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _staffRepo.GetPagedItems(request, x => x.Active == true)
                : await _staffRepo.GetPagedItems(request, x => x.FirstName.Contains(request.SearchTerm.ToLower().Trim())
                            || x.LastName.Contains(request.SearchTerm.ToLower().Trim()));

            return _mapper.Map<PagedResponse<StaffResponseDto>>(staff);
        }

        public async Task<string> ToggleStaffStatus(Guid id)
        {
            Staff staff = await _staffRepo.GetByIdAsync(id);

            if (staff == null)
                return $"Staff with id {id} does not exist";

            staff.Active = !staff.Active;

            await _staffRepo.UpdateAsync(staff);

            if (staff.Active == false)
            {
                return $"Staff activated successfully";
            }
            else
            {
                return $"Staff deleted successfully";
            }
        }
    }
}