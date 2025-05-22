using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Bonzer.Propman.App.Suprema
{
    public class UserCreateUpdateDto
    {
        public UserCreateUpdate User { get; set; }
    }

    public class UserCreateUpdate
    {
        public string name { get; set; }
        public string user_id { get; set; }
        public IdDto user_group_id { get; set; }
        public bool disabled { get; set; }
        public DateTime start_datetime { get; set; }
        public DateTime expiry_datetime { get; set; }
        public List<IdDto> access_groups { get; set; }
        public int? permission { get; set; }
        public string email { get; set; }
        public string department { get; set; }
        public string user_title { get; set; }
        public string photo { get; set; }
        public string phone { get; set; }
        public string login_id { get; set; }
        public string password { get; set; }
        public string user_ip { get; set; }
    }
}