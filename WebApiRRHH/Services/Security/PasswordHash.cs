using System.Security.Cryptography;
using System.Text;

namespace WebApiRRHH.Services.Security
{
    public interface IPasswordHash
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        string GenerateSecureToken(int length = 32);
    }

    public class PasswordHash : IPasswordHash
    {
        private const int WorkFactor = 12; // Factor de costo para BCrypt

        /// <summary>
        /// Hash de contraseña usando BCrypt
        /// BCrypt es más seguro que SHA256 porque:
        /// 1. Es adaptativo (puede aumentar el costo computacional)
        /// 2. Incluye salt automáticamente
        /// 3. Es resistente a ataques de rainbow tables
        /// </summary>
        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));

            // Usar BCrypt para hash de contraseñas
            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        /// <summary>
        /// Verifica si una contraseña coincide con su hash
        /// </summary>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Genera un token seguro aleatorio para reset de contraseña
        /// </summary>
        public string GenerateSecureToken(int length = 32)
        {
            using var rng = RandomNumberGenerator.Create();
            var tokenData = new byte[length];
            rng.GetBytes(tokenData);

            return Convert.ToBase64String(tokenData)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }

    /// <summary>
    /// Servicio para encriptación de datos sensibles (AES)
    /// Usado para encriptar información adicional, no contraseñas
    /// </summary>
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }

    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public EncryptionService(IConfiguration configuration)
        {
            // En producción, estas claves deben venir de Azure Key Vault o similar
            var encryptionKey = configuration["Encryption:Key"]
                ?? throw new InvalidOperationException("Encryption key not configured");

            // Generar key e IV desde la clave configurada
            using var sha256 = SHA256.Create();
            _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));

            // IV debe ser de 16 bytes para AES
            _iv = sha256.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey)).Take(16).ToArray();
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
                return plainText;

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return Convert.ToBase64String(cipherBytes);
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
                return cipherText;

            try
            {
                using var aes = Aes.Create();
                aes.Key = _key;
                aes.IV = _iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor();
                var cipherBytes = Convert.FromBase64String(cipherText);
                var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

                return Encoding.UTF8.GetString(plainBytes);
            }
            catch
            {
                // Si falla la desencriptación, retornar el texto original
                return cipherText;
            }
        }
    }
}