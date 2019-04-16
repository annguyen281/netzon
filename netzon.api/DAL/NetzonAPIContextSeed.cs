using Microsoft.Extensions.Logging;
using Netzon.Api.Entities;
using Netzon.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Netzon.Api.DAL
{
    public class NetzonAPIContextSeed
    {
        private readonly IEncryptionService _encryptionService = new EncryptionService();

        public async Task SeedAsync(NetzonAPIContext context, ILogger<NetzonAPIContextSeed> logger, int? retry = 0)
        {
            int retryForAvaiability = retry.Value;

            try
            {
                    var urAdministrators = new UserRole
                    {
                        Name = "Administrators",
                    };
                    var urRegister = new UserRole
                    {
                        Name = "Register",
                    };

                    IEnumerable<UserRole> userRoles = new List<UserRole>()
                    {
                        urAdministrators,
                        urRegister
                    };
            
                    context.UserRoles.AddRange(userRoles);

                    var user = new User()
                    {
                        FirstName = "Administrator",
                        Username = "admin@netzon.com.se",
                        Email = "admin@netzon.com.se",
                        CreatedOn = DateTime.Now
                    };

                    user.UserRole = urAdministrators;
                    user.PasswordSalt = _encryptionService.CreateSaltKey();
                    user.PasswordHash = _encryptionService.CreatePasswordHash("admin" , user.PasswordSalt);

                    context.Users.Add(user);

                    await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (retryForAvaiability < 10)
                {
                    retryForAvaiability++;
                    
                    logger.LogError(ex.Message,$"There is an error migrating data for NetzonAPIContextSeed");

                    await SeedAsync(context, logger, retryForAvaiability);
                }
            }
        }
    }
}