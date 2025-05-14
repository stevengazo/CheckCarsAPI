using CheckCarsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CheckCarsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager,User,Guest")]
    public class RolesController : ControllerBase
    {
        #region Private Fields

        private readonly UserManager<CheckCarsAPI.Models.UserApp> _userManager;
        private readonly SignInManager<CheckCarsAPI.Models.UserApp> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        #endregion

        #region Constructors

        public RolesController
            (
            UserManager<CheckCarsAPI.Models.UserApp> userM,
            SignInManager<CheckCarsAPI.Models.UserApp> SignInM,
            RoleManager<IdentityRole> roleM
            )
        {
            _roleManager = roleM;
            _userManager = userM;
            _signInManager = SignInM;
        }

        #endregion

        #region endpoints
        [HttpGet("GetRoles")]
        public IActionResult GetRoles()
        {
            var roles = _roleManager.Roles.ToDictionary(e=>e.Id,e=>e.Name);
            return Ok(roles);
        }

        [HttpGet("GetUserRoles/{id}")]
        public async Task<IActionResult> GetUserRoles(string id)
        {
            var user = _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return null;
            }
            var roles =await _userManager.GetRolesAsync(user.Result);
            if (roles == null)
            {
                return null;
            }
            return Ok(roles);
        }

        [HttpPost("")]
        public IActionResult PostRoles(SetUserRole userRole)
        {
            try
            {
                UserApp? user = _userManager.FindByIdAsync(userRole.UserId).Result;
                if (user == null)
                {
                    return NotFound("User not found");
                }
                IdentityRole? rol = _roleManager.FindByIdAsync(userRole.RoleId).Result;
                if (rol == null)
                {
                    return NotFound("Role not found");
                }
                if(rol != null && user != null)
                {
                    var result = _userManager.AddToRoleAsync(user, rol.Name).Result;

                }
                else
                {
                    return NotFound("User or Role not found");
                }
                return Ok("Role assigned successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete()]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult DeleteRoles(SetUserRole userRole)
        {
            try
            {
                UserApp? user = _userManager.FindByIdAsync(userRole.UserId).Result;
                if (user == null)
                {
                    return NotFound("User not found");
                }
                IdentityRole? rol = _roleManager.FindByIdAsync(userRole.RoleId).Result;
                if (rol == null)
                {
                    return NotFound("Role not found");
                }
                if (rol != null && user != null)
                {
                    var result = _userManager.RemoveFromRoleAsync(user, rol.Name).Result;
                }
                else
                {
                    return NotFound("User or Role not found");
                }
                return Ok("Role removed successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
   
    }


    public class SetUserRole
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }
    }
}