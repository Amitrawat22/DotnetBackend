using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnetapp.Models
{
    public class VerifyOtp
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; }
    }
}