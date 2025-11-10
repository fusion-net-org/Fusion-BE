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
            // User send request - Nguoi di thue - Vendor
            var vendor = await _context.Users.SingleOrDefaultAsync(x => x.Email == vendorEmail, cancellationToken);
            if (vendor == null)
                throw CustomExceptionFactory.
                    CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Vendor"));

            var companyRequester = await _context.CompanyMembers.FirstOrDefaultAsync(v => v.UserId == vendor.Id && v.CompanyId == request.RequesterCompanyId, cancellationToken);

            if (companyRequester == null)
                throw CustomExceptionFactory.CreateBadRequestError("Vendor is not a member of the requested company");

            var companyExecutor = await _context.Companies.SingleOrDefaultAsync(e => e.Id == request.ExecutorCompanyId, cancellationToken);
            if (companyExecutor == null)
                throw CustomExceptionFactory.
                    CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Company Executor"));

            if (companyRequester.CompanyId == request.ExecutorCompanyId)
                throw CustomExceptionFactory.CreateBadRequestError("Executor company cannot be the same as requester company");

            var isFriendShip = await _context.CompanyFriendships.SingleOrDefaultAsync(v =>
                    ((v.CompanyAId == companyRequester.CompanyId && v.CompanyBId == request.ExecutorCompanyId) ||
                    (v.CompanyAId == request.ExecutorCompanyId && v.CompanyBId == companyRequester.CompanyId)) 
                    && v.Status == "Active",
                    cancellationToken);

            if (isFriendShip == null)
                throw CustomExceptionFactory.
                       CreateBadRequestError("Company friendship not found between requester and executor");

            if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate > request.EndDate)
                throw CustomExceptionFactory.CreateBadRequestError("StartDate must be before EndDate");


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
                throw CustomExceptionFactory.CreateBadRequestError("A pending request with the same project already exists");

            request.RequesterCompanyId = companyRequester.CompanyId;
            request.CreatedBy = vendor.Id;
            request.CreateAt = DateTime.UtcNow.AddHours(7);
            request.IsDeleted = false;
            request.ConvertedProjectId = null;
            request.Code = code;
            request.Status = "Pending";

            var projectRequest = await _context.ProjectRequests.AddAsync(request, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var result = await _context.ProjectRequests
                .Include(x => x.RequesterCompany)
                    .ThenInclude(rc => rc.OwnerUser)
                .Include(x => x.ExecutorCompany)
                    .ThenInclude(ec => ec.OwnerUser)
                .Include(x => x.CreatedByNavigation)
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x => x.Id == projectRequest.Entity.Id, cancellationToken);

            return result!;
        }


        public async Task<bool> DeleteProjectRequestAsync(Guid id,string reason, Guid currentUserId, CancellationToken cancellationToken = default)
        {
            var existingRequest = await _context.ProjectRequests
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (existingRequest == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                    "Project request not found");

            //// Rule: chỉ cho xóa khi Pending
            //if (existingRequest.Status != ProjectRequestStatusEnum.Pending.ToString())
            //    throw CustomExceptionFactory.CreateBadRequestError(
            //        "Invalid status",
            //        "Only pending requests can be deleted");

            existingRequest.IsDeleted = true;
            existingRequest.UpdateAt = DateTime.UtcNow.AddHours(7);
            existingRequest.DeletedBy = currentUserId;
            existingRequest.ReasonDelete = reason;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> RestoreProjectRequestAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default)
        {
            var existingRequest = await _context.ProjectRequests
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (existingRequest == null)
                throw CustomExceptionFactory.CreateNotFoundError("Project request not found");

            if (existingRequest.IsDeleted != true)
                throw CustomExceptionFactory.CreateBadRequestError("Project request is not deleted");

            if (existingRequest.DeletedBy != currentUserId)
                throw CustomExceptionFactory.CreateForbiddenError();

            // Restore
            existingRequest.IsDeleted = false;
            existingRequest.DeletedBy = null;
            existingRequest.UpdateAt = DateTime.UtcNow;
            existingRequest.ReasonDelete = null;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<ProjectRequest> UpdateProjectRequestAsync(Guid id, ProjectRequest request, string vendorEmail, CancellationToken cancellationToken)
        {
            // Lấy vendor theo email
            var vendor = await _context.Users.SingleOrDefaultAsync(x => x.Email == vendorEmail, cancellationToken);
            if (vendor == null)
                throw CustomExceptionFactory.CreateNotFoundError("Vendor not found");

            var companyRequester = await _context.CompanyMembers.SingleOrDefaultAsync(v => v.UserId == vendor.Id && v.CompanyId == request.RequesterCompanyId, cancellationToken);
            if (companyRequester == null)
                throw CustomExceptionFactory.CreateNotFoundError("Company Vendor not found");

            var existingRequest = await _context.ProjectRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (existingRequest == null)
                throw CustomExceptionFactory.CreateNotFoundError("Project request not found");

            // Rule: chỉ Requester mới được update
            if (existingRequest.RequesterCompanyId != companyRequester.CompanyId)
                throw CustomExceptionFactory.CreateBadRequestError("You are not allowed to update this project request");

            // Rule: chỉ được update khi đang Pending
            if (existingRequest.Status != ProjectRequestStatusEnum.Pending.ToString())
                throw CustomExceptionFactory.CreateBadRequestError(
                    ResponseMessages.INVALID_INPUT.FormatMessage("Invalid Status"),
                    "Only Pending requests can be updated");



            if (request.ExecutorCompanyId.HasValue && request.ExecutorCompanyId != existingRequest.ExecutorCompanyId)
            {
                if (existingRequest.Status == ProjectRequestStatusEnum.Rejected.ToString())
                {
                    throw CustomExceptionFactory.CreateBadRequestError("Cannot change executor because the request has been rejected");
                }

                // Check friendship khi đổi executor
                var isFriendShip = await _context.CompanyFriendships.SingleOrDefaultAsync(v =>
                    v.CompanyAId == companyRequester.CompanyId && v.CompanyBId == request.ExecutorCompanyId, cancellationToken);

                if (isFriendShip == null)
                    throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.NOT_FOUND.FormatMessage($"Relationship between Requester and Executor"));

                existingRequest.ExecutorCompanyId = request.ExecutorCompanyId;
            }

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

            // Cập nhật thời gian
            existingRequest.UpdateAt = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync(cancellationToken);

            // Trả ra kèm các thông tin liên quan
            var result = await _context.ProjectRequests
                .Include(x => x.RequesterCompany)
                    .ThenInclude(rc => rc.OwnerUser)
                .Include(x => x.ExecutorCompany)
                    .ThenInclude(ec =>ec.OwnerUser)
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
            if (filter.ViewMode.HasValue)
            {
                if (filter.ViewMode == ProjectRequestViewMode.AsRequester)
                    query = query.Where(x => x.RequesterCompanyId == userCompanyId);
                else if (filter.ViewMode == ProjectRequestViewMode.AsExecutor)
                    query = query.Where(x => x.ExecutorCompanyId == userCompanyId);
            }
            else
            {
                query = query.Where(x => x.RequesterCompanyId == userCompanyId);
            }

            // Keyword
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                query = query.Where(x => x.Name.Contains(filter.Keyword) || x.Code.Contains(filter.Keyword));

            // Status
            if (filter.Status.HasValue)
                query = query.Where(x => x.Status == filter.Status.Value.ToString());

            if (filter.DateFilterType.HasValue)
            {
                if (filter.DateRange?.From != null && filter.DateRange?.To != null)
                {
                    var from = filter.DateRange.From.Value.ToDateTime(TimeOnly.MinValue);
                    var to = filter.DateRange.To.Value.ToDateTime(TimeOnly.MaxValue);

                    query = filter.DateFilterType switch
                    {
                        DateFilterType.CreatedDate =>
                            query.Where(x =>
                                x.CreateAt >= from &&
                                x.CreateAt <= to
                            ),

                        DateFilterType.StartEndDate =>
                            query.Where(x =>
                                x.StartDate >= filter.DateRange.From &&
                                x.EndDate <= filter.DateRange.To),

                        DateFilterType.ApprovedDate =>
                            query.Where(x =>
                                x.UpdateAt != null &&
                                x.UpdateAt >= from &&
                                x.UpdateAt <= to &&
                                x.Status == ProjectRequestStatusEnum.Accepted.ToString()
                            ),

                        DateFilterType.RejectedDate =>
                            query.Where(x =>
                                x.UpdateAt != null &&
                                x.UpdateAt >= from &&
                                x.UpdateAt <= to &&
                                x.Status == ProjectRequestStatusEnum.Rejected.ToString()
                            ),
                        DateFilterType.PendingDate =>
                            query.Where(x =>
                                x.CreateAt != null &&
                                x.CreateAt >= from &&
                                x.CreateAt <= to &&
                                x.Status == ProjectRequestStatusEnum.Pending.ToString()
                            ),

                        _ => query
                    };
                }
            }
            else
            {
                if (filter.DateRange?.From != null || filter.DateRange?.To != null)
                    throw CustomExceptionFactory.CreateBadRequestError("Must choose DateFilterType when using date range filter.");
            }

            return await query.ToPagedResultAsync(
                    filter,
                    cancellationToken
                    );
        }

        public async Task<PagedResult<ProjectRequest>> SearchProjectRequestAsync(ProjectRequestSearchRequest filter, Guid userCompanyId, Guid partnerId, CancellationToken cancellationToken = default)
        {
            var query = _context.ProjectRequests
        .Include(x => x.RequesterCompany)
        .Include(x => x.ExecutorCompany)
        .Include(x => x.CreatedByNavigation)
        .Include(x => x.Project)
        .Where(x =>
            (x.RequesterCompanyId == userCompanyId && x.ExecutorCompanyId == partnerId) ||
            (x.RequesterCompanyId == partnerId && x.ExecutorCompanyId == userCompanyId))
        .AsQueryable();

            // ViewMode filter
            if (filter.ViewMode.HasValue)
            {
                if (filter.ViewMode == ProjectRequestViewMode.AsRequester)
                    query = query.Where(x => x.RequesterCompanyId == userCompanyId);
                else if (filter.ViewMode == ProjectRequestViewMode.AsExecutor)
                    query = query.Where(x => x.ExecutorCompanyId == userCompanyId);
            }

            // Keyword
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                query = query.Where(x => x.Name.Contains(filter.Keyword) || x.Code.Contains(filter.Keyword));

            // Status
            if (filter.Status.HasValue)
                query = query.Where(x => x.Status == filter.Status.Value.ToString());

            if (filter.DateFilterType.HasValue)
            {
                if (filter.DateRange?.From != null && filter.DateRange?.To != null)
                {
                    var from = filter.DateRange.From.Value.ToDateTime(TimeOnly.MinValue);
                    var to = filter.DateRange.To.Value.ToDateTime(TimeOnly.MaxValue);

                    query = filter.DateFilterType switch
                    {
                        DateFilterType.CreatedDate =>
                            query.Where(x =>
                                x.CreateAt >= from &&
                                x.CreateAt <= to
                            ),

                        DateFilterType.StartEndDate =>
                            query.Where(x =>
                                x.StartDate >= filter.DateRange.From &&
                                x.EndDate <= filter.DateRange.To),

                        DateFilterType.ApprovedDate =>
                            query.Where(x =>
                                x.UpdateAt != null &&
                                x.UpdateAt >= from &&
                                x.UpdateAt <= to &&
                                x.Status == ProjectRequestStatusEnum.Accepted.ToString()
                            ),

                        DateFilterType.RejectedDate =>
                            query.Where(x =>
                                x.UpdateAt != null &&
                                x.UpdateAt >= from &&
                                x.UpdateAt <= to &&
                                x.Status == ProjectRequestStatusEnum.Rejected.ToString()
                            ),
                        DateFilterType.PendingDate =>
                            query.Where(x =>
                                x.CreateAt != null &&
                                x.CreateAt >= from &&
                                x.CreateAt <= to &&
                                x.Status == ProjectRequestStatusEnum.Pending.ToString()
                            ),

                        _ => query
                    };

                }
            }
            else
            {
                if (filter.DateRange?.From != null || filter.DateRange?.To != null)
                    throw CustomExceptionFactory.CreateBadRequestError("Must choose DateFilterType when using date range filter.");
            }
            return await query.ToPagedResultAsync(filter, cancellationToken);

        }
            

        public async Task<ProjectRequest?> GetProjectRequestByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var projectRequest = await _context.ProjectRequests
                .Include(x => x.RequesterCompany)
                    .ThenInclude(rc =>rc.OwnerUser)
                .Include(x => x.ExecutorCompany)
                    .ThenInclude(ec => ec.OwnerUser)
                .Include(x => x.CreatedByNavigation)
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (projectRequest == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("ProjectRequest"));

            return projectRequest;
        }

        public async Task<ProjectRequest> AcceptProjectRequestAsync(Guid requestId, string executorEmail ,CancellationToken cancellationToken = default)
        {

            var request = await _context.ProjectRequests
                .Include(x => x.RequesterCompany)
                    .ThenInclude(rc => rc.OwnerUser)
                .Include(x => x.ExecutorCompany)
                    .ThenInclude(ec => ec.OwnerUser)
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
                     ResponseMessages.NOT_FOUND.FormatMessage("Executor"));

            var executorMember = await _context.CompanyMembers.SingleOrDefaultAsync(x => x.UserId == executor.Id && x.CompanyId == request.ExecutorCompanyId, cancellationToken); 
            if (executorMember == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                     ResponseMessages.NOT_FOUND.FormatMessage($"User in this {request.ExecutorCompany.Name}"));

            //Cập nhật trạng thái
            request.Status = ProjectRequestStatusEnum.Accepted.ToString();
            request.UpdateAt = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync(cancellationToken);

            return request;
        }

        public async Task<ProjectRequest> RejectProjectRequestAsync(Guid requestId, string executorEmail, string reason, CancellationToken cancellationToken = default)
        {
            var request = await _context.ProjectRequests
                .Include(x => x.RequesterCompany)
                    .ThenInclude(rc => rc.OwnerUser)
                .Include(x => x.ExecutorCompany)
                    .ThenInclude(ec => ec.OwnerUser)
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

            var executorMember = await _context.CompanyMembers
                .SingleOrDefaultAsync(x => x.UserId == executor.Id && x.CompanyId == request.ExecutorCompanyId, cancellationToken);

            if (executorMember == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                     ResponseMessages.NOT_FOUND.FormatMessage($"User not belong to {request.ExecutorCompany.Name}"));

            // ✅ Cập nhật trạng thái Reject
            request.Status = ProjectRequestStatusEnum.Rejected.ToString();
            request.UpdateAt = DateTime.UtcNow.AddHours(7);
            request.ReasonReject = reason;

            await _context.SaveChangesAsync(cancellationToken);

            return request;
        }
    }
}
