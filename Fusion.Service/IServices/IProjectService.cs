
using Fusion.Service.ViewModels.Project.Requests;
using Fusion.Service.ViewModels.Project.Responses;

namespace Fusion.Service.IServices
{
    public interface IProjectService 
    {
        Task<ProjectsResponse> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);
    }
}
