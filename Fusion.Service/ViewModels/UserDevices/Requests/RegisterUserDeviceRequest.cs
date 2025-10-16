using Fusion.Repository.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.UserDevices.Requests
{
    public class RegisterUserDeviceRequest
    {
        public string DeviceToken { get; set; } = default!;
        public string? Platform { get; set; } // Web / Android / iOS
        public string? DeviceName { get; set; }
    }
}
