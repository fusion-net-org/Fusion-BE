
using AutoMapper;
using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.Comment.Request;
using Fusion.Service.ViewModels.Comment.Response;
using Fusion.Service.ViewModels.Companies.Requests;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.Task.Request;
using Fusion.Service.ViewModels.Task.Response;
using Fusion.Service.ViewModels.Tickets.Requests;
using Fusion.Service.ViewModels.Tickets.Responses;
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

        //----------------------------     Partner  ---------------------------------------------
        CreateMap<CompanyFriendshipResponse, CompanyFriendship>().ReverseMap();

        CreateMap<UpdateSelfUserRequest, User>()
               .ForMember(dest => dest.Avatar, opt => opt.Ignore())
               .ForMember(dest => dest.UpdateAt, opt => opt.MapFrom(src => DateTime.UtcNow))
               .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<User, AdminUserResponse>();
        CreateMap<User, CompanyUserResponse>();
        CreateMap<User, SelfUserResponse>();

        //----------------------------     entity: Company ---------------------------------------------
        CreateMap<Company, CompanyResponse>()
            .ForMember(dest => dest.OwnerUserName, otp => otp.MapFrom(src => src.OwnerUser.UserName)).ReverseMap();

        CreateMap<CompanyRequest, Company>()
            .ForAllMembers(opt =>
                            opt.Condition((src, dest, srcMember) => srcMember != null));

        //----------------------------     entity: CompanyMember ---------------------------------------------
        CreateMap<CompanyMember, CompanyMemberResponse>()
            .ForMember(dest => dest.MemberId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company!.Name))
            .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src => src.User!.UserName));

        //----------------------------     entity: Ticket ---------------------------------------------
        CreateMap<Ticket, TicketResponse>().ReverseMap();
        CreateMap<TicketRequest, Ticket>().ReverseMap();


        //----------------------------     entity: Task ---------------------------------------------
        CreateMap<ProjectTask, ProjectTaskResponse>().ReverseMap();
        CreateMap<ProjectTaskRequest, ProjectTask>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));


        //----------------------------     entity: Comment ---------------------------------------------
        CreateMap<Comment, CommentResponse>().ReverseMap();
        CreateMap<CommentRequest, Comment>().ReverseMap();
    }
}
