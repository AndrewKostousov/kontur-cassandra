using System;
using System.Security.Cryptography;
using System.Text;

namespace SKBKontur.Catalogue.Objects
{
    public class TripleDesCrypt
    {
        public TripleDesCrypt(string keyString)
        {
            this.keyString = keyString;
        }

        public string Encrypt(string secret)
        {
            if(secret == null)
                return null;
            var crypt = GetCrypt();
            var encryptor = crypt.CreateEncryptor();
            var secretBytes = Encoding.UTF8.GetBytes(secret);
            var encryptedBytes = encryptor.TransformFinalBlock(secretBytes, 0, secretBytes.Length);
            return Convert.ToBase64String(encryptedBytes);
        }

        public string Decrypt(string encryptedString)
        {
            try
            {
                if(encryptedString == null)
                    return null;
                var crypt = GetCrypt();
                var decryptor = crypt.CreateDecryptor();
                var encryptedBytes = Convert.FromBase64String(encryptedString);
                var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch(Exception)
            {
                return null;
            }
        }

        private SymmetricAlgorithm GetCrypt()
        {
            var crypt = new TripleDESCryptoServiceProvider();
            var split = keyString.Split('|');
            crypt.Key = Convert.FromBase64String(split[0]);
            crypt.IV = Convert.FromBase64String(split[1]);
            return crypt;
        }

        private readonly string keyString;
    }
}