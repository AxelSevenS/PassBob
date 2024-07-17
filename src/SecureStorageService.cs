namespace PassBob;

using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

public static class SecureStorageService {
	private static readonly string MasterKeyKey = "MasterKey";

	public static async Task SetMasterKeyAsync(string masterKey) {
		if (string.IsNullOrWhiteSpace(masterKey)) {
			SecureStorage.Remove(MasterKeyKey);
			return;
		}
		await SecureStorage.SetAsync(MasterKeyKey, EncryptionHelper.Encrypt(masterKey, Encoding.UTF8.GetBytes(masterKey)));
	}

	public static async Task<string?> GetEncryptedMasterKeyAsync() {
		return await SecureStorage.GetAsync(MasterKeyKey);
	}
}
