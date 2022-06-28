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
            CreateMap<UserCreateRequestDto, User>()
                //.ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

            CreateMap<StudentCreateRequestDto, Student>();

            //Positions
            CreateMap<PositionDto, Position>();
            CreateMap<Position, PositionDto>();
            CreateMap<Position, PositionResponseDto>();

            //Faculty
            CreateMap<CreateFacultyDto, Faculty>();

            //Department
            CreateMap<DeptCreateDto, Department>();

            // Role
            CreateMap<RoleDto, Role>();
            CreateMap<Role, RoleResponseDto>();

            // Update Staff by Patch
            CreateMap<UpdateStaffDto, Staff>()
                .ForPath(dest => dest.User.Email, opt => opt.MapFrom(src => src.Email));

            // Update Staff by Put
            CreateMap<UpdateAddressDto, Address>();

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
        }
    }
}
