using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace dotnetapp.Models
{
    public class ApplicationUser:IdentityUser
    {
        
        [MaxLength(30)]
        public string Name{get; set;} = null!;
        
        [MaxLength(10)]
        public string MobileNumber { get; set; } = "";

        
        public string? ResetOtp { get; set; }
        public DateTime? OtpExpiryTime { get; set; }
    }
}