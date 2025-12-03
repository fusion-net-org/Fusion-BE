using AutoMapper;
using CloudinaryDotNet.Actions;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Company_Member;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Fusion.Repository.ViewModels;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Companies.Email;
using Fusion.Service.ViewModels.Companies.Requests;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.Notifications.Requests;
using Fusion.Service.ViewModels.Permission.Responses;
using Fusion.Service.ViewModels.Role.Responses;
using Fusion.Service.ViewModels.UserRole.Responses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Travelogue.Repository.Caching;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Fusion.Service.Services
{
    public class CompanyMemberService : ICompanyMemberService
    {

        private readonly ICompanyMemberRepository _companyMemberRepository;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMailService _mailService;
        private readonly IMapper _mapper;
        private readonly ICompanyActivityService _logService;
        private readonly ICurrentService _currentService;

        public CompanyMemberService(ICompanyMemberRepository companyMemberRepository, IProjectMemberRepository projectMemberRepository,
            INotificationService notificationService, IUserRepository userRepository, ICompanyRepository companyRepository,
            IMapper mapper, IMailService mailService, ICompanyActivityService logService, ICurrentService currentService)
        {

            _companyMemberRepository = companyMemberRepository;
            _projectMemberRepository = projectMemberRepository;
            _notificationService = notificationService;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _mailService = mailService;
            _mapper = mapper;
            _logService = logService;
            _currentService = currentService;
        }

        public async Task<CompanyMemberResponse?> FiredMemberFromCompany(string terminatorEmail, string firedMemberMail, string reason, Guid companyId, CancellationToken token = default)
        {
            var result = await _companyMemberRepository.FiredMemberFromCompany(terminatorEmail, firedMemberMail, reason, companyId, token);

            var response = await _companyMemberRepository.GetCompanyMemberByIdAsync(result.Id);

            var emailBody = MailUtils.FiredMemberFromCompany(
                               terminatorEmail,
                               result.User.UserName,
                               result.Company.Name,
                               reason);

            await _mailService.SendEmailAsync(new MailRequest()
            {
                Subject = $"{result.User.UserName} has been fired from {result.Company.Name}",
                Body = emailBody,
                ToEmail = result.User.Email
            });

            var currentUserName = await GetUserName(_currentService.GetUserId());
            var log = new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = _currentService.GetUserId(),
                Title = "Fired Member From Company",
                Description = $"User:'{currentUserName}' deleted member with user id '{result.User.Id}' has left the company .",

            };

            await _notificationService.CreateNotificationAsync(new SendNotificationRequest
            {
                UserId = result.User.Id,
                Title = $"You have been fired from {result.Company.Name}",
                Body = $"You have been removed from the company due to: {reason}",
                LinkKey = "HOME_PAGE",
                IdLink = companyId,
                Event = "CompanyMemberFired",
                NotificationType = "COMPANY"
            }, token);

            return _mapper.Map<CompanyMemberResponse>(response);
        }

        public async Task<PagedResult<CompanyMemberResponse>> GetPagedCompanyMemberByCompanyIdAsync(
         Guid companyId, string mail, CompanyMemberPagedSearchRequest request, CancellationToken token = default)
        {
            var result = await _companyMemberRepository
                .GetPagedCompanyMemberByCompanyIdAsync(companyId, mail, request, token);

            var userIds = result.Items.Where(m => m.User != null).Select(m => m.User.Id).Distinct().ToList();
            var rolesByUser = await _companyMemberRepository
                .GetUserRolesMapInCompanyAsync(companyId, userIds, token);

            var memberResponses = new List<CompanyMemberResponse>(result.Items.Count);

            foreach (var member in result.Items)
            {
                var numberProjectJoin = await _projectMemberRepository
                    .GetTotalProjectsForMemberInCompanyAsync(member.User.Id, companyId, token);

                var dto = _mapper.Map<CompanyMemberResponse>(member);
                dto.NumberProductJoin = numberProjectJoin;

                //if (member.User != null && rolesByUser.TryGetValue(member.User.Id, out var role))
                //{
                //    dto.roleName = role.RoleName;
                //}

                if (member.User != null && rolesByUser.TryGetValue(member.User.Id, out var roleList))
                {
                    // Convert list role -> "A, B, C"
                    dto.roleName = string.Join(", ", roleList.Select(r => r.RoleName));
                }

                memberResponses.Add(dto);
            }

            return new PagedResult<CompanyMemberResponse>
            {
                Items = memberResponses,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }
        public async Task<PagedResult<CompanyMemberResponse>> GetPagedCompanyMemberAdminAsync(CompanyMemberPagedSearchAdminRequest request, string email, CancellationToken token = default)
        {
            var user = await _userRepository.GetUserByEmailAsync(email, token);

            if (user == null)
            {
                throw CustomExceptionFactory.CreateNotFoundError("User not found.");
            }

            if (user.IsSystemAdmin == false)
            {
                throw CustomExceptionFactory.CreateBadRequestError("User is not an admin.");
            }

            var result = await _companyMemberRepository.GetPagedCompanyMemberAdminAsync(request, token);

            var memberResponses = new List<CompanyMemberResponse>();
            foreach (var member in result.Items)
            {
                var numberProjectJoin = await _projectMemberRepository.GetTotalProjectsForMemberAsync(member.User.Id, token);

                var dto = _mapper.Map<CompanyMemberResponse>(member);
                dto.NumberProductJoin = numberProjectJoin;

                memberResponses.Add(dto);
            }

            return new PagedResult<CompanyMemberResponse>
            {
                Items = memberResponses,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }

        public async Task<CompanyMemberResponse?> InviteMemberToCompany(string inviterEmail, string inviteeMemberMail, Guid companyId, CancellationToken cancellationToken)
        {
            var result = await _companyMemberRepository.InviteMemberToCompany(inviterEmail, inviteeMemberMail, companyId, cancellationToken);

            var response = await _companyMemberRepository.GetCompanyMemberByIdAsync(result.Id);

            var owner_user = await _userRepository.GetUserByEmailAsync(inviterEmail);

            var inviteToken = result.Id.ToString();


            var inviteUrl = $"https://localhost:7160/api/companymember/accept?tokenConfirm={inviteToken}";
            var rejectUrl = $"https://localhost:7160/api/companymember/reject?tokenConfirm={inviteToken}";

            var emailBody = MailUtils.InviteMemberToCompany(
                                owner_user.UserName,
                                result.User.UserName,
                                result.Company.Name,
                                inviteUrl,
                                rejectUrl,
                                10);

            await _mailService.SendEmailAsync(new MailRequest()
            {
                Subject = $"Welcome {result.User.UserName} joined our {result.Company.Name}!",
                Body = emailBody,
                ToEmail = result.User.Email
            });

            string currentUserName = await GetUserName(_currentService.GetUserId());
            string userResult = await GetUserName(result.User.Id);
            var log = new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = _currentService.GetUserId(),
                Title = "Invite Member To Company",
                Description = $"User :'{currentUserName}' sent a company invitation to user: '{userResult}'.",

            };
            await _logService.CreateLog(log, cancellationToken);

            await _notificationService.CreateNotificationAsync(new SendNotificationRequest
            {
                UserId = result.User.Id,
                Title = $"You have been invited to join {result.Company.Name}",
                Body = $"User {currentUserName} has invited you to become a member of company {result.Company.Name}.",
                LinkKey = "COMPANY_DETAIL_PAGE",
                IdLink = companyId,
                Event = "CompanyMemberInvited",
                NotificationType = "COMPANY",
            }, cancellationToken);

            return _mapper.Map<CompanyMemberResponse>(response);

        }

        public async Task<CompanyMemberResponse?> AcceptJoinMemberToCompany(string tokenConfirm, CancellationToken cancellationToken = default)
        {
            //var cacheKey = $"company_invite:{tokenConfirm}";
            //var data = await _cacheService.GetAsync<InviteMemberRequest>(cacheKey, cancellationToken);

            var data = await _companyMemberRepository.GetCompanyMemberByIdAsync(long.Parse(tokenConfirm));

            if (data == null)
                throw CustomExceptionFactory.
                   CreateNotFoundError("Token is invalid");

            var result = await _companyMemberRepository.AcceptJoinMemberToCompany(data.UserId.Value, data.CompanyId.Value, cancellationToken);

            var userResult = await GetUserName(data.UserId.Value);
            var log = new CompanyActivityLog
            {
                CompanyId = data.CompanyId.Value,
                ActorUserId = data.UserId.Value,
                Title = "Accept Join Member To Company",
                Description = $"User '{userResult}' has agreed to join the company.",

            };
            await _logService.CreateLog(log, cancellationToken);

            await _notificationService.CreateNotificationAsync(new SendNotificationRequest
            {
                UserId = (Guid)data.Company.OwnerUserId,
                Title = $"{userResult} has joined your company",
                Body = $"{userResult} accepted your invitation and is now a company member.",
                LinkKey = "MEMBER_PAGE",
                IdLink = data.CompanyId.Value,
                Event = "CompanyMemberAccepted",
                NotificationType = "COMPANY"
            }, cancellationToken);
            return _mapper.Map<CompanyMemberResponse>(result);

        }

        public async Task<CompanyMemberResponse?> RejectJoinMemberToCompany(string tokenConfirm, CancellationToken cancellationToken = default)
        {
            //var cacheKey = $"company_invite:{tokenConfirm}";
            //var data = await _cacheService.GetAsync<InviteMemberRequest>(cacheKey, cancellationToken);

            var data = await _companyMemberRepository.GetCompanyMemberByIdAsync(long.Parse(tokenConfirm));

            if (data == null)
                throw CustomExceptionFactory.
                   CreateNotFoundError("Token is invalid");

            var result = await _companyMemberRepository.RejectJoinMemberToCompany(data.UserId.Value, data.CompanyId.Value, cancellationToken);

            var userResult = await GetUserName(data.UserId.Value);
            var log = new CompanyActivityLog
            {
                CompanyId = data.CompanyId.Value,
                ActorUserId = data.UserId.Value,
                Title = "Reject Join Member To Company",
                Description = $"User:'{userResult}' has refured to join the company.",

            };
            await _logService.CreateLog(log, cancellationToken);

            await _notificationService.CreateNotificationAsync(new SendNotificationRequest
            {
                UserId = (Guid)data.Company.OwnerUserId,
                Title = $"{userResult} has rejected the invite",
                Body = $"{userResult} refused your invitation to join the company.",
                LinkKey = "HOME_PAGE",
                IdLink = data.CompanyId.Value,
                Event = "CompanyMemberRejected",
                NotificationType = "COMPANY"
            }, cancellationToken);
            return _mapper.Map<CompanyMemberResponse>(result);
        }

        public async Task<CompanyMemberResponse?> RemoveMemberFromCompany(string terminatorEmail, Guid userId, Guid companyId, CancellationToken token = default)
        {
            var result = await _companyMemberRepository.RemoveMemberFromCompany(terminatorEmail, userId, companyId, token);

            var currentUserName = await GetUserName(_currentService.GetUserId());
            var userResult = await GetUserName(userId);
            var log = new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = _currentService.GetUserId(),
                Title = "Remove Member From Company",
                Description = $"User:'{currentUserName}' deleted member with user '{userResult}' has left the company.",

            };
            await _logService.CreateLog(log);

            await _notificationService.CreateNotificationAsync(new SendNotificationRequest
            {
                UserId = userId,
                Title = $"You have been removed from the company",
                Body = $"User {currentUserName} removed you from the company.",
                LinkKey = "HOME_PAGE",
                IdLink = companyId,
                Event = "CompanyMemberRemoved",
                NotificationType = "COMPANY"
            }, token);
            return _mapper.Map<CompanyMemberResponse>(result);
        }

        public async Task<List<CompanyMemberResponse>> GetMembersByStatus(Guid companyId, string status, CancellationToken token = default)
        {
            var members = await _companyMemberRepository.GetMembersByStatus(companyId, status, token);
            return _mapper.Map<List<CompanyMemberResponse>>(members);
        }

        public async Task<Dictionary<string, int>> GetSummaryStatusByCompanyId(Guid companyId, CancellationToken token = default)
        {
            return await _companyMemberRepository.GetSummaryStatusByCompanyId(companyId, token);
        }
        public async Task<AddMemberRoleInCompanyResponse?> RemoveRoleForMemberInCompany(Guid companyId, List<int> roleIds, Guid memberId, string removerEmail, CancellationToken token)
        {
            var removedRoles = await _companyMemberRepository.RemoveRoleForMemberInCompany(companyId, roleIds, memberId, removerEmail, token);

            if (!removedRoles.Any())
                throw CustomExceptionFactory.CreateBadRequestError("Role not found for user in company");

            var userWithRoles = await _userRepository.GetUserWithRolesAndPermissionsInCompanyAsync(memberId, companyId);

            if (userWithRoles == null)
                throw CustomExceptionFactory.CreateNotFoundError("User not found after removing roles");

            var response = new AddMemberRoleInCompanyResponse
            {
                UserId = userWithRoles.Id,
                UserName = userWithRoles.UserName ?? "",
                Roles = userWithRoles.UserRoles.Select(ur => new RoleResponse
                {
                    RoleId = ur.Role.Id,
                    RoleName = ur.Role.RoleName ?? "",
                    Permissions = ur.Role.RolePermissions.Select(rp => new PermissionResponse
                    {
                        FunctionCode = rp.Function.FunctionCode ?? "",
                        FunctionName = rp.Function.FunctionName ?? "",
                        PageCode = rp.Function.PageCode ?? "",
                        IsAccess = rp.IsAccess
                    }).ToList()
                }).ToList()
            };

            var currentUserName = await GetUserName(_currentService.GetUserId());
            var userResult = await GetUserName(memberId);

            var log = new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = _currentService.GetUserId(),
                Title = "Remove Role From Member In Company",
                Description = $"User '{currentUserName}' removed roles [{string.Join(",", removedRoles.Select(r => r.RoleId))}] from user '{userResult}'",
            };

            await _logService.CreateLog(log);

            return response;
        }

        public async Task<AddMemberRoleInCompanyResponse?> AddRoleForMemberInCompany(Guid companyId, List<int> roleIds, Guid memberId, string inviterEmail, CancellationToken token)
        {
            var addRole = await _companyMemberRepository.AddRoleForMemberInCompany(companyId, roleIds, memberId, inviterEmail, token);

            if (!addRole.Any())
                throw CustomExceptionFactory.CreateBadRequestError("Role is existed for user in company");

            var userWithRoles = await _userRepository.GetUserWithRolesAndPermissionsInCompanyAsync(memberId, companyId);

            if (userWithRoles == null)
                throw CustomExceptionFactory.CreateNotFoundError("User not found after adding roles");

            var response = new AddMemberRoleInCompanyResponse
            {
                UserId = userWithRoles.Id,
                UserName = userWithRoles.UserName ?? "",
                Roles = userWithRoles.UserRoles.Select(ur => new RoleResponse
                {
                    RoleId = ur.Role.Id,
                    RoleName = ur.Role.RoleName ?? "",
                    Permissions = ur.Role.RolePermissions.Select(rp => new PermissionResponse
                    {
                        FunctionCode = rp.Function.FunctionCode ?? "",
                        FunctionName = rp.Function.FunctionName ?? "",
                        PageCode = rp.Function.PageCode ?? "",
                        IsAccess = rp.IsAccess
                    }).ToList()
                }).ToList()
            };

            var currentUserName = await GetUserName(_currentService.GetUserId());
            var userResult = await GetUserName(memberId);
            var log = new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = _currentService.GetUserId(),
                Title = "Add Role For Member In Company",
                Description = $"User:'{currentUserName}'added role {response.Roles}to user  '{userResult}' has left the company.",

            };
            await _logService.CreateLog(log);

            return response;
        }

        public async Task<string?> GetUserName(Guid userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            return user.UserName;
        }

        public async Task<CompanyMemberResponse?> GetCompanyMemberByCompanyIdAndUserIdAsync(Guid companyId, Guid userId, CancellationToken token = default)
        {
            var member = await _companyMemberRepository
                .GetCompanyMemberByCompanyIdAndUserIdAsync(companyId, userId, token);

            if (member == null)
                return null;

            var dto = _mapper.Map<CompanyMemberResponse>(member);

            // Optional: tính số dự án user tham gia trong công ty (đồng bộ với API khác)
            var numberProjectJoin = await _projectMemberRepository.GetTotalProjectsForMemberInCompanyAsync(userId, companyId, token);
            dto.NumberProductJoin = numberProjectJoin;

            var performance = await _projectMemberRepository.GetMemberPerformanceAsync(userId, companyId, token);
            dto.Productivity = performance?.Productivity ?? 0;
            dto.Communication = performance?.Communication ?? 0;
            dto.Teamwork = performance?.Teamwork ?? 0;
            dto.ProblemSolving = performance?.ProblemSolving ?? 0;

            var stats = await _projectMemberRepository.GetMemberStatsAsync(userId, companyId, token);
            dto.Score = stats?.Score ?? 0;
            dto.HoursPerWeek = stats?.HoursPerWeek ?? 0;

            dto.Efficiency = stats?.Efficiency ?? new EfficiencyChart
            {
                OnTimePercent = 0,
                LatePercent = 0,
                PendingPercent = 0,
            };

            dto.ScoreTrendChart = stats?.ScoreTrendChart ?? new LineChart
            {
                Data = new List<ScoreTrend>()
            };

            dto.PriorityDistribution = stats?.PriorityDistribution ?? new PieChart
            {
                Segments = new List<PieChartSegment>()
            };

            return dto;
        }
        public async Task<PagedResult<CompanyMemberResponseV2>> GetCompanyMemberByUserIdAsync(
        Guid userId,
        CompanyMemberPagedRequest request,
        CancellationToken token = default)
        {
            var result = await _companyMemberRepository.GetCompanyMemberByUserIdAsync(userId, request, token);

            var responses = result.Items.Select(cm =>
            {
                var response = _mapper.Map<CompanyMemberResponseV2>(cm);

                response.Roles = cm.User?.UserRoles?
                    .Where(ur => ur.Role != null && ur.Role.CompanyId == cm.CompanyId)
                    .Select(ur => ur.Role.RoleName)
                    .ToList() ?? new List<string>();

                return response;
            }).ToList();


            return new PagedResult<CompanyMemberResponseV2>
            {
                Items = responses,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }
        public async Task<CompanyMemberResponse?> AcceptJoinMemberById(long memberId, CancellationToken token = default)
        {
            var member = await _companyMemberRepository.AcceptJoinMemberByIdAsync(memberId, token);

            var userName = await GetUserName(member.User.Id);

            // Ghi log
            await _logService.CreateLog(new CompanyActivityLog
            {
                CompanyId = member.CompanyId.Value,
                ActorUserId = _currentService.GetUserId(),
                Title = "Accept Join Member To Company",
                Description = $"User '{userName}' has been accepted to join the company."
            }, token);

            // Tạo notification cho owner
            await _notificationService.CreateNotificationAsync(new SendNotificationRequest
            {
                UserId = member.Company.OwnerUserId.Value,
                Title = $"{userName} has joined your company",
                Body = $"{userName} is now a member of your company.",
                LinkKey = "MEMBER_PAGE",
                IdLink = member.CompanyId.Value,
                Event = "CompanyMemberAccepted",
                NotificationType = "COMPANY"
            }, token);

            return _mapper.Map<CompanyMemberResponse>(member);
        }

        public async Task<CompanyMemberResponse?> RejectJoinMemberById(long memberId, CancellationToken token = default)
        {
            var member = await _companyMemberRepository.RejectJoinMemberByIdAsync(memberId, token);

            var userName = await GetUserName(member.User.Id);

            // Ghi log
            await _logService.CreateLog(new CompanyActivityLog
            {
                CompanyId = member.CompanyId.Value,
                ActorUserId = _currentService.GetUserId(),
                Title = "Reject Join Member To Company",
                Description = $"User '{userName}' has rejected the invitation to join the company."
            }, token);

            // Tạo notification cho owner
            await _notificationService.CreateNotificationAsync(new SendNotificationRequest
            {
                UserId = member.Company.OwnerUserId.Value,
                Title = $"{userName} has rejected the invite",
                Body = $"{userName} refused to join the company.",
                LinkKey = "HOME_PAGE",
                IdLink = member.CompanyId.Value,
                Event = "CompanyMemberRejected",
                NotificationType = "COMPANY"
            }, token);

            return _mapper.Map<CompanyMemberResponse>(member);
        }


    }
}
