using AutoMapper;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using System.Text.Json;

namespace OnlineVoting.Api.Mapper
{
    public class AuditMappingProfile : Profile
    {
        public AuditMappingProfile()
        {
            CreateMap<AuditLocation, AuditLocationResponse>();

            CreateMap<AuditTrail, AuditTrailResponse>()
                .ForMember(dest => dest.Outcome, opt => opt.MapFrom(src => src.Outcome.Name))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
                .ForMember(dest => dest.OldValues, opt =>
                {
                    opt.MapFrom(src => DeserializeAuditValues(src.OldValues));
                    opt.AllowNull();
                })
                .ForMember(dest => dest.NewValues, opt =>
                {
                    opt.MapFrom(src => DeserializeAuditValues(src.NewValues));
                    opt.AllowNull();
                });
        }

        private static Dictionary<string, JsonElement>? DeserializeAuditValues(string? values)
        {
            if (string.IsNullOrWhiteSpace(values))
                return null;

            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(values);
        }
    }
}
