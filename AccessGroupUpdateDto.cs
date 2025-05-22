using System.Collections.Generic;

namespace Bonzer.Propman.App.Suprema;

public class AccessGroupUpdateDto
{
	public AccessGroupUpdate AccessGroup { get; set; }
}

public class AccessGroupUpdate
{
	public string name { get; set; }
	public string description { get; set; }
	public List<UserIdDto> users { get; set; }
	public List<UserIdDto> delete_users { get; set; }
	public List<IdDto> user_groups { get; set; }
	public List<IdDto> access_levels { get; set; }
	public List<IdDto> floor_levels { get; set; }
}