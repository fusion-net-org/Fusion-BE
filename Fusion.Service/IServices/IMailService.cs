using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.Internal;
using Fusion.Service.ViewModels.Companies.Email;

namespace Fusion.Service.IServices
{
    public interface IMailService
    {
        public Task SendEmailAsync(MailRequest mailRequest);
    }
}

