using Azure.Core;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Partner;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using System.Threading;

namespace Fusion.Repository.Repositories
{
    public class CompanyFriendshipRepository : GenericRepository<CompanyFriendship>, ICompanyFriendshipRepository
    {
        private readonly FusionDbContext _context;
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserRepository _userRepository;
        public CompanyFriendshipRepository(FusionDbContext context, ICompanyRepository companyRepository, IUserRepository userRepository) : base(context)
        {
            _context = context;
            _companyRepository = companyRepository;
            _userRepository = userRepository;
        }

        public async Task<CompanyFriendship> AcceptCompanyFriendship(long id, Guid currentUserId)
        {
            var friendship = await _context.CompanyFriendships
                .Include(f => f.CompanyB)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (friendship == null)
                throw new CustomException(StatusCodes.Status404NotFound, "FRIENDSHIP_NOT_FOUND", "Not Found Friendship");

            if (friendship.CompanyB.OwnerUserId != currentUserId)
                throw new CustomException(StatusCodes.Status403Forbidden, "FORBIDDEN", "Only company B owner can approve this request.");

            friendship.Status = "Active";
            friendship.RespondedAt = DateTime.UtcNow.AddHours(7);
            friendship.UpdatedAt = DateTime.UtcNow.AddHours(7);

            _context.CompanyFriendships.Update(friendship);
            await _context.SaveChangesAsync();
            return friendship;
        }

        public async Task<CompanyFriendship> CancelCompanyFriendship(long id, Guid currentUserId)
        {
            var friendship = await _context.CompanyFriendships
                .Include(f => f.CompanyB)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (friendship == null)
                throw new CustomException(StatusCodes.Status404NotFound, "FRIENDSHIP_NOT_FOUND", "Not Found Friendship");

            if (friendship.CompanyB.OwnerUserId != currentUserId)
                throw new CustomException(StatusCodes.Status403Forbidden, "FORBIDDEN", "Only company B owner can cancel this request.");

            friendship.Status = "Inactive";
            friendship.RespondedAt = DateTime.UtcNow.AddHours(7);
            friendship.UpdatedAt = DateTime.UtcNow.AddHours(7);

            _context.CompanyFriendships.Update(friendship);
            await _context.SaveChangesAsync();
            return friendship;
        }

        public async Task<List<CompanyFriendship>> GetCompanyFriendshipByCompanyID(Guid userID, Guid companyID)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == companyID && c.OwnerUserId == userID);

            if (company == null)
            {
                throw new CustomException(
                    statusCode: StatusCodes.Status404NotFound,
                    errorCode: "COMPANY_NOT_FOUND",
                    errorMessage: $"Company with ID {companyID} does not exist."
                );
            }

