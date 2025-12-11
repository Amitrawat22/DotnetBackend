using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using dotnetapp.Services;
using dotnetapp.Models;
using Microsoft.AspNetCore.Authorization;
using dotnetapp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace dotnetapp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthService _authService;

        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;
        public AuthenticationController(IAuthService authService, ApplicationDbContext context, IEmailService emailService, UserManager<ApplicationUser> userManager)
        {
            _authService = authService;
            _context = context;
            _emailService = emailService;
            _userManager = userManager;
        }

        [HttpPost]
        [Route("login")]
        //[Authorize(Roles = UserRoles.Customer + "," + UserRoles.Gardener)]
        [AllowAnonymous] // access login without jwt token
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            // validation for invalid
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Bad Request." });
            }
            try
            {
                var (status, result) = await _authService.Login(model);
                if (status == 200)
                {
                    // result has the token object
                    return StatusCode(200, result);
                }
                else
                {
                    // result ->0 invalid 

                    var log = new ErrorLog
                    {
                        Source = "AuthenticatonController.login",
                        Message = "Bad Request",
                        TimeStamp = DateTime.UtcNow
                    };

                    _context.ErrorLogs.Add(log);
                    await _context.SaveChangesAsync();
                    return Unauthorized(result);

                }
            }
            catch (Exception e)
            {

                var log = new ErrorLog
                {
                    Source = "AuthenticatonController.login",
                    Message = e.Message,
                    TimeStamp = DateTime.UtcNow
                };

                _context.ErrorLogs.Add(log);
                await _context.SaveChangesAsync();
                //internal server error
                return StatusCode(500, new { message = $"Internal Server Error: {e.Message}" });
            }
        }
        [HttpPost]
        [Route("register")]
        // [Authorize(Roles=UserRoles.Customer +","+ UserRoles.Gardener)]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] User model)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid payload.");
            }
            try
            {
                //extracting role :
                string role = model.UserRole;
                var (status, message) = await _authService.Registration(model, role);
                if (status == 201)
                {
                    return StatusCode(201, new { Message = message });
                }
                else
                {
                    var log = new ErrorLog
                    {
                        Source = "AuthenticatonController.register",
                        Message = "Bad Request",
                        TimeStamp = DateTime.UtcNow
                    };

                    _context.ErrorLogs.Add(log);
                    await _context.SaveChangesAsync();
                    // user already exist or creation fails
                    return BadRequest(new { Message = message });
                }
            }
            catch (Exception e)
            {

                var log = new ErrorLog
                {
                    Source = "AuthenticatonController.register",
                    Message = e.Message,
                    TimeStamp = DateTime.UtcNow
                };

                _context.ErrorLogs.Add(log);
                await _context.SaveChangesAsync();

                return StatusCode(500, new { Message = $"Internal Server Error: {e.Message}" });
            }
        }

        [HttpPost("forgot")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPassword model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
                return BadRequest(new { Message = "No user found with this email." });

            // Generate OTP
            string otp = new Random().Next(100000, 999999).ToString();

            user.ResetOtp = otp;
            user.OtpExpiryTime = DateTime.UtcNow.AddMinutes(10);

            await _context.SaveChangesAsync();

            // Send email
            await _emailService.SendEmailAsync(
                model.Email,
                "Password Reset OTP",
                $"Your OTP for resetting your password is: {otp}\n(It expires in 10 minutes.)"
            );

            return Ok(new { Message = "OTP sent to your email." });
        }


        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtp model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
                return BadRequest(new { Message = "User not found." });

            if (user.ResetOtp != model.Otp)
                return BadRequest(new { Message = "Invalid OTP." });

            if (user.OtpExpiryTime < DateTime.UtcNow)
                return BadRequest(new { Message = "OTP expired." });

            return Ok(new { Message = "OTP verified successfully." });
        }


        [HttpPost("reset")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPassword model)
        {
            var identityUser = await _userManager.FindByEmailAsync(model.Email);
            var customUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (identityUser == null || customUser == null)
                return BadRequest(new { Message = "User not found." });

            // Validate OTP
            // if (customUser.ResetOtp != model.Otp)
            //     return BadRequest(new { Message = "Invalid OTP." });

            // if (customUser.OtpExpiryTime < DateTime.Now)
            //     return BadRequest(new { Message = "OTP expired." });

            // Identity password reset
            var token = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
            var result = await _userManager.ResetPasswordAsync(identityUser, token, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(new { Message = "Password reset failed.", Errors = result.Errors });

            // Clear OTP
            customUser.ResetOtp = null;
            customUser.OtpExpiryTime = null;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Password reset successful." });
        }

    }
}