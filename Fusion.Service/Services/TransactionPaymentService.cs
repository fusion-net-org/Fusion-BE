
//using AutoMapper;
//using Fusion.Repository.Bases.Exceptions;
//using Fusion.Repository.Bases.Page;
//using Fusion.Repository.Bases.Page.TransactionPayment;
//using Fusion.Repository.Bases.Responses;
//using Fusion.Repository.Entities;
//using Fusion.Repository.Enums;
//using Fusion.Repository.IRepositories;
//using Fusion.Service.Commons.Helpers;
//using Fusion.Service.IServices;
//using Fusion.Service.ViewModels.TransactionPayment.Requests;
//using Fusion.Service.ViewModels.TransactionPayment.Responses;

//namespace Fusion.Service.Services;

//public class TransactionPaymentService : ITransactionPaymentService
//{
//    private readonly ITransactionPaymentRepository _transactionPaymentRepository;
//    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
//    private readonly IUserRepository _userRepository;
//    private readonly IMapper _mapper;
//    private readonly ICurrentService _currentService;

//    public TransactionPaymentService(ITransactionPaymentRepository transactionPaymentRepository, ISubscriptionPlanRepository subscriptionPlanRepository,
//        IUserRepository userRepository, IMapper mapper, ICurrentService currentService)
//    {
//        _transactionPaymentRepository = transactionPaymentRepository;
//        _subscriptionPlanRepository = subscriptionPlanRepository;
//        _userRepository = userRepository;
//        _mapper = mapper;
//        _currentService = currentService;
//    }

//    public async Task<TransactionPaymentResponse> CreateAsync(TransactionPaymentCreateRequest req, CancellationToken ct = default)
//    {
//        if (req == null)
//            throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT);

//        if (req.PlanId == Guid.Empty)
//            throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT);

//        var plan = await _subscriptionPlanRepository.GetByIdWithNavAsync(req.PlanId, ct);

//        if (plan == null)
//            throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Subscription plan"));
//        if (plan.Price == null)
//            throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Price"));

//        var userId = _currentService.GetUserId();
//        var entity = new TransactionPayment
//        {
//            Id = Guid.NewGuid(),
//            UserId = userId,
//            PlanId = req.PlanId,
//            Amount = plan.Price.Price,
//            Currency = plan.Price.Currency,
//            Status = PaymentStatus.Pending.ToString()
//        };

//        var created = await _transactionPaymentRepository.CreateAsync(entity, ct);

//        var withNav = await _transactionPaymentRepository.GetByIdWithNavAsync(created.Id, ct);
//        return _mapper.Map<TransactionPaymentResponse>(withNav);
//    }

//    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
//      => _transactionPaymentRepository.DeleteAsync(id, ct);

//    public async Task<TransactionPaymentDetailResponse?> GetDetailAsync(Guid id, CancellationToken ct = default)
//    {
//        var entity = await _transactionPaymentRepository.GetByIdWithNavAsync(id, ct);
//        return entity == null ? null : _mapper.Map<TransactionPaymentDetailResponse>(entity);
//    }

//    public async Task<PagedResult<TransactionPaymentResponse>> GetPagedAsync(TransactionPaymentPagedRequest request, CancellationToken ct = default)
//    {
//        var paged = await _transactionPaymentRepository.GetPagedAsync(request, ct);
//        var items = _mapper.Map<List<TransactionPaymentResponse>>(paged.Items);

//        return new PagedResult<TransactionPaymentResponse>
//        {
//            Items = items,
//            TotalCount = paged.TotalCount,
//            PageNumber = paged.PageNumber,
//            PageSize = paged.PageSize
//        };

//    }

//    public async Task<bool> UpdateAsync(Guid id, TransactionPaymentUpdateRequest req, CancellationToken ct = default)
//    {
//        var current = await _transactionPaymentRepository.GetByIdWithNavAsync(id, ct);
//        if (current == null) return false;

//        if (req.OrderCode.HasValue) current.OrderCode = req.OrderCode.Value;
//        if (req.PaymentLinkId != null) current.PaymentLinkId = req.PaymentLinkId;

//        if (req.Amount.HasValue) current.Amount = req.Amount.Value;
//        if (req.Description != null) current.Description = req.Description;
//        if (req.AccountNumber != null) current.AccountNumber = req.AccountNumber;
//        if (req.Reference != null) current.Reference = req.Reference;
//        if (req.TransactionDateTime.HasValue) current.TransactionDateTime = req.TransactionDateTime.Value;
//        if (req.Currency != null) current.Currency = req.Currency;
//        if (req.CounterAccountBankId != null) current.CounterAccountBankId = req.CounterAccountBankId;
//        if (req.CounterAccountBankName != null) current.CounterAccountBankName = req.CounterAccountBankName;
//        if (req.CounterAccountName != null) current.CounterAccountName = req.CounterAccountName;
//        if (req.CounterAccountNumber != null) current.CounterAccountNumber = req.CounterAccountNumber;
//        if (req.PaymentMethod != null) current.PaymentMethod = req.PaymentMethod;
//        if (req.Status != null) current.Status = req.Status;

//        return await _transactionPaymentRepository.UpdateAsync(current, ct);
//    }

//}
