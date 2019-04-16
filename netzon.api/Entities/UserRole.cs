using System.Collections.Generic;

namespace Netzon.Api.Entities
{
    public partial class UserRole : BaseEntity
    {
        private ICollection<User> _users;

        public string Name { get; set; }

        public virtual ICollection<User> Users
        {
            get { return _users ?? (_users = new List<User>()); }
            set { _users = value; }
        }
    }
}