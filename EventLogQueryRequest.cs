using System.Collections.Generic;

namespace Bonzer.Propman.App.Suprema;

public class EventLogQueryRequest
{
	public Query Query { get; set; }
}

public class Condition
{
	public string column { get; set; }
	public int @operator { get; set; }
	public List<string> values { get; set; }
}

public class Order
{
	public string column { get; set; }
	public bool descending { get; set; }
}

public class Query
{
	public int limit { get; set; }
	public List<Condition> conditions { get; set; }
	public List<Order> orders { get; set; }
}
