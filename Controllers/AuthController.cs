using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Data;

namespace TaskFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _users;
        private readonly SignInManager<AppUser> _signIn;

        public AuthController(UserManager<AppUser> users, SignInManager<AppUser> signIn)
        {
            _users = users; _signIn = signIn;
        }

        public record RegisterDto(string Email, string Password);
        public record LoginDto(string Email, string Password);

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var user = new AppUser { UserName = dto.Email, Email = dto.Email, EmailConfirmed = true };
            var result = await _users.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);
            await _signIn.SignInAsync(user, isPersistent: true);
            return Ok(new { ok = true });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _users.FindByEmailAsync(dto.Email);
            if (user is null) return Unauthorized();

            var passOk = await _signIn.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
            if (!passOk.Succeeded) return Unauthorized();

            await _signIn.SignInAsync(user, isPersistent: true);
            return Ok(new { ok = true });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signIn.SignOutAsync();
            return Ok(new { ok = true });
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            if (!(User.Identity?.IsAuthenticated ?? false)) return Ok(null);
            var user = await _users.GetUserAsync(User);
            return Ok(new { id = user!.Id, email = user.Email });
        }
    }
}
