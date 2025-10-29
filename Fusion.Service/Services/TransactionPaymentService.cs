

using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.TransactionPayment;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.TransactionPayment.Requests;
using Fusion.Service.ViewModels.TransactionPayment.Responses;

namespace Fusion.Service.Services;

public class TransactionPaymentService : ITransactionPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentService _currentService;
    private readonly ITransactionPaymentRepository _transactionPaymentRepository;
    private readonly IMapper _mapper;
    private readonly IGenericRepository<TransactionPayment> _transactionPayement;
    private readonly IUserService _userService;
    private readonly ISubscriptionPackageService _subscriptionPackageService;

    public TransactionPaymentService(IUnitOfWork unitOfWork, ICurrentService currentService,
        ITransactionPaymentRepository transactionPaymentRepository, IMapper mapper, IUserService userService,
        ISubscriptionPackageService subscriptionPackageService)
    {
        _unitOfWork = unitOfWork;
        _currentService = currentService;
        _transactionPaymentRepository = transactionPaymentRepository;
        _mapper = mapper;
        _transactionPayement = _unitOfWork.Repository<TransactionPayment>();
        _userService = userService;
        _subscriptionPackageService = subscriptionPackageService;
    }
    public async Task<TransactionPaymentResponse> CreateTransactionPaymentAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _currentService.GetUserId();
        if (userId == Guid.Empty)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("User"));

        var subscriptionPackage = await _subscriptionPackageService.GetSubscriptionByIdAsync(request.PackageId, cancellationToken);

        var orderCode = DateTimeOffset.UtcNow.ToUnixTimeSeconds() * 1000
              + Random.Shared.Next(0, 999);
        var user = await _userService.GetByIdAsync(userId, cancellationToken);

        var entity = _mapper.Map<TransactionPayment>(request);

        entity.UserId = userId;
        entity.TransactionCode = orderCode.ToString();
        entity.Amount = subscriptionPackage.Price;
        entity.Status = "Pending";
        entity.CreatedAt = DateTime.UtcNow;


        await _transactionPayement.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();


        var response = new TransactionPaymentResponse
        {
            id = entity.Id,
            CustomerName = user.UserName,
            PackageName = subscriptionPackage.Name,
            TransactionCode = entity.TransactionCode,
            Amount = entity.Amount,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        };

        return response;
    }

    public async Task<PagedResult<TransactionForAdminResponse>> GetAllTransactionForAdminAsync(
    AdminTransactionSearch request,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.SortColumn))
        {
            request.SortColumn = nameof(TransactionForAdminResponse.CreatedAt);
            request.SortDescending = true;
        }

        // Lấy query chưa materialize từ repo
        var baseQuery = _transactionPaymentRepository.GetListPaymentForAdminQuery(request);

        // ProjectTo -> IQueryable<TransactionForAdminResponse>
        var projected = _mapper.ProjectTo<TransactionForAdminResponse>(
            baseQuery,
            parameters: null);

        // Phân trang + sort dùng extension có sẵn
        var paged = await projected.ToPagedResultAsync(request, cancellationToken);
        return paged;
    }

    public async Task<Guid> GetLasterTransactionForUserAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentService.GetUserId();
        if (userId == Guid.Empty)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("User"));
        var transaction = await _transactionPaymentRepository.GetLasterTransactionForUserAsync(userId, cancellationToken);

        if (transaction == null)
            throw CustomExceptionFactory.CreateNotFoundError("No transaction found for this user.");

        return transaction.Id;

    }

    public async Task<TransactionPaymentResponse> GetTransactionByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionPayement.FindAsync(x => x.TransactionCode == code, cancellationToken);
        if (transaction == null)
            throw CustomExceptionFactory.CreateNotFoundError(
              ResponseMessages.NOT_FOUND.FormatMessage("Transaction panyment"));

        var userId = _currentService.GetUserId();
        if (userId == Guid.Empty)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("User"));

        var user = await _userService.GetByIdAsync(userId, cancellationToken);

        var subscriptionPackage = await _unitOfWork.Repository<SubscriptionPackage>().FindAsync(x => x.Id == transaction.PackageId);
        if (subscriptionPackage == null)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Subscription package"));

        var response = new TransactionPaymentResponse
        {
            id = transaction.Id,
            CustomerName = user.UserName,
            PackageName = subscriptionPackage.Name,
            TransactionCode = transaction.TransactionCode,
            Amount = transaction.Amount,
            Status = transaction.Status,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt,
        };

        return response;
    }
       
}
