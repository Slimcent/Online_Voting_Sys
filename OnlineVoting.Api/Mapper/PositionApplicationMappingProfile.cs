using AutoMapper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Api.Mapper
{
    public class PositionApplicationMappingProfile : Profile
    {
        public PositionApplicationMappingProfile()
        {
            CreateMap<CreatePositionApplicationRequest, PositionApplication>()
                .ForMember(destination => destination.Id, option => option.Ignore())
                .ForMember(destination => destination.StudentId, option => option.Ignore())
                .ForMember(destination => destination.PositionApplicationStatusId, option => option.Ignore())
                .ForMember(destination => destination.ElectionPositionId, option => option.MapFrom(source => source.ElectionPositionId.Trim()));

            CreateMap<PositionApplication, CreatePositionApplicationResponse>()
                .ForMember(destination => destination.PositionApplicationId, option => option.MapFrom(source => source.Id))
                .ForMember(destination => destination.ApplicationStatus, option => option.MapFrom(source => source.PositionApplicationStatus.Name))
                .ForMember(destination => destination.InvoiceId, option => option.Ignore())
                .ForMember(destination => destination.InvoiceNumber, option => option.Ignore())
                .ForMember(destination => destination.Amount, option => option.Ignore())
                .ForMember(destination => destination.Currency, option => option.Ignore())
                .ForMember(destination => destination.InvoiceStatus, option => option.Ignore());

            CreateMap<PositionApplication, PositionApplicationResponse>()
                .ForMember(destination => destination.PositionApplicationId, option => option.MapFrom(source => source.Id))
                .ForMember(destination => destination.ElectionPositionId, option => option.MapFrom(source => source.ElectionPositionId))
                .ForMember(destination => destination.StudentId, option => option.MapFrom(source => source.StudentId))
                .ForMember(destination => destination.RegistrationNumber, option => option.MapFrom(source => source.Student.RegNumber))
                .ForMember(destination => destination.StudentName, option => option.MapFrom(source =>
                    source.Student.User != null ? $"{source.Student.User.FirstName} {source.Student.User.LastName}".Trim() : string.Empty))
                .ForMember(destination => destination.StudentEmail, option => option.MapFrom(source =>
                    source.Student.User != null ? source.Student.User.Email : null))
                .ForMember(destination => destination.ElectionName, option => option.MapFrom(source => source.ElectionPosition.Election.Name))
                .ForMember(destination => destination.PositionName, option => option.MapFrom(source => source.ElectionPosition.Position.Name))
                .ForMember(destination => destination.ApplicationStatus, option => option.MapFrom(source => source.PositionApplicationStatus.Name))
                .ForMember(destination => destination.InvoiceId, option => option.MapFrom(source => source.Invoice != null ? source.Invoice.Id : null))
                .ForMember(destination => destination.InvoiceNumber, option => option.MapFrom(source => source.Invoice != null ? source.Invoice.InvoiceNumber : null))
                .ForMember(destination => destination.Amount, option => option.MapFrom(source => source.Invoice != null ? source.Invoice.Amount : (decimal?)null))
                .ForMember(destination => destination.Currency, option => option.MapFrom(source => source.Invoice != null ? source.Invoice.Currency : null))
                .ForMember(destination => destination.InvoiceStatus, option => option.MapFrom(source => source.Invoice != null ? source.Invoice.InvoiceStatus.Name : null))
                .ForMember(destination => destination.Contestant, option => option.MapFrom(source => source.Contestant));

            CreateMap<Contestant, ContestantResponse>()
                .ForMember(destination => destination.ContestantId, option => option.MapFrom(source => source.Id))
                .ForMember(destination => destination.PositionApplicationId, option => option.MapFrom(source => source.PositionApplicationId));
        }
    }
}
