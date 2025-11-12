
using AutoMapper;
using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.Comment.Request;
using Fusion.Service.ViewModels.Comment.Response;
using Fusion.Service.ViewModels.Companies.Requests;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.CompanySubscription.Requests;
using Fusion.Service.ViewModels.CompanySubscription.Responses;
using Fusion.Service.ViewModels.Notifications.Requests;
using Fusion.Service.ViewModels.Notifications.Responses;
using Fusion.Service.ViewModels.Project.Requests;
using Fusion.Service.ViewModels.Project.Responses;
using Fusion.Service.ViewModels.ProjectMembers.Responses;
using Fusion.Service.ViewModels.Projects.Requests;
using Fusion.Service.ViewModels.Projects.Responses;
using Fusion.Service.ViewModels.SubscriptionPlan.Requests;
using Fusion.Service.ViewModels.SubscriptionPlan.Responses;
using Fusion.Service.ViewModels.Task.Request;
using Fusion.Service.ViewModels.Task.Response;
using Fusion.Service.ViewModels.TicketComment;
using Fusion.Service.ViewModels.Tickets.Requests;
using Fusion.Service.ViewModels.Tickets.Responses;
using Fusion.Service.ViewModels.TransactionPayment.Responses;
using Fusion.Service.ViewModels.Users.Requests;
using Fusion.Service.ViewModels.Users.Responses;
using Fusion.Service.ViewModels.UserSubscription.Requests;
using Fusion.Service.ViewModels.UserSubscription.Responses;
using Fusion.Service.ViewModels.WorkflowStatus;

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
                        src => (src.CompanyB != null ? src.CompanyB.ProjectCompanies.Count + src.CompanyB.ProjectCompanyRequests.Count : 0)))
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
                        src => src.ProjectCompanies.Count + src.ProjectCompanyRequests.Count
                        ))
            .ForMember(dest => dest.ListProjects, opt => opt.MapFrom(
                        src => src.ProjectCompanies.Concat(src.ProjectCompanyRequests)
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
                        src => src.ProjectCompanies.Count + src.ProjectCompanyRequests.Count
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
             .ForMember(dest => dest.MemberPhoneNumber, opt => opt.MapFrom(src => src.User!.Phone))
             .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User!.Gender))
             .ForMember(dest => dest.IsOwner, opt => opt.MapFrom(src =>
                 src.Company != null && src.User != null && src.Company.OwnerUser != null &&
                 src.Company.OwnerUser.UserName == src.User.UserName))
             .ForMember(dest => dest.NumberCompanyJoin,
                 opt => opt.MapFrom(src => src.User.CompanyMembers.Count(cm => cm.IsDeleted == false)));

        //----------------------------     entity: Ticket ---------------------------------------------
        CreateMap<Ticket, TicketResponse>()
                  .ForMember(dest => dest.SubmittedByName,
                             opt => opt.MapFrom(src => src.SubmittedByNavigation != null ? src.SubmittedByNavigation.UserName : null))
                  .ReverseMap(); 
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
        CreateMap<CreateProjectRequestRequest, ProjectRequest>();
          

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
            .ForMember(dest => dest.RequesterCompanyLogoUrl,
                    opt => opt.MapFrom(src =>
                    src.RequesterCompany != null ? src.RequesterCompany.AvatarCompany : null))
            .ForMember(dest => dest.ExecutorCompanyLogoUrl,
                    opt => opt.MapFrom(src =>
                    src.ExecutorCompany != null ? src.ExecutorCompany.AvatarCompany : null))
            .ForMember(dest => dest.isHaveProject,
                    opt => opt.MapFrom(src => src.Project != null && src.Project.Id != Guid.Empty))
            .ReverseMap();

        //--------------------------- entity: Transaction Payment ---------------------------------------------
        // List item
        CreateMap<TransactionPayment, TransactionPaymentResponse>()
            .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User != null ? s.User.UserName : null))
            .ForMember(d => d.PlanName, opt => opt.MapFrom(s => s.SubscriptionPlan != null ? s.SubscriptionPlan.Name : null));

        // Detail item
        CreateMap<TransactionPayment, TransactionPaymentDetailResponse>();
        //----------------------------     entity: Project  ---------------------------------------------
        CreateMap<Project, ProjectResponse>();


        //--------------------------- entity: SubscriptionPlan ---------------------------------------------
        CreateMap<SubscriptionPlanCreateRequest, SubscriptionPlan>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
             .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
             .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

        CreateMap<SubscriptionPlanFeatureRequest, SubscriptionPlanFeature>();
        CreateMap<SubscriptionPlanPriceRequest, SubscriptionPlanPrice>();

        // ReverseMap
        CreateMap<SubscriptionPlan, SubscriptionPlanResponse>();
        CreateMap<SubscriptionPlanFeature, SubscriptionPlanFeatureRequest>();
        CreateMap<SubscriptionPlanPrice, SubscriptionPlanPriceRequest>();

        //detail
        CreateMap<SubscriptionPlan, SubscriptionPlanDetailResponse>();
        CreateMap<SubscriptionPlanPrice, SubscriptionPlanPriceResponse>();
        CreateMap<SubscriptionPlanFeature, SubscriptionPlanFeatureResponse>();

        //----------------------------     entity: Notification ---------------------------------------------
        CreateMap<Notification, NotificationResponse>()
            .ForMember(dest => dest.LinkUrl, opt => opt.MapFrom(src => src.LinkUrlMobile)) 
            .ForMember(dest => dest.LinkUrlWeb, opt => opt.MapFrom(src => src.LinkUrlWeb));
        CreateMap<SendNotificationRequest, Notification>();
        CreateMap<SendAllNotificationRequest, Notification>();

        //----------------------------     entity: Project ---------------------------------------------
        // ===================== Project (Create) =====================
        CreateMap<CreateProjectRequest, Project>()
            .ForMember(d => d.IsHired, o => o.MapFrom(s => s.isHired))
            .ForMember(d => d.CompanyRequestId, o => o.MapFrom(s =>
                s.CompanyHiredId.HasValue && s.CompanyHiredId.Value != Guid.Empty ? s.CompanyHiredId : (Guid?)null))
            .ForMember(d => d.ProjectRequestId, o => o.MapFrom(s =>
                s.ProjectRequestId.HasValue && s.ProjectRequestId.Value != Guid.Empty ? s.ProjectRequestId : (Guid?)null))
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.CreateAt, o => o.Ignore())
            .ForMember(d => d.UpdateAt, o => o.Ignore())
            .ForMember(d => d.Id, o => o.Ignore());

        // ===================== Project (List) =====================
        CreateMap<Project, ProjectsResponse>()
            .ForMember(d => d.isHired, o => o.MapFrom(s => s.IsHired));

        // ===================== Sprint =====================
        CreateMap<Sprint, SprintDto>();

        // ===================== Project (Detail) =====================
        CreateMap<Project, ProjectDetailResponse>()
     .ForMember(d => d.IsHired, o => o.MapFrom(s => s.IsHired))
     .ForMember(d => d.CompanyName, o => o.MapFrom(s => s.Company != null ? s.Company.Name : null))
     .ForMember(d => d.CompanyHiredName, o => o.MapFrom(s => s.CompanyRequest != null ? s.CompanyRequest.Name : null))
     .ForMember(d => d.CreatedByName, o => o.MapFrom(s => s.CreatedByNavigation != null ? s.CreatedByNavigation.UserName : null))
     .ForMember(d => d.Sprints, o => o.MapFrom(s => s.Sprints.Where(x => !x.IsDeleted)));


        // ===================== User Subscription =====================
        CreateMap<UserSubscription, UserSubscriptionDetailResponse>()
             .ForMember(d => d.Entitlements, opt => opt.MapFrom(s => s.UserSubscriptionEntitlements));

        CreateMap<UserSubscriptionEntitlement, UserSubscriptionEntitlementResponse>()
            .ForMember(d => d.FeatureKey, opt => opt.MapFrom(s => s.FeatureKey.ToString()));

        CreateMap<UserSubscription, UserSubscriptionListItem>();
        CreateMap<UserSubscriptionCreateRequest, UserSubscription>();

        // ===================== Company Subscription =====================

        #region create companysubscription
        CreateMap<CompanySubscriptionCreateRequest, CompanySubscription>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CompanySubscriptionEntitlements, opt => opt.MapFrom(src => src.Entitlements));

        CreateMap<CompanySubscriptionEntitlementCreateRequest, CompanySubscriptionEntitlement>()
              .ForMember(dest => dest.Id, opt => opt.Ignore())
              .ForMember(dest => dest.CompanySubscriptionId, opt => opt.Ignore())
              .ForMember(dest => dest.Remaining, opt => opt.Ignore());

        CreateMap<CompanySubscription, CompanySubscriptionDetailResponse>();
        CreateMap<CompanySubscriptionEntitlement, CompanySubscriptionEntitlementDetailResponse>();
        #endregion

        #region update companysubscription
        CreateMap<CompanySubscriptionUpdateRequest, CompanySubscription>()
                 .ForMember(dest => dest.CompanySubscriptionEntitlements, opt => opt.MapFrom(src => src.Entitlements));

        CreateMap<CompanySubscriptionEntitlementUpdateRequest, CompanySubscriptionEntitlement>();
        #endregion
        #region detail + list companysubscription
        CreateMap<CompanySubscription, CompanySubscriptionDetailResponse>()
             .ForMember(dest => dest.Entitlements, opt => opt.MapFrom(src => src.CompanySubscriptionEntitlements));

        CreateMap<CompanySubscriptionEntitlement, CompanySubscriptionEntitlementDetailResponse>();

        CreateMap<CompanySubscription, CompanySubscriptionListResponse>();

        CreateMap<CompanySubscription, CompanySubscriptionActiveResponse>()
       .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
       .ForMember(dest => dest.CompanySubscriptionEntitlements, opt => opt.MapFrom(src => src.CompanySubscriptionEntitlements));
        #endregion


        // ===================== Project (Detail) =====================
        CreateMap<Project, ProjectDetailResponse>()
     .ForMember(d => d.IsHired, o => o.MapFrom(s => s.IsHired))
     .ForMember(d => d.CompanyName, o => o.MapFrom(s => s.Company != null ? s.Company.Name : null))
     .ForMember(d => d.CompanyHiredName, o => o.MapFrom(s => s.CompanyRequest != null ? s.CompanyRequest.Name : null))
     .ForMember(d => d.CreatedByName, o => o.MapFrom(s => s.CreatedByNavigation != null ? s.CreatedByNavigation.UserName : null))
     .ForMember(d => d.Sprints, o => o.MapFrom(s => s.Sprints.Where(x => !x.IsDeleted)));

        CreateMap<Project, ProjectResponseVersion3>()
      .ForMember(d => d.CompanyExecutorName, o => o.MapFrom(s => s.Company != null ? s.Company.Name : null))
      .ForMember(d => d.CompanyRequestName, o => o.MapFrom(s => s.CompanyRequest != null ? s.CompanyRequest.Name : null))
      .ForMember(d => d.IsHired, o => o.MapFrom(s => s.IsHired))
      .ForMember(d => d.StartDate, o => o.MapFrom(s => s.StartDate))
      .ForMember(d => d.EndDate, o => o.MapFrom(s => s.EndDate))
      .ForMember(d => d.CreateAt, o => o.MapFrom(s => s.CreateAt))
      .ForMember(d => d.UpdateAt, o => o.MapFrom(s => s.UpdateAt))
      .ForMember(d => d.Code, o => o.MapFrom(s => s.Code))
      .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
      .ForMember(d => d.Description, o => o.MapFrom(s => s.Description))
      .ForMember(d => d.Status, o => o.MapFrom(s => s.Status))
      .ForMember(d => d.WorkflowId, o => o.MapFrom(s => s.WorkflowId))
      .ForMember(d => d.CreatedBy, o => o.MapFrom(s => s.CreatedBy))
      .ForMember(d => d.CreateByName, o => o.MapFrom(s => s.CreatedByNavigation != null ? s.CreatedByNavigation.UserName : null));

        // ===================== Project Member =====================
        CreateMap<ProjectMember, ProjectMemberResponseV2>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.User != null ? src.User.Phone : null))
            .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User != null ? src.User.Avatar : null))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User != null ? src.User.Gender : null))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.User != null && src.User.Status))
            .ForMember(dest => dest.IsPartner, opt => opt.MapFrom(src => src.IsPartner))
            .ForMember(dest => dest.IsViewAll, opt => opt.MapFrom(src => src.IsViewAll))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
        // ===================== Workflow status =====================
        CreateMap<WorkflowStatus, WorkflowStatusResponse>().ReverseMap();


        // ===================== TicketComment =====================
        CreateMap<TicketComment, TicketCommentResponse>()
         .ForMember(dest => dest.AuthorUserName,
             opt => opt.MapFrom(src => src.AuthorUser != null ? src.AuthorUser.UserName : null))
         .ForMember(dest => dest.AuthorUserAvatar,
             opt => opt.MapFrom(src => src.AuthorUser != null ? src.AuthorUser.Avatar : null))
         .ForMember(dest => dest.IsOwner, opt => opt.MapFrom((src, dest, _, context) =>
         {
             var currentUserId = (Guid)context.Items["CurrentUserId"];
             return src.AuthorUserId == currentUserId || (src.Ticket != null && src.Ticket.SubmittedBy == currentUserId);
         }))
         .ReverseMap();


        CreateMap<TicketCommentRequest, TicketComment>()
            .ForMember(dest => dest.CreateAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<TicketCommentRequestUpdate, TicketComment>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

    }    
}
