using System;
using System.Collections.Generic;
using System.Text;

namespace Bonzer.Propman.App.Suprema
{
    public class AccessGroupCollectionDto
    {
        public string total { get; set; }
        public List<AccessGroupDto> rows { get; set; }
    }

    public class AccessGroupDto
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public List<UserId> users { get; set; }
        public string user_count { get; set; }
        public List<IdNameDescDto> access_levels { get; set; }
        public List<UserGroupDto> user_groups { get; set; }
    }
}