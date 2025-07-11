using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;

namespace Yandex.Music.Api.Common
{
    /// <summary>
    /// Класс для шифровки
    /// </summary>
    public class Encryptor
    {
        #region Поля

        private readonly string IV = "encryption";
        private readonly byte[] IVHash;

        private readonly byte[] keyHash;

        private readonly MD5 md5;
        private readonly Aes aesAlg;


        #endregion Поля

        #region Вспомогательные функции

        private byte[] GetHash(string value)
        {
            return md5.ComputeHash(Encoding.UTF8.GetBytes(value));
        }

        #endregion Вспомогательные функции

        #region Основные функции

        public Encryptor(string key)
        {
            md5 = MD5.Create();

            aesAlg = Aes.Create();
            aesAlg.BlockSize = 128;
            aesAlg.Padding = PaddingMode.PKCS7;

            keyHash = GetHash(key);
            IVHash = GetHash(IV);
        }

        public byte[] Encrypt(byte[] data)
        {
            byte[] bytes = default;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(ms, aesAlg.CreateEncryptor(keyHash, IVHash), CryptoStreamMode.Write))
                {

                    csEncrypt.Write(data, 0, data.Length);

                    if (!csEncrypt.HasFlushedFinalBlock)
                        csEncrypt.FlushFinalBlock();
                }

                bytes = ms.ToArray();
            }
            return bytes;
        }

        public byte[] Decrypt(byte[] data)
        {
            byte[] bytes = default;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream csDecrypt = new CryptoStream(ms, aesAlg.CreateDecryptor(keyHash, IVHash), CryptoStreamMode.Write))
                {
                    csDecrypt.Write(data, 0, data.Length);

                    if (!csDecrypt.HasFlushedFinalBlock)
                        csDecrypt.FlushFinalBlock();
                }
                bytes = ms.ToArray();
            }
            return bytes;
        }

        #endregion Основные функции
    }
}