using System.ComponentModel.DataAnnotations;

namespace PassBob;

public class LoginModel {
	[Required]
	public string MasterKey { get; set; }
}