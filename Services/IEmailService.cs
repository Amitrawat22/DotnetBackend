using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnetapp.Services
{
    public interface IEmailService
    {
        public Task SendEmailAsync(string to, string subject, string body);
    }
}