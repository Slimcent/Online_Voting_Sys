using AutoMapper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Api.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateUserRequest, User>()
                //.ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

            CreateMap<CreateStudentRequest, Student>();

            //Positions
            CreateMap<CreateWithNameRequest, Position>();
            CreateMap<Position, CreateWithNameRequest>();
            CreateMap<Position, PositionResponseDto>();

            //Faculty
            CreateMap<CreateWithNameRequest, Faculty>();

            //Department
            CreateMap<CreateDepartmentRequest, Department>();

            // Role
            CreateMap<CreateRoleRequest, Role>();
            CreateMap<Role, RoleResponseDto>();

            // Create staff
            CreateMap<CreateStaffRequest, CreateUserRequest>();

            // Update Staff by Patch
            CreateMap<UpdateStaffRequest, Staff>()
                .ForPath(dest => dest.User.Email, opt => opt.MapFrom(src => src.Email));

            // Update Staff by Put
            CreateMap<UpdateAddressRequest, Address>();

            // Get all staff and Get staff by Id 
            CreateMap<Staff, StaffResponseDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.LastName} {src.FirstName}"))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src =>
                $"{src.Address.PlotNo} {src.Address.StreetName} {src.Address.State} {src.Address.Nationality}"));

            // Get Staff by Email
            CreateMap<User, StaffResponseDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.Staff.LastName} {src.Staff.FirstName}"))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Staff.PhoneNumber))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src =>
                $"{src.Staff.Address.PlotNo} {src.Staff.Address.StreetName} {src.Staff.Address.State} {src.Staff.Address.Nationality}"));

            CreateMap<CreateStudentRequest, CreateUserRequest>();

            CreateMap<CreateStudentRequest, CreateUserRequest>();
        }
    }
}