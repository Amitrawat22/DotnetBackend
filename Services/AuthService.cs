using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using dotnetapp.Data;
using dotnetapp.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;

namespace dotnetapp.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;
        private static readonly List<string> AcceptedRoles = new()
        {
            UserRoles.Gardener,
            UserRoles.Customer
        };

        //IdentityRole -> class that manages userAccount, and authorization
        // private readonly ApplicationDbContext _context; --removed this as was not being used anywhere

        //IConfiguration-> interface to access and manage application configuration settings.
        private  readonly IConfiguration _configuration;
        public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<(int status, string message)> Registration(User model, string role)
        {
            // 1.  check if email already exists
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
            {
                return (400, "User already exists.");
            }

            // 2. new User -> register
            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                // SecurityStamp = Guid.NewGuid().ToString(), //-> property to identify user
                UserName = model.Username,
                Name = model.Username,
                MobileNumber = model.MobileNumber,


            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                // error during user creation
                return (500, "User creation failed! Please check user details and try again.");
            }

            if (!AcceptedRoles.Contains(role))
            {
                return (400, "Enter a valid role!");
            }
            // 3. Assigning roles
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            await _userManager.AddToRoleAsync(user, role);
            // 4 return success msg
            return (201, "User created successfully!");
        }

        public async Task<(int status, object result)> Login(LoginModel model)
        {
            // 1. search user by email
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return (401, new { message = "Invalid Email or Password." });
            }
            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return (401, new { message = "Invalid Email or Password." });
            }
            //generate tokens
            var userRoles = await _userManager.GetRolesAsync(user);
            var authClaims = new List<Claim>{
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = GenerateToken(authClaims);
            return (200, new { success = "Success", token = token });
        }

        private string GenerateToken(IEnumerable<Claim> claims)
        {
            var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddDays(1), // token expiry time
                claims: claims,
                signingCredentials: new SigningCredentials(Key, SecurityAlgorithms.HmacSha256)
            );
            return new JwtSecurityTokenHandler().WriteToken(token); // convert  token to string
        }
    }
}