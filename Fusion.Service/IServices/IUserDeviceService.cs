using Fusion.Repository.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.IServices
{
    public interface IUserDeviceService
    {
        public Task RegisterAsync(Guid userId, string token, string platform, string? deviceName, CancellationToken cancellationToken = default);

    }
}
