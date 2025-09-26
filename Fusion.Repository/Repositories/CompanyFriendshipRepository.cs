using Fusion.Repository.Bases.Exceptions;
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


        public async Task<CompanyFriendship> InviteCompanyFriendship(Guid companyAId, Guid companyBId, Guid requesterId)
        {
            var checkCompanyB = await _context.Companies.FirstOrDefaultAsync(x => x.Id == companyBId);

            if (checkCompanyB == null)
            {
                throw new CustomException(
                    statusCode: StatusCodes.Status404NotFound,
                    errorCode: "COMPANY_NOT_FOUND",
                    message: $"Company with id {companyBId} does not exist"
                );
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

    }
}
