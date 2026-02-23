using System;
using System.Security.Cryptography;
using System.Text;

namespace TPassMan.DB.Services;

public class EncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(byte[] keyBytes)
    {
        if (keyBytes is null) throw new ArgumentNullException(nameof(keyBytes));
        if (keyBytes.Length != 16 && keyBytes.Length != 24 && keyBytes.Length != 32)
            throw new ArgumentException("Key must be 16, 24 or 32 bytes", nameof(keyBytes));
        _key = keyBytes;
    }

    public string Encrypt(string plaintext)
    {
        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var tag = new byte[16];
        var cipher = new byte[plainBytes.Length];

        using (var aes = new AesGcm(_key, tag.Length))
            aes.Encrypt(nonce, plainBytes, cipher, tag);

        var output = new byte[nonce.Length + tag.Length + cipher.Length];
        Buffer.BlockCopy(nonce, 0, output, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, output, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipher, 0, output, nonce.Length + tag.Length, cipher.Length);

        return Convert.ToBase64String(output);
    }

    public string Decrypt(string encryptedBase64)
    {
        var combined = Convert.FromBase64String(encryptedBase64);

        var nonce = new byte[12];
        var tag = new byte[16];
        var cipher = new byte[combined.Length - nonce.Length - tag.Length];

        Buffer.BlockCopy(combined, 0, nonce, 0, nonce.Length);
        Buffer.BlockCopy(combined, nonce.Length, tag, 0, tag.Length);
        Buffer.BlockCopy(combined, nonce.Length + tag.Length, cipher, 0, cipher.Length);

        var plain = new byte[cipher.Length];
        using (var aes = new AesGcm(_key, tag.Length))
            aes.Decrypt(nonce, cipher, tag, plain);

        return Encoding.UTF8.GetString(plain);
    }
}