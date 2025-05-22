using System;
using System.Collections.Generic;
using System.Text;

namespace Bonzer.Propman.App.Suprema
{
    public class UserGroupCreateDto
    {
        public UserGroupCreate UserGroup { get; set; }
    }

    public class UserGroupCreate
    {
        public IdDto parent_id { get; set; }
        public int depth { get; set; }
        public string name { get; set; }
    }
}