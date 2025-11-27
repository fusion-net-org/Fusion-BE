using System.ComponentModel.Design;
using Azure.Core;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Company;
using Fusion.Repository.Bases.Page.User;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Fusion.Repository.ViewModels.Companies;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace Fusion.Repository.Repositories
{
    public class CompanyRepository : GenericRepository<Company>, ICompanyRepository
    {
        private readonly FusionDbContext _context;

        public CompanyRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<PagedResult<Company>> GetAllCompaniesAsync(string userMail, CompanyPagedSearchRequestVersion2 request, Guid? selectedCompanyId, CancellationToken cancellationToken = default)
        {


            var query = _dbSet
                .Include(x => x.CompanyMembers)
                .Include(x => x.OwnerUser)
                .Include(x => x.ProjectCompanies)
                .Include(c => c.ProjectCompanyRequests)
                .Include(c => c.CompanyFriendshipCompanyAs)
                .Include(c => c.CompanyFriendshipCompanyBs)
                .Where(c =>
                            c.OwnerUser.Email == userMail ||
                            c.CompanyMembers.Any(m => m.User.Email == userMail))
                .AsQueryable();

            query = query.Where(c => (bool)!c.IsDeleted);


            if (request.RelationShipEnums.HasValue)
            {
                switch (request.RelationShipEnums.Value)
                {
                    case ProjectSearchRelationShipEnums.Owner:
                        // User là chủ sở hữu công ty
                        query = query.Where(c => c.OwnerUser.Email == userMail);
                        break;

                    case ProjectSearchRelationShipEnums.Member:
                        // User là chủ sở hữu hoặc thành viên công ty
                        query = query.Where(c =>
                            c.CompanyMembers.Any(m => m.User.Email == userMail));
                        break;
                }
            }
        
            // search
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim().ToLower();
                query = query.Where(u =>
                    (u.Name ?? string.Empty).ToLower().Contains(keyword) ||
                    (u.TaxCode ?? string.Empty).ToLower().Contains(keyword) ||
                    (u.PhoneNumber ?? string.Empty).ToLower().Contains(keyword) ||
                    (u.Email ?? string.Empty).ToLower().Contains(keyword) ||
                    (u.Website ?? string.Empty).ToLower().Contains(keyword) ||
                    (u.Address ?? string.Empty).ToLower().Contains(keyword)
                );
            }

            if (request.DayFrom.HasValue)
            {
                query = query.Where(c => c.CreateAt >= request.DayFrom.Value);
            }

            if (request.DayTo.HasValue)
            {
                query = query.Where(c => c.CreateAt <= request.DayTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.OwnerUserName))
            {
                query = query.Where(u => (u.OwnerUser.UserName ?? "").Contains(request.OwnerUserName));
            }


            return await query.ToPagedResultAsync(request, cancellationToken);
        }

        public async Task<PagedResult<Company>> GetAllCompaniesAsyncIncludingAllCompany(string userMail, CompanyPagedSearchRequestVersion2 request, Guid? selectedCompanyId, CancellationToken cancellationToken = default)
        {

            var query = _dbSet
                .Include(x => x.CompanyMembers)
                .Include(x => x.OwnerUser)
                .Include(x => x.ProjectCompanies)
                .Include(c => c.ProjectCompanyRequests)
                .Include(c => c.CompanyFriendshipCompanyAs)
                .Include(c => c.CompanyFriendshipCompanyBs)
                //.Where(c =>
                //            c.OwnerUser.Email == userMail ||
                //            c.CompanyMembers.Any(m => m.User.Email == userMail))
                .AsQueryable();

            query = query.Where(c => (bool)!c.IsDeleted);


            if (request.RelationShipEnums.HasValue)
            {
                switch (request.RelationShipEnums.Value)
                {
                    case ProjectSearchRelationShipEnums.Owner:
                        // User là chủ sở hữu công ty
                        query = query.Where(c => c.OwnerUser.Email == userMail);
                        break;

                    case ProjectSearchRelationShipEnums.Member:
                        // User là chủ sở hữu hoặc thành viên công ty
                        query = query.Where(c =>
                            c.CompanyMembers.Any(m => m.User.Email == userMail));
                        break;
                }
            }
        

            // search
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim().ToLower();
                query = query.Where(u =>
                    (u.Name ?? string.Empty).ToLower().Contains(keyword) ||
                    (u.TaxCode ?? string.Empty).ToLower().Contains(keyword) ||
                    (u.PhoneNumber ?? string.Empty).ToLower().Contains(keyword) ||
                    (u.Email ?? string.Empty).ToLower().Contains(keyword) ||
                    (u.Website ?? string.Empty).ToLower().Contains(keyword) ||
                    (u.Address ?? string.Empty).ToLower().Contains(keyword)
                );
            }

            if (request.DayFrom.HasValue)
            {
                query = query.Where(c => c.CreateAt >= request.DayFrom.Value);
            }

            if (request.DayTo.HasValue)
            {
                query = query.Where(c => c.CreateAt <= request.DayTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.OwnerUserName))
            {
                query = query.Where(u => (u.OwnerUser.UserName ?? "").Contains(request.OwnerUserName));
            }


            return await query.ToPagedResultAsync(request, cancellationToken);
        }
        public async Task<PagedResult<Company>> GetPagedCompaniesAsync(string userMail, CompanyPagedSearchRequest request, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Include(x => x.CompanyMembers)
                .Include(x => x.OwnerUser)
                .Include(x => x.ProjectCompanies)
                .Include(c => c.ProjectCompanyRequests)
                .Include(c => c.CompanyFriendshipCompanyAs)
                .Include(c => c.CompanyFriendshipCompanyBs)
                .Where(c =>
                            c.OwnerUser.Email == userMail ||
                            c.CompanyMembers.Any(m => m.User.Email == userMail))
                .AsQueryable();

            if (request.RelationShipEnums.HasValue)
            {
                switch (request.RelationShipEnums.Value)
                {
                    case ProjectSearchRelationShipEnums.Owner:
                        // User là chủ sở hữu công ty
                        query = query.Where(c => c.OwnerUser.Email == userMail);
                        break;

                    case ProjectSearchRelationShipEnums.Member:
                        // User là chủ sở hữu hoặc thành viên công ty
                        query = query.Where(c =>
                            c.CompanyMembers.Any(m => m.User.Email == userMail));
                        break;
                }
            }
            else
            {
                query = query.Where(c =>
                        c.OwnerUser.Email == userMail ||
                        c.CompanyMembers.Any(m => m.User.Email == userMail));
            }


            // search
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim().ToLower();
                query = query.Where(u =>
                    (u.Name ?? "").ToLower().Contains(keyword) ||
                    (u.TaxCode ?? "").ToLower().Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(request.OwnerUserName))
            {
                query = query.Where(u => (u.OwnerUser.UserName ?? "").Contains(request.OwnerUserName));
            }

            if (!string.IsNullOrWhiteSpace(request.Detail))
            {
                query = query.Where(u => (u.Detail ?? "").Contains(request.Detail));
            }

            if (request.TotalProject.HasValue)
            {
                if (request.SortDescending)
                {
                    query = query.Where(u => u.ProjectCompanies.Count + u.ProjectCompanyRequests.Count >= request.TotalProject);
                }
                else
                {
                    query = query.Where(u => u.ProjectCompanies.Count + u.ProjectCompanyRequests.Count <= request.TotalProject);

                }
            }

            if (request.TotalMember.HasValue)
            {
                if (request.SortDescending)
                {
                    query = query.Where(u => u.CompanyMembers.Count >= request.TotalMember);
                }
                else
                {
                    query = query.Where(u => u.CompanyMembers.Count <= request.TotalMember);

                }
            }

            return await query.ToPagedResultAsync(request, cancellationToken);

        }

        public async Task<PagedResult<Company>> GetPagedCompaniesAdminAsync(string adminEmail, CompanyPagedSearchRequest request, CancellationToken cancellationToken = default)
        {
            var isAdmin = _context.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .Any(u => u.Email == adminEmail &&
                            u.UserRoles.Any(ur => ur.Role.RoleName == "Admin"));
            if (!isAdmin)
                throw CustomExceptionFactory.CreateNotFoundError("Admin is not existed in company");

            var query = _dbSet
                .Include(x => x.CompanyMembers)
                .Include(x => x.OwnerUser)
                .Include(x => x.ProjectCompanies)
                .Include(c => c.ProjectCompanyRequests)
                .Include(c => c.CompanyFriendshipCompanyAs)
                .Include(c => c.CompanyFriendshipCompanyBs)

                .AsQueryable();

            // search
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim().ToLower();
                query = query.Where(u =>
                    (u.Name ?? "").ToLower().Contains(keyword) ||
                    (u.TaxCode ?? "").ToLower().Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(request.OwnerUserName))
            {
                query = query.Where(u => (u.OwnerUser.UserName ?? "").Contains(request.OwnerUserName));
            }

            if (!string.IsNullOrWhiteSpace(request.Detail))
            {
                query = query.Where(u => (u.Detail ?? "").Contains(request.Detail));
            }

            if (request.TotalProject.HasValue)
            {
                if (request.SortDescending)
                {
                    query = query.Where(u => u.ProjectCompanies.Count + u.ProjectCompanyRequests.Count >= request.TotalProject);
                }
                else
                {
                    query = query.Where(u => u.ProjectCompanies.Count + u.ProjectCompanyRequests.Count <= request.TotalProject);

                }
            }

            if (request.TotalMember.HasValue)
            {
                if (request.SortDescending)
                {
                    query = query.Where(u => u.CompanyMembers.Count >= request.TotalMember);
                }
                else
                {
                    query = query.Where(u => u.CompanyMembers.Count <= request.TotalMember);

                }
            }

            return await query.ToPagedResultAsync(request, cancellationToken);
        }

        public async Task<(int Active, int Inactive)> GetCompanyStatusCountsAsync(CancellationToken cancellationToken = default)
        {
            var row = await _context.Companies
                .AsNoTracking()
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    // null => coi như false (active)
                    Active = g.Count(x => !(x.IsDeleted ?? false)),
                    Inactive = g.Count(x => (x.IsDeleted ?? false))
                })
                .FirstOrDefaultAsync(cancellationToken);

            return (row?.Active ?? 0, row?.Inactive ?? 0);
        }

        public async Task<List<Company>> GetCompaniesCreatedInYearAsync(int year, CancellationToken ct = default)
        {
            var start = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddYears(1);

            // Nếu CreatedAt là DateTime?:
            return await _context.Companies
                .AsNoTracking()
                .Where(c => c.CreateAt != null &&
                            c.CreateAt >= start && c.CreateAt < end)
                .ToListAsync(ct);
        }
        public async Task<Company?> AddCompanyAsync(User user, string image_company, string avatar_company, Company new_company, CancellationToken cancellationToken)
        {
            new_company.ImageCompany = image_company;
            new_company.AvatarCompany = avatar_company;
            new_company.OwnerUserId = user.Id;
            new_company.IsDeleted = false;
            new_company.CreateAt = DateTime.UtcNow.AddHours(7);

            var company = await _context.Companies.AddAsync(new_company);
            await _context.SaveChangesAsync();
            return company.Entity;
        }

        public async Task<Company?> UpdateCompanyAsync(string image_company, string avatar_company, Guid companyId, Company update_company, CancellationToken cancellationToken = default)
        {
            var existed_company = await _context.Companies.FindAsync(companyId);

            existed_company.Name = update_company.Name ?? existed_company.Name;
            existed_company.TaxCode = update_company.TaxCode ?? existed_company.TaxCode;
            existed_company.Detail = update_company.Detail ?? existed_company.Detail;
            existed_company.Email = update_company.Email ?? existed_company.Email;
            existed_company.PhoneNumber = update_company.PhoneNumber ?? existed_company.PhoneNumber;
            existed_company.Address = update_company.Address ?? existed_company.Address;
            existed_company.Website = update_company.Website ?? existed_company.Website;
            existed_company.ImageCompany = image_company;
            existed_company.AvatarCompany = avatar_company;
            existed_company.UpdateAt = DateTime.UtcNow.AddHours(7);

            var company = _context.Companies.Update(existed_company);

            await _context.SaveChangesAsync(cancellationToken);
            return company.Entity;
        }

        public async Task<bool?> DeleteCompanyAsync(Company company, CancellationToken cancellationToken = default)
        {
            company.IsDeleted = true;
            _context.Companies.Update(company);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<Company?> GetCompanyByTaxCode(string taxcode)
        {
            var company = await _context.Companies
                .Include(x => x.CompanyMembers)
                .Include(x => x.OwnerUser)
                .Include(x => x.ProjectCompanies)
                .Include(c => c.ProjectCompanyRequests)
                .Include(c => c.CompanyFriendshipCompanyAs)
                .Include(c => c.CompanyFriendshipCompanyBs)
                .SingleOrDefaultAsync(x => x.TaxCode == taxcode);

            return company;
        }

        public async Task<Company?> GetCompanyByEmail(string email)
        {
            var company = await _context.Companies
                .Include(x => x.CompanyMembers)
                .Include(x => x.OwnerUser)
                .Include(x => x.ProjectCompanies)
                .Include(c => c.ProjectCompanyRequests)
                .Include(c => c.CompanyFriendshipCompanyAs)
                .Include(c => c.CompanyFriendshipCompanyBs)
                .SingleOrDefaultAsync(x => x.Email == email);

            return company;
        }

        public async Task<Guid?> GetCompanyIdByUserId(Guid userId)
        {
            var company = await _context.Companies
                .Include(x => x.CompanyMembers)
                .Include(x => x.OwnerUser)
                .Include(x => x.ProjectCompanies)
                .Include(c => c.ProjectCompanyRequests)
                .Include(c => c.CompanyFriendshipCompanyAs)
                .Include(c => c.CompanyFriendshipCompanyBs)
                .FirstOrDefaultAsync(x => x.OwnerUserId == userId);

            return company?.Id;
        }

        public async Task<string> GetCompanyNameByGuid(Guid company)
        {
            var companyName = await _context.Companies.FindAsync(company);
            return companyName?.Name;
        }

        public async Task<string> GetMailCompanyByGuid(Guid companyId)
        {
            var company = await _context.Companies.FindAsync(companyId);
            return company?.Email;
        }

        public async Task<Company?> GetCompanyByIdAsync(Guid Id)
        {
            return await _context.Companies
                    .Include(c => c.OwnerUser)
                    .Include(c => c.ProjectRequestRequesterCompanies)
                    .Include(c => c.ProjectRequestExecutorCompanies)
                    .Include(c => c.ProjectCompanies).ThenInclude(pc => pc.ProjectRequest).ThenInclude(pr => pr.Contract)
                    .Include(c => c.ProjectCompanyRequests).ThenInclude(pch => pch.ProjectRequest).ThenInclude(pr => pr.Contract)
                    .Include(c => c.CompanyMembers)
                    .Include(c => c.CompanyFriendshipCompanyAs)
            .ThenInclude(cf => cf.CompanyB)
                .ThenInclude(c => c.OwnerUser)
        .Include(c => c.CompanyFriendshipCompanyAs)
            .ThenInclude(cf => cf.CompanyB.ProjectCompanies)
        .Include(c => c.CompanyFriendshipCompanyAs)
            .ThenInclude(cf => cf.CompanyB.ProjectCompanyRequests)
        .Include(c => c.CompanyFriendshipCompanyBs)
            .ThenInclude(cf => cf.CompanyA)
                .ThenInclude(c => c.OwnerUser)
        .Include(c => c.CompanyFriendshipCompanyBs)
            .ThenInclude(cf => cf.CompanyA.ProjectCompanies)
        .Include(c => c.CompanyFriendshipCompanyBs)
            .ThenInclude(cf => cf.CompanyA.ProjectCompanyRequests)
        .FirstOrDefaultAsync(c => c.Id == Id);
        }

        public async Task<List<object>> GetCompanyProjectSummaryAsync(Guid companyId)
        {
            var projects = await _context.Projects
                .Include(x => x.Sprints)
                    .ThenInclude(x => x.ProjectTasks)
                .Where(p => p.CompanyId == companyId)
                .Select(p => new
                {
                    ProjectId = p.Id,
                    ProjectName = p.Name,
                    Sprints = p.Sprints.Select(s => new
                    {
                        SprintId = s.Id,
                        SprintName = s.Name,
                        ProjectTasks = s.ProjectTasks.Select(t => new
                        {
                            TaskId = t.Id,
                            Title = t.Title,
                            Point = t.Point
                        }).ToList()
                    }).ToList()
                })
                .ToListAsync();

            return projects.Cast<object>().ToList();
        }

        public async Task<List<object>> GetCompanyUserTasksAsync(Guid companyId)
        {
            var data = await _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.TaskWorkflows)
                    .ThenInclude(w => w.AssignUser)
                .Where(t => t.Project.CompanyId == companyId && !t.IsDeleted)
                .SelectMany(
                    t => t.TaskWorkflows.Select(w => new
                    {
                        UserId = w.AssignUserId,
                        UserName = w.AssignUser.UserName,
                        TaskStatus = t.Status,
                        DueDate = t.DueDate,
                        UpdateAt = t.UpdateAt
                    })
                )
                .ToListAsync();

            return data.Cast<object>().ToList();
        }

        public Task<int> GetAllCompanyAsync(CancellationToken cancellationToken = default)
        {
            return _context.Companies
                 .AsNoTracking()
                 .CountAsync(cancellationToken);
        }

        public async Task<PagedResult<Company>> GetAllCompanyOfOwnerAsync(Guid userId, CancellationToken ct = default)
        {
            var query = _context.Companies
         .AsNoTracking()
         .Where(c => c.OwnerUserId == userId);
            var req = new CompanyPagedSearchRequest
            {
                PageNumber = 1,
                PageSize = int.MaxValue,
                SortColumn = nameof(Company.Id),
                SortDescending = false
            };

            return await query.ToPagedResultAsync(req, ct);
        }
        public async Task<List<Company>> GetAllCompanyActiveOfCurrentIdAsync(Guid userId, CancellationToken ct = default)
        {
            var query = _context.Companies
                  .AsNoTracking()
                  .Where(c => c.IsDeleted != true &&
                   (
                       c.OwnerUserId == userId ||
                       c.CompanyMembers.Any(m =>
                           m.UserId == userId &&
                           m.IsDeleted != true)
                   )).ToList();

          

            return query;
        }
        public async Task<PagedResult<Company>> GetAllCompanyOfMemberAsync(Guid userId, CancellationToken ct = default)
        {
            var query = _context.Companies
          .AsNoTracking()
          .Where(c => c.CompanyMembers.Any(m => m.UserId == userId));

            var req = new CompanyPagedSearchRequest
            {
                PageNumber = 1,
                PageSize = int.MaxValue,
                SortColumn = nameof(Company.Id),
                SortDescending = false
            };

            return await query.ToPagedResultAsync(req, ct);
        }

        // =============== OverView =============================================
        public async Task<int> GetTotalCompaniesAsync(CancellationToken ct = default)
        {
            return await _context.Companies.CountAsync(c => c.IsDeleted == false || c.IsDeleted == null, ct);
        }
        public async Task<CompanyGrowthAndStatusOverviewDto> GetCompanyGrowthAndStatusOverviewAsync(DateTime fromUtc,DateTime toUtc,CancellationToken ct = default)
        {
            var nowUtc = DateTime.UtcNow;

            var baseQuery = _context.Companies.AsNoTracking();
            var activeQuery = baseQuery.Where(c => c.IsDeleted != true);

            var totalCompanies = await baseQuery.CountAsync(ct);
            var activeCompanies = await activeQuery.CountAsync(ct);
            var deletedCompanies = totalCompanies - activeCompanies;

            // ✅ fromUtc & toUtc đều là DateTime, so sánh OK
            var growthRaw = await activeQuery
                .Where(c => c.CreateAt >= fromUtc && c.CreateAt <= toUtc)
                .GroupBy(c => new { c.CreateAt.Year, c.CreateAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    NewCompanies = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync(ct);

            var growth = new List<CompanyGrowthPointDto>();
            var cumulative = 0;

            foreach (var item in growthRaw)
            {
                cumulative += item.NewCompanies;
                growth.Add(new CompanyGrowthPointDto
                {
                    Year = item.Year,
                    Month = item.Month,
                    NewCompanies = item.NewCompanies,
                    CumulativeCompanies = cumulative
                });
            }

            var thirtyDaysAgo = nowUtc.AddDays(-30);

            var newCompaniesLast30Days = await activeQuery
                .CountAsync(c => c.CreateAt >= thirtyDaysAgo, ct);

            return new CompanyGrowthAndStatusOverviewDto
            {
                TotalCompanies = totalCompanies,
                ActiveCompanies = activeCompanies,
                DeletedCompanies = deletedCompanies,
                NewCompaniesLast30Days = newCompaniesLast30Days,
                Growth = growth
            };
        }

        public async Task<CompanyProjectLoadOverviewDto> GetCompanyProjectLoadOverviewAsync( CancellationToken ct = default)
        {
            // Lấy toàn bộ company đang active
            var activeCompanyIds = await _context.Companies
                .AsNoTracking()
                .Where(c => c.IsDeleted != true)
                .Select(c => c.Id)
                .ToListAsync(ct);

            var totalCompanies = activeCompanyIds.Count;

            // Đếm số project theo company
            // TODO: nếu entity của bạn tên khác: Project / CompanyProject... thì chỉnh lại cho đúng
            var projectCounts = await _context.Projects
                .AsNoTracking()
                .GroupBy(p => p.CompanyId)            // nếu FK khác tên thì đổi ở đây
                .Select(g => new { CompanyId = g.Key, ProjectCount = g.Count() })
                .ToDictionaryAsync(x => x.CompanyId, x => x.ProjectCount, ct);

            // Helper xác định bucket
            static string GetBucketKey(int count) =>
                count switch
                {
                    0 => "0",
                    <= 2 => "1-2",
                    <= 5 => "3-5",
                    <= 10 => "6-10",
                    <= 20 => "11-20",
                    _ => "21+"
                };

            static string GetBucketLabel(string key) =>
                key switch
                {
                    "0" => "0 projects",
                    "1-2" => "1–2 projects",
                    "3-5" => "3–5 projects",
                    "6-10" => "6–10 projects",
                    "11-20" => "11–20 projects",
                    "21+ " => "21+ projects",
                    _ => key
                };

            var bucketOrder = new[] { "0", "1-2", "3-5", "6-10", "11-20", "21+" };
            var bucketDict = new Dictionary<string, CompanyProjectLoadBucketDto>();

            foreach (var companyId in activeCompanyIds)
            {
                projectCounts.TryGetValue(companyId, out var projectCount); // không có thì = 0
                var key = GetBucketKey(projectCount);

                if (!bucketDict.TryGetValue(key, out var bucket))
                {
                    bucket = new CompanyProjectLoadBucketDto
                    {
                        BucketKey = key,
                        Label = GetBucketLabel(key),
                        CompanyCount = 0,
                        TotalProjects = 0,
                    };
                    bucketDict[key] = bucket;
                }

                bucket.CompanyCount += 1;
                bucket.TotalProjects += projectCount;
            }

            var buckets = bucketDict.Values
                .OrderBy(b => Array.IndexOf(bucketOrder, b.BucketKey))
                .ToList();

            var totalProjects = buckets.Sum(b => b.TotalProjects);

            return new CompanyProjectLoadOverviewDto
            {
                TotalCompanies = totalCompanies,
                TotalProjects = totalProjects,
                Buckets = buckets
            };
        }
        public async Task<List<CompanyMonthlyNewPoint>> GetMonthlyNewCompaniesInYearAsync(int year,CancellationToken ct = default)
        {
            return await _context.Companies
                .AsNoTracking()
                .Where(c => c.CreateAt.Year == year)
                .GroupBy(c => new { c.CreateAt.Year, c.CreateAt.Month })
                .Select(g => new CompanyMonthlyNewPoint
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    NewCompanies = g.Count()
                })
                .ToListAsync(ct);
        }

        public async Task<Company?> GetCompanyByPhoneNumber(string phone)
        {
            var company = await _context.Companies
                           .Include(x => x.CompanyMembers)
                           .Include(x => x.OwnerUser)
                           .Include(x => x.ProjectCompanies)
                           .Include(c => c.ProjectCompanyRequests)
                           .Include(c => c.CompanyFriendshipCompanyAs)
                           .Include(c => c.CompanyFriendshipCompanyBs)
                           .SingleOrDefaultAsync(x => x.PhoneNumber == phone);

            return company;
        }
    }
}
