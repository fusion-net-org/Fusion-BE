
    using Fusion.Repository.IRepositories;
    using Fusion.Repository.Repositories;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using Microsoft.Extensions.Logging;

    namespace Fusion.Repository.Data;

    public class UnitOfWork : IUnitOfWork
    {

        private readonly FusionDbContext _context;
        private IDbContextTransaction? _currentTransaction;

        //Khai báo chung
        private ICompanyRepository _companyRepository;
        private UserRepository _userRepository;
   
        private readonly Dictionary<Type, object> _repositories = new();
        private readonly ILogger<UnitOfWork> _logger;

        public ICompanyRepository companyRepository
        {
            get
            {
                return _companyRepository ??= new CompanyRepository(_context);
            }
        }

        public IUserRepository userRepository
        {
            get
            {
                return _userRepository ??= new UserRepository(_context);
            }
        }

	    public UnitOfWork(FusionDbContext context, ILogger<UnitOfWork> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public IGenericRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);
            if (!_repositories.ContainsKey(type))
            {
                var repositoryInstance = new GenericRepository<T>(_context);
                _repositories.Add(type, repositoryInstance);
            }
            return (IGenericRepository<T>)_repositories[type];
        }
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null) return;
            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
                throw new InvalidOperationException("No active transaction to commit.");

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _currentTransaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Commit transaction failed, rolling back.");
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null) return;

            try
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
                _logger.LogWarning("Transaction rolled back.");
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Get all entries that are followed by the context and are in the added state  
            foreach (var entry in _context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added))
            {
                var keyProperty = entry.Metadata.FindPrimaryKey()?.Properties.FirstOrDefault();
                if (keyProperty == null) continue;

                var clrProp = entry.Entity.GetType().GetProperty(keyProperty.Name);
                if (clrProp?.PropertyType == typeof(Guid))
                {
                    var current = (Guid)clrProp.GetValue(entry.Entity)!;
                    if (current == Guid.Empty)
                        clrProp.SetValue(entry.Entity, Guid.NewGuid());
                }
            }

            return await _context.SaveChangesAsync(cancellationToken);
        }
        private async Task DisposeTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
        public void Dispose()
        {
            _context.Dispose();
            _currentTransaction?.Dispose();
        }

	  
    }
