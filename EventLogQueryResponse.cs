using System;
using System.Collections.Generic;

namespace Bonzer.Propman.App.Suprema;

public class EventLogQueryResponse
{
	public EventCollection EventCollection { get; set; }
	public ResponseDto Response { get; set; }
}


public class EventCollection
{
	public List<EventLog> rows { get; set; }
}

public class EventTypeId
{
	public string code { get; set; }
}

public class EventLog
{
	public string id { get; set; }
	public DateTime server_datetime { get; set; }
	public DateTime datetime { get; set; }
	public string index { get; set; }
	public string user_id_name { get; set; }
	public UserId user_id { get; set; }
	public UserGroupId user_group_id { get; set; }
	public IdNameDto device_id { get; set; }
	public EventTypeId event_type_id { get; set; }
	public string is_dst { get; set; }
	public Timezone timezone { get; set; }
	public string user_update_by_device { get; set; }
	public string hint { get; set; }
}

public class Timezone
{
	public string half { get; set; }
	public string hour { get; set; }
	public string negative { get; set; }
}

public class UserGroupId
{
	public string id { get; set; }
	public string name { get; set; }
}


