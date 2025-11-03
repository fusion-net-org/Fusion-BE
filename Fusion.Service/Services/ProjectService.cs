using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Project;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Project.Requests;
using Fusion.Service.ViewModels.Project.Responses;
using Fusion.Service.ViewModels.ProjectMembers.Responses;


namespace Fusion.Service.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _repo;
        private readonly ICurrentService _currentService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public ProjectService(IProjectRepository repo, ICurrentService currentService, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _currentService = currentService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ProjectsResponse> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
        {
            var currentId = _currentService.GetUserId();
            if (currentId == Guid.Empty) throw CustomExceptionFactory.CreateUnauthorizedError();

            // Chuẩn hóa input rỗng/empty GUID -> null
            if (!request.CompanyHiredId.HasValue || request.CompanyHiredId == Guid.Empty) request.CompanyHiredId = null;
            if (!request.ProjectRequestId.HasValue || request.ProjectRequestId == Guid.Empty) request.ProjectRequestId = null;

            var company = await _unitOfWork.Repository<Company>().FindAsync(c => c.Id == request.CompanyId, cancellationToken);
            if (company == null) throw CustomExceptionFactory.CreateNotFoundError("CompanyId.");

            if (request.isHired)
            {
                // Bắt buộc đủ cả 2
                if (!request.CompanyHiredId.HasValue) throw CustomExceptionFactory.CreateBadRequestError("CompanyHiredId is required when isHired = true.");
                if (!request.ProjectRequestId.HasValue) throw CustomExceptionFactory.CreateBadRequestError("ProjectRequestId is required when isHired = true.");

                if (request.CompanyHiredId == request.CompanyId)
                    throw CustomExceptionFactory.CreateBadRequestError("CompanyHiredId must be different from CompanyId.");

                // Company thuê phải tồn tại
                var hiredExists = await _unitOfWork.Repository<Company>().FindAsync(x => x.Id == request.CompanyHiredId.Value, cancellationToken);
                if (hiredExists == null) throw CustomExceptionFactory.CreateNotFoundError("CompanyHiredId not found.");

                // ProjectRequest phải tồn tại & chưa được gắn vào Project nào khác
                var pr = await _unitOfWork.Repository<ProjectRequest>().FindAsync(x => x.Id == request.ProjectRequestId.Value, cancellationToken);
                if (pr == null) throw CustomExceptionFactory.CreateNotFoundError("ProjectRequestId not found.");

                var prInUse = await _unitOfWork.Repository<Project>().FindAsync(
                    x => x.ProjectRequestId == request.ProjectRequestId, cancellationToken);
                if (prInUse != null) throw CustomExceptionFactory.CreateBadRequestError("This ProjectRequest is already linked to another Project.");
            }
            else
            {

                request.CompanyHiredId = null;
                request.ProjectRequestId = null;
            }

            // Ràng buộc ngày (nếu có)
            if (request.StartDate.HasValue && request.EndDate.HasValue && request.EndDate < request.StartDate)
                throw CustomExceptionFactory.CreateBadRequestError("EndDate must be greater than or equal to StartDate.");

            var project = _mapper.Map<Project>(request);
            project.Status = "Active";

            var created = await _repo.CreateProjectAsync(currentId, project, cancellationToken);
            return _mapper.Map<ProjectsResponse>(created);
        }

        public async Task<PagedResult<ProjectListResponse>> GetAllProjectAsync(ProjectSearchRequest req, CancellationToken ct = default)
        {
            //var result = await _repo.GetAllProjectAsync(req, ct);
            //return result.Map(p => _mapper.Map<ProjectListResponse>(p));
            throw new NotImplementedException();
        }
        public Task<(int Todo, int Cancel, int Finish)> GetCountProjectByStatusAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
        public async Task<PagedResult<AllProjectOfMememberResponse>> GetProjectByMemberIdAsync(Guid userId, ProjectSearchRequest req, CancellationToken ct = default)
        {
            var projectsPaged = await _repo.GetProjectByMemberIdAsync(userId, req, ct);

            if (projectsPaged == null || !projectsPaged.Items.Any())
                throw CustomExceptionFactory.CreateNotFoundError("No projects found for this member in the specified company.");

            var items = projectsPaged.Items.Select(p =>
            {
                // Lấy membership hiện tại của user (repo đã Include ProjectMembers)
                var me = p.ProjectMembers?.FirstOrDefault(pm => pm.UserId == userId);


                // Nếu entity ProjectMember có IsViewAll -> lấy; nếu không bạn thay theo field thực tế
                var isViewAll = me?.IsViewAll ?? false;

                return new AllProjectOfMememberResponse
                {
                    Id = p.Id,
                    Name = p.Name ?? string.Empty,
                    Code = p.Code ?? string.Empty,
                    Status = p.Status ?? string.Empty,
                };
            }).ToList();

            return new PagedResult<AllProjectOfMememberResponse>
            {
                Items = items,
                TotalCount = projectsPaged.TotalCount,
                PageNumber = projectsPaged.PageNumber,
                PageSize = projectsPaged.PageSize
            };


        }
        public async Task<PagedResult<AllProjectOfMememberResponse>> GetProjectByActorIdAsync(Guid userId, ProjectSearchRequest req, CancellationToken ct = default)
        {
            var projectsPaged = await _repo.GetProjectByActorIdAsync(userId, req, ct);

            if (projectsPaged == null || !projectsPaged.Items.Any())
                throw CustomExceptionFactory.CreateNotFoundError("No projects found for this member in the specified company.");

            var items = projectsPaged.Items.Select(p =>
            {
                var me = p.ProjectMembers?.FirstOrDefault(pm => pm.UserId == userId);


                var isViewAll = me?.IsViewAll ?? false;

                return new AllProjectOfMememberResponse
                {
                    Id = p.Id,
                    Name = p.Name ?? string.Empty,
                    Code = p.Code ?? string.Empty,
                    Status = p.Status ?? string.Empty,
                };
            }).ToList();

            return new PagedResult<AllProjectOfMememberResponse>
            {
                Items = items,
                TotalCount = projectsPaged.TotalCount,
                PageNumber = projectsPaged.PageNumber,
                PageSize = projectsPaged.PageSize
            };


        }
        public Task<ProjectDetailResponse> GetProjectDetailAsync(Guid id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
