using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace dotnetapp.Models
{
    public class User
    {
        [Key]
        public int UserId{get; set;}= default!;

        [Required, EmailAddress]
        public string Email {get; set;}=null!;

        [Required]
        public string Password {get; set;}=null!;

        [Required]
        public string Username {get; set;}=null!;

        [Required, Phone]
        public string MobileNumber {get; set;}=null!;

        [Required]
        public string UserRole {get; set;}=null!;
    }
}