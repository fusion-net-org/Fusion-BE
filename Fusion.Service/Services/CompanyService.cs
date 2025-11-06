
using System.ComponentModel.Design;
using AutoMapper;
using FluentValidation;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Company;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Companies.Requests;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.Notifications.Requests;
using Fusion.Service.ViewModels.Project.Responses;
using Fusion.Service.ViewModels.Sprint.Responses;
using Fusion.Service.ViewModels.Task.Response;
using Fusion.Service.ViewModels.Users.Responses;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Fusion.Service.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IMapper _mapper;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyMemberRepository _companyMemberRepository;
        private readonly IValidator<CompanyRequest> _validator;
        private readonly ICompanyFriendshipRepository _companyFriendshipRepository;
        private readonly IMailService _mailService;
        private readonly ICompanyActivityService _logService;
        private readonly ICurrentService _currentService;
        private readonly INotificationService _notificationService;

        public CompanyService(IMapper mapper, ICompanyRepository companyRepository, ICloudinaryService cloudinaryService
            , IUserRepository userRepository, ICompanyMemberRepository companyMemberRepository, IValidator<CompanyRequest> validator, ICompanyFriendshipRepository companyFriendshipRepository,
            IMailService mailService, ICompanyActivityService logService, ICurrentService currentService, INotificationService notificationService)
        {
            _mapper = mapper;
            _companyRepository = companyRepository;
            _cloudinaryService = cloudinaryService;
            _userRepository = userRepository;
            _companyMemberRepository = companyMemberRepository;
            _validator = validator;
            _companyFriendshipRepository = companyFriendshipRepository;
            _mailService = mailService;
            _logService = logService;
            _currentService = currentService;
            _notificationService = notificationService;
        }

        public async Task<CompanyResponse> CreateCompanyAsync(CompanyRequest request, string Email, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw CustomExceptionFactory.CreateBadRequestError(
                    ResponseMessages.INVALID_INPUT);

            //true: bắt buộc phải có image_company
            await _validator.ValidateAndThrowAsync(
                request,
                opts => opts.IncludeRuleSets("Create"),
                cancellationToken
                );

            //check người đăng kí có tồn tại đăng kí hay không
            var user = await _userRepository.GetUserByEmailAsync(Email, cancellationToken);
            if (user == null)
                throw CustomExceptionFactory.
                    CreateBadRequestError(ResponseMessages.INVALID_INPUT.FormatMessage("Email incorrect!"));

            //check tax-code có tồn tại duy nhất hay không (trong hệ thống)
            var company_taxcode_existed = await _companyRepository.GetCompanyByTaxCode(request.TaxCode);
            if (company_taxcode_existed != null)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.EXISTED.FormatMessage("Tax-code"));

            var company_email_existed = await _companyRepository.GetCompanyByEmail(request.Email);
            if (company_email_existed != null)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.EXISTED.FormatMessage("Company Email"));

            var image_company = await _cloudinaryService.UploadImageAsync(request.ImageCompany, "CompanyBanner", cancellationToken);
            var avatar_company = await _cloudinaryService.UploadImageAsync(request.AvatarCompany, "CompanyAvatar", cancellationToken);

            var company = _mapper.Map<Company>(request);

            var newCompany = await _companyRepository.AddCompanyAsync(user, image_company, avatar_company, company, cancellationToken);

            if (newCompany == null)
                throw CustomExceptionFactory.CreateInternalServerError(ResponseMessages.INTERNAL_SERVER_ERROR.FormatMessage("Add Company fail"));

            await _notificationService.CreateNotificationAsync(new SendNotificationRequest
            {
                UserId = user.Id,
                Title = $"Company {newCompany.Name} has been successfully created!",
                Body = $"You have just created a new company with tax code {newCompany.TaxCode}. You can start managing it now.",
                LinkKey = "COMPANY_DETAIL_PAGE",
                IdLink = newCompany.Id,
                Event = "CompanyCreated",
                NotificationType = "COMPANY",
            }, cancellationToken);


            var newMember = await _companyMemberRepository.AddCompanyMemberAsync(new CompanyMember
            {
                CompanyId = newCompany.Id,
                UserId = user.Id,
                Status = "Active",
                IsDeleted = false,
                JoinedAt = DateTime.UtcNow.AddHours(7),
            }, cancellationToken);

            if (newMember == null)
                throw CustomExceptionFactory.CreateInternalServerError(ResponseMessages.INTERNAL_SERVER_ERROR.FormatMessage("Add new member fail"));


            var emailBody = MailUtils.CreateCompanyThankYouEmail(
                user.UserName, newCompany.Name, "http://localhost:5173/", "http://localhost:5173/company");

            await _mailService.SendEmailAsync(new ViewModels.Companies.Email.MailRequest()
            {
                Subject = $"Welcome {company.Name} to Fusion Platform - Manage, Collaborate, and Grow",
                Body = emailBody,
                ToEmail = user.Email
            });
            return _mapper.Map<CompanyResponse>(newCompany);

        }
        public async Task<PagedResult<CompanyResponse>> GetPagedCompaniesAsync(string userMail, CompanyPagedSearchRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw CustomExceptionFactory.CreateBadRequestError(
                    ResponseMessages.INVALID_INPUT);

            var result = await _companyRepository.GetPagedCompaniesAsync(userMail, request, cancellationToken);

            if (result == null || result.Items.Count == 0)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Companies"));

            var list = new PagedResult<CompanyResponse>
            {
                Items = _mapper.Map<List<CompanyResponse>>(result.Items),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };

            foreach (var item in list.Items)
            {
                var company = result.Items.FirstOrDefault(c => c.Id == item.Id);
                if (company == null) continue;

                var friendships = company.CompanyFriendshipCompanyAs
                    .Concat(company.CompanyFriendshipCompanyBs)
                    .ToList();

                item.TotalApproved = friendships.Count(f => f.Status == "Active");
                item.TotalWaitForApprove = friendships.Count(f => f.Status == "Pending");
                item.TotalPartners = friendships.Count();
            }
            return list;
        }

        public async Task<PagedResult<CompanyResponseVersion2>> GetAllCompaniesAsync(
                                                                                    string userMail,
                                                                                    CompanyPagedSearchRequestVersion2 request,
                                                                                    Guid? selectedCompanyId = null,
                                                                                    CancellationToken cancellationToken = default)
        {
            var currentUser = await _userRepository.GetUserByEmailAsync(userMail);
            if (currentUser == null)
                throw CustomExceptionFactory.CreateUnauthorizedError("Don't find information user!");

            var currentUserId = currentUser.Id;

            Guid? currentCompanyA = selectedCompanyId ?? await _companyRepository.GetCompanyIdByUserId(currentUserId);

            var partnerCompanyIds = new List<Guid>();

            var pendingPartnerIds = new List<Guid>();

            if (currentCompanyA != null)
            {
                var friendships = await _companyFriendshipRepository.GetCompanyFriendshipByCompanyID(currentUserId, currentCompanyA.Value);

                partnerCompanyIds = friendships
                     .Where(f => f.Status.ToLower() == "active")
                     .Select(f => f.CompanyAId == currentCompanyA ? f.CompanyBId : f.CompanyAId)
                     .Where(id => id.HasValue)
                     .Select(id => id.Value)
                     .Distinct()
                     .ToList();



                pendingPartnerIds = friendships
                    .Where(f => f.Status.ToLower() == "Pending".ToLower())
                    .Select(f => f.CompanyAId == currentCompanyA ? f.CompanyBId : f.CompanyAId)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .Distinct()
                    .ToList();

            }


            var result = await _companyRepository.GetAllCompaniesAsync(userMail, request, selectedCompanyId, cancellationToken);

            if (result == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Companies"));


            var list = new PagedResult<CompanyResponseVersion2>
            {

                Items = result.Items.Select(company =>
                {
                    var item = _mapper.Map<CompanyResponseVersion2>(company);

                    item.isOwner = company.OwnerUserId == currentUserId;
                    item.isPartner = currentCompanyA != null && partnerCompanyIds.Contains(company.Id);
                    item.isPendingAprovePartner = currentCompanyA != null && pendingPartnerIds.Contains(company.Id);

                    return item;
                }).ToList(),

                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };

            foreach (var item in list.Items)
            {
                var company = result.Items.FirstOrDefault(c => c.Id == item.Id);
                if (company == null) continue;

                var friendships = company.CompanyFriendshipCompanyAs
                    .Concat(company.CompanyFriendshipCompanyBs)
                    .ToList();

                item.TotalApproved = friendships.Count(f => f.Status == "Active");
                item.TotalWaitForApprove = friendships.Count(f => f.Status == "Pending");
                item.TotalPartners = friendships.Count();
            }

            return list;
        }

        public async Task<PagedResult<CompanyResponse>> GetPagedCompaniesAdminAsync(string adminEmail, CompanyPagedSearchRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw CustomExceptionFactory.CreateBadRequestError(
                    ResponseMessages.INVALID_INPUT);

            var result = await _companyRepository.GetPagedCompaniesAdminAsync(adminEmail, request, cancellationToken);

            if (result == null || result.Items.Count == 0)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Companies"));

            var list = new PagedResult<CompanyResponse>
            {
                Items = _mapper.Map<List<CompanyResponse>>(result.Items),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };

            foreach (var item in list.Items)
            {
                var company = result.Items.FirstOrDefault(c => c.Id == item.Id);
                if (company == null) continue;

                var friendships = company.CompanyFriendshipCompanyAs
                    .Concat(company.CompanyFriendshipCompanyBs)
                    .ToList();

                item.TotalApproved = friendships.Count(f => f.Status == "Active");
                item.TotalWaitForApprove = friendships.Count(f => f.Status == "Pending");
                item.TotalPartners = friendships.Count();
            }
            return list;
        }
        public async Task<Guid?> GetCompanyIdByUserId(Guid userId)
        {
            return await _companyRepository.GetCompanyIdByUserId(userId);
        }
        public async Task<CompanyResponse> GetCompanyByIdAsync(Guid companyId, CancellationToken cancellationToken = default)
        {
            var company = await _companyRepository.GetCompanyByIdAsync(companyId);


            var friendships = company.CompanyFriendshipCompanyAs
                .Concat(company.CompanyFriendshipCompanyBs)
                .ToList();

            var partners = new List<PartnerResponse>();
            var addedCompanyIds = new HashSet<Guid>();

            foreach (var friendship in friendships)
            {
                var partnerCompany = friendship.CompanyAId == companyId ? friendship.CompanyB : friendship.CompanyA;

                if (partnerCompany != null && partnerCompany.Id != companyId && addedCompanyIds.Add(partnerCompany.Id))
                {
                    partners.Add(new PartnerResponse
                    {
                        CompanyId = partnerCompany.Id,
                        Name = partnerCompany.Name,
                        OwnerUserName = partnerCompany.OwnerUser?.UserName,
                        TaxCode = partnerCompany.TaxCode,
                        RespondedAt = friendship.RespondedAt,
                        CreatedAt = friendship.CreatedAt,
                        TotalProject = partnerCompany.ProjectCompanies.Count + partnerCompany.ProjectCompanyHireds.Count,
                    });
                }
            }

            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Company"));

            var result = _mapper.Map<CompanyResponse>(company);

            result.TotalApproved = friendships.Count(f => f.Status == "Active");
            result.TotalWaitForApprove = friendships.Count(f => f.Status == "Pending");
            result.TotalPartners = friendships.Count();
            result.ListPartners = partners;

            return result;
        }
        public async Task<string> GetCompanyNameByGuid(Guid company)
        {
            return await _companyRepository.GetCompanyNameByGuid(company);
        }
        public async Task<CompanyResponse> UpdateCompanyAsync(Guid companyId, CompanyRequest request, string Email, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT);

            // false: không bắt buộc ImageCompany
            await _validator.ValidateAndThrowAsync(
                request,
                opts => opts.IncludeRuleSets("Update"),
                cancellationToken);

            #region Validate Business Company Update
            var user = await _userRepository.GetUserByEmailAsync(Email, cancellationToken);
            if (user == null)
                throw CustomExceptionFactory.
                    CreateBadRequestError(ResponseMessages.NOT_FOUND.FormatMessage("Email!"));

            var company = await _companyRepository.GetCompanyByIdAsync(companyId);
            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Company"));

            if (company.OwnerUserId != user.Id)
                throw CustomExceptionFactory
                    .CreateBadRequestError(ResponseMessages.BAD_REQUEST, $"Company is not belong to {company.OwnerUser.UserName}");

            if (!string.IsNullOrEmpty(request.Email) && request.Email != company.Email)
            {
                var company_email_existed = await _companyRepository.GetCompanyByEmail(request.Email);
                if (company_email_existed != null)
                    throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.EXISTED.FormatMessage("Company Email"));
            }

            if (!string.IsNullOrEmpty(request.TaxCode) && request.TaxCode != company.TaxCode)
            {
                var company_taxcode_existed = await _companyRepository.GetCompanyByTaxCode(request.TaxCode);
                if (company_taxcode_existed != null)
                    throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.DUPLICATE.FormatMessage("Tax-code"));
            }

            var image_company = "";
            var avatar_company = "";

            if (request.ImageCompany != null && request.ImageCompany.Length > 0)
            {
                await _cloudinaryService.DeleteImageAsync(_cloudinaryService.ExtractPublicIdFromUrl(company.ImageCompany), cancellationToken);
                image_company = await _cloudinaryService.UploadImageAsync(request.ImageCompany, "CompanyBanner", cancellationToken);
            }
            else
            {
                image_company = company.ImageCompany;
            }

            if (request.AvatarCompany != null && request.AvatarCompany.Length > 0)
            {
                await _cloudinaryService.DeleteImageAsync(_cloudinaryService.ExtractPublicIdFromUrl(company.AvatarCompany), cancellationToken);
                avatar_company = await _cloudinaryService.UploadImageAsync(request.AvatarCompany, "CompanyAvatar", cancellationToken);
            }
            else
            {
                avatar_company = company.AvatarCompany;
            }
            #endregion

            var result = await _companyRepository.UpdateCompanyAsync(image_company, avatar_company, companyId, _mapper.Map<Company>(request), cancellationToken);

            await _notificationService.CreateNotificationAsync(new SendNotificationRequest
            {
                UserId = user.Id,
                Title = $"Company {company.Name} has been updated successfully!",
                Body = $"You have successfully updated information for company {company.Name}.",
                LinkKey = "COMPANY_DETAIL_PAGE",
                IdLink = company.Id,
                Event = "CompanyUpdated",
                NotificationType = "COMPANY",
            }, cancellationToken);


            var log = new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = user.Id,
                Title = "Update Company Information",
                Description = $"Company '{company.Name}' information has been updated by user id:'{user.UserName}'.",

            };
            await _logService.CreateLog(log, cancellationToken);
            return _mapper.Map<CompanyResponse>(result);
        }
        public async Task<string> GetMailCompanyByGuid(Guid company)
        {
            return await _companyRepository.GetMailCompanyByGuid(company);
        }
        public async Task<bool> DeleteCompanyAsync(Guid companyId, string Email, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetUserByEmailAsync(Email, cancellationToken);
            if (user == null)
                throw CustomExceptionFactory.
                    CreateBadRequestError("Email incorrect!");

            var company = await _companyRepository.GetCompanyByIdAsync(companyId);
            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Company"));

            if (company.OwnerUserId != user.Id)
                throw CustomExceptionFactory
                    .CreateNotFoundError("Owner User in this company");

            await _companyRepository.DeleteCompanyAsync(company, cancellationToken);

            var log = new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = user.Id,
                Title = "Deleted Company",
                Description = $"Company '{company.Name}' has been deleted by user id:'{user.UserName}'.",

            };
            await _logService.CreateLog(log, cancellationToken);

            await _notificationService.CreateNotificationAsync(new SendNotificationRequest
            {
                UserId = user.Id,
                Title = $"Company {company.Name} has been deleted successfully!",
                Body = $"The company with tax code {company.TaxCode} has been removed from your account.",
                LinkKey = "COMPANY_LIST_PAGE",
                IdLink = company.Id,
                Event = "CompanyDeleted",
                NotificationType = "COMPANY",
            }, cancellationToken);

            return true;
        }
        public async Task<CompanySummaryResponse> GetCompanySummaryAsync(Guid companyId)
        {
            var rawProjects = await _companyRepository.GetCompanyProjectSummaryAsync(companyId);

            var projectSummary = new List<ProjectSummaryResponse>();

            foreach (var projObj in rawProjects)
            {
                var projType = projObj.GetType();
                var sprintsProp = projType.GetProperty("Sprints");

                if (sprintsProp == null)
                    continue;

                var sprints = sprintsProp.GetValue(projObj) as IEnumerable<object>;
                var projId = (Guid)(projType.GetProperty("ProjectId")?.GetValue(projObj) ?? Guid.Empty);
                var projName = projType.GetProperty("ProjectName")?.GetValue(projObj)?.ToString() ?? string.Empty;

                var sprintSummaryList = new List<SprintSummaryResponse>();

                foreach (var sprintObj in sprints)
                {
                    var sprintType = sprintObj.GetType();
                    var sprintId = (Guid)(sprintType.GetProperty("SprintId")?.GetValue(sprintObj) ?? Guid.Empty);
                    var sprintName = sprintType.GetProperty("SprintName")?.GetValue(sprintObj)?.ToString() ?? string.Empty;
                    var tasks = sprintType.GetProperty("ProjectTasks")?.GetValue(sprintObj) as IEnumerable<object>;

                    var taskSummaryList = new List<TaskSummaryResponse>();
                    if (tasks != null)
                    {
                        foreach (var taskObj in tasks)
                        {
                            var taskType = taskObj.GetType();
                            taskSummaryList.Add(new TaskSummaryResponse
                            {
                                Id = (Guid)(taskType.GetProperty("TaskId")?.GetValue(taskObj) ?? Guid.Empty),
                                Title = taskType.GetProperty("Title")?.GetValue(taskObj)?.ToString() ?? string.Empty,
                                Point = (int?)(taskType.GetProperty("Point")?.GetValue(taskObj) ?? 0),
                                Status = taskType.GetProperty("Status")?.GetValue(taskObj)?.ToString() ?? string.Empty,
                            });
                        }
                    }

                    sprintSummaryList.Add(new SprintSummaryResponse
                    {
                        Id = sprintId,
                        Name = sprintName,
                        TaskCount = taskSummaryList.Count,
                        TotalPoint = taskSummaryList.Sum(t => t.Point ?? 0),
                        Tasks = taskSummaryList
                    });
                }

                projectSummary.Add(new ProjectSummaryResponse
                {
                    Id = projId,
                    Name = projName,
                    SprintCount = sprintSummaryList.Count,
                    TotalTask = sprintSummaryList.Sum(s => s.TaskCount),
                    TotalPoint = sprintSummaryList.Sum(s => s.TotalPoint),
                    Sprints = sprintSummaryList
                });
            }

            return new CompanySummaryResponse
            {
                CompanyId = companyId,
                TotalProject = projectSummary.Count,
                TotalSprint = projectSummary.Sum(p => p.SprintCount),
                TotalTask = projectSummary.Sum(p => p.TotalTask),
                TotalPoint = projectSummary.Sum(p => p.TotalPoint),
                Projects = projectSummary
            };
        }
        public async Task<CompanyPerformanceResponse> GetCompanyPerformanceAsync(Guid companyId)
        {
            var rawData = await _companyRepository.GetCompanyUserTasksAsync(companyId);

            if (rawData == null || !rawData.Any())
                throw CustomExceptionFactory.CreateNotFoundError("No task data found for this company.");

            // Gom nhóm theo user để tính hiệu suất từng người
            var userPerformanceList = rawData
                .GroupBy(d =>
                {
                    dynamic item = d;
                    return new { item.UserId, item.UserName };
                })
                .Select(g =>
                {
                    int onTime = 0;
                    int late = 0;
                    int notCompleted = 0;

                    foreach (dynamic t in g)
                    {
                        string status = (string)(t.TaskStatus ?? "");
                        DateTime? due = (DateTime?)t.DueDate;
                        DateTime? updated = (DateTime?)t.UpdateAt;

                        if (status.Equals("Done", StringComparison.OrdinalIgnoreCase))
                        {
                            if (due.HasValue && updated.HasValue && updated <= due)
                                onTime++;
                            else
                                late++;
                        }
                        else
                        {
                            notCompleted++;
                        }
                    }

                    var totalCompleted = onTime + late;

                    return new UserPerformanceResponse
                    {
                        UserId = g.Key.UserId,
                        UserName = g.Key.UserName,
                        OnTimeCount = onTime,
                        LateCount = late,
                        NotCompletedCount = notCompleted,
                        OnTimePercent = totalCompleted > 0
                            ? Math.Round((double)onTime / totalCompleted * 100, 2)
                            : 0,
                        LatePercent = totalCompleted > 0
                            ? Math.Round((double)late / totalCompleted * 100, 2)
                            : 0
                    };
                })
                .ToList();

            // Trả kết quả tổng hợp toàn công ty
            return new CompanyPerformanceResponse
            {
                CompanyId = companyId,
                TotalMembers = userPerformanceList.Count,
                Data = userPerformanceList
            };
        }
        public async Task<bool> DeleteCompanyByAdminAsync(Guid companyId, CancellationToken cancellationToken = default)
        {

            var company = await _companyRepository.GetCompanyByIdAsync(companyId);
            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Company"));

            await _companyRepository.DeleteCompanyAsync(company, cancellationToken);

            var userId = _currentService.GetUserId();
            var log = new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = userId,
                Title = "Deleted Company",
                Description = $"Company '{company.Name}' has been deleted by admin.",

            };
            await _logService.CreateLog(log, cancellationToken);
            return true;
        }
        public async Task<CompanyResponse> UpdateCompanyByAdminAsync(Guid companyId, CompanyRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT);

            // false: không bắt buộc ImageCompany
            await _validator.ValidateAndThrowAsync(
                request,
                opts => opts.IncludeRuleSets("Update"),
                cancellationToken);


            var company = await _companyRepository.GetCompanyByIdAsync(companyId);
            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Company"));

            if (!string.IsNullOrEmpty(request.Email) && request.Email != company.Email)
            {
                var company_email_existed = await _companyRepository.GetCompanyByEmail(request.Email);
                if (company_email_existed != null)
                    throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.EXISTED.FormatMessage("Company Email"));
            }

            if (!string.IsNullOrEmpty(request.TaxCode) && request.TaxCode != company.TaxCode)
            {
                var company_taxcode_existed = await _companyRepository.GetCompanyByTaxCode(request.TaxCode);
                if (company_taxcode_existed != null)
                    throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.DUPLICATE.FormatMessage("Tax-code"));
            }

            var image_company = "";
            var avatar_company = "";

            if (request.ImageCompany != null && request.ImageCompany.Length > 0)
            {
                await _cloudinaryService.DeleteImageAsync(_cloudinaryService.ExtractPublicIdFromUrl(company.ImageCompany), cancellationToken);
                image_company = await _cloudinaryService.UploadImageAsync(request.ImageCompany, "CompanyBanner", cancellationToken);
            }
            else
            {
                image_company = company.ImageCompany;
            }

            if (request.AvatarCompany != null && request.AvatarCompany.Length > 0)
            {
                await _cloudinaryService.DeleteImageAsync(_cloudinaryService.ExtractPublicIdFromUrl(company.AvatarCompany), cancellationToken);
                avatar_company = await _cloudinaryService.UploadImageAsync(request.AvatarCompany, "CompanyAvatar", cancellationToken);
            }
            else
            {
                avatar_company = company.AvatarCompany;
            }

            var result = await _companyRepository.UpdateCompanyAsync(image_company, avatar_company, companyId, _mapper.Map<Company>(request), cancellationToken);

            var userId = _currentService.GetUserId();
            var log = new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = userId,
                Title = "Update Company Information",
                Description = $"Company '{company.Name}' information has been updated by admin.",

            };
            await _logService.CreateLog(log, cancellationToken);
            return _mapper.Map<CompanyResponse>(result);
        }
        public async Task<CompanyStatusCountsVm> GetCompanyStatusCountsAsync(CancellationToken cancellationToken = default)
        {
            var result = await _companyRepository.GetCompanyStatusCountsAsync(cancellationToken);

            return new CompanyStatusCountsVm
            {
                Active = result.Active,
                Inactive = result.Inactive,
                Total = result.Active + result.Inactive,
            };
        }
        public async Task<CompanyMonthlyStatsVm> GetCompaniesCreatedByMonthAsync(int year, CancellationToken ct = default)
        {
            if (year <= 0) year = DateTime.UtcNow.Year;

            var companies = await _companyRepository.GetCompaniesCreatedInYearAsync(year, ct);

            var counts = new int[12];
            foreach (var c in companies)
            {
                var month = c.CreateAt.Month;

                if (month is >= 1 and <= 12)
                    counts[month - 1]++;
            }

            return new CompanyMonthlyStatsVm
            {
                Year = year,
                MonthlyCounts = counts,
                Total = counts.Sum() 
            };
        }
        public async Task<PagedResult<CompanyOfOwnerResponse>> GetAllCompanyOfOwnerAsync(Guid userId, CancellationToken ct = default)
        {
            var result = await _companyRepository.GetAllCompanyOfOwnerAsync(userId, ct);

            if (result == null || result.Items.Count == 0)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Companies"));

            return new PagedResult<CompanyOfOwnerResponse>
            {
                Items = result.Items.Select(c => new CompanyOfOwnerResponse
                {
                    Id = c.Id,
                    Name = c.Name ?? string.Empty,
                    TaxCode = c.TaxCode ?? string.Empty,
                    CreateAt = c.CreateAt
                }).ToList(),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };

        }
        public async Task<PagedResult<CompanyOfUserResponse>> GetAllCompanyOfMemberAsync(Guid userId, CancellationToken ct = default)
        {
            var result = await _companyRepository.GetAllCompanyOfMemberAsync(userId, ct);

            if (result == null || result.Items.Count == 0)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Companies"));

            var items = result.Items.Select(c =>
            {
                var joinedAt = c.CompanyMembers?
                                   .FirstOrDefault(m => m.UserId == userId && m.IsDeleted != true)
                                   ?.JoinedAt
                               ?? c.CreateAt; 

                return new CompanyOfUserResponse
                {
                    Id = c.Id,
                    Name = c.Name ?? string.Empty,
                    TaxCode = c.TaxCode ?? string.Empty,
                    JoinAt = joinedAt
                };
            }).ToList();

            return new PagedResult<CompanyOfUserResponse>
            {
                Items = items,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }
    }
}