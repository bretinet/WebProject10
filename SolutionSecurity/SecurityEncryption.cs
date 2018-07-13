using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SolutionSecurity
{
    public class SecurityEncryption
    {
        private const string StrPermutation = "axdwoutiyqf";
        private const int BytePermutation1 = 0x19;
        private const int BytePermutation2 = 0x59;
        private const int BytePermutation3 = 0x17;
        private const int BytePermutation4 = 0x41;
        private const int BytePermutation5 = 0x39;
        private const int BytePermutation6 = 0x13;
        private const int BytePermutation7 = 0x29;
        private const int BytePermutation8 = 0x71;



        public static string Encrypt(string strData)
        {

            return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(strData)));
        }


        // decoding
        public static string Decrypt(string strData)
        {
            return Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(strData)));
        }



        public static byte[] Encrypt(byte[] strData)
        {
            var passbytes = new Rfc2898DeriveBytes(StrPermutation,
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

            var memstream = new MemoryStream();
            Aes aes = new AesManaged();
            aes.Key = passbytes.GetBytes(aes.KeySize / 8);

            aes.IV = passbytes.GetBytes(aes.BlockSize / 8);


            var cryptostream = new CryptoStream(
                                    memstream,
                                    aes.CreateEncryptor(),
                                    CryptoStreamMode.Write);

            cryptostream.Write(strData, 0, strData.Length);
            cryptostream.Close();

            return memstream.ToArray();
        }

        public static byte[] Decrypt(byte[] strData)
        {
            var passbytes =
            new Rfc2898DeriveBytes(StrPermutation,
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

            var memstream = new MemoryStream();
            Aes aes = new AesManaged();
            aes.Key = passbytes.GetBytes(aes.KeySize / 8);
            aes.IV = passbytes.GetBytes(aes.BlockSize / 8);

            var cryptostream = new CryptoStream(memstream,
            aes.CreateDecryptor(), CryptoStreamMode.Write);
            cryptostream.Write(strData, 0, strData.Length);
            cryptostream.Close();
            return memstream.ToArray();
        }
        
    }
}

