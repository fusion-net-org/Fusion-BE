using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.Services
{
    public static class PermissionCatalog
    {
        // Auth
        public const string AuthRegister = "REGISTER";
        public const string AuthLogin = "LOGIN";
        public const string AuthLogout = "LOGOUT";
        public const string AuthUpdateProfile = "UPDATE_PROFILE";
        public const string AuthGetCompanyList = "GET_COMPANY_LIST";
        public const string AuthGetNotification = "GET_NOTIFICATION";
        public const string AuthViewCompanyDashboard = "VIEW_COMPANY_DASHBOARD";
        public const string AuthViewProjectDashboard = "VIEW_PROJECT_DASHBOARD";
        public const string AuthSwitchCompany = "SWITCH_COMPANY";

        // Company
        public const string CompanyCreate = "CREATE_COMPANY";
        public const string CompanyUpdate = "UPDATE_COMPANY";
        public const string CompanyArchive = "ARCHIVE_COMPANY";
        public const string CompanyManage = "MANAGE_COMPANY";

        // Role & Member
        public const string RoleCreate = "CREATE_ROLE";
        public const string RoleAssignPermission = "ASSIGN_PERMISSION";
        public const string MemberInvite = "INVITE_MEMBER";
        public const string MemberManage = "MANAGE_MEMBERS";
        public const string MemberRemove = "REMOVE_MEMBER";
        public const string MemberUpdateRole = "UPDATE_ROLE";
        public const string MemberAssignRole = "ASSIGN_ROLE";

        // Partner
        public const string PartnerManage = "MANAGE_PARTNERSHIP";
        public const string PartnerView = "VIEW_PARTNER_COMPANY";
        public const string PartnerSendInvite = "SEND_INVITE_PARTNER";
        public const string PartnerAcceptInvite = "ACCEPT_INVITE_PARTNER";
        public const string PartnerRejectInvite = "REJECT_INVITE_PARTNER";
        public const string PartnerManageRequest = "MANAGE_PARTNER_REQUEST";

        // Project Request
        public const string ProjectRequestCreate = "CREATE_PRQ";
        public const string ProjectRequestUpdate = "UPDATE_PRQ";
        public const string ProjectRequestView = "VIEW_PRQ";
        public const string ProjectRequestAccept = "ACCEPT_PRQ";
        public const string ProjectRequestReject = "REJECT_PRQ";
        public const string ProjectRequestDelete = "DELETE_PRQ";

        // Project & Sprint
        public const string ProjectCreate = "CREATE_PROJECT";
        public const string ProjectUpdate = "UPDATE_PROJECT";
        public const string ProjectViewList = "VIEW_PROJECT_LIST";
        public const string ProjectManage = "MANAGE_PROJECT";
        public const string ProjectArchive = "ARCHIVE_PROJECT";
        public const string SprintManage = "MANAGE_SPRINT";
        public const string SprintCrud = "CRUD_SPRINT";
        public const string SprintGenerateAI = "GENERATE_SPRINT_AI";

        // Task
        public const string TaskCrud = "CRUD_TASK";
        public const string TaskManage = "MANAGE_TASK";
        public const string TaskAssignMember = "ASSIGN_MEMBER";
        public const string TaskChangeStatus = "CHANGE_TASK_STATUS";
        public const string TaskComment = "COMMENT_TASK";
        public const string TaskDeleteComment = "DELETE_COMMENT";

        // Ticket (sẽ mapping tiếp các id 44+ trong DB của bạn)
        // ...

        // Seed cho đồng bộ DB <-> code
        public static readonly (int Id, string Code)[] Seed = new[]
        {
            (1, AuthRegister),
            (2, AuthLogin),
            (3, AuthLogout),
            (4, AuthUpdateProfile),
            (5, AuthGetCompanyList),
            (6, AuthGetNotification),
            (7, AuthViewCompanyDashboard),
            (8, AuthViewProjectDashboard),
            (9, AuthSwitchCompany),

            (10, CompanyCreate),
            (11, CompanyUpdate),
            (12, CompanyArchive),
            (13, CompanyManage),

            (14, RoleCreate),
            (15, RoleAssignPermission),
            (16, MemberInvite),
            (17, MemberManage),
            (18, MemberRemove),
            (19, MemberUpdateRole),
            (20, MemberAssignRole),

            (21, PartnerManage),
            (22, PartnerView),
            (23, PartnerSendInvite),
            (24, PartnerAcceptInvite),
            (25, PartnerRejectInvite),
            (26, PartnerManageRequest),

            (27, ProjectRequestCreate),
            (28, ProjectRequestUpdate),
            (29, ProjectRequestView),
            (30, ProjectRequestAccept),
            (31, ProjectRequestReject),
            (32, ProjectRequestDelete),

            (33, ProjectCreate),
            (34, ProjectUpdate),
            (35, ProjectViewList),
            (36, ProjectManage),
            (37, ProjectArchive),
            (38, SprintManage),
            (39, SprintCrud),
            (40, SprintGenerateAI),

            (41, TaskCrud),
            (42, TaskManage),
            (43, TaskAssignMember),
            (44, TaskChangeStatus),
            (45, TaskComment),
            (46, TaskDeleteComment),
        };
    }
}

