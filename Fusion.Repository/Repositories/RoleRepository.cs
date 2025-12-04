using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Role;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        private readonly FusionDbContext _context;

        public RoleRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Role> CreateRoleAsync(Role role, CancellationToken ct)
        {
            var exists = await _context.Roles
             .AnyAsync(r => r.CompanyId == role.CompanyId
                   && r.RoleName == role.RoleName
                   && r.Status == "Active", ct);

            if (exists)
                throw CustomExceptionFactory.CreateBadRequestError("Role name already exists for this company.");


            role.CreatedAt = DateTime.UtcNow.AddHours(7);
            role.Status = "Active";

            await _context.Roles.AddAsync(role, ct);
            await _context.SaveChangesAsync(ct);

            return role;
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId, CancellationToken ct)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId, ct);
        }

        public async Task<PagedResult<Role>> GetRolesPagedAsync(RolePagedRequest request, CancellationToken ct = default)
        {
            var query = _context.Roles.AsQueryable();

            if (request.CompanyId.HasValue)
                query = query.Where(r => r.CompanyId == request.CompanyId);

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim().ToLower();

                query = query.Where(r =>
                    (r.RoleName ?? "").ToLower().Contains(keyword) ||
                    (r.Description ?? "").ToLower().Contains(keyword)
                );
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(r => r.Status == request.Status);


            if (request.CreatedAtFrom.HasValue && request.CreatedAtTo.HasValue)
            {
                var from = request.CreatedAtFrom.Value.Date;
                var to = request.CreatedAtTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(r => r.CreatedAt >= from && r.CreatedAt <= to);
            }
            else if (request.CreatedAtFrom.HasValue)
            {
                var from = request.CreatedAtFrom.Value.Date;
                query = query.Where(r => r.CreatedAt >= from);
            }
            else if (request.CreatedAtTo.HasValue)
            {
                var to = request.CreatedAtTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(r => r.CreatedAt <= to);
            }

            return await query.ToPagedResultAsync(request, ct);
        }

        public async Task<Role> UpdateRoleAsync(int roleId, Role updatedRole, CancellationToken ct)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId, ct);

            if (role == null)
                throw CustomExceptionFactory.CreateNotFoundError("Role not found.");

            var exists = await _context.Roles
                .AnyAsync(r => r.CompanyId == role.CompanyId
                   && r.RoleName == updatedRole.RoleName
                   && r.Id != roleId
                   && r.Status == "Active", ct);

            if (exists)
                throw CustomExceptionFactory.CreateBadRequestError("Role name already exists for this company.");


            role.RoleName = updatedRole.RoleName;
            role.Description = updatedRole.Description;
            role.UpdatedAt = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync(ct);

            return role;
        }

        public async Task<bool> DeleteRoleAsync(int roleId, string reason,CancellationToken ct)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId, ct);

            if (role == null)
                throw CustomExceptionFactory.CreateNotFoundError("Role not found.");

            if (role.Status == "Inactive")
                throw CustomExceptionFactory.CreateBadRequestError("Role is already inactive and cannot be deleted.");

            role.Status = "Inactive";
            role.Reason = reason;
            role.UpdatedAt = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync(ct);

            return true;
        }


    }

}
