using System;
using System.Collections.Generic;
using System.Text;

namespace Bonzer.Propman.App.Suprema
{
    public class UserDto
    {
        public string user_id { get; set; }
        public string name { get; set; }
        public string gender { get; set; }
        public string email { get; set; }
        public string birthday { get; set; }
        public string photo_exists { get; set; }
        public string pin_exists { get; set; }
        public string login_id { get; set; }
        public string password_exists { get; set; }
        public string updated_count { get; set; }
        public string last_modified { get; set; }
        public string idx_last_modified { get; set; }
        public DateTime start_datetime { get; set; }
        public DateTime expiry_datetime { get; set; }
        public string security_level { get; set; }
        public string display_duration { get; set; }
        public string display_count { get; set; }
        public Permission permission { get; set; }
        public string inherited { get; set; }
        public IdNameDto user_group_id { get; set; }
        public string disabled { get; set; }
        public string expired { get; set; }
        public string idx_user_id { get; set; }
        public string idx_user_id_num { get; set; }
        public string idx_name { get; set; }
        public string idx_phone { get; set; }
        public string idx_email { get; set; }
        public string fingerprint_template_count { get; set; }
        public string face_count { get; set; }
        public string visual_face_count { get; set; }
        public string card_count { get; set; }
        public string need_to_update_pw { get; set; }
        public List<IdNameDto> access_groups { get; set; }
    }

    public class Filter
    {
        public List<string> UserGroup { get; set; }
        public List<string> DeviceGroup { get; set; }
        public List<string> DoorGroup { get; set; }
        public List<string> ElevatorGroup { get; set; }
        public List<string> ZoneType { get; set; }
        public List<string> AccessGroup { get; set; }
        public List<string> GraphicMapGroup { get; set; }
    }

    public class Module
    {
        public PermissionDetail Dashboard { get; set; }
        public PermissionDetail User { get; set; }
        public PermissionDetail Device { get; set; }
        public PermissionDetail Door { get; set; }
        public PermissionDetail Elevator { get; set; }
        public PermissionDetail Zone { get; set; }
        public PermissionDetail AccessControl { get; set; }
        public PermissionDetail Monitoring { get; set; }
        public PermissionDetail TA { get; set; }
        public PermissionDetail Setting { get; set; }
        public PermissionDetail Video { get; set; }
        public PermissionDetail Visitor { get; set; }
    }

    public class Permission
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public Filter filter { get; set; }
        public Module module { get; set; }
        //public IdStringDto device { get; set; }
        //public IdStringDto user { get; set; }
    }
}