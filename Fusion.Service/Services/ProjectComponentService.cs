using AutoMapper;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.ProjectComponent;

namespace Fusion.Service.Services
{
    public class ProjectComponentService : IProjectComponentService
    {
        private readonly IProjectComponentRepository _repository;
        private readonly IMapper _mapper;
        private readonly ICurrentService _currentService;

        public ProjectComponentService(
            IProjectComponentRepository repository,
            IMapper mapper,
            ICurrentService currentService)
        {
            _repository = repository;
            _mapper = mapper;
            _currentService = currentService;
        }

        public async Task<List<ProjectComponentResponse>> CreateManyAsync(
          List<CreateProjectComponent> requests,
          CancellationToken cancellationToken = default)
        {
            if (requests == null || !requests.Any())
                return new List<ProjectComponentResponse>();

            var userId = _currentService.GetUserId();

            var entities = _mapper.Map<List<ProjectComponent>>(requests);

            foreach (var entity in entities)
            {
                entity.Id = Guid.NewGuid();
                entity.CreatedBy = userId;
                entity.CreatedAt = DateTime.UtcNow;
            }

            var results = await _repository.CreateManyAsync(entities, cancellationToken);
            return _mapper.Map<List<ProjectComponentResponse>>(results);
        }


        public async Task<ProjectComponentResponse?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            return entity == null ? null : _mapper.Map<ProjectComponentResponse>(entity);
        }

        public async Task<List<ProjectComponentResponse>> GetByProjectIdAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var list = await _repository.GetByProjectIdAsync(projectId, cancellationToken);
            return _mapper.Map<List<ProjectComponentResponse>>(list);
        }

        public async Task<List<ProjectComponentResponse>> GetByProjectRequestIdAsync(
            Guid projectRequestId,
            CancellationToken cancellationToken = default)
        {
            var list = await _repository.GetByProjectRequestIdAsync(projectRequestId, cancellationToken);
            return _mapper.Map<List<ProjectComponentResponse>>(list);
        }

        public async Task<ProjectComponentResponse> UpdateAsync(
            UpdateProjectComponent request,
            CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new Exception("ProjectComponent not found");

            _mapper.Map(request, entity);
            var updated = await _repository.UpdateAsync(entity, cancellationToken);

            return _mapper.Map<ProjectComponentResponse>(updated);
        }

        public async Task<bool> DeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _repository.DeleteAsync(id, cancellationToken);
        }
    }
}
