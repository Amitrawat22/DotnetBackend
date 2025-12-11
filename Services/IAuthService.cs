using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnetapp.Models;

namespace dotnetapp.Services
{
    public interface IAuthService
    {
        public Task<(int status, string message)> Registration(User model, string role);
        public Task<(int status, object result)> Login(LoginModel model);
 
    }
}