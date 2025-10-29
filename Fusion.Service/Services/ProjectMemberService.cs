using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.ProjectMembers.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.Services
{
    public class ProjectMemberService: IProjectMemberService
    {
        public readonly IProjectMemberRepository _projectMemberRepository;

        public ProjectMemberService(IProjectMemberRepository projectMemberRepository)
        {
            _projectMemberRepository = projectMemberRepository;
        }

        public async Task<MemberProjectListResponse> GetProjectsByMemberAsync(Guid companyId, Guid userId)
        {
            var projects = await _projectMemberRepository.GetProjectsByMemberAsync(companyId, userId);

            if (projects == null || !projects.Any())
                throw CustomExceptionFactory.CreateNotFoundError("No projects found for this member in the specified company.");

            var projectResponse = projects.Select(p => new ProjectBelongToMemberResponse
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Status = p.Status,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                //Xác định công ty này là chủ hay là đối tác
                IsHired = p.IsHired,
            }).ToList();

            return new MemberProjectListResponse
            {
                CompanyId = companyId,
                UserId = userId,
                TotalProject = projectResponse.Count,
                Projects = projectResponse
            };
        }

    }
}
