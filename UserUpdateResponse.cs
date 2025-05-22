namespace Bonzer.Propman.App.Suprema;

public class UserUpdateResponse
{
	public FailedUserCollection FailedUserCollection { get; set; }
	public ResponseDto Response { get; set; }
}
public class FailedUserCollection
{
	public string total { get; set; }
	public string rows { get; set; }
}
