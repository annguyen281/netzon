using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Netzon.Api.DTOs;
using Netzon.Api.Entities;
using Netzon.Api.Services;
using System.Linq;
using NSwag.Annotations;

namespace Netzon.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Authorize]
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private IUserService _userService;
        private IMapper _mapper;
        private IConfiguration _config;

        public AuthenticationController(IUserService userService,
            IMapper mapper,
            IConfiguration config)
        {
            this._userService = userService;
            this._mapper = mapper;
            this._config = config;
        }

        /// <summary>
        /// Login by username and password
        /// </summary>
        /// <param name="userDTO">User object that need to have username and password as mandatory</param>
        /// <returns>User object with valid token include</returns>
        [AllowAnonymous]
        [HttpPost("login")]
        [SwaggerOperation("Login")]
        public IActionResult Login([FromBody]UserDTO userDTO)
        {
            IActionResult response = Unauthorized();

            var user = _userService.Authenticate(userDTO.Username, userDTO.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            string tokenString = BuildToken(user);

            // return basic user info (without password) and token to store client side
            response = Ok(new
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = tokenString
            });

            return response;
        }

        /// <summary>
        /// Add a new admin by admin only
        /// </summary>
        /// <param name="userDTO">User object that need to have username and password as mandatory</param>
        /// <returns></returns>
        [HttpPost("admin")]
        public ActionResult<UserDTO> Admin([FromBody]UserDTO userDTO)
        {
            try
            {
                if (this.User.Claims.First(i => i.Type == "typ").Value != "1") // Not Admin
                    return BadRequest(new { message = "Only admin user should be able to make other admins" });

                if (string.IsNullOrEmpty(userDTO.Username) || string.IsNullOrEmpty(userDTO.Password))
                    return BadRequest(new { message = "Useranem and Password are required" });

                var user = _userService.Create(userDTO, true);

                if (user == null)
                    return BadRequest(new { message = "Failed to create new admin user" });

                return _mapper.Map<UserDTO>(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private string BuildToken(User user)
        {
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.NameId, Convert.ToString(user.Id)),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Typ, Convert.ToString(user.UserRoleId))
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddMinutes(30),
              signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}