using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Netzon.Api.Services;
using AutoMapper;
using Netzon.Api.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;
using Netzon.Api.Entities;
using System.Threading.Tasks;

namespace Netzon.Api.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private IMapper _mapper;
        private IConfiguration _config;

        public UsersController(IUserService userService,
            IMapper mapper,
            IConfiguration config)
        {
            this._userService = userService;
            this._mapper = mapper;
            this._config = config;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Register([FromBody]UserDTO userDTO)
        {
            IActionResult response = Unauthorized();

            if (string.IsNullOrEmpty(userDTO.Username))
                return BadRequest(new { message = "Username is missing" });

            if (_userService.IsRegistered(userDTO.Username))
                return BadRequest(new { message = "Username \"" + userDTO.Username + "\" is already taken" });

            if (string.IsNullOrEmpty(userDTO.FirstName) || string.IsNullOrEmpty(userDTO.LastName))
                return BadRequest(new { message = "First name and last name are required" });

            userDTO.Password = string.IsNullOrEmpty(userDTO.Password) ? "123456" : userDTO.Password;
            var user = _userService.Create(userDTO);

            if (user == null)
                return BadRequest(new { message = "User register not successfully" });

            // return basic user info (without password) and token to store client side
            response = Ok(userDTO);

            return response;
        }

        [HttpGet("{id}")]
        public ActionResult<UserDTO> GetUser(int id)
        {
            var user = _userService.GetById(id);

            if (user == null)
            {
                return NotFound();
            }

            return _mapper.Map<UserDTO>(user);
        }

        private string BuildToken(User user)
        {
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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