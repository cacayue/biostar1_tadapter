using System;
using System.Collections.Generic;
using System.Text;

namespace Bonzer.Propman.App.Suprema
{
    public class UserId
    {
        public string user_id { get; set; }
        public string name { get; set; }
    }

    public class UserIdDto
    {
        public int user_id { get; set; }
    }

    public class IdDto
    {
        public int id { get; set; }
    }

    public class IdStringDto
    {
        public string id { get; set; }
    }

    public class IdNameDto
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class IdNameDescDto
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

    public class PermissionDetail
    {
        public string read { get; set; }
        public string write { get; set; }
    }
}