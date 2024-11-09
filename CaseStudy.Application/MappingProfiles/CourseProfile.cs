using AutoMapper;
using CaseStudy.Application.Models.Holiday;
using CaseStudy.Core.Entities;

namespace CaseStudy.Application.MappingProfiles;

public class HolidayProfile : Profile
{
    public HolidayProfile()
    {
        // Add mapping for HolidayCreateModel to Holiday
        CreateMap<HolidayCreateModel, Holiday>()
            .ForMember(dest => dest.RefId, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.Now));
        // Add mapping for Holiday to HolidayCreateResponseModel
        CreateMap<Holiday, HolidayCreateResponseModel>();
        // Add mapping for HolidayUpdateModel to Holiday
        CreateMap<HolidayUpdateModel, Holiday>()
            .ForMember(dest => dest.RefId, opt => opt.UseDestinationValue())
            .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.Now));
        // Add mapping for Holiday to HolidayUpdateResponseModel
        CreateMap<Holiday, HolidayUpdateResponseModel>();
    }
}