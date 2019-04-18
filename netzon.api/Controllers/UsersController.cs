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
using System.Linq;

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
        public ActionResult<UserDTO> Register([FromBody]UserDTO userDTO)
        {
            if (string.IsNullOrEmpty(userDTO.Username))
                return BadRequest(new { message = "Username is missing" });

            if (_userService.GetByUserName(userDTO.Username) != null)
                return BadRequest(new { message = "Username \"" + userDTO.Username + "\" is already taken" });

            if (string.IsNullOrEmpty(userDTO.FirstName) || string.IsNullOrEmpty(userDTO.LastName))
                return BadRequest(new { message = "First name and last name are required" });

            userDTO.Password = string.IsNullOrEmpty(userDTO.Password) ? "123456" : userDTO.Password;
            var user = _userService.Create(userDTO, false);

            if (user == null)
                return BadRequest(new { message = "Failed to create new user" });
            
            return _mapper.Map<UserDTO>(user);
        }

        [AllowAnonymous]
        [HttpPatch]
        public ActionResult<UserDTO> Update([FromBody]UserDTO userDTO)
        {
            User user = null;
            try 
            {
                if (this.User.Claims.First(i => i.Type == "NameId").Value != userDTO.Id.ToString()) // Not logged-in user
                    return BadRequest(new { message = "Cannot update profile of another user" }); 

                user = _userService.Update(userDTO);

                if (user == null)
                    throw new Exception("Failed to update user");
            } 
            catch(Exception ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }

            return _mapper.Map<UserDTO>(user);
        }

        [HttpGet("{id}")]
        public ActionResult<UserDTO> GetUser(int id)
        {
            if (this.User.Claims.First(i => i.Type == "NameId").Value != id.ToString()) // Not logged-in user
                return BadRequest(new { message = "Cannot get profile of another user" }); 

            var user = _userService.GetById(id);

            if (user == null)
            {
                return NotFound();
            }

            return _mapper.Map<UserDTO>(user);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _userService.Delete(id);
            
            return Ok();
        }
    }
}