using IdentityService.API.Application.Models;
using IdentityService.API.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        public AuthController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost]
        public async  Task<IActionResult> Login(LoginRequestModel requestModel)
        {
            var result=await _identityService.Login(requestModel);

            return Ok(result);
        }
    }
}
