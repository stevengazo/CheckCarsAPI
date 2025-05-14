using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CheckCarsAPI.Services;
using CheckCarsAPI.Data;
using CheckCarsAPI.Models;
using NuGet.Packaging;
using Microsoft.AspNetCore.Authorization;


namespace CheckCarsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly UserManager<CheckCarsAPI.Models.UserApp> _userManager;
        private readonly SignInManager<CheckCarsAPI.Models.UserApp> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;

        public AccountController(
            UserManager<CheckCarsAPI.Models.UserApp> userM,
            SignInManager<CheckCarsAPI.Models.UserApp> SignInM,
            IConfiguration iconfig,
            EmailService serviceemail,
            ApplicationDbContext applicationDb,
            ILogger<AccountController> logger,
            RoleManager<IdentityRole> roleM
            )
        {
            _emailService = serviceemail;
            _userManager = userM;
            _signInManager = SignInM;
            _configuration = iconfig;
            _dbContext = applicationDb;
            _logger = logger;
            _roleManager = roleM;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            int haveUsers = _dbContext.Users.Count();
            var user = new CheckCarsAPI.Models.UserApp { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            await AssignRol(user);

            if (result.Succeeded && haveUsers > 0)
            {
                return Ok(new { result = "User created Successfully" });
            }
            if (result.Succeeded && haveUsers == 0)
            {
                var u = _dbContext.Users.FirstOrDefault();
                if (u == null)
                {
                    return BadRequest(new { result = "User not found" });
                }
                u.EmailConfirmed = true;
                _dbContext.Users.Update(u);
                _dbContext.SaveChanges();
                return Ok(new { result = "User created Successfully. First User" });
            }
            return BadRequest(result);
        }

        /// <summary>
        /// Validates a JWT token and returns user-related information if valid.
        /// </summary>
        /// <param name="jwt">The JWT token to validate.</param>
        /// <returns>Returns a success message with user info if the token is valid; otherwise, returns an error.</returns>
        [HttpPost("Check")]
        public async Task<IActionResult> Check(string jwt)
        {
            _logger.LogError("Check method request..");
            // Ensure the JWT token is provided in the request
            if (string.IsNullOrEmpty(jwt))
            {
                return BadRequest(new { message = "Token is required." });
            }

            // Initialize the JWT handler to process the token
            var tokenHandler = new JwtSecurityTokenHandler();

            // Retrieve the secret key used to validate the token signature
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is not configured.");
            }

            // Convert the secret key string to a byte array
            var key = Encoding.UTF8.GetBytes(jwtKey);

            try
            {
                // Validate the JWT token using the specified parameters
                tokenHandler.ValidateToken(jwt, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, // Ensure the signature is valid
                    IssuerSigningKey = new SymmetricSecurityKey(key), // Use the configured key
                    ValidateIssuer = true, // Validate the token's issuer
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true, // Validate the token's audience
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true // Ensure the token hasn't expired
                }, out SecurityToken validatedToken);

                // Cast the validated token to a JwtSecurityToken
                var jwtToken = (JwtSecurityToken)validatedToken;

                // Extract the 'sub' (username) claim from the token
                var username = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;

                // Return a success response with the username from the token
                await Task.CompletedTask;
                return Ok(new
                {
                    message = "Token is valid.",
                    username
                });
            }
            catch (SecurityTokenException ex)
            {
                // Handle token validation failures (invalid signature, expired, etc.)
                return Unauthorized(new { message = "Invalid token.", error = ex.Message });
            }
            catch (Exception ex)
            {
                // Handle other unexpected errors
                return BadRequest(new { message = "An error occurred.", error = ex.Message });
            }
        }

        [HttpPost("Login")]
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
        
        [HttpPost("Forgot")]
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

            var htmlContent = $@"
                <html>
                  <body style='font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 20px;'>
                    <div style='max-width: 600px; margin: auto; background-color: #ffffff; padding: 30px; border-radius: 10px; box-shadow: 0 2px 8px rgba(0,0,0,0.05);'>
                      <h2 style='color: #2c3e50;'>Solicitud para restablecer contraseña</h2>
                      <p>Hola,</p>
                      <p>Hemos recibido una solicitud para restablecer la contraseña de tu cuenta en <strong>CheckCarsAPI</strong>.</p>
      
                      <p><strong>Tu código para restablecer la contraseña es:</strong></p>
                      <div style='padding: 15px; background-color: #f0f0f0; border-radius: 6px; font-size: 18px; font-weight: bold; text-align: center; letter-spacing: 1px;'>
                        {token}
                      </div>

                      <p>Este código es válido por los próximos <strong>15 minutos</strong>.</p>
                      <p>Si tú no solicitaste este cambio, puedes ignorar este mensaje con seguridad.</p>

                      <br />
                      <p style='color: #888;'>— El equipo de CheckCarsAPI</p>
                    </div>
                  </body>
                </html>";

            // Enviar el enlace por correo (implementa un servicio de correo aquí)
            await _emailService.SendEmailAsync(email, "🔐 Restablece tu contraseña - CheckCarsAPI", htmlContent);

            return Ok(new { message = "Password reset link has been sent to your email." });
        }

        [HttpGet("Echo")]
        public Task<IActionResult> Echo()
        {
            return Task.FromResult<IActionResult>(Ok());
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


        [Authorize]
        [HttpGet]
        public IActionResult Verificar()
        {
            var nombre = User.Identity?.Name;
            var autenticado = User.Identity?.IsAuthenticated;
            var roles = string.Join(", ", User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value));
            return Content($"Usuario: {nombre}, Autenticado: {autenticado}, Roles: {roles}");
        }
        #region  Private Methods


        private async Task AssignRol(UserApp user)
        {
            var role = await _userManager.GetRolesAsync(user);
            if (role.Count == 0)
            {
                if (_dbContext.Users.Count() == 1)
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }
            }
        }

        /// <summary>
        /// Generates a JSON Web Token (JWT) for the specified user.
        /// </summary>
        /// <param name="user">The user for whom the token is being generated.</param>
        /// <returns>A JWT token string.</returns>
        private string GenerateJwtToken(CheckCarsAPI.Models.UserApp user)
        {
            try
            {
                var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

                var userRoles = _userManager.GetRolesAsync(user).Result;

                foreach (var role in userRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                string jwtKey = _configuration["Jwt:Key"] ?? string.Empty;

                if (string.IsNullOrEmpty(jwtKey))
                {
                    throw new InvalidOperationException("JWT Key is not configured.");
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(12),
                    signingCredentials: creds);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Token generation error: " + ex.Message);
                throw;
            }
        }


        public class ResetClass
        {
            public string email { get; set; } = string.Empty;
            public string token { get; set; } = string.Empty;
            public string password { get; set; } = string.Empty;
        }

        public class RegisterModel
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
        public class LoginModel
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        #endregion
    }
}
