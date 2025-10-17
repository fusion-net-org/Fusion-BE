using System.Threading;
using Azure.Core;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{
    public class CompanyFriendshipRepository : GenericRepository<CompanyFriendship>, ICompanyFriendshipRepository
    {
        private readonly FusionDbContext _context;
        public CompanyFriendshipRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<CompanyFriendship> AcceptCompanyFriendship(long id)
        {
            var friendship = await _context.CompanyFriendships.FindAsync(id);
            if (friendship == null)
                throw new CustomException(StatusCodes.Status404NotFound, "FRIENDSHIP_NOT_FOUND", "Not Found Friendship");

            friendship.Status = "Active";
            friendship.RespondedAt = DateTime.UtcNow.AddDays(7);
            friendship.UpdatedAt = DateTime.UtcNow.AddDays(7);

            _context.CompanyFriendships.Update(friendship);
            await _context.SaveChangesAsync();
            return friendship;
        }

        public async Task<CompanyFriendship> CancelCompanyFriendship(long id)
        {
            var friendship = await _context.CompanyFriendships.FindAsync(id);
            if (friendship == null)
                throw new CustomException(StatusCodes.Status404NotFound, "FRIENDSHIP_NOT_FOUND", "Not Found Friendship");

            friendship.Status = "Inactive";
            friendship.RespondedAt = DateTime.UtcNow.AddDays(7);
            friendship.UpdatedAt = DateTime.UtcNow.AddDays(7);

            _context.CompanyFriendships.Update(friendship);
            await _context.SaveChangesAsync();
            return friendship;
        }

        public async Task<PagedResult<CompanyFriendship>> GetCompanyFriendshipByOwnerUserID(Guid ownerUserID, PagedRequest request, CancellationToken cancellationToken = default)
        {
            var query = _dbSet              
                  .Include(cf => cf.CompanyB)
                      .ThenInclude(c => c.CompanyMembers)
                  .Include(cf => cf.CompanyB)
                      .ThenInclude(c => c.ProjectCompanies)
                  .Include(cf => cf.CompanyB)
                      .ThenInclude(c => c.ProjectCompanyHireds)
                  .Where(cf => cf.CompanyA.OwnerUserId == ownerUserID)
                  .AsQueryable();

            return await query.ToPagedResultAsync(request, cancellationToken);
        }


        public async Task<List<CompanyFriendship>> GetCompanyFriendshipByStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return new List<CompanyFriendship>();

            var normalized = status.Trim().ToLowerInvariant();

            if (normalized != "pending" && normalized != "active" && normalized != "inactive")
                throw new CustomException(StatusCodes.Status400BadRequest, "INVALID_STATUS", "Status must be 'Pending', 'Active', or 'Inactive'.");

            var listCompanyFriendship = await _context.CompanyFriendships
                .Where(x => !string.IsNullOrEmpty(x.Status) && x.Status.ToLower() == normalized)
                .ToListAsync();

            return listCompanyFriendship;
        }



        public async Task<CompanyFriendship> InviteCompanyFriendship(Guid companyAId, Guid companyBId, Guid requesterId)
        {
            var checkCompanyB = await _context.Companies.FirstOrDefaultAsync(x => x.Id == companyBId);

            if (checkCompanyB == null)
            {
                throw new CustomException(
                    statusCode: StatusCodes.Status404NotFound,
                    errorCode: "COMPANY_NOT_FOUND",
                    errorMessage: $"Company with id {companyBId} does not exist"
                );
            }

            // Check duplicate friendship
            var existingFriendship = await _context.CompanyFriendships.FirstOrDefaultAsync(x =>
                (x.CompanyAId == companyAId && x.CompanyBId == companyBId) ||
                (x.CompanyAId == companyBId && x.CompanyBId == companyAId));

            if (existingFriendship != null)
            {
                if (existingFriendship.Status == "Active")
                {
                    throw new CustomException(StatusCodes.Status400BadRequest,
                        "FRIENDSHIP_ALREADY_EXISTS",
                        "Both Company is partners.");
                }

                if (existingFriendship.Status == "Pending")
                {
                    throw new CustomException(StatusCodes.Status400BadRequest,
                        "FRIENDSHIP_PENDING",
                        "Friendship partner is pending handle.");
                }

                if (existingFriendship.Status == "Inactive")
                {
                    throw new CustomException(StatusCodes.Status400BadRequest,
                        "FRIENDSHIP_REJECTED",
                        "Friendship partner is reject before.");
                }
            }

            var friendship = new CompanyFriendship
            {
                CompanyAId = companyAId,
                CompanyBId = companyBId,
                RequesterId = requesterId,
                Status = "Pending",
                LastActionBy = requesterId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CompanyFriendships.Add(friendship);
            await _context.SaveChangesAsync();
            return friendship;
        }
        public async Task<object> GetCompanyFriendshipStatusSummary(Guid ownerUserId)
        {
            var query = _context.CompanyFriendships
                .Include(cf => cf.CompanyA)
                .Include(cf => cf.CompanyB)
                .Where(cf => cf.CompanyA.OwnerUserId == ownerUserId);

            var totalPending = await query.CountAsync(cf => cf.Status == "Pending".ToLower());
            var totalActive = await query.CountAsync(cf => cf.Status == "Active".ToLower());
            var totalInactive = await query.CountAsync(cf => cf.Status == "Inactive".ToLower());

            return new
            {
                Pending = totalPending,
                Active = totalActive,
                Inactive = totalInactive,
                Total = totalPending + totalActive + totalInactive
            };
        }

    }
}
