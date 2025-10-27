using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.Services
{
    public class UserDeviceService : IUserDeviceService
    {
        private readonly IUserDeviceRepository _userDeviceRepository;

        public UserDeviceService(IUserDeviceRepository userDeviceRepository)
        {
            _userDeviceRepository = userDeviceRepository;
        }

        public async Task RegisterAsync(Guid userId, string token, string platform, string? deviceName, CancellationToken cancellationToken = default)
        {
            if (!Enum.TryParse<DevicePlatform>(platform, true, out var platformEnum))
            {
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT,$"Invalid platform: {platform}");
            }

            await _userDeviceRepository.AddOrUpdateAsync(userId, token, platform.ToUpper(), deviceName, cancellationToken);
        }
    }
}
