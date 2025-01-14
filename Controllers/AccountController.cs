using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CheckCarsAPI.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using CheckCarsAPI.Services;

namespace CheckCarsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {

        private readonly EmailService _emailService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<IdentityUser> userM,
        SignInManager<IdentityUser> SignInM,
        IConfiguration iconfig,
        EmailService serviceemail
            )
        {
            _emailService = serviceemail;
            _userManager = userM;
            _signInManager = SignInM;
            _configuration = iconfig;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                return Ok(new { result = "User created Successfully" });
            }
            return BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = GenerateJwtToken(user);
                return Ok(new { Token = token });
            }
            return Unauthorized();
        }
        [HttpPost("forgot")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(new
                {
                    message = "Not Found"
                });
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Construir un enlace para enviar por correo electrónico
            var resetUrl = Url.Action(
                "ResetPassword",
                "Account",
                new { token, email },
                Request.Scheme);

            // Enviar el enlace por correo (implementa un servicio de correo aquí)
            await _emailService.SendEmailAsync(email, "Password Reset CheckCarsAPI", $"Reset token password is: {token}");

            return Ok(new { message = "Password reset link has been sent to your email." });
        }
        [HttpPost("Reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetClass reset)
        {
            var user = await _userManager.FindByEmailAsync(reset.email);
            if (user == null)
            {
                return NotFound(new
                {
                    message = "Invalid User"
                });
            }
            var result = await _userManager.ResetPasswordAsync(user, reset.token, reset.password);
            if (result.Succeeded)
            {
                return Ok(new { message = "Password reset successfully." });
            }

            return BadRequest(new { errors = result.Errors });
        }

        #region  Private Methods


        private string GenerateJwtToken(IdentityUser user)
        {
            try
            {
                var claims = new[]
           {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    issuer: HttpContext.Request.Host.ToString(),
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(12),
                    signingCredentials: creds);
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (ArgumentOutOfRangeException r)
            {
                Console.WriteLine("Verifique el tamaño de la llave. " + r.Message);
                throw;
            }
        }


        public class ResetClass
        {
            public string email { get; set; }
            public string token { get; set; }
            public string password { get; set; }
        }

        public class RegisterModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
        public class LoginModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        #endregion
    }
}
