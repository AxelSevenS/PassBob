using SQLite;

namespace PassBob;

public class Password {
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }
	public string Website { get; set; }
	public string Username { get; set; }
	public string EncryptedPassword { get; set; }
}
