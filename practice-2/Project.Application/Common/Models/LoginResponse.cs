using Application.DTOs;

namespace API.Common.Models
{
    public class LoginResponse
    {
        public TokenType AccessToken { get; set; }
        public TokenType RefreshToken { get; set; }


    }
}