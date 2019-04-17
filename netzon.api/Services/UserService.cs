using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Netzon.Api.DAL;
using Netzon.Api.DTOs;
using Netzon.Api.Entities;

namespace Netzon.Api.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll();
        User GetById(int id);
        User Create(UserDTO user, bool isAdmin = false);
        void Delete(int id);
        User Update(UserDTO userDTO);
        User GetByUserName(string userName);
    }

    public class UserService : IUserService
    {
        private NetzonAPIContext _context;
        private IEncryptionService _encryptionService;

        public UserService(NetzonAPIContext context, IEncryptionService encryptionService)
        {
            this._context = context;
            this._encryptionService = encryptionService;
        }

        public User Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            var user = GetByUserName(username);

            // check if username exists
            if (user == null)
                return null;

            // check if password is correct
            if (!_encryptionService.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            // authentication successful
            return user;
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users.Where(u => u.Deleted == false);
        }

        public User GetById(int id)
        {
            return _context.Users.Find(id);
        }

        public User Create(UserDTO userDTO, bool isAdmin = false)
        {
            // validation
            if (string.IsNullOrWhiteSpace(userDTO.Password))
                throw new Exception("Password is required");

            if (_context.Users.Any(x => x.Username == userDTO.Username))
                throw new Exception("Username \"" + userDTO.Username + "\" is already taken");

            string passwordSalt = _encryptionService.CreateSaltKey();
            string passwordHash = _encryptionService.CreatePasswordHash(userDTO.Password, passwordSalt);

            User user = new User()
            {
                FirstName = userDTO.FirstName,
                LastName = userDTO.LastName,
                Username = userDTO.Username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Deleted = false,
                CreatedOn = DateTime.Now,
                LastLoginDate = DateTime.MinValue,
                UserRole = new UserRole() { Id = isAdmin ? 1 : 2 }  // 1 = Admins, 2 = Registers
            };

            _context.Users.Add(user);

            _context.SaveChanges();

            return user;
        }

        public User Update(UserDTO userDTO)
        {
            var user = _context.Users.Find(userDTO.Id);

            if (user == null)
                throw new Exception("User not found");

            if (userDTO.Username != user.Username)
            {
                // username has changed so check if the new username is already taken
                if (_context.Users.Any(x => x.Username == userDTO.Username))
                    throw new Exception("Username " + userDTO.Username + " is already taken");
            }

            // update user properties
            user.FirstName = userDTO.FirstName;
            user.LastName = userDTO.LastName;
            user.Username = userDTO.Username;

            // update password if it was entered
            if (!string.IsNullOrWhiteSpace(userDTO.Password))
            {
                user.PasswordSalt = _encryptionService.CreateSaltKey();
                user.PasswordHash = _encryptionService.CreatePasswordHash(userDTO.Password, user.PasswordSalt);
            }

            _context.Users.Update(user);
            _context.SaveChanges();

            return user;
        }

        public void Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                user.Deleted = true;
                _context.Users.Update(user);

                _context.SaveChanges();
            }
        }

        public User GetByUserName(string userName)
        {
            return _context.Users.SingleOrDefault(x => x.Username == userName);
        }
    }
}