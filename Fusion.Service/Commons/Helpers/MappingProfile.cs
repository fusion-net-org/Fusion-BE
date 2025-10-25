
using AutoMapper;
using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.Comment.Request;
using Fusion.Service.ViewModels.Comment.Response;
using Fusion.Service.ViewModels.Companies.Requests;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.Task.Request;
using Fusion.Service.ViewModels.Task.Response;
using Fusion.Service.ViewModels.Projects.Requests;
using Fusion.Service.ViewModels.Projects.Responses;
using Fusion.Service.ViewModels.Tickets.Requests;
using Fusion.Service.ViewModels.Tickets.Responses;
using Fusion.Service.ViewModels.Users.Requests;
using Fusion.Service.ViewModels.Users.Responses;
using Fusion.Service.ViewModels.SubscriptionPackage.Responses;
using Fusion.Service.ViewModels.SubscriptionPackage.Requests;
using Fusion.Service.ViewModels.Notifications.Responses;
using Fusion.Service.ViewModels.Notifications.Requests;
using Fusion.Service.ViewModels.TransactionPayment.Requests;

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

        //----------------------------     entity: Partner  ---------------------------------------------
        CreateMap<CompanyFriendship, CompanyFriendshipResponse>()
             .ForMember(dest => dest.TotalProject, opt => opt.MapFrom(
                        src => (src.CompanyB != null ? src.CompanyB.ProjectCompanies.Count + src.CompanyB.ProjectCompanyHireds.Count : 0)))
             .ForMember(dest => dest.TotalMember, opt => opt.MapFrom(
                       src => (src.CompanyB != null ? src.CompanyB.CompanyMembers.Count : 0)))
             .ReverseMap();


        CreateMap<UpdateSelfUserRequest, User>()
               .ForMember(dest => dest.Avatar, opt => opt.Ignore())
               .ForMember(dest => dest.UpdateAt, opt => opt.MapFrom(src => DateTime.UtcNow))
               .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<User, AdminUserResponse>();
        CreateMap<User, CompanyUserResponse>();
        CreateMap<User, SelfUserResponse>();

        //----------------------------     entity: Company ---------------------------------------------
        CreateMap<Company, CompanyResponse>()
            .ForMember(dest => dest.OwnerUserName, otp => otp.MapFrom(src => src.OwnerUser.UserName))
            .ForMember(dest => dest.OwnerUserAvatar, otp => otp.MapFrom(src => src.OwnerUser.Avatar))
            .ForMember(dest => dest.ListMembers, opt => opt.MapFrom(src => src.CompanyMembers))
            .ForMember(dest => dest.TotalProject, opt => opt.MapFrom(
                        src => src.ProjectCompanies.Count + src.ProjectCompanyHireds.Count
                        ))
            .ForMember(dest => dest.ListProjects, opt => opt.MapFrom(
                        src => src.ProjectCompanies.Concat(src.ProjectCompanyHireds)
                        ))
            .ForMember(dest => dest.TotalMember, opt => opt.MapFrom(
                        src => src.CompanyMembers.Count))
            .ReverseMap();

        CreateMap<CompanyRequest, Company>()
            .ForAllMembers(opt =>
                            opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Company,CompanyResponseVersion2>()
            .ForMember(dest => dest.OwnerUserName, otp => otp.MapFrom(src => src.OwnerUser.UserName))
            .ForMember(dest => dest.OwnerUserAvatar, otp => otp.MapFrom(src => src.OwnerUser.Avatar))
            .ForMember(dest => dest.TotalProject, opt => opt.MapFrom(
                        src => src.ProjectCompanies.Count + src.ProjectCompanyHireds.Count
                        ))
            .ForMember(dest => dest.TotalMember, opt => opt.MapFrom(
                        src => src.CompanyMembers.Count))
            //.ForMember(dest => dest.isOwner, opt => opt.MapFrom((src, dest, destMember, context) =>
            //{
            //    if (context.Items.ContainsKey("CurrentUserId") && context.Items["CurrentUserId"] is Guid currentUserId)
            //    {
            //        return src.OwnerUserId == currentUserId;
            //    }
            //    return false;
            //}))
            // .ForMember(dest => dest.isPartner, opt => opt.MapFrom((src, dest, destMember, context) =>
            // {
            //     if (context.Items.TryGetValue("PartnerCompanyIds", out var partnerIdsObj)
            //         && partnerIdsObj is List<Guid> partnerIds)
            //     {
            //         return partnerIds.Contains(src.Id);
            //     }
            //     return false;
            // }))
            .ReverseMap();

        //----------------------------     entity: CompanyMember ---------------------------------------------
        CreateMap<CompanyMember, CompanyMemberResponse>()
             .ForMember(dest => dest.MemberId, opt => opt.MapFrom(src => src.UserId))
             .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company!.Name))
             .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src => src.User!.UserName))
             .ForMember(dest => dest.MemberAvatar, opt => opt.MapFrom(src => src.User!.Avatar))
             .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User!.Email))
             .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.User!.Phone))
             .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User!.Gender))
             .ForMember(dest => dest.IsOwner, opt => opt.MapFrom(src =>
                 src.Company != null && src.User != null && src.Company.OwnerUser != null &&
                 src.Company.OwnerUser.UserName == src.User.UserName))
             .ForMember(dest => dest.NumberCompanyJoin,
                 opt => opt.MapFrom(src => src.User.CompanyMembers.Count(cm => cm.IsDeleted == false)));


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
        CreateMap<CommentRequestUpdate, Comment>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        //----------------------------     entity: Project Request ---------------------------------------------
        CreateMap<CreateProjectRequestRequest, ProjectRequest>()
            .ForMember(dest => dest.Status, opt =>
            opt.MapFrom(src => src.Status.HasValue ? src.Status.Value.ToString() : null));

        CreateMap<UpdateProjectRequestRequest, ProjectRequest>()
            .ForMember(dest => dest.Status, opt =>
            opt.MapFrom(src => src.Status.HasValue ? src.Status.Value.ToString() : null));

        CreateMap<ProjectRequest, ProjectRequestResponse>()
            .ForMember(dest => dest.RequesterCompanyName,
                    opt => opt.MapFrom(src => src.RequesterCompany != null ? src.RequesterCompany.Name : null))
            .ForMember(dest => dest.ExecutorCompanyName,
                    opt => opt.MapFrom(src => src.ExecutorCompany != null ? src.ExecutorCompany.Name : null))
            .ForMember(dest => dest.CreatedName,
                    opt => opt.MapFrom(src => src.CreatedByNavigation != null ? src.CreatedByNavigation.UserName : null))
            .ForMember(dest => dest.ProjectName,
                    opt => opt.MapFrom(src => src.Name)) // Name trong ProjectRequest map sang ProjectName
            .ForMember(dest => dest.ConvertedProjectId,
                    opt => opt.MapFrom(src => src.Project != null ? src.Project.Id : (Guid?)null))
            .ReverseMap();

        //--------------------------- entity: Transaction Payment ---------------------------------------------
        CreateMap<CreateTransactionRequest, TransactionPayment>();

        //----------------------------     entity: Project  ---------------------------------------------
        CreateMap<Project, ProjectResponse>();


        //--------------------------- entity: SubscriptionPackage ---------------------------------------------
        CreateMap<SubscriptionRequest, SubscriptionPackage>()
          .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<SubscriptionPackage, SubscriptionAdminResponse>();
        CreateMap<SubscriptionPackage, SubscriptionResponse>();

        //----------------------------     entity: Notification ---------------------------------------------
        CreateMap<Notification, NotificationResponse>();
        CreateMap<SendNotificationRequest, Notification>();

    }

}
