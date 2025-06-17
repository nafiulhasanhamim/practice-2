using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using API.Common.Models;
using API.Controllers;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] // Allow all endpoints in this controller to be accessed without authentication by default
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IUserManagementService _user;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IUserManagementService user,
            ILogger<AuthenticationController> logger)
        {
            _userManager = userManager;
            _user = user;
            _emailService = emailService;
            _logger = logger;
        }

        // Handle CORS preflight requests for the entire controller
        [HttpOptions]
        public IActionResult HandleOptions()
        {
            return Ok();
        }

        // Handle CORS preflight for specific routes
        [HttpOptions("{*path}")]
        public IActionResult HandleOptionsWildcard()
        {
            return Ok();
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUser)
        {
            try
            {
                var tokenResponse = await _user.CreateUserWithTokenAsync(registerUser);

                if (!tokenResponse.IsSuccess)
                {
                    return ApiResponse.BadRequest(tokenResponse.Message ?? "Failed to create user");
                }

                var confirmationLink = Url.Action(
                    nameof(ConfirmEmail),
                    "Authentication",
                    new { tokenResponse.Response!.Token, email = registerUser.Email },
                    Request.Scheme);

                var messages = new Message(new string[] { registerUser.Email! }, "Confirmation Email", confirmationLink!);
                _emailService.SendEmail(messages);

                return ApiResponse.Success($"User created & Email Sent to {registerUser.Email} Successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return ApiResponse.BadRequest("An error occurred during registration");
            }
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return ApiResponse.BadRequest("This user does not exist!");
                }

                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return ApiResponse.Success("Email Verified Successfully");
                }

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse.BadRequest($"Email confirmation failed: {errors}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email confirmation");
                return ApiResponse.BadRequest("An error occurred during email confirmation");
            }
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ApiResponse.BadRequest("Invalid login data");
                }

                var loginOtpResponse = await _user.GetOtpByLoginAsync(loginModel);

                if (loginOtpResponse.Response == null)
                {
                    return ApiResponse.Unauthorized("Invalid username or password");
                }

                var user = loginOtpResponse.Response.User;

                if (user.TwoFactorEnabled)
                {
                    var token = loginOtpResponse.Response.Token;
                    var message = new Message(
                        new string[] { user.Email! },
                        "OTP Confirmation",
                        token);
                    _emailService.SendEmail(message);

                    return ApiResponse.Success(new
                    {
                        message = $"We have sent an OTP to your Email {user.Email}",
                        requiresOtp = true
                    });
                }

                if (await _userManager.CheckPasswordAsync(user, loginModel.Password!))
                {
                    var serviceResponse = await _user.GetJwtTokenAsync(user);
                    return ApiResponse.Success(serviceResponse);
                }

                return ApiResponse.Unauthorized("Invalid username or password");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return ApiResponse.BadRequest("An error occurred during login");
            }
        }

        [HttpPost]
        [Route("login-2fa")]
        public async Task<IActionResult> LoginWithOTP([FromBody] LoginWithOTPDTO loginWithOTP)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ApiResponse.BadRequest("Invalid OTP data");
                }

                var jwt = await _user.LoginUserWithJWTokenAsync(loginWithOTP.Code, loginWithOTP.Username);

                if (jwt.IsSuccess)
                {
                    return ApiResponse.Success(jwt.Response);
                }

                return ApiResponse.BadRequest("Invalid code");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during 2FA login");
                return ApiResponse.BadRequest("An error occurred during 2FA login");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("forgot-password")]
        public async Task<IActionResult> ForgotPassword([Required][FromBody] ForgotPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.Email))
                {
                    return ApiResponse.BadRequest("Email is required");
                }

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    // Don't reveal that the user doesn't exist
                    return ApiResponse.Success($"If the email exists, a password reset link has been sent to {request.Email}");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var forgotPasswordLink = Url.Action(
                    nameof(ResetPassword),
                    "Authentication",
                    new { token, email = user.Email },
                    Request.Scheme);

                var message = new Message(
                    new string[] { user.Email! },
                    "Forgot password link",
                    forgotPasswordLink!);
                _emailService.SendEmail(message);

                return ApiResponse.Success($"Password reset link has been sent to {user.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password");
                return ApiResponse.BadRequest("An error occurred while processing your request");
            }
        }

        [HttpGet("reset-password")]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPasswordDTO { Token = token, Email = email };
            return ApiResponse.Success(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPassword)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ApiResponse.BadRequest("Invalid reset password data");
                }

                var user = await _userManager.FindByEmailAsync(resetPassword.Email);
                if (user == null)
                {
                    return ApiResponse.BadRequest("Invalid request");
                }

                var resetPassResult = await _userManager.ResetPasswordAsync(
                    user,
                    resetPassword.Token,
                    resetPassword.Password);

                if (!resetPassResult.Succeeded)
                {
                    var errors = string.Join(", ", resetPassResult.Errors.Select(e => e.Description));
                    return ApiResponse.BadRequest($"Password reset failed: {errors}");
                }

                return ApiResponse.Success("Password has been changed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return ApiResponse.BadRequest("An error occurred while resetting your password");
            }
        }

        [HttpPost]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] LoginResponse tokens)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ApiResponse.BadRequest("Invalid token data");
                }

                var jwt = await _user.RenewAccessTokenAsync(tokens);

                if (jwt.IsSuccess)
                {
                    return ApiResponse.Success(jwt.Response);
                }

                return ApiResponse.BadRequest("Invalid or expired refresh token");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return ApiResponse.BadRequest("An error occurred while refreshing your token");
            }
        }
    }

    // DTO for forgot password request
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}