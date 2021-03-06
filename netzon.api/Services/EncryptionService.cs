using System;
using System.Security.Cryptography;
using System.Text;

namespace Netzon.Api.Services
{
    /// <summary>
    /// Encryption service
    /// </summary>
    public interface IEncryptionService
    {
        string CreateSaltKey();

        string CreatePasswordHash(string password, string saltkey);

        bool VerifyPasswordHash(string passsword, string hashedPassword, string passwordSalt);
    }

    public class EncryptionService : IEncryptionService
    {
        private const int SALT_KEY_SIZE = 5;
        public EncryptionService()
        {
        }

        public virtual string CreateSaltKey()
        {
            //generate a cryptographic random number
            using (var provider = new RNGCryptoServiceProvider())
            {
                var buff = new byte[SALT_KEY_SIZE];
                provider.GetBytes(buff);

                // Return a Base64 string representation of the random number
                return Convert.ToBase64String(buff);
            }
        }

        public virtual string CreatePasswordHash(string password, string saltkey)
        {
            byte[] data = Encoding.UTF8.GetBytes(string.Concat(password, saltkey));

            var algorithm = HashAlgorithm.Create("SHA512");
            if (algorithm == null)
                throw new ArgumentException("Unrecognized hash name");

            var hashByteArray = algorithm.ComputeHash(data);

            return BitConverter.ToString(hashByteArray).Replace("-", "");
        }

        public bool VerifyPasswordHash(string password, string passwordHash, string passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            string computedHash = CreatePasswordHash(password, passwordSalt);

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != passwordHash[i]) return false;
            }

            return true;
        }
    }
}