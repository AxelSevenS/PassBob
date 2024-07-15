using System.ComponentModel.DataAnnotations.Schema;
using SQLite;

namespace PassBob;

public class Password {

	[PrimaryKey, AutoIncrement]
	public uint Id { get; set; }

	public string Website { get; set; }
	public string Username { get; set; }

	public string EncryptedPassword {
		get => _encryptedPassword;
		set {
			if (PasswordDatabase.MasterKeyBytes is null) return;

			_encryptedPassword = value;
			_decryptedPassword = EncryptionHelper.Decrypt(value, PasswordDatabase.MasterKeyBytes);
		}
	}
	private string _encryptedPassword;

	[NotMapped]
	public string DecryptedPassword {
		get => _decryptedPassword;
		set{
			if (PasswordDatabase.MasterKeyBytes is null) return;

			_decryptedPassword = value;
			_encryptedPassword = EncryptionHelper.Encrypt(value, PasswordDatabase.MasterKeyBytes);
		}
	}
	private string _decryptedPassword;

	[NotMapped]
	public bool IsVisible { get; set; }
}