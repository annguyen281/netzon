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

        string CreateHash(byte[] data);
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
            return CreateHash(Encoding.UTF8.GetBytes(string.Concat(password, saltkey)));
        }

        public virtual string CreateHash(byte[] data)
        {
            var algorithm = HashAlgorithm.Create("SHA512");
            if (algorithm == null)
                throw new ArgumentException("Unrecognized hash name");

            var hashByteArray = algorithm.ComputeHash(data);
            return BitConverter.ToString(hashByteArray).Replace("-", "");
        }
    }
}