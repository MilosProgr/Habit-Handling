using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using DevHabit.Api.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;

namespace DevHabit.Api.Services;

public class EncryptionService(IOptions<EncryptionOptions> options)
{
    private readonly byte[] _masterkey = Convert.FromBase64String(options.Value.Key);
    private const int IvSize = 16;

    public string Encrypt(string plainText)
    {
        try
        {
            using var aes = System.Security.Cryptography.Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = _masterkey;
            aes.IV = RandomNumberGenerator.GetBytes(IvSize);

            using var memoryStream = new MemoryStream();
            memoryStream.Write(aes.IV, 0, IvSize);

            using (ICryptoTransform encryptor = aes.CreateEncryptor())
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            using (var streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(plainText);
            }
            return Convert.ToBase64String(memoryStream.ToArray());
        }
        catch(CryptographicException ex)
        {
            throw new InvalidOperationException("Enkripcija neuspensa",ex);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("Nevalidan cipher format", ex);
        }


    }

    public string Decrypt(string cipherText)
    {
        try
        {
            byte[] cipherData = Convert.FromBase64String(cipherText);
            if (cipherData.Length < IvSize)
            {
                throw new InvalidOperationException("Invalid cipher text format");
            }

            byte[] iv = new byte[IvSize];
            byte[] encryptedData = new byte[cipherData.Length - IvSize];

            Buffer.BlockCopy(cipherData, 0, iv, 0, IvSize);
            Buffer.BlockCopy(cipherData,IvSize,encryptedData,0,encryptedData.Length);

            using var aes = System.Security.Cryptography.Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = _masterkey;
            aes.IV = iv;

            using MemoryStream memoryStream = new (encryptedData);
            using ICryptoTransform decryptor = aes.CreateDecryptor();
            using CryptoStream cryptoStream = new (memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new(cryptoStream);

            return streamReader.ReadToEnd();
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException("Enkripcija neuspensa", ex);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("Nevalidan cipher format", ex);
        }
        
    }
}
