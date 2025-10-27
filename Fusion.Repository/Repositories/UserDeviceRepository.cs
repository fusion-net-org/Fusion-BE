using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Repositories
{
    public class UserDeviceRepository: GenericRepository<UserDevice>, IUserDeviceRepository
    {
        private readonly FusionDbContext _context;
        public UserDeviceRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task AddOrUpdateAsync(Guid userId, string token, string platform, string? deviceName, CancellationToken cancellationToken = default)
        {
            var existing = await _context.UserDevices.FirstOrDefaultAsync(d => d.DeviceToken == token && d.UserId == userId, cancellationToken);

            if (existing != null)
            {
                existing.UserId = userId;
                existing.Platform = platform;
                existing.DeviceName = deviceName;
                existing.IsActive = true;
            }
            else
            {
                var device = new UserDevice()
                {
                    UserId = userId,
                    Platform = platform,
                    DeviceName = deviceName,
                    DeviceToken = token,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddHours(7)
                };

               await _context.UserDevices.AddAsync(device);
            }

            await _context.SaveChangesAsync(cancellationToken);

        }

        public async Task<List<string>> GetTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var token = await _context.UserDevices
              .Where(d => d.UserId == userId && d.IsActive.Value)
              .Select(d => d.DeviceToken)
              .ToListAsync(cancellationToken);

            return token;
        }

        public async Task DeactivateTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            var device = await _context.UserDevices
                .FirstOrDefaultAsync(d => d.DeviceToken == token, cancellationToken);

            if (device != null)
            {
                device.IsActive = false;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
