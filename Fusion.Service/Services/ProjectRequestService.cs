using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.ProjectRequest;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.Projects.Requests;
using Fusion.Service.ViewModels.Projects.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.Services
{
    public class ProjectRequestService : IProjectRequestService
    {
        private readonly IProjectRequestRepository _projectRequestRepository;
        private readonly IMapper _mapper;

        public ProjectRequestService(IProjectRequestRepository projectRequestRepository, IMapper mapper)
        {
            _projectRequestRepository = projectRequestRepository;
            _mapper = mapper;
        }

        public async Task<ProjectRequestResponse> AcceptProjectRequestAsync(Guid requestId, string executorEmail, CancellationToken cancellationToken = default)
        {
            var result = await _projectRequestRepository.AcceptProjectRequestAsync(requestId, executorEmail, cancellationToken);
            return _mapper.Map<ProjectRequestResponse>(result);
        }

        public async Task<ProjectRequestResponse> AddProjectRequestAsync(CreateProjectRequestRequest request, string vendorEmail, CancellationToken cancellationToken)
        {
            var projectRequest = _mapper.Map<ProjectRequest>(request);
            var code = ProjectCodeUtil.GenerateProjectRequestCode();
            var response = await _projectRequestRepository.AddProjectRequestAsync(projectRequest, vendorEmail, code, cancellationToken);
            return _mapper.Map<ProjectRequestResponse>(response);
        }

        public async Task<bool> DeleteProjectRequestAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _projectRequestRepository.DeleteProjectRequestAsync(id, cancellationToken);
            return result;
        }

        public async Task<ProjectRequestResponse?> GetProjectRequestByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _projectRequestRepository.GetProjectRequestByIdAsync(id, cancellationToken);
            return _mapper?.Map<ProjectRequestResponse?>(result);
        }

        public async Task<ProjectRequestRejectResponse> RejectProjectRequestAsync(Guid requestId, string executorEmail, string reason, CancellationToken cancellationToken = default)
        {
            var result = await _projectRequestRepository.RejectProjectRequestAsync(requestId, executorEmail, cancellationToken);

            if (!result)
                throw CustomExceptionFactory.CreateBadRequestError(
                    ResponseMessages.FAILED.FormatMessage("Reject project request failed"));

            return new ProjectRequestRejectResponse
            {
                Reason = reason,
                RejectedAt = DateTime.UtcNow.AddHours(7),
                RejectedBy = executorEmail,
                RequestId = requestId,
                Status = ProjectRequestStatusEnum.Rejected.ToString(),
            };
        }

        public async Task<PagedResult<ProjectRequestResponse>> SearchProjectRequestAsync(ProjectRequestSearchRequest filter, Guid userCompanyId, CancellationToken cancellationToken = default)
        {
            if (filter == null)
                throw CustomExceptionFactory.CreateBadRequestError(
                    ResponseMessages.INVALID_INPUT);

            var result = await _projectRequestRepository.SearchProjectRequestAsync(filter, userCompanyId, cancellationToken);

            if (result == null || result.Items.Count == 0)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Project Request"));

            var list = new PagedResult<ProjectRequestResponse>
            {
                Items = _mapper.Map<List<ProjectRequestResponse>>(result.Items),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
            return list;
        }

        public async Task<ProjectRequestResponse> UpdateProjectRequestAsync(Guid id, UpdateProjectRequestRequest request, string vendorEmail, CancellationToken cancellationToken = default)
        {
            var projectRequest = _mapper.Map<ProjectRequest>(request);
            var response = await _projectRequestRepository.UpdateProjectRequestAsync(id, projectRequest, vendorEmail, cancellationToken);
            return _mapper.Map<ProjectRequestResponse>(response);
        }
    }
}
