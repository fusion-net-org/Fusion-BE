
using AutoMapper;
using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.Comment.Request;
using Fusion.Service.ViewModels.Comment.Response;
using Fusion.Service.ViewModels.Companies.Requests;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.CompanySubscription.Requests;
using Fusion.Service.ViewModels.CompanySubscription.Responses;
using Fusion.Service.ViewModels.FeatureCatalog.Requests;
using Fusion.Service.ViewModels.FeatureCatalog.Responses;
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
using Fusion.Service.ViewModels.TransactionPayment.Requests;
using Fusion.Service.ViewModels.TransactionPayment.Responses;
using Fusion.Service.ViewModels.Users.Requests;
using Fusion.Service.ViewModels.Users.Responses;
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

        CreateMap<CompanyMember, CompanyMemberResponseV2>()
            .ForMember(d => d.CompanyName, o => o.MapFrom(s => s.Company.Name))
            .ForMember(d => d.CompanyEmail, o => o.MapFrom(s => s.Company.Email))
            .ForMember(d => d.CompanyOwner, o => o.MapFrom(s => s.Company.OwnerUser.UserName))
            .ForMember(d => d.CompanyAvatar, o => o.MapFrom(s => s.Company.AvatarCompany))
            .ForMember(d => d.CompanyPhone, o => o.MapFrom(s => s.Company.PhoneNumber))
            .ForMember(d => d.CompanyAddress, o => o.MapFrom(s => s.Company.Address))
            .ForMember(d => d.CompanyCreateAt, o => o.MapFrom(s => s.Company.CreateAt))
            .ForMember(d => d.MemberJoinAt, o => o.MapFrom(s => s.JoinedAt))

            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.UserName))
            .ForMember(d => d.UserEmail, o => o.MapFrom(s => s.User.Email))
            .ForMember(d => d.UserPhone, o => o.MapFrom(s => s.User.Phone))
            .ForMember(d => d.UserAvatar, o => o.MapFrom(s => s.User.Avatar));


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
        // Entity -> LIST response
        CreateMap<TransactionPayment, TransactionPaymentResponse>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User != null ? s.User.UserName : null))
            .ForMember(d => d.PlanName, o => o.MapFrom(s => s.SubscriptionPlan != null ? s.SubscriptionPlan.Name : null));

        // Entity -> DETAIL response
        CreateMap<TransactionPayment, TransactionPaymentDetailResponse>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User != null ? s.User.UserName : null))
            .ForMember(d => d.PlanName, o => o.MapFrom(s => s.SubscriptionPlan != null ? s.SubscriptionPlan.Name : null));

        // Create request -> Entity (chỉ nhận PlanId; các trường còn lại set ở Service)
        CreateMap<TransactionPaymentCreateRequest, TransactionPayment>()
            .ForMember(d => d.PlanId, o => o.MapFrom(s => s.PlanId))
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.UserSubscriptionId, o => o.Ignore())
            .ForMember(d => d.Amount, o => o.Ignore())
            .ForMember(d => d.Currency, o => o.Ignore())
            .ForMember(d => d.Description, o => o.Ignore())
            .ForMember(d => d.Reference, o => o.Ignore())
            .ForMember(d => d.OrderCode, o => o.Ignore())
            .ForMember(d => d.PaymentLinkId, o => o.Ignore())
            .ForMember(d => d.PaymentMethod, o => o.Ignore())
            .ForMember(d => d.Provider, o => o.Ignore())
            .ForMember(d => d.TransactionDateTime, o => o.Ignore())
            .ForMember(d => d.DueAt, o => o.Ignore())
            .ForMember(d => d.PaidAt, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.Type, o => o.Ignore())
            // snapshot pricing (set tại Service khi khởi tạo draft)
            .ForMember(d => d.ChargeUnitSnapshot, o => o.Ignore())
            .ForMember(d => d.BillingPeriodSnapshot, o => o.Ignore())
            .ForMember(d => d.PeriodCountSnapshot, o => o.Ignore())
            .ForMember(d => d.SeatCountSnapshot, o => o.Ignore())
            .ForMember(d => d.PaymentModeSnapshot, o => o.Ignore())
            .ForMember(d => d.InstallmentIndex, o => o.Ignore())
            .ForMember(d => d.InstallmentTotal, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            // navs
            .ForMember(d => d.User, o => o.Ignore())
            .ForMember(d => d.SubscriptionPlan, o => o.Ignore());

        //----------------------------     entity: Project  ---------------------------------------------
        CreateMap<Project, ProjectResponse>();


        //--------------------------- entity: SubscriptionPlan ---------------------------------------------
        // Request -> Entity
        CreateMap<SubscriptionPlanCreateRequest, SubscriptionPlan>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.Price, o => o.Ignore())
            .ForMember(d => d.Features, o => o.Ignore());

        CreateMap<SubscriptionPlanUpdateRequest, SubscriptionPlan>()
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.Price, o => o.Ignore())
            .ForMember(d => d.Features, o => o.Ignore());

        CreateMap<SubscriptionPlanPriceInput, SubscriptionPlanPrice>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.PlanId, o => o.Ignore());

        // Entity -> Response
        CreateMap<SubscriptionPlanPrice, SubscriptionPlanPriceResponse>();

        CreateMap<SubscriptionPlan, SubscriptionPlanListItemResponse>();

        CreateMap<SubscriptionPlan, SubscriptionPlanDetailResponse>()
            .ForMember(d => d.Price, o => o.MapFrom(s => s.Price))
            .ForMember(d => d.Features, o => o.MapFrom(s =>
                s.Features != null
                    ? s.Features.Select(f => new SubscriptionPlanFeatureResponse
                    {
                        FeatureId = f.FeatureId,
                        FeatureCode = f.Feature != null ? f.Feature.Code : null,
                        FeatureName = f.Feature != null ? f.Feature.Name : null,
                        Enabled = f.Enabled
                    })
                    : new List<SubscriptionPlanFeatureResponse>()));

        // for customer 
        // Price -> PlanPricePreviewResponse
        CreateMap<SubscriptionPlanPrice, PlanPricePreviewResponse>()
            .ForMember(d => d.Amount, o => o.MapFrom(s => s.Price));

        // Plan -> SubscriptionPlanCustomerResponse
        CreateMap<SubscriptionPlan, SubscriptionPlanCustomerResponse>()
            .ForMember(d => d.Price, o => o.MapFrom(s => s.Price))
            .ForMember(d => d.FeaturesPreview, o => o.MapFrom(s =>
                (s.Features ?? Enumerable.Empty<SubscriptionPlanFeature>())
                    .Where(f => f.Enabled && f.Feature != null)
                    .OrderBy(f => f.Feature!.Name)
                    .Select(f => new PlanFeatureChipResponse
                    {
                        Name = f.Feature!.Name
                    })
                    .ToList()
            ));
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


        // ===================== Feature Catalog =====================
        // Request -> Entity
        CreateMap<FeatureCreateRequest, Feature>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore());

        CreateMap<FeatureUpdateRequest, Feature>()
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore());

        // Entity -> Response
        CreateMap<Feature, FeatureResponse>();
        CreateMap<Feature, FeatureActiveResponse>();


        // ===================== User Subscription =====================
        // Entitlement → VM
        CreateMap<UserSubscriptionEntitlement, EntitlementVm>()
            .ForMember(d => d.FeatureId, o => o.MapFrom(s => s.FeatureId))
            .ForMember(d => d.Code, o => o.MapFrom(s => s.Feature != null ? s.Feature.Code : null))
            .ForMember(d => d.Name, o => o.MapFrom(s => s.Feature != null ? s.Feature.Name : null))
            .ForMember(d => d.Enabled, o => o.MapFrom(s => s.Enabled));

        // Base: UserSubscription → UserSubscriptionResponse
        CreateMap<UserSubscription, UserSubscriptionResponse>()
            .ForMember(d => d.PlanName, o => o.MapFrom(s => s.Plan != null ? s.Plan.Name : string.Empty))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.UnitPrice, o => o.MapFrom(s => s.UnitPriceSnapshot))
            .ForMember(d => d.Currency, o => o.MapFrom(s => s.CurrencySnapshot ?? "VND"))
            .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt));

        // Detail: kế thừa base + thêm snapshots & entitlements
        CreateMap<UserSubscription, UserSubscriptionDetailResponse>()
            .IncludeBase<UserSubscription, UserSubscriptionResponse>()
            .ForMember(d => d.IsFullPackage, o => o.MapFrom(s => s.IsFullPackageSnapshot))
            .ForMember(d => d.LicenseScope, o => o.MapFrom(s => s.LicenseScopeSnapshot.ToString()))
            .ForMember(d => d.CompanyShareLimit, o => o.MapFrom(s => s.CompanyShareLimitSnapshot))
            .ForMember(d => d.SeatsPerCompanyLimit, o => o.MapFrom(s => s.SeatsPerCompanyLimitSnapshot))
            .ForMember(d => d.PeriodCount, o => o.MapFrom(s => s.PeriodCountSnapshot))
            .ForMember(d => d.BillingPeriod, o => o.MapFrom(s => s.BillingPeriodSnapshot.ToString()))
            .ForMember(d => d.PaymentMode, o => o.MapFrom(s => s.PaymentModeSnapshot.ToString()))
            .ForMember(d => d.InstallmentCount, o => o.MapFrom(s => s.InstallmentCountSnapshot))
            .ForMember(d => d.InstallmentInterval, o => o.MapFrom(s => s.InstallmentIntervalSnapshot.HasValue
                                                                      ? s.InstallmentIntervalSnapshot.Value.ToString()
                                                                      : null))
            .ForMember(d => d.Entitlements, o => o.MapFrom(s => s.Entitlements));

        // ===================== Company Subscription =====================

        CreateMap<CompanySubscriptionCreateRequest, CompanySubscription>()
            .ForMember(d => d.Id, o => o.Ignore()) 
            .ForMember(d => d.Status, o => o.Ignore()) 
            .ForMember(d => d.SharedOn, o => o.Ignore()) 
            .ForMember(d => d.UpdatedAt, o => o.Ignore()) 
            .ForMember(d => d.ExpiredAt, o => o.Ignore())
            .ForMember(d => d.SeatsLimitSnapshot, o => o.Ignore())
            .ForMember(d => d.SeatsLimitUnit, o => o.Ignore()) 
            .ForMember(d => d.Company, o => o.Ignore()) // nav
            .ForMember(d => d.UserSubscription, o => o.Ignore()) 
            .ForMember(d => d.Entitlements, o => o.Ignore());

        CreateMap<CompanySubscription, CompanySubscriptionListResponse>()
            .ForMember(d => d.CompanyName, o => o.MapFrom(s => s.Company.Name))
            .ForMember(d => d.PlanName, o => o.MapFrom(s => s.UserSubscription.Plan.Name))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.UserSubscription.User.UserName))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.SharedOn, o => o.MapFrom(s => s.SharedOn))
            .ForMember(d => d.ExpiredAt, o => o.MapFrom(s => s.ExpiredAt))  
            .ForMember(d => d.SeatsLimitSnapshot, o => o.MapFrom(s => s.SeatsLimitSnapshot))
            .ForMember(d => d.SeatsLimitUnit, o => o.MapFrom(s => s.SeatsLimitUnit));

        CreateMap<CompanySubscription, CompanySubscriptionDetailResponse>()
            .IncludeBase<CompanySubscription, CompanySubscriptionListResponse>()
            .ForMember(d => d.CompanyId, o => o.MapFrom(s => s.CompanyId))
            .ForMember(d => d.UserSubscriptionId, o => o.MapFrom(s => s.UserSubscriptionId))
            .ForMember(d => d.OwnerUserId, o => o.MapFrom(s => s.OwnerUserId))
            .ForMember(d => d.Entitlements, o => o.MapFrom(s => s.Entitlements));

        CreateMap<CompanySubscriptionEntitlement, CompanySubscriptionEntitlementDetailResponse>()
            .ForMember(d => d.FeatureId, o => o.MapFrom(s => s.FeatureId))
            .ForMember(d => d.FeatureCode, o => o.MapFrom(s => s.Feature.Code))
            .ForMember(d => d.FeatureName, o => o.MapFrom(s => s.Feature.Name))
            .ForMember(d => d.Category, o => o.MapFrom(s => s.Feature.Category))
            .ForMember(d => d.Enabled, o => o.MapFrom(s => s.Enabled));


        // Dùng cho màn dropdown chọn subscription active
        CreateMap<CompanySubscription, CompanySubscriptionActiveResponse>()
            .ForMember(d => d.NameSubscription,
                o => o.MapFrom(s => s.UserSubscription.Plan.Name)) // hoặc s.UserSubscription.Plan.DisplayName nếu có
            .ForMember(d => d.SeatsLimitSnapshot,
                o => o.MapFrom(s => s.SeatsLimitSnapshot))
            .ForMember(d => d.SeatsLimitUnit,
                o => o.MapFrom(s => s.SeatsLimitUnit))
            .ForMember(d => d.CompanySubscriptionEntitlements,
                o => o.MapFrom(s => s.Entitlements));

        CreateMap<CompanySubscriptionEntitlement, CompanySubscriptionEntitlementDropdownResponse>()
            .ForMember(d => d.Id,
                o => o.MapFrom(s => s.Id))
            .ForMember(d => d.FeatureName,
                o => o.MapFrom(s => s.Feature.Name));

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

        // ===================== Project task =====================
        CreateMap<ProjectTaskRequest, ProjectTask>()
            .ForMember(d => d.Id, opt => opt.Ignore()) // create: service tự gán
            .ForMember(d => d.CreateAt, opt => opt.Ignore())
            .ForMember(d => d.UpdateAt, opt => opt.Ignore())
            .ForMember(d => d.Code, opt => opt.Ignore())
            .ForMember(d => d.Status, opt => opt.Ignore())
            .ForMember(d => d.CurrentStatusId, opt => opt.Ignore())
            .ForMember(d => d.OrderInSprint, opt => opt.Ignore())
            .ForMember(d => d.IsBacklog, opt => opt.Ignore())
            .ForMember(d => d.RemainingHours, opt => opt.Ignore())
            .ForMember(d => d.IsDeleted, opt => opt.Ignore())
            .ForMember(d => d.Assignees, opt => opt.Ignore());

        CreateMap<ProjectTask, ProjectTaskResponse>()
            .ForMember(d => d.AssigneeIds, opt => opt.MapFrom(s => s.Assignees.Select(a => a.UserId)));



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
