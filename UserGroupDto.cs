using System;
using System.Collections.Generic;
using System.Text;

namespace Bonzer.Propman.App.Suprema
{
    public class UserGroupDto
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public IdNameDto parent_id { get; set; }
        public string depth { get; set; }
        public string inherited { get; set; }
        public string face_count { get; set; }
        public string user_count { get; set; }
        public List<IdNameDto> user_groups { get; set; }
    }
}