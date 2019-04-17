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

namespace Netzon.Api.Controllers
{
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

        [AllowAnonymous]
        [HttpPost("login")]
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

        [HttpPost("admin")]
        public ActionResult<UserDTO> Admin([FromBody]UserDTO userDTO)
        {
            if (this.User.Claims.First(i => i.Type == "IsAdmin").Value != "1") // Not Admin
                return BadRequest(new { message = "Only admin user should be able to make other admins" }); 

            if (string.IsNullOrEmpty(userDTO.FirstName) || string.IsNullOrEmpty(userDTO.LastName) || string.IsNullOrEmpty(userDTO.Password))
                return BadRequest(new { message = "First name, Last name and Password are required" });

            var user = _userService.Create(userDTO, true);

            if (user == null)
                return BadRequest(new { message = "Failed to create new user" });
            
            return _mapper.Map<UserDTO>(user);
        }

        private string BuildToken(User user)
        {
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("IsAdmin", Convert.ToString(user.UserRoleId))
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