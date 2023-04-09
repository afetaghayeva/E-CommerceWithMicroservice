using IdentityService.API.Application.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityService.API.Application.Services
{
    public class IdentityService : IIdentityService
    {
        public Task<LoginResponseModel> Login(LoginRequestModel requestModel)
        {
            //DB Process

            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, requestModel.UserName),
                new Claim(ClaimTypes.Name,"AfetAghayeva")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TechBuddySecretKeyShouldBeLong"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiry = DateTime.Now.AddDays(10);
            var token = new JwtSecurityToken(claims: claims, expires: expiry, signingCredentials: creds, notBefore: DateTime.Now);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(token);

            LoginResponseModel responseModel = new()
            {
                UserName = requestModel.UserName,
                UserToken = encodedJwt,
            };

            return Task.FromResult(responseModel);
        }
    }
}
