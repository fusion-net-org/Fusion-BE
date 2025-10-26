using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fusion.Service.Services
{
    // ViewModel & DTO dùng int cho RoleId
    public record RoleVm(int RoleId, string Name, string? Description);
    public record RoleIdsDto(List<int> RoleIds);

    public interface IMemberRoleService
    {
        Task<List<RoleVm>> GetAsync(Guid companyId, Guid userId, CancellationToken ct);
        Task AddAsync(Guid companyId, Guid userId, RoleIdsDto dto, CancellationToken ct);
        Task ReplaceAsync(Guid companyId, Guid userId, RoleIdsDto dto, CancellationToken ct);
        Task RemoveAsync(Guid companyId, Guid userId, int roleId, CancellationToken ct);
    }

    public interface IPermissionCache
    {
        Task InvalidateAsync(Guid userId, Guid companyId);
    }

    public class NoopPermissionCache : IPermissionCache
    {
        public Task InvalidateAsync(Guid userId, Guid companyId) => Task.CompletedTask;
    }

    public class MemberRoleService : IMemberRoleService
    {
        private readonly IUserRoleRepository _repo;
        private readonly IPermissionCache _cache;
        private readonly ICompanyActivityService _logService;
        private readonly ICurrentService _currentService;

        public MemberRoleService(IUserRoleRepository repo, IPermissionCache cache, ICompanyActivityService logService, ICurrentService currentService)
        {
            _repo = repo; _cache = cache;
            _logService = logService;
            _currentService = currentService;
        }

        public async Task<List<RoleVm>> GetAsync(Guid companyId, Guid userId, CancellationToken ct)
        {
            var roles = await _repo.GetRolesAsync(companyId, userId, ct);
            return roles.Select(r => new RoleVm(r.Id, r.RoleName, r.Description)).ToList();
        }

        public async Task AddAsync(Guid companyId, Guid userId, RoleIdsDto dto, CancellationToken ct)
        {
            if (!await _repo.IsMemberAsync(companyId, userId, ct))
                throw new InvalidOperationException("Member not found.");

            var ids = dto?.RoleIds ?? new List<int>();
            if (ids.Count == 0) return;

            await _repo.AddAsync(companyId, userId, ids, ct);

            var log = new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = _currentService.GetUserId(),
                Title = "Add role to member",
                Description = $"User id:'{_currentService.GetUserId()}' has added role id: {ids} for user: '{userId}'.",

            };
            await _logService.CreateLog(log);
            await _cache.InvalidateAsync(userId, companyId);
        }

        public async Task ReplaceAsync(Guid companyId, Guid userId, RoleIdsDto dto, CancellationToken ct)
        {
            if (!await _repo.IsMemberAsync(companyId, userId, ct))
                throw new InvalidOperationException("Member not found.");

            var ids = dto?.RoleIds ?? new List<int>();

            await _repo.ReplaceAsync(companyId, userId, ids, ct);
            var log = new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = _currentService.GetUserId(),
                Title = "Replace member roles",
                Description = $"User id:'{_currentService.GetUserId()}' has replaced roles to: {ids} for user: '{userId}'.",
            };
            await _logService.CreateLog(log);
            await _cache.InvalidateAsync(userId, companyId);
        }

        public async Task RemoveAsync(Guid companyId, Guid userId, int roleId, CancellationToken ct)
        {
            await _repo.RemoveAsync(companyId, userId, roleId, ct);
            var log = new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = _currentService.GetUserId(),
                Title = "Remove role from member",
                Description = $"User id:'{_currentService.GetUserId()}' has removed role id: {roleId} from user: '{userId}'.",
            };
            await _logService.CreateLog(log);
            await _cache.InvalidateAsync(userId, companyId);
        }
    }
}
