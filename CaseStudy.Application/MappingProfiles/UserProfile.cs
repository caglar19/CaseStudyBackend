using AutoMapper;
using CaseStudy.Application.Models.User;
using CaseStudy.Core.Entities;

namespace CaseStudy.Application.MappingProfiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        // Add mapping for CreateUserModel to ApplicationUser
        CreateMap<CreateUserModel, User>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => "user" + Guid.NewGuid().ToString()))
            .ForMember(dest => dest.RefId, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.Now));
        // Add mapping for ApplicationUser to CreateCreateUserResponseModel
        CreateMap<User, CreateUserResponseModel>();
        // Add mapping for UpdateUserModel to ApplicationUser
        CreateMap<UpdateUserModel, User>()
            .ForMember(dest => dest.RefId, opt => opt.UseDestinationValue())
            .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.Now));
        // Add mapping for ApplicationUser to UpdateUserResponseModel
        CreateMap<User, UpdateUserResponseModel>();
    }
}