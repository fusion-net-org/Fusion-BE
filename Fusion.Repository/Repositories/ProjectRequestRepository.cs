using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.ProjectRequest;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


namespace Fusion.Repository.Repositories
{
    public class ProjectRequestRepository : GenericRepository<ProjectRequest>, IProjectRequestRepository
    {
        private readonly FusionDbContext _context;

        public ProjectRequestRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ProjectRequest> AddProjectRequestAsync(ProjectRequest request, string vendorEmail, string code, CancellationToken cancellationToken)
        {
            var vendor = await _context.Users.SingleOrDefaultAsync(x => x.Email == vendorEmail, cancellationToken);
            if (vendor == null)
                throw CustomExceptionFactory.
                    CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Vendor not found"));

            var companyRequester = await _context.CompanyMembers.FirstOrDefaultAsync(v => v.UserId == vendor.Id && v.CompanyId == request.RequesterCompanyId, cancellationToken);

            if (companyRequester == null)
                throw CustomExceptionFactory.CreateBadRequestError(
                    ResponseMessages.INVALID_INPUT.FormatMessage("Invalid requester company"),
                    "Vendor is not a member of the requested company");

            var companyExecutor = await _context.Companies.SingleOrDefaultAsync(e => e.Id == request.ExecutorCompanyId, cancellationToken);
            if (companyExecutor == null)
                throw CustomExceptionFactory.
                    CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Company Executor not found"));

            if (companyRequester.CompanyId == request.ExecutorCompanyId)
                throw CustomExceptionFactory.CreateBadRequestError(
                    ResponseMessages.INVALID_INPUT.FormatMessage("Invalid Company"), "Executor company cannot be the same as requester company");

            var isFriendShip = await _context.CompanyFriendships.SingleOrDefaultAsync(v =>
                    (v.CompanyAId == companyRequester.CompanyId && v.CompanyBId == request.ExecutorCompanyId) ||
                    (v.CompanyAId == request.ExecutorCompanyId && v.CompanyBId == companyRequester.CompanyId),
                    cancellationToken);

            if (isFriendShip == null)
                throw CustomExceptionFactory.
                       CreateBadRequestError(ResponseMessages.INVALID_INPUT.FormatMessage("Invalid Company"), "Company friendship not found between requester and executor");

            if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate > request.EndDate)
                throw CustomExceptionFactory.CreateBadRequestError(
                    ResponseMessages.INVALID_INPUT.FormatMessage("Invalid Date"), "StartDate must be before EndDate");


            /*Kiểm tra xem cái dự án tạo có trùng tên hay code hay ko*/
            var existingRequest = await _context.ProjectRequests.AnyAsync(x =>
                x.RequesterCompanyId == companyRequester.CompanyId &&
                x.ExecutorCompanyId == request.ExecutorCompanyId &&
                (
                    x.Name == request.Name ||
                    (!string.IsNullOrEmpty(request.Code) && x.Code == request.Code)
                ) &&
                x.Status == ProjectRequestStatusEnum.Pending.ToString(), cancellationToken);

            if (existingRequest)
                throw CustomExceptionFactory.CreateBadRequestError(
                    ResponseMessages.DUPLICATE.FormatMessage("A pending request with the same project already exists"));

            request.RequesterCompanyId = companyRequester.CompanyId;
            request.CreatedBy = vendor.Id;
            request.CreateAt = DateTime.UtcNow.AddHours(7);
            request.IsDeleted = false;
            request.ConvertedProjectId = null;
            request.Code = code;

            var projectRequest = await _context.ProjectRequests.AddAsync(request, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var result = await _context.ProjectRequests
                .Include(x => x.RequesterCompany)
                .Include(x => x.ExecutorCompany)
                .Include(x => x.CreatedByNavigation)
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x => x.Id == projectRequest.Entity.Id, cancellationToken);

            return result!;
        }


        public async Task<bool> DeleteProjectRequestAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var existingRequest = await _context.ProjectRequests
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (existingRequest == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Project request not found"));

            // Rule: chỉ cho xóa khi Pending
            if (existingRequest.Status != ProjectRequestStatusEnum.Pending.ToString())
                throw CustomExceptionFactory.CreateBadRequestError(
                    ResponseMessages.INVALID_INPUT.FormatMessage("Invalid status"),
                    "Only pending requests can be deleted");

            existingRequest.IsDeleted = true;
            existingRequest.UpdateAt = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }


        public async Task<ProjectRequest> UpdateProjectRequestAsync(Guid id, ProjectRequest request, string vendorEmail, CancellationToken cancellationToken)
        {
            // Lấy vendor theo email
            var vendor = await _context.Users.SingleOrDefaultAsync(x => x.Email == vendorEmail, cancellationToken);
            if (vendor == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Vendor not found"));

            var companyRequester = await _context.CompanyMembers.SingleOrDefaultAsync(v => v.UserId == vendor.Id, cancellationToken);
            if (companyRequester == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Company Vendor not found"));

            var existingRequest = await _context.ProjectRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (existingRequest == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Project request not found"));

            // Rule: chỉ Requester mới được update
            if (existingRequest.RequesterCompanyId != companyRequester.CompanyId)
                throw CustomExceptionFactory.CreateBadRequestError("You are not allowed to update this project request");

            //// Rule: chỉ được update khi đang Pending
            //if (existingRequest.Status != ProjectRequestStatusEnum.Pending.ToString())
            //    throw CustomExceptionFactory.CreateBadRequestError(
            //        ResponseMessages.INVALID_INPUT.FormatMessage("Invalid Status"),
            //        "Only Pending requests can be updated");

            // Update các field cho phép sửa
            if (!string.IsNullOrEmpty(request.Name))
                existingRequest.Name = request.Name;

            if (!string.IsNullOrEmpty(request.Code))
                existingRequest.Code = request.Code;

            if (!string.IsNullOrEmpty(request.Description))
                existingRequest.Description = request.Description;

            if (request.StartDate.HasValue)
                existingRequest.StartDate = request.StartDate;

            if (request.EndDate.HasValue)
                existingRequest.EndDate = request.EndDate;

            if (!string.IsNullOrEmpty(request.Status))
                existingRequest.Status = request.Status;

            if (request.ExecutorCompanyId.HasValue && request.ExecutorCompanyId != existingRequest.ExecutorCompanyId)
            {
                // Check friendship khi đổi executor
                var isFriendShip = await _context.CompanyFriendships.SingleOrDefaultAsync(v =>
                    v.CompanyAId == companyRequester.CompanyId && v.CompanyBId == request.ExecutorCompanyId, cancellationToken);

                if (isFriendShip == null)
                    throw CustomExceptionFactory.CreateBadRequestError("Invalid Company", "No friendship between requester and new executor");

                existingRequest.ExecutorCompanyId = request.ExecutorCompanyId;
            }

            // Cập nhật thời gian
            existingRequest.UpdateAt = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync(cancellationToken);

            // Trả ra kèm các thông tin liên quan
            var result = await _context.ProjectRequests
                .Include(x => x.RequesterCompany)
                .Include(x => x.ExecutorCompany)
                .Include(x => x.CreatedByNavigation)
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x => x.Id == existingRequest.Id, cancellationToken);

            return result!;
        }

        public async Task<PagedResult<ProjectRequest>> SearchProjectRequestAsync(ProjectRequestSearchRequest filter, Guid userCompanyId, CancellationToken cancellationToken = default)
        {
            var query = _context.ProjectRequests
                .Include(x => x.RequesterCompany)
                .Include(x => x.ExecutorCompany)
                .Include(x => x.CreatedByNavigation)
                .Include(x => x.Project)
                .AsQueryable();

            // ViewMode filter
            if (filter.ViewMode == ProjectRequestViewMode.AsRequester)
                query = query.Where(x => x.RequesterCompanyId == userCompanyId);
            else if (filter.ViewMode == ProjectRequestViewMode.AsExecutor)
                query = query.Where(x => x.ExecutorCompanyId == userCompanyId);

            // Keyword
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                query = query.Where(x => x.Name.Contains(filter.Keyword) || x.Code.Contains(filter.Keyword));

            // Status
            if (filter.Status.HasValue)
                query = query.Where(x => x.Status == filter.Status.Value.ToString());

            // StartDate
            if (filter.StartDate?.From != null)
                query = query.Where(x => x.StartDate >= filter.StartDate.From);
            if (filter.StartDate?.To != null)
                query = query.Where(x => x.StartDate <= filter.StartDate.To);

            // EndDate
            if (filter.EndDate?.From != null)
                query = query.Where(x => x.EndDate >= filter.EndDate.From);
            if (filter.EndDate?.To != null)
                query = query.Where(x => x.EndDate <= filter.EndDate.To);

            return await query.ToPagedResultAsync(
                new PagedRequest
                {
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    SortColumn = filter.SortField,
                    SortDescending = filter.SortDirection.ToLower() == "desc"
                },
                cancellationToken
            );
        }

        public async Task<ProjectRequest?> GetProjectRequestByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var projectRequest = await _context.ProjectRequests
                .Include(x => x.RequesterCompany)
                .Include(x => x.ExecutorCompany)
                .Include(x => x.CreatedByNavigation)
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted != true, cancellationToken);

            if (projectRequest == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("ProjectRequest not found"));

            return projectRequest;
        }

