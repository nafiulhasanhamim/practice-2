using API.Common.Models;
using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUserManagementService
    {
        Task<ApiResponseUser<CreateUserResponse>> CreateUserWithTokenAsync(RegisterUserDto registerUser);
        Task<ApiResponseUser<LoginOtpResponse>> GetOtpByLoginAsync(LoginDto loginModel);
        Task<ApiResponseUser<LoginResponse>> LoginUserWithJWTokenAsync(string otp, string userName);
        Task<ApiResponseUser<LoginResponse>> GetJwtTokenAsync(ApplicationUser user);
        Task<ApiResponseUser<LoginResponse>> RenewAccessTokenAsync(LoginResponse tokens);



    }
}
