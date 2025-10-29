using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Project.Requests;
using Fusion.Service.ViewModels.Project.Responses;
using Fusion.Service.ViewModels.Projects.Responses;


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

    }
}
