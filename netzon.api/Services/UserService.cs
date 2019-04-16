using System;
using System.Collections.Generic;
using System.Linq;
using Netzon.Api.DAL;
using Netzon.Api.Entities;

namespace Netzon.Api.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll();
        User GetById(int id);
        User Create(User user, string password);
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

        public User Create(User user, string password)
        {
            // validation
            if (string.IsNullOrWhiteSpace(password))
                throw new Exception("Password is required");

            if (_context.Users.Any(x => x.Username == user.Username))
                throw new Exception("Username \"" + user.Username + "\" is already taken");

            user.PasswordSalt = _encryptionService.CreateSaltKey();
            user.PasswordHash = _encryptionService.CreatePasswordHash(password, user.PasswordSalt);

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
    }
}