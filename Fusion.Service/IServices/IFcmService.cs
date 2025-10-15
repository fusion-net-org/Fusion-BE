using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.IServices
{
    public interface IFcmService
    {
        public Task SendToUserAsync(Guid userId, string title, string? body, string? linkUrlWeb = null, string? linkUrlMobile = null, CancellationToken cancellationToken = default);

    }
}
