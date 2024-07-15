namespace PassBob;

using System.Text;
using SQLite;

public class PasswordDatabase {
	public static string? MasterKey {
		get => _masterKey;
		private set {
			_masterKey = value;
			MasterKeyBytes = value is null ? null : Encoding.UTF8.GetBytes(value);
		}
	}
	private static string? _masterKey;
	public static byte[]? MasterKeyBytes { get; private set; }


	private static readonly SQLiteAsyncConnection _database;


	static PasswordDatabase() {
		string dbPath = FileSystem.AppDataDirectory;
		Directory.CreateDirectory(dbPath);
		_database = new SQLiteAsyncConnection(Path.Combine(dbPath, "passwords.db3"));
		_database.CreateTableAsync<Password>().Wait();
	}

	public static async Task SetMasterKeyDestructive(string newMasterKey) {
		MasterKey = newMasterKey;

		// Clear all passwords from the database
		await _database.DeleteAllAsync<Password>();

		// Save the new master password securely
		await SecureStorageService.SetMasterKeyAsync(newMasterKey);
	}

	public static async Task UpdateMasterKey(string newMasterKey) {
		if (MasterKeyBytes is null) throw new InvalidDataException("Can't Update Master Password when you are not logged in");

		List<Password> passwords = await GetPasswordsAsync();
		byte[] newMasterKeyBytes = Encoding.UTF8.GetBytes(newMasterKey);

		foreach (Password password in passwords) {
			// Decrypt with the old master password
			string decryptedPassword = EncryptionHelper.Decrypt(password.EncryptedPassword, MasterKeyBytes);

			// Encrypt with the new master password
			password.EncryptedPassword = EncryptionHelper.Encrypt(decryptedPassword, newMasterKeyBytes);
		}

		MasterKey = newMasterKey;

		// Save the updated passwords
		foreach (Password password in passwords) {
			await SavePasswordAsync(password);
		}

		// Save the new master password securely
		await SecureStorageService.SetMasterKeyAsync(newMasterKey);
	}

	public static async Task<bool> TryLoginAsync(string key) {
		string? currentEncryptedKey = await SecureStorageService.GetEncryptedMasterKeyAsync();
		if (currentEncryptedKey is null) return false;

		if (key != EncryptionHelper.Decrypt(currentEncryptedKey, Encoding.UTF8.GetBytes(key))) return false;

		MasterKey = key;
		return true;
	}

	public static Task<List<Password>> GetPasswordsAsync() {
		return _database.Table<Password>().ToListAsync();
	}

	public static Task<int> SavePasswordAsync(Password password) {
		if (password.Id != 0) {
			return _database.UpdateAsync(password);
		}
		else {
			return _database.InsertAsync(password);
		}
	}

	public static Task<int> DeletePasswordAsync(Password password) {
		return _database.DeleteAsync(password);
	}
}