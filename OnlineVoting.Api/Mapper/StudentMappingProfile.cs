using AutoMapper;
using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Api.Mapper
{
    public class StudentMappingProfile : Profile
    {
        public StudentMappingProfile()
        {
            CreateMap<UploadStudentRequest, ExtractDataFromExcelRequest>();
        }
    }
}
