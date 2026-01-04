using Fusion.Service.ViewModels.ProjectComponent;

namespace Fusion.Service.IServices
{
    public interface IProjectComponentService
    {
        Task<List<ProjectComponentResponse>> CreateManyAsync(
         List<CreateProjectComponent> requests,
         CancellationToken cancellationToken = default);

        Task<ProjectComponentResponse?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<List<ProjectComponentResponse>> GetByProjectIdAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);

        Task<List<ProjectComponentResponse>> GetByProjectRequestIdAsync(
            Guid projectRequestId,
            CancellationToken cancellationToken = default);

        Task<ProjectComponentResponse> UpdateAsync(
            UpdateProjectComponent request,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}
