using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bonzer.Propman.App.Suprema
{
    public class AccessGroupCreateDto
    {
        public AccessGroup AccessGroup { get; set; }
    }

    public class AccessGroup
    {
        public string name { get; set; }
        public string description { get; set; }
        public List<UserIdDto> users { get; set; }
        public List<IdDto> user_groups { get; set; }
        public List<IdDto> access_levels { get; set; }
        public List<FloorLevel> floor_levels { get; set; }
    }

    public class FloorLevel
    {
        public string id { get; set; }
        public string name { get; set; }

        [JsonProperty("$$hashKey")]
        public string hashKey { get; set; }
    }
}