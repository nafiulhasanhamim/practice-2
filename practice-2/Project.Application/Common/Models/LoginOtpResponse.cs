using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace API.Common.Models
{
    public class LoginOtpResponse
    {
        public string Token { get; set; } = null!;
        public bool IsTwoFactorEnable { get; set; }
        public ApplicationUser User { get; set; } = null!;

    }
}
