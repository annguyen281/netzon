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
using System.Collections.Generic;

namespace Netzon.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
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

        /// <summary>
        /// Register to become member
        /// </summary>
        /// <param name="userDTO">User object that need to have username, first name and last name as mandatory. The default password is 123456</param>
        /// <returns></returns>
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

        /// <summary>
        /// Update profile of current logged-in user
        /// </summary>
        /// <param name="userDTO">User object. Cannot update username</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPatch]
        public ActionResult<UserDTO> Update([FromBody]UserDTO userDTO)
        {
            User user = null;
            try 
            {
                if (this.User.Claims.First(i => i.Type.EndsWith("nameidentifier")).Value != userDTO.Id.ToString()) // Not logged-in user
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

        /// <summary>
        /// Get project of logged-in user
        /// </summary>
        /// <param name="id">Id of user</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public ActionResult<UserDTO> GetUser(int id)
        {
            if (this.User.Claims.First(i => i.Type.EndsWith("nameidentifier")).Value != id.ToString()) // Not logged-in user
                return BadRequest(new { message = "Cannot get profile of another user" }); 

            var user = _userService.GetById(id);

            if (user == null)
            {
                return NotFound();
            }

            return _mapper.Map<UserDTO>(user);
        }

        /// <summary>
        /// Get list of users by admin only
        /// </summary>
        /// <param name="query"></param>
        /// <param name="isDeleted"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<List<UserDTO>> GetUsers(string query = "", bool isDeleted = false, int pageIndex = 0, int pageSize = 10)
        {
            if (this.User.Claims.First(i => i.Type == "typ").Value != "1") // Not admin
                return BadRequest(new { message = "Only admin can get users" }); 

            var users = _userService.GetAll(query, isDeleted, pageIndex, pageSize);

            if (users == null)
                return NotFound();

            List<UserDTO> userDTOs = new List<UserDTO>();
            foreach(User user in users)
                userDTOs.Add(_mapper.Map<UserDTO>(user));
            
            return userDTOs;
        }

        /// <summary>
        /// Delete user itself or by admin
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (this.User.Claims.First(i => i.Type.EndsWith("nameidentifier")).Value != id.ToString() && // Not logged-in user
                this.User.Claims.First(i => i.Type == "typ").Value != "1") // Not Admin 
                return BadRequest(new { message = "You only can delete yourself or you are admin" }); 

            _userService.Delete(id);
            
            return Ok();
        }
    }
}