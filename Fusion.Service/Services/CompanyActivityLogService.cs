

using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.CompanyActivityLog;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Service.Services
{
    public class CompanyActivityLogService : ICompanyActivityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICompanyActivityLogRepository _repo;
        private readonly ICurrentService _currentService;

        public CompanyActivityLogService(IUnitOfWork unitOfWork, ICompanyActivityLogRepository repo, ICurrentService currentService)
        {
            _unitOfWork = unitOfWork;
            _repo = repo;
            _currentService = currentService;
        }

        public async Task<CompanyActivityLog> CreateLog(CompanyActivityLog log, CancellationToken cancellationToken = default)
        {

           if(log == null)
                throw CustomExceptionFactory.
                      CreateBadRequestError(ResponseMessages.INVALID_INPUT.FormatMessage("log"));
            try
            {
                log.CreatedAt = DateTime.UtcNow;
                log.IsDeleted = false;
                log.IsView = false;
                await _unitOfWork.Repository<CompanyActivityLog>().AddAsync(log);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return log;
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database update failed when creating log.", dbEx);
            }
        }

        public async Task<bool> DeleteLogAsync(Guid id)
        {
            var log = await _repo.GetLogByIdAsync(id);
            if(log == null)
            {
                throw CustomExceptionFactory.
                      CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("CompanyActivityLog", id));
            }
            try
            {
                log.IsDeleted = true;
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database update failed when deleting log.", dbEx);
            }
        }

        public Task<CompanyActivityLog?> GetByIdAsync(Guid id, CancellationToken ct = default)
          => _repo.GetLogByIdAsync(id, ct);

        public async Task<PagedResult<CompanyActivityLog>> GetPagedAsync(Guid companyId, CompanyActivityLogPagedSearchRequest? request, CancellationToken ct = default)
        {
            var userId = _currentService.GetUserId();
            if (userId == Guid.Empty)
                throw CustomExceptionFactory.CreateUnauthorizedError("Unauthorized");

            request ??= new CompanyActivityLogPagedSearchRequest();

            if (string.IsNullOrWhiteSpace(request.SortColumn))
            {
                request.SortColumn = "CreatedAt";
                request.SortDescending = true;
            }
            return await _repo.GetPagedLogsByCompanyIdAsync(companyId, userId, request, ct);
        }

        public async Task<bool> UpdateIsView(bool isView,Guid companyId, CancellationToken ct = default)
        {
            var userId = _currentService.GetUserId();
            var result = await _repo.UpdateIsViewLog(isView, companyId, userId, ct);

            return result;
        }
    }
}
