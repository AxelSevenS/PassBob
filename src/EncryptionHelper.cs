namespace PassBob;
using System.Security.Cryptography;
using System.IO;
using System;

public class EncryptionHelper {
	private const int KeySize = 32; // 256 bits
	private const int Iterations = 10000;

	private static byte[] DeriveKey(byte[] masterKeyBytes, byte[] salt) {
		using Rfc2898DeriveBytes keyDerivationFunction = new(masterKeyBytes, salt, Iterations, HashAlgorithmName.SHA256);
		return keyDerivationFunction.GetBytes(KeySize);
	}

	public static string Encrypt(string text, byte[] masterKeyBytes) {
		if (masterKeyBytes is null) {
			throw new InvalidDataException("Cannot Encrypt or Decrypt while not Authenticated");
		}

		using Aes aesAlg = Aes.Create();
		aesAlg.GenerateIV();
		byte[] iv = aesAlg.IV;
		byte[] salt = new byte[16];
		using (RandomNumberGenerator rng = RandomNumberGenerator.Create()) {
			rng.GetBytes(salt);
		}

		aesAlg.Key = DeriveKey(masterKeyBytes, salt);
		ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, iv);

		using MemoryStream msEncrypt = new();
		msEncrypt.Write(iv, 0, iv.Length);
		msEncrypt.Write(salt, 0, salt.Length);
		using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write)) {
			using StreamWriter swEncrypt = new(csEncrypt);
			swEncrypt.Write(text);
		}

		return Convert.ToBase64String(msEncrypt.ToArray());
	}

	public static bool TryDecrypt(string cipherText, byte[] masterKeyBytes, out string decrypted) {
		if (masterKeyBytes is null) {
			throw new InvalidDataException("Cannot Encrypt or Decrypt while not Authenticated");
		}

		try {
			byte[] fullCipher = Convert.FromBase64String(cipherText);

			using Aes aesAlg = Aes.Create();
			byte[] iv = new byte[aesAlg.BlockSize / 8];
			byte[] salt = new byte[16];
			byte[] cipher = new byte[fullCipher.Length - iv.Length - salt.Length];

			Array.Copy(fullCipher, iv, iv.Length);
			Array.Copy(fullCipher, iv.Length, salt, 0, salt.Length);
			Array.Copy(fullCipher, iv.Length + salt.Length, cipher, 0, cipher.Length);

			aesAlg.Key = DeriveKey(masterKeyBytes, salt);
			aesAlg.IV = iv;
			ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

			using MemoryStream msDecrypt = new(cipher);
			using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
			using StreamReader srDecrypt = new(csDecrypt);

			decrypted = srDecrypt.ReadToEnd();
			return true;
		}
		catch (CryptographicException) {
			decrypted = string.Empty;
			return false;
		}
	}
}