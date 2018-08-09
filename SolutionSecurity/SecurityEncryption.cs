using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace SolutionSecurity
{
    public class SecurityEncryption
    {
        private const int bits = 8;
        private const string StrPermutation = "axdwoutiyqf";
        private const int BytePermutation1 = 0x19;
        private const int BytePermutation2 = 0x59;
        private const int BytePermutation3 = 0x17;
        private const int BytePermutation4 = 0x41;
        private const int BytePermutation5 = 0x39;
        private const int BytePermutation6 = 0x13;
        private const int BytePermutation7 = 0x29;
        private const int BytePermutation8 = 0x71;

        public static string Encrypt(string data)
        {
            return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(data)));
        }

        public static string Decrypt(string data)
        {
            try
            {
                return Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(data)));
            }
            catch
            {
                return null;
            }
        }

        public static byte[] Encrypt(byte[] strData)
        {
            Rfc2898DeriveBytes passBytes = new Rfc2898DeriveBytes(StrPermutation,
                new byte[] {
                    BytePermutation1,
                    BytePermutation2,
                    BytePermutation3,
                    BytePermutation4,
                    BytePermutation5,
                    BytePermutation6,
                    BytePermutation7,
                    BytePermutation8
                });

            MemoryStream memoryStream = new MemoryStream();
            Aes aes = new AesManaged();

            aes.Key = passBytes.GetBytes(aes.KeySize / bits);
            aes.IV = passBytes.GetBytes(aes.BlockSize / bits);

            CryptoStream cryptoStream = new CryptoStream(
                memoryStream,
                aes.CreateEncryptor(),
                CryptoStreamMode.Write);

            cryptoStream.Write(strData, 0, strData.Length);
            cryptoStream.Close();

            return memoryStream.ToArray();
        }

        public static byte[] Decrypt(byte[] strData)
        {
            Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(
                StrPermutation,
                new byte[] {
                    BytePermutation1,
                    BytePermutation2,
                    BytePermutation3,
                    BytePermutation4,
                    BytePermutation5,
                    BytePermutation6,
                    BytePermutation7,
                    BytePermutation8
            });

            MemoryStream memoryStream = new MemoryStream();
            Aes aes = new AesManaged();

            aes.Key = deriveBytes.GetBytes(aes.KeySize / bits);
            aes.IV = deriveBytes.GetBytes(aes.BlockSize / bits);

            CryptoStream cryptoStream = new CryptoStream(
                memoryStream,
                aes.CreateDecryptor(),
                CryptoStreamMode.Write);

            cryptoStream.Write(strData, 0, strData.Length);
            cryptoStream.Close();

            return memoryStream.ToArray();
        }
    }
}

