using System;
using System.Collections.Generic;

namespace Netzon.Api.Entities
{
    public class User : BaseEntity
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public User()
        {
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public bool Deleted {get; set;}
        public DateTime CreatedOn {get;set;}
        public DateTime LastLoginDate {get;set;}
        public int UserRoleId {get; set;}
    }
}