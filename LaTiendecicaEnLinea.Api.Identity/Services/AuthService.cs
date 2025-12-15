using LaTiendecicaEnLinea.Api.Identity.Dtos.Auth.Responses;
using LaTiendecicaEnLinea.Shared;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LaTiendecicaEnLinea.Api.Identity.Services
{
    public class AuthService : IAuthService
    {
        private UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IPublishEndpoint _publishEndpoint;


        public AuthService(
            UserManager<IdentityUser> userManager,
            IConfiguration configuration,
            IPublishEndpoint publishEndpoint
        )
        {
            _userManager = userManager;
            _configuration = configuration;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<ResponseLogin?> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return null;
            }

            var result = await _userManager.CheckPasswordAsync(user, password);

            if (!result)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);

            // Claim - agregar roles al token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, roles.FirstOrDefault() ?? "NoRole")
            };

            // Generate JWT Token

            var secretKey = _configuration["JWT:SecretKey"];
            var audience = _configuration["JWT:Audience"];
            var issuer = _configuration["JWT:Issuer"];
            var expirationMinutes = int.Parse(_configuration["JWT:ExpiryInMinutes"]!);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: creds
            );

            var encryptedToken = new JwtSecurityTokenHandler().WriteToken(token);

            return new ResponseLogin
            {
                Token = encryptedToken,
                ExpirationAtUtc = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };



        }

        public async Task<bool> Register(string email, string password)
        {
            var result = await _userManager.CreateAsync(new IdentityUser
            {
                UserName = email.Split("@")[0],
                Email = email
            }, password);

            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                await _publishEndpoint.Publish(new UserCreatedEvent(user.Id, user.Email!));
            }

            if (result != null) return true;

            return false;

        }

    }
}
