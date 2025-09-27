

using AutoMapper;
using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.Users.Requests;
using Fusion.Service.ViewModels.Users.Responses;

namespace Fusion.Service.Commons.Helpers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        //----------------------------     entity: User ---------------------------------------------
        CreateMap<RegisterRequest, User>()
               .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.FirstName}{src.LastName}"))
               .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
               .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore())
               .ForMember(dest => dest.CreateAt, opt => opt.MapFrom(src => DateTime.UtcNow))
               .ForMember(dest => dest.UpdateAt, opt => opt.MapFrom(src => DateTime.UtcNow))
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => true));

        CreateMap<UpdateSelfUserRequest, User>()
               .ForMember(dest => dest.Avatar, opt => opt.Ignore())
               .ForMember(dest => dest.UpdateAt, opt => opt.MapFrom(src => DateTime.UtcNow))
               .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<User, AdminUserResponse>();
        CreateMap<User, CompanyUserResponse>();
        CreateMap<User, SelfUserResponse>();


    }
}
