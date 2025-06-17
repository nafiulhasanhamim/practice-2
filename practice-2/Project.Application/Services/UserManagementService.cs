using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API.Common.Models;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services
{

    public class UserManagementService : IUserManagementService
    {
        // private readonly UserManager<IdentityUser> _userManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        // private readonly SignInManager<IdentityUser> _signInManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;


        // public UserManagementService(UserManager<IdentityUser> userManager,
        public UserManagementService(UserManager<ApplicationUser> userManager,
            // RoleManager<IdentityRole> roleManager, SignInManager<IdentityUser> signInManager)
            RoleManager<IdentityRole> roleManager, IConfiguration configuration, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<ApiResponseUser<CreateUserResponse>> CreateUserWithTokenAsync(RegisterUserDto registerUser)
        {
            // Check if the user already exists
            //Check User Exist 
            if (registerUser.Password != registerUser.ConfirmPassword)
            {
                return new ApiResponseUser<CreateUserResponse> { IsSuccess = false, StatusCode = 403, Message = "Password Mismatch!" };
            }
            var userExist = await _userManager.FindByEmailAsync(registerUser.Email!);
            var userNameExist = await _userManager.FindByNameAsync(registerUser.Username!);

            if (userExist != null)
            {
                return new ApiResponseUser<CreateUserResponse> { IsSuccess = false, StatusCode = 403, Message = "User already exists!" };
            }
            else if (userNameExist != null)
            {
                return new ApiResponseUser<CreateUserResponse> { IsSuccess = false, StatusCode = 403, Message = "User with this userName already exists!" };
            }
            //Add the User in the database
            // IdentityUser user = new()
            ApplicationUser user = new()
            {
                Email = registerUser.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerUser.Username,
                EmailConfirmed = false,
                TwoFactorEnabled = false,
            };
            // var roleCheck = await _roleManager.RoleExistsAsync(registerUser.Role!);

            if (await _roleManager.RoleExistsAsync(registerUser.Role!))
            {
                var result = await _userManager.CreateAsync(user, registerUser.Password!);
                foreach (var error in result.Errors)
                {
                    Console.WriteLine("after entering errors");

                    if (error.Code.Contains("Password"))
                    {
                        var message = $"Weak password: {error.Description}";
                        return new ApiResponseUser<CreateUserResponse> { IsSuccess = false, StatusCode = 403, Message = message };
                    }
                }
                if (!result.Succeeded)
                {
                    return new ApiResponseUser<CreateUserResponse> { IsSuccess = false, StatusCode = 403, Message = "User failed to create" };
                }
                //Add role to the user....
                await _userManager.AddToRoleAsync(user, registerUser.Role!);

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                return new ApiResponseUser<CreateUserResponse> { IsSuccess = true, StatusCode = 201, Message = "User is created", Response = new CreateUserResponse() { Token = token } };
            }
            else
            {
                return new ApiResponseUser<CreateUserResponse> { IsSuccess = false, StatusCode = 403, Message = "This role doesnot exists" };
            }

        }

        public async Task<ApiResponseUser<LoginOtpResponse>> GetOtpByLoginAsync(LoginDto loginModel)
        {
            var user = await _userManager.FindByNameAsync(loginModel.Username!);
            if (user != null)
            {

                if (!user.EmailConfirmed)
                {
                    return new ApiResponseUser<LoginOtpResponse>
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = $"User doesnot exist."
                    };
                }
                
                await _signInManager.SignOutAsync();
                await _signInManager.PasswordSignInAsync(user, loginModel.Password!, false, true);
                if (user.TwoFactorEnabled)
                {
                    var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
                    return new ApiResponseUser<LoginOtpResponse>
                    {
                        Response = new LoginOtpResponse()
                        {
                            User = user,
                            Token = token,
                            IsTwoFactorEnable = user.TwoFactorEnabled
                        },
                        IsSuccess = true,
                        StatusCode = 200,
                        Message = $"OTP send to the email {user.Email}"
                    };
                }
                else
                {
                    return new ApiResponseUser<LoginOtpResponse>
                    {
                        Response = new LoginOtpResponse()
                        {
                            User = user,
                            Token = string.Empty,
                            IsTwoFactorEnable = user.TwoFactorEnabled
                        },
                        IsSuccess = true,
                        StatusCode = 200,
                        Message = $"2FA is not enabled"
                    };
                }
            }
            else
            {
                return new ApiResponseUser<LoginOtpResponse>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"User doesnot exist."
                };
            }
        }

        public async Task<ApiResponseUser<LoginResponse>> GetJwtTokenAsync(ApplicationUser user)
        {
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var jwtToken = GetToken(authClaims); //access token
            var refreshToken = GenerateRefreshToken();
            _ = int.TryParse(_configuration["JWT:RefreshTokenValidity"], out int refreshTokenValidity);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(refreshTokenValidity);

            await _userManager.UpdateAsync(user);
            return new ApiResponseUser<LoginResponse>
            {
                Response = new LoginResponse()
                {
                    AccessToken = new TokenType()
                    {
                        Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                        ExpiryTokenDate = jwtToken.ValidTo
                    },
                    RefreshToken = new TokenType()
                    {
                        Token = user.RefreshToken,
                        ExpiryTokenDate = (DateTime)user.RefreshTokenExpiry
                    }
                },

                IsSuccess = true,
                StatusCode = 200,
                Message = $"Token created"
            };
        }

        public async Task<ApiResponseUser<LoginResponse>> LoginUserWithJWTokenAsync(string otp, string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            var signIn = await _signInManager.TwoFactorSignInAsync("Email", otp, false, false);
            if (signIn.Succeeded)
            {
                if (user != null)
                {
                    return await GetJwtTokenAsync(user);
                }
            }
            return new ApiResponseUser<LoginResponse>()
            {

                Response = new LoginResponse()
                {

                },
                IsSuccess = false,
                StatusCode = 400,
                Message = $"Invalid Otp"
            };
        }

        public async Task<ApiResponseUser<LoginResponse>> RenewAccessTokenAsync(LoginResponse tokens)
        {
            var accessToken = tokens.AccessToken;
            var refreshToken = tokens.RefreshToken;
            var principal = GetClaimsPrincipal(accessToken.Token);
            var user = await _userManager.FindByNameAsync(principal.Identity.Name);
            if (refreshToken.Token != user.RefreshToken && refreshToken.ExpiryTokenDate <= DateTime.Now)
            {
                return new ApiResponseUser<LoginResponse>
                {

                    IsSuccess = false,
                    StatusCode = 400,
                    Message = $"Token invalid or expired"
                };
            }
            var response = await GetJwtTokenAsync(user);
            return response;
        }

        private ClaimsPrincipal GetClaimsPrincipal(string accessToken)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);

            return principal;

        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new Byte[64];
            var range = RandomNumberGenerator.Create();
            range.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            _ = int.TryParse(_configuration["JWT:TokenValidityInMinutes"], out int tokenValidityInMinutes);
            var expirationTimeUtc = DateTime.UtcNow.AddMinutes(tokenValidityInMinutes);
            var localTimeZone = TimeZoneInfo.Local;
            var expirationTimeInLocalTimeZone = TimeZoneInfo.ConvertTimeFromUtc(expirationTimeUtc, localTimeZone);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddDays(2),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
    }
}