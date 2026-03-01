using System.Security.Cryptography;

namespace Stargazer.Orleans.Utility.Cryptography
{
    public partial class Cryptography
    {

        private class InvalidHashException : Exception
        {
            public InvalidHashException()
            {
            }

            public InvalidHashException(string message)
                : base(message)
            {
            }

            public InvalidHashException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        private class CannotPerformOperationException : Exception
        {
            public CannotPerformOperationException()
            {
            }

            public CannotPerformOperationException(string message)
                : base(message)
            {
            }

            public CannotPerformOperationException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        public class PasswordStorage
        {
            // These constants may be changed without breaking existing hashes.
            public const int SALT_BYTES = 24;

            // The number of bytes of salt. By default, 24 bytes, which is 192 bits. This is more than enough. This constant should not be changed.

            public const int HASH_BYTES = 18;
            public const int PBKDF2_ITERATIONS = 64000; // PBKDF2 迭代次数，默认32000

            /// <summary>
            /// 创建hash+salt后的密码
            /// </summary>
            /// <param name="password">原始密码，比如abcdef</param>
            /// <param name="secret_key">密钥</param>
            /// <returns></returns>
            public static string CreateHash(string password, out string secret_key)
            {
                byte[] salt = new byte[SALT_BYTES];
                // Generate a random salt
                try
                {
                    using (RandomNumberGenerator csprng = RandomNumberGenerator.Create())
                    {
                        csprng.GetBytes(salt);
                    }
                }
                catch (CryptographicException ex)
                {
                    throw new CannotPerformOperationException("Random number generator not available.", ex);
                }
                catch (ArgumentNullException ex)
                {
                    throw new CannotPerformOperationException("Invalid argument given to random number generator.", ex);
                }

                byte[] hash = PBKDF2(password, salt, PBKDF2_ITERATIONS, HASH_BYTES);

                // format: algorithm:iterations:hashSize:salt:hash

                /*
                 *  algorithm is the name of the cryptographic hash function ("sha1").
                    iterations is the number of PBKDF2 iterations ("64000").
                    hashSize is the length, in bytes, of the hash field (after decoding).
                    salt is the salt, base64 encoded.
                    hash is the PBKDF2 output, base64 encoded. It must encode hashSize bytes.
                 *
                 */
                secret_key = Convert.ToBase64String(salt);
                return Convert.ToBase64String(hash);
            }

            /// <summary>
            /// 验证密码是否有效
            /// </summary>
            /// <param name="password">原始密码字符串</param>
            /// <param name="passwordHash">已加密的字符串</param>
            /// <param name="secret_key">密钥</param>
            /// <returns></returns>
            public static bool VerifyPassword(string password, string passwordHash, string secret_key)
            {
                byte[] salt = null;
                try
                {
                    salt = Convert.FromBase64String(secret_key);
                }
                catch (ArgumentNullException ex)
                {
                    throw new CannotPerformOperationException("Invalid argument given to Convert.FromBase64String", ex);
                }
                catch (FormatException ex)
                {
                    throw new InvalidHashException("Base64 decoding of salt failed.", ex);
                }

                byte[] hash = null;
                try
                {
                    hash = Convert.FromBase64String(passwordHash);
                }
                catch (ArgumentNullException ex)
                {
                    throw new CannotPerformOperationException("Invalid argument given to Convert.FromBase64String", ex);
                }
                catch (FormatException ex)
                {
                    throw new InvalidHashException("Base64 decoding of pbkdf2 output failed.", ex);
                }

                byte[] testHash = PBKDF2(password, salt, PBKDF2_ITERATIONS, hash.Length);
                return SlowEquals(hash, testHash);
            }

            private static bool SlowEquals(byte[] a, byte[] b)
            {
                uint diff = (uint)a.Length ^ (uint)b.Length;
                for (int i = 0; i < a.Length && i < b.Length; i++)
                {
                    diff |= (uint)(a[i] ^ b[i]);
                }
                return diff == 0;
            }

            private static byte[] PBKDF2(string password, byte[] salt, int iterations, int outputBytes)
            {
                using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA512))
                {
                    return pbkdf2.GetBytes(outputBytes);
                }
            }
        }
    }
}
