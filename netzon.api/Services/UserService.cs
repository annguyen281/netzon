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
        User Create(UserDTO user);
        bool IsRegistered(string userName);

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

            var user = _context.Users.SingleOrDefault(x => x.Username == username);

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

        public User Create(UserDTO userDTO)
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
                UserRole = new UserRole() { Id = 2 } // 2 = Registers
            };

            _context.Users.Add(user);

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

        public bool IsRegistered(string userName)
        {
            return _context.Users.Any(x => x.Username == userName);
        }
    }
}