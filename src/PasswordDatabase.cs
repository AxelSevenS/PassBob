namespace PassBob;

using SQLite;

public class PasswordDatabase {
	private readonly SQLiteAsyncConnection _database;

	public PasswordDatabase() {
		string dbPath = FileSystem.AppDataDirectory;
		Directory.CreateDirectory(dbPath);
		_database = new SQLiteAsyncConnection(Path.Combine(dbPath, "passwords.db3"));
		_database.CreateTableAsync<Password>().Wait();
	}

	public Task<List<Password>> GetPasswordsAsync() {
		return _database.Table<Password>().ToListAsync();
	}

	public Task<int> SavePasswordAsync(Password password) {
		if (password.Id != 0) {
			return _database.UpdateAsync(password);
		}
		else {
			return _database.InsertAsync(password);
		}
	}

	public Task<int> DeletePasswordAsync(Password password) {
		return _database.DeleteAsync(password);
	}
}