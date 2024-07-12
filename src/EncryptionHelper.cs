using System.Security.Cryptography;
using System.Text;

namespace PassBob;

public static class EncryptionHelper {
	private static readonly byte[] Key = Encoding.UTF8.GetBytes(MauiProgram.Configuration["Encryption:Key"]);

	public static string Encrypt(string text) {
		using Aes aesAlg = Aes.Create();
		aesAlg.Key = Key;
		aesAlg.GenerateIV();
		byte[] iv = aesAlg.IV;
		ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, iv);

		using MemoryStream msEncrypt = new();
		msEncrypt.Write(iv, 0, iv.Length);
		using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write)) {
			using StreamWriter swEncrypt = new(csEncrypt);
			swEncrypt.Write(text);
		}

		return Convert.ToBase64String(msEncrypt.ToArray());
	}

	public static string Decrypt(string cipherText) {
		byte[] fullCipher = Convert.FromBase64String(cipherText);

		using Aes? aesAlg = Aes.Create();
		byte[] iv = new byte[aesAlg.BlockSize / 8];
		byte[] cipher = new byte[fullCipher.Length - iv.Length];

		Array.Copy(fullCipher, iv, iv.Length);
		Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

		aesAlg.Key = Key;
		aesAlg.IV = iv;
		ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

		using MemoryStream msDecrypt = new(cipher);
		using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
		using StreamReader srDecrypt = new(csDecrypt);
		return srDecrypt.ReadToEnd();
	}
}