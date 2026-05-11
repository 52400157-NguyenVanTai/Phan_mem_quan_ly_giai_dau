using System;
using System.Security.Cryptography;

namespace DAL
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public static string HashPassword(string password)
        {
            byte[] salt = new byte[SaltSize];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = Hash(password, salt, Iterations);

            return string.Format("PBKDF2${0}${1}${2}", Iterations, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
            {
                return false;
            }

            if (storedHash.StartsWith("$2a$") || storedHash.StartsWith("$2b$") || storedHash.StartsWith("$2y$"))
            {
                return BCrypt.Net.BCrypt.Verify(password, storedHash);
            }

            string[] parts = storedHash.Split('$');
            if (parts.Length != 4 || parts[0] != "PBKDF2")
            {
                return false;
            }

            int iterations;
            if (!int.TryParse(parts[1], out iterations))
            {
                return false;
            }

            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expectedHash = Convert.FromBase64String(parts[3]);
            byte[] actualHash = Hash(password, salt, iterations);

            return FixedTimeEquals(actualHash, expectedHash);
        }

        private static byte[] Hash(string password, byte[] salt, int iterations)
        {
            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(HashSize);
            }
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            int diff = 0;
            for (int i = 0; i < left.Length; i++)
            {
                diff |= left[i] ^ right[i];
            }

            return diff == 0;
        }
    }
}
