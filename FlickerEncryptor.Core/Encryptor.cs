using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FlickerEncryptor.Client
{
    public enum AESDepth
    {
        AES128 = 128,
        AES192 = 192,
        AES256 = 256
    }
    public class AESEncrypt
    {
        private const int _iteration = 10173;
        private const int _blockSize = 128;
        private const int _saltDepth = 16;

        public byte[] Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes, AESDepth keyDepth)
        {
            byte[] encryptedBytes = null;

            byte[] saltBytes = GenerateSalt(16);

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);

                    AES.KeySize = (int)keyDepth;
                    AES.BlockSize = _blockSize;
                    AES.Key = key.GetBytes(AES.KeySize);
                    AES.GenerateIV();
                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.WriteByte((byte)keyDepth);
                        cs.WriteByte((byte)_saltDepth);
                        cs.Write(saltBytes, 0, saltBytes.Length);
                        cs.WriteByte((byte)AES.IV.Length);
                        cs.Write(AES.IV, 0, AES.IV.Length);
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        public byte[] Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;


            using (MemoryStream ms = new MemoryStream(bytesToBeDecrypted))
            {

                var keyDepth = ms.ReadByte();

                //Read Salt
                var saltDepth = ms.ReadByte();
                byte[] salt = new byte[saltDepth];
                ms.Read(salt, 0, saltDepth);

                //Read IV
                var ivDepth = ms.ReadByte();
                byte[] iv = new byte[ivDepth];
                ms.Read(iv, 0, ivDepth);

                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    var key = new Rfc2898DeriveBytes(passwordBytes, salt, _iteration);

                    AES.KeySize = (int)keyDepth;
                    AES.BlockSize = _blockSize;
                    AES.Key = key.GetBytes(AES.KeySize);
                    AES.IV = iv;
                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }


        public void EncryptFile(string filename, string outfilename, string psw)
        {
            string file = filename;
            string password = psw;

            byte[] bytesToBeEncrypted = File.ReadAllBytes(file); //read bytes to encrypt them 
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password); //read with UTF8 encoding the password.
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes); //hash the psw

            //byte[] bytesEncrypted = Encrypt(bytesToBeEncrypted, passwordBytes);

            string fileEncrypted = outfilename;

            //File.WriteAllBytes(fileEncrypted, bytesEncrypted);
        }
        public void DecryptFile(string filename, string outfilename, string psw)
        {
            string fileEncrypted = filename;
            string password = psw;

            byte[] bytesToBeDecrypted = File.ReadAllBytes(fileEncrypted);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            //byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

            string file = outfilename;
            //File.WriteAllBytes(file, bytesDecrypted);
        }

        private byte[] GenerateSalt(int length)
        {
            var salt = new byte[length];
            using (var random = new RNGCryptoServiceProvider())
            {
                random.GetNonZeroBytes(salt);
                return salt;
            }
        }

    }
}