        public async Task<ProjectRequest> AcceptProjectRequestAsync(Guid requestId, string executorEmail ,CancellationToken cancellationToken = default)
        {

            var request = await _context.ProjectRequests
                .Include(x => x.ExecutorCompany)
                .Include(x => x.RequesterCompany)
                .Include(x => x.CreatedByNavigation)
                .Include(x => x.Project)
                .SingleOrDefaultAsync(x => x.Id == requestId && x.IsDeleted == false, cancellationToken);

            if (request == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Project request not found"));

            if (request.Status != ProjectRequestStatusEnum.Pending.ToString())
                throw CustomExceptionFactory.CreateBadRequestError("Request has already been processed");

            var executor = await _context.Users.SingleOrDefaultAsync(x => x.Email == executorEmail, cancellationToken);

            if (executor == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                     ResponseMessages.NOT_FOUND.FormatMessage("User not found"));

            var executorMember = await _context.CompanyMembers.SingleOrDefaultAsync(x => x.UserId == executor.Id && x.CompanyId == request.ExecutorCompanyId, cancellationToken); 
            if (executorMember == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                     ResponseMessages.NOT_FOUND.FormatMessage($"User not belong to {request.ExecutorCompany.Name}"));

            //Cập nhật trạng thái
            request.Status = ProjectRequestStatusEnum.Accepted.ToString();
            request.UpdateAt = DateTime.UtcNow.AddHours(7);

            var project = new Project
            {
                Id = Guid.NewGuid(),
                ProjectRequestId = request.Id,
                Name = request.Name,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CreateAt = DateTime.UtcNow.AddHours(7),
                CompanyHiredId = request.ExecutorCompanyId,
                CompanyId = request.RequesterCompanyId,
                IsHired = true,
                Status = "Open",
                CreatedBy = request.CreatedBy,
                Code = request.Code
            };

            await _context.Projects.AddAsync(project, cancellationToken);
            request.ConvertedProjectId = project.Id;

            await _context.SaveChangesAsync(cancellationToken);

            return request;
        }

        public async Task<bool> RejectProjectRequestAsync(Guid requestId, string executorEmail, CancellationToken cancellationToken = default)
        {
            var request = await _context.ProjectRequests
                .Include(x => x.ExecutorCompany)
                .Include(x => x.RequesterCompany)
                .SingleOrDefaultAsync(x => x.Id == requestId && x.IsDeleted == false, cancellationToken);

            if (request == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Project request not found"));

            if (request.Status != ProjectRequestStatusEnum.Pending.ToString())
                throw CustomExceptionFactory.CreateBadRequestError("Request has already been processed");

            var executor = await _context.Users.SingleOrDefaultAsync(x => x.Email == executorEmail, cancellationToken);
            if (executor == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                     ResponseMessages.NOT_FOUND.FormatMessage("User not found"));

            var executorMember = await _context.CompanyMembers
                .SingleOrDefaultAsync(x => x.UserId == executor.Id && x.CompanyId == request.ExecutorCompanyId, cancellationToken);

            if (executorMember == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                     ResponseMessages.NOT_FOUND.FormatMessage($"User not belong to {request.ExecutorCompany.Name}"));

            // ✅ Cập nhật trạng thái Reject
            request.Status = ProjectRequestStatusEnum.Rejected.ToString();
            request.UpdateAt = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
