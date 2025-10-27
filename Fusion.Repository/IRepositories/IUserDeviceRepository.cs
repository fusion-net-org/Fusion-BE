using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.IRepositories
{
    public interface IUserDeviceRepository : IGenericRepository<UserDevice>
    {
        public Task<List<string>> GetTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        public Task AddOrUpdateAsync(Guid userId, string token, string platform, string? deviceName, CancellationToken cancellationToken = default);
        public Task DeactivateTokenAsync(string token, CancellationToken cancellationToken = default);
    }
}