            var query = _context.CompanyFriendships
               .Where(cf => cf.CompanyAId == companyID || cf.CompanyBId == companyID &&
              (cf.Status.ToLower() == "active" || cf.Status.ToLower() == "pending"));
            return query.ToList();
        }
        public async Task<PagedResult<CompanyFriendship>> GetCompanyFriendshipByCompanyIDVersion2(Guid userID,Guid companyID,CompanyFriendshipSearchRequest request,CancellationToken cancellationToken = default)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == companyID && c.OwnerUserId == userID);

            if (company == null)
            {
                throw new CustomException(
                    statusCode: StatusCodes.Status404NotFound,
                    errorCode: "COMPANY_NOT_FOUND",
                    errorMessage: $"Company with ID {companyID} does not exist or does not belong to this user."
                );
            }

            var query = _context.CompanyFriendships
                .Include(cf => cf.CompanyA)
                    .ThenInclude(c => c.OwnerUser)
                .Include(cf => cf.CompanyB)
                    .ThenInclude(c => c.OwnerUser)
                .Include(cf => cf.CompanyA)
                    .ThenInclude(c => c.CompanyMembers)
                .Include(cf => cf.CompanyB)
                    .ThenInclude(c => c.CompanyMembers)
                .Include(cf => cf.CompanyA)
                    .ThenInclude(c => c.ProjectCompanies)
                .Include(cf => cf.CompanyB)
                    .ThenInclude(c => c.ProjectCompanies)
                .Include(cf => cf.CompanyA)
                    .ThenInclude(c => c.ProjectCompanyHireds)
                .Include(cf => cf.CompanyB)
                    .ThenInclude(c => c.ProjectCompanyHireds)
                .Where(cf =>
                    (cf.CompanyAId == companyID || cf.CompanyBId == companyID) &&
                    (cf.Status.ToLower() == "active" || cf.Status.ToLower() == "pending"))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim().ToLower();

                query = query.Where(u =>
                    (u.Status ?? "").ToLower().Contains(keyword)
                    || (u.CompanyA.Name ?? "").ToLower().Contains(keyword)
                    || (u.CompanyB.Name ?? "").ToLower().Contains(keyword)
                    || (u.CompanyA.OwnerUser.UserName ?? "").ToLower().Contains(keyword)
                    || (u.CompanyB.OwnerUser.UserName ?? "").ToLower().Contains(keyword)
                    || (u.CompanyA.TaxCode ?? "").ToLower().Contains(keyword)
                    || (u.CompanyB.TaxCode ?? "").ToLower().Contains(keyword)
                );
            }

            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                var from = request.FromDate.Value.Date;
                var to = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);

                query = query.Where(x => x.CreatedAt >= from && x.CreatedAt <= to);
            }
            else if (request.FromDate.HasValue)
            {
                var from = request.FromDate.Value.Date;
                query = query.Where(x => x.CreatedAt >= from);
            }
            else if (request.ToDate.HasValue)
            {
                var to = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.CreatedAt <= to);
            }

            return await query.ToPagedResultAsync(request, cancellationToken);
        }

        public async Task<PagedResult<CompanyFriendship>> GetCompanyFriendshipByOwnerUserID(Guid ownerUserID, CompanyFriendshipSearchRequest request, CancellationToken cancellationToken = default)
        {
            var query = _context.CompanyFriendships
               .Include(cf => cf.CompanyB)
                   .ThenInclude(c => c.CompanyMembers)
               .Include(cf => cf.CompanyB)
                   .ThenInclude(c => c.ProjectCompanies)
               .Include(cf => cf.CompanyB)
                   .ThenInclude(c => c.ProjectCompanyHireds)
               .Include(cf => cf.CompanyB)
                   .ThenInclude(c => c.OwnerUser)
               .Where(cf => cf.CompanyA.OwnerUserId == ownerUserID || cf.CompanyB.OwnerUserId == ownerUserID)
               .AsQueryable();

            // search
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim().ToLower();

                query = query.Where(u =>
                    (u.Status ?? "").ToLower().Contains(keyword)
                    || (u.CompanyB.Name ?? "").ToLower().Contains(keyword)
                    || (u.CompanyB.OwnerUser.UserName ?? "").ToLower().Contains(keyword)
                    || (u.CompanyB.TaxCode ?? "").ToLower().Contains(keyword)

                
                    );
            }
            // filter following fromdate todate
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                var from = request.FromDate.Value.Date;
                var to = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);

                query = query.Where(x => x.CreatedAt >= from && x.CreatedAt <= to);
            }
            else if (request.FromDate.HasValue)
            {
                var from = request.FromDate.Value.Date;
                query = query.Where(x => x.CreatedAt >= from);
            }
            else if (request.ToDate.HasValue)
            {
                var to = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.CreatedAt <= to);
            }

            return await query.ToPagedResultAsync(request, cancellationToken);
        }


        public async Task<PagedResult<CompanyFriendship>> GetCompanyFriendshipByStatus(
             Guid ownerUserID,
             Guid companyID,
             string status,
             PagedRequest request,
             CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new CustomException(StatusCodes.Status400BadRequest, "INVALID_STATUS", "Status cannot be empty.");

            var normalized = status.Trim().ToLowerInvariant();

            if (normalized != "pending" && normalized != "active" && normalized != "inactive")
                throw new CustomException(StatusCodes.Status400BadRequest, "INVALID_STATUS", "Status must be 'Pending', 'Active', or 'Inactive'.");

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == companyID && c.OwnerUserId == ownerUserID, cancellationToken);

            if (company == null)
            {
                throw new CustomException(
                    StatusCodes.Status403Forbidden,
                    "FORBIDDEN_COMPANY",
                    "You do not have permission to view this company's friendships."
                );
            }

            var query = _context.CompanyFriendships
                .Include(cf => cf.CompanyA)
                    .ThenInclude(c => c.OwnerUser)
                .Include(cf => cf.CompanyB)
                    .ThenInclude(c => c.OwnerUser)
                .Include(cf => cf.CompanyB)
                    .ThenInclude(c => c.CompanyMembers)
                .Include(cf => cf.CompanyB)
                    .ThenInclude(c => c.ProjectCompanies)
                .Include(cf => cf.CompanyB)
                    .ThenInclude(c => c.ProjectCompanyHireds)
                .Where(cf =>
                    (cf.CompanyAId == companyID || cf.CompanyBId == companyID) &&
                    (cf.CompanyA.OwnerUserId == ownerUserID || cf.CompanyB.OwnerUserId == ownerUserID))
                .AsQueryable();

            query = query.Where(x => !string.IsNullOrEmpty(x.Status) && x.Status.ToLower() == normalized);

            return await query.ToPagedResultAsync(request, cancellationToken);
        }




        public async Task<CompanyFriendship> InviteCompanyFriendship(Guid companyAId, Guid companyBId, Guid requesterId,string? note)
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

            if (note == "string" || string.IsNullOrWhiteSpace(note))
            {
                note = null;
            }

            var checkCompanyA = await _context.Companies
                  .FirstOrDefaultAsync(c =>
                      c.Id == companyAId &&
                      c.OwnerUserId == requesterId); 

            if (checkCompanyA == null)
            {
                throw new CustomException(
                    statusCode: StatusCodes.Status403Forbidden,
                    errorCode: "UNAUTHORIZED_COMPANY_ACCESS",
                    errorMessage: $"Company request Does not exist or requester does not have permission to invite from this company."
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
                Note = note,
                LastActionBy = requesterId,
                CreatedAt = DateTime.UtcNow.AddHours(7),
                UpdatedAt = DateTime.UtcNow.AddHours(7)
            };

            _context.CompanyFriendships.Add(friendship);
            await _context.SaveChangesAsync();
            return friendship;
        }
        public async Task<object> GetCompanyFriendshipStatusSummary(Guid ownerUserId, Guid? companyId = null)
        {
            var query = _context.CompanyFriendships
                .Include(cf => cf.CompanyA)
                .Include(cf => cf.CompanyB)
                .AsQueryable();
            if (companyId.HasValue && companyId.Value != Guid.Empty)
            {
                query = query.Where(cf =>
                    cf.CompanyAId == companyId.Value || cf.CompanyBId == companyId.Value);
            }
            else
            {
                query = query.Where(cf =>
                    cf.CompanyA.OwnerUserId == ownerUserId || cf.CompanyB.OwnerUserId == ownerUserId);
            }

            var totalPending = await query.CountAsync(cf => cf.Status.ToLower() == "pending");
            var totalActive = await query.CountAsync(cf => cf.Status.ToLower() == "active");
            var totalInactive = await query.CountAsync(cf => cf.Status.ToLower() == "inactive");

            return new
            {
                Pending = totalPending,
                Active = totalActive,
                Inactive = totalInactive,
                Total = totalPending + totalActive + totalInactive
            };
        }


        /********************************************************************Mobile****************************************************************************/

        public async Task<PagedResult<CompanyFriendship>> GetCompanyFriendshipByCompanyID(Guid ownerUserID, Guid companyID, CompanyFriendshipSearchRequest request, CancellationToken token)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == companyID && c.OwnerUserId == ownerUserID);

            if (company == null)
            {
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Company"));
            }

            var query = _context.CompanyFriendships
                .Include(cf => cf.CompanyA)
                    .ThenInclude(c => c.OwnerUser)
                .Include(cf => cf.CompanyA.ProjectCompanies)
                .Include(cf => cf.CompanyA.ProjectCompanyHireds)
                .Include(cf => cf.CompanyB)
                    .ThenInclude(c => c.OwnerUser)
                .Include(cf => cf.CompanyB.ProjectCompanies)
                .Include(cf => cf.CompanyB.ProjectCompanyHireds)
                .Where(cf =>
                    (cf.CompanyAId == companyID || cf.CompanyBId == companyID)
                    && (cf.CompanyA.OwnerUserId == ownerUserID || cf.CompanyB.OwnerUserId == ownerUserID)
                    && (cf.Status ?? "").Trim().ToLower() == "active")
                .AsQueryable();

            // search
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim().ToLower();

                query = query.Where(u =>
                // Nếu công ty hiện tại là A => lọc theo CompanyB
                     (u.CompanyAId == companyID && (
                        (u.CompanyB.Name ?? "").ToLower().Contains(keyword)
                        || (u.CompanyB.OwnerUser.UserName ?? "").ToLower().Contains(keyword)
                        || (u.CompanyB.TaxCode ?? "").ToLower().Contains(keyword)
                    ))
                // Nếu công ty hiện tại là B => lọc theo CompanyA
                    || (u.CompanyBId == companyID && (
                        (u.CompanyA.Name ?? "").ToLower().Contains(keyword)
                        || (u.CompanyA.OwnerUser.UserName ?? "").ToLower().Contains(keyword)
                        || (u.CompanyA.TaxCode ?? "").ToLower().Contains(keyword)
                    ))
                );

            }
            // filter following fromdate todate
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                var from = request.FromDate.Value.Date;
                var to = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);

                query = query.Where(x => x.CreatedAt >= from && x.CreatedAt <= to);
            }
            else if (request.FromDate.HasValue)
            {
                var from = request.FromDate.Value.Date;
                query = query.Where(x => x.CreatedAt >= from);
            }
            else if (request.ToDate.HasValue)
            {
                var to = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.CreatedAt <= to);
            }

            return await query.ToPagedResultAsync(request, token);
        }

        public async Task<object> GetCompanyFriendshipStatusSummary(Guid ownerUserId, Guid companyId)
        {
            var query = _context.CompanyFriendships
         .Include(cf => cf.CompanyA)
         .Include(cf => cf.CompanyB)
         .Where(cf =>
             (cf.CompanyA.OwnerUserId == ownerUserId || cf.CompanyB.OwnerUserId == ownerUserId)
             && (cf.CompanyAId == companyId || cf.CompanyBId == companyId)
         );

            var totalPending = await query.CountAsync(cf => cf.Status == "pending");
            var totalActive = await query.CountAsync(cf => cf.Status == "active");
            var totalInactive = await query.CountAsync(cf => cf.Status == "inactive");

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
