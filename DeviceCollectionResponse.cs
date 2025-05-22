using System;
using System.Collections.Generic;

namespace Bonzer.Propman.App.Suprema;

public class DeviceCollectionResponse
{
    public DeviceCollection DeviceCollection { get; set; }
    public ResponseDto Response { get; set; }
}

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Authentication
    {
        public string enable_private_auth { get; set; }
        public string enable_server_matching { get; set; }
        public string matching_timeout { get; set; }
        public string enable_full_access { get; set; }
        public string enable_face_group_matching { get; set; }
        public string auth_timeout { get; set; }
        public string enable_global_apb { get; set; }
        public string global_apb_fail_action { get; set; }
        public string face_detection_level { get; set; }
        public List<OperationMode> operation_modes { get; set; }
    }

    public class AuxInput
    {
        public string port_index { get; set; }
        public string type { get; set; }
        public string enabled { get; set; }
        public string config { get; set; }
        public string aux_index { get; set; }
        public string switch_type { get; set; }
    }

    public class Barcode
    {
        public string use_barcode { get; set; }
        public string scan_timeout { get; set; }
        public string use_visual_barcode { get; set; }
        public string camera_timeout { get; set; }
        public string motion_sensor { get; set; }
    }

    public class Buzzer
    {
        public string count { get; set; }
        public List<Signal> signals { get; set; }
    }

    public class Card
    {
        public string byte_order { get; set; }
        public string smart_card_byte_order { get; set; }
        public string use_wiegand_format { get; set; }
        public string use_csn { get; set; }
        public string use_em { get; set; }
        public string use_mifare_felica { get; set; }
        public string use_wiegand { get; set; }
        public string use_iclass { get; set; }
        public string use_HIDprox { get; set; }
        public string use_smart { get; set; }
        public string use_classic_plus { get; set; }
        public string use_desfire_ev1 { get; set; }
        public string use_SR_SE { get; set; }
        public string use_SEOS { get; set; }
        public string use_mobile { get; set; }
        public string use_NFC { get; set; }
        public string use_BLE { get; set; }
    }

    public class Channel
    {
        public string index { get; set; }
        public string mode { get; set; }
        public string baudrate { get; set; }
    }

    public class DeviceCollection
    {
        public string total { get; set; }
        public List<Device> rows { get; set; }
    }

    


    

    public class Display
    {
        public string language { get; set; }
        public string background { get; set; }
        public string background_theme { get; set; }
        public string menu_timeout { get; set; }
        public string message_timeout { get; set; }
        public string backlight_timeout { get; set; }
        public string display_datetime { get; set; }
        public string use_screen_saver { get; set; }
        public string time_format { get; set; }
        public string volume { get; set; }
        public string use_voice { get; set; }
        public string device_private_message { get; set; }
        public string server_private_message { get; set; }
        public string date_type { get; set; }
    }

    public class Dtmf
    {
        public string mode { get; set; }
        public string exit_button { get; set; }
    }

    public class EventFilter
    {
        public string rows { get; set; }
        public string total { get; set; }
    }

    public class Face
    {
        public string duplicate_check { get; set; }
        public string security_level { get; set; }
        public string sensitivity { get; set; }
        public string use_template_image { get; set; }
        public string proximity_level { get; set; }
        public string scan_wait_time { get; set; }
        public string max_rotation { get; set; }
        public string face_width_min { get; set; }
        public string face_width_max { get; set; }
        public string search_range_x { get; set; }
        public string search_range_width { get; set; }
        public string detection_distance_min { get; set; }
        public string detection_distance_max { get; set; }
        public string wide_search { get; set; }
        public string lfd_level { get; set; }
        public string quick_enrollment { get; set; }
        public string preview_option { get; set; }
        public string operation_mode { get; set; }
    }

    public class Fingerprint
    {
        public string security_level { get; set; }
        public string fast_mode { get; set; }
        public string sensitivity { get; set; }
        public string show_image { get; set; }
        public string scan_timeout { get; set; }
        public string detect_afterimage { get; set; }
        public string template_format { get; set; }
        public string sensor_mode { get; set; }
        public string advanced_enrollment { get; set; }
        public string duplicate_check { get; set; }
    }

    public class FormatId
    {
        public string id { get; set; }
    }

    public class Input
    {
        //public SupervisedInput supervised_inputs { get; set; }
        public string input_port_num { get; set; }
        public string support_aux_input { get; set; }
        public List<AuxInput> aux_inputs { get; set; }
        public InputConfig input_config { get; set; }
    }

    public class InputConfig
    {
        public string input_port_num { get; set; }
    }

    public class InputDeviceId
    {
        public string id { get; set; }
        public string name { get; set; }
        public IdNameDto device_type_id { get; set; }
        public Input input { get; set; }
        public EventFilter event_filter { get; set; }
        public Voip voip { get; set; }
    }

    public class InputEventId
    {
        public string code { get; set; }
        public string name { get; set; }
    }

    public class InputScheduleId
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class IntelligentInfo
    {
        public string supportIConfig { get; set; }
        public string useFailCode { get; set; }
        public string cardFormat { get; set; }
        public string osdpID { get; set; }
    }

    public class Lan
    {
        public string enable_dhcp { get; set; }
        public string ip { get; set; }
        public string gateway { get; set; }
        public string subnet_mask { get; set; }
        public string device_port { get; set; }
        public string server_port { get; set; }
        public string connection_mode { get; set; }
        public string mtu_size { get; set; }
        public string baseband { get; set; }
        public string dns_addr { get; set; }
    }

    public class Led
    {
        public string count { get; set; }
        public List<Signal> signals { get; set; }
    }

    public class OperationMode
    {
        public string device_id { get; set; }
        public string mode { get; set; }
        public ScheduleId schedule_id { get; set; }
    }

    public class Outbound
    {
        public string use_prox_ser { get; set; }
        public string prox_ip_addrs { get; set; }
        public string prox_ser_port { get; set; }
    }

    public class OutputDeviceId
    {
        public string id { get; set; }
        public string name { get; set; }
        public IdNameDto device_type_id { get; set; }
    }

    public class OutputSignal
    {
        public string status { get; set; }
        public Led led { get; set; }
        public Buzzer buzzer { get; set; }
    }

    public class OutputSignalId
    {
        public string id { get; set; }
        public string name { get; set; }
        public string delay { get; set; }
        public string count { get; set; }
        public string on_duration { get; set; }
        public string off_duration { get; set; }
    }

    public class ParentDeviceId
    {
        public string id { get; set; }
        public string name { get; set; }
        public IdNameDto device_type_id { get; set; }
    }

   


    public class Device
    {
        public string id { get; set; }
        public string name { get; set; }
        public IdNameDto device_type_id { get; set; }
        public string status { get; set; }
        public Rs485 rs485 { get; set; }
        public IdNameDto device_group_id { get; set; }
        public Version version { get; set; }
        public Lan lan { get; set; }
        public Authentication authentication { get; set; }
        public Fingerprint fingerprint { get; set; }
        public Face face { get; set; }
        public Card card { get; set; }
        public Display display { get; set; }
        public System system { get; set; }
        public string dst1 { get; set; }
        public string dst2 { get; set; }
        public List<OutputSignal> output_signals { get; set; }
        public List<TriggerAction> trigger_actions { get; set; }
        public Wiegand wiegand { get; set; }
        public Tna tna { get; set; }
        public Input input { get; set; }
        public Voip voip { get; set; }
        public Barcode barcode { get; set; }
        public string pktversion { get; set; }
        public string support_occupancy { get; set; }
        public List<SlaveDevice> slave_devices { get; set; }
        public Rtsp rtsp { get; set; }
        public string print_status { get; set; }
        public ParentDeviceId parent_device_id { get; set; }
        public WiegandIo wiegand_io { get; set; }
    }

    public class Rs485
    {
        public string mode { get; set; }
        public List<Channel> channels { get; set; }
        public string parent_rs485_info { get; set; }
        public string connected_channel_index { get; set; }
        public IntelligentInfo intelligentInfo { get; set; }
    }

    public class Rtsp
    {
        public string enabled { get; set; }
    }

    public class ScheduleId
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string use_daily_iteration { get; set; }
        public DateTime start_date { get; set; }
        public string days_of_iteration { get; set; }
    }

    public class Server
    {
        public string address { get; set; }
        public string user_id { get; set; }
        public string password { get; set; }
        public string port { get; set; }
        public string autid { get; set; }
        public string regdur { get; set; }
    }

    public class Signal
    {
        public string color { get; set; }
        public string duration { get; set; }
        public string delay { get; set; }
        public string tone { get; set; }
        public string fadeout { get; set; }
    }

    public class SlaveDevice
    {
        public string id { get; set; }
        public string name { get; set; }
        public IdNameDto device_type_id { get; set; }
        public string status { get; set; }
        public Rs485 rs485 { get; set; }
        public IdNameDto device_group_id { get; set; }
        public ParentDeviceId parent_device_id { get; set; }
        public Version version { get; set; }
        public Lan lan { get; set; }
        public Usb usb { get; set; }
        public Authentication authentication { get; set; }
        public Fingerprint fingerprint { get; set; }
        public Face face { get; set; }
        public Card card { get; set; }
        public Display display { get; set; }
        public System system { get; set; }
        public string dst1 { get; set; }
        public string dst2 { get; set; }
        public Wlan wlan { get; set; }
        public Tna tna { get; set; }
        public Input input { get; set; }
        public EventFilter event_filter { get; set; }
        public Voip voip { get; set; }
        public Barcode barcode { get; set; }
        public string pktversion { get; set; }
        public string support_occupancy { get; set; }
        public Rtsp rtsp { get; set; }
        public List<SlaveDevice> slave_devices { get; set; }
    }

    public class SupervisedInput
    {
        public string port_index { get; set; }
        public string type { get; set; }
        public string enabled { get; set; }
        public string supervised_index { get; set; }
    }

    public class System
    {
        public string timezone { get; set; }
        public string sync_time { get; set; }
        public string interphone_type { get; set; }
        public string enable_clear_on_tamper { get; set; }
        public string use_alphanumeric { get; set; }
        public string camera_frequency { get; set; }
        public string use_card_operation { get; set; }
    }

    public class Tna
    {
        public string mode { get; set; }
        public string required { get; set; }
        public string fixed_code { get; set; }
        public List<TnaKey> tna_keys { get; set; }
    }

    public class TnaKey
    {
        public string enabled { get; set; }
        public string label { get; set; }
        public string icon { get; set; }
    }

    public class TriggerAction
    {
        public string index { get; set; }
        public InputDeviceId input_device_id { get; set; }
        public string input_type { get; set; }
        public string input_port { get; set; }
        public string input_switch { get; set; }
        public string input_duration { get; set; }
        public InputScheduleId input_schedule_id { get; set; }
        public InputEventId input_event_id { get; set; }
        public OutputDeviceId output_device_id { get; set; }
        public OutputSignalId output_signal_id { get; set; }
        public string output_relay { get; set; }
        public string output_function_mask { get; set; }
        public string output_priority { get; set; }
        public string output_type { get; set; }
        public string delay { get; set; }
        public string sound_index { get; set; }
        public string stop_flag { get; set; }
        public string led_color { get; set; }
        public string sound_count { get; set; }
    }

    public class Usb
    {
        public string enable_usb { get; set; }
        public string enable_usb_memory { get; set; }
    }

    public class Version
    {
        public string product_name { get; set; }
        public string hardware { get; set; }
        public string firmware { get; set; }
        public string kernel { get; set; }
        public string firmware_rev { get; set; }
        public string kernel_rev { get; set; }
    }

    public class Voip
    {
        public Server server { get; set; }
        public Dtmf dtmf { get; set; }
        public string phonebook_list { get; set; }
        public Outbound outbound { get; set; }
        public string use_extension_num { get; set; }
        public string speakerVolume { get; set; }
        public string micVolume { get; set; }
        public string use { get; set; }
    }

    public class Wiegand
    {
        public string wiegand_in_out { get; set; }
        public string out_pulse_width { get; set; }
        public string out_pulse_interval { get; set; }
        public string enable_bypass_mode { get; set; }
        public FormatId format_id { get; set; }
        public string enable_fail_code { get; set; }
        public string fail_code { get; set; }
        public string wiegand_output_info { get; set; }
        public List<WiegandInput> wiegand_inputs { get; set; }
        public WiegandCsnId wiegand_csn_id { get; set; }
    }

    public class WiegandBuzzer
    {
        public IdStringDto device_id { get; set; }
        public string output_index { get; set; }
    }

    public class WiegandCsnId
    {
        public string id { get; set; }
    }

    public class WiegandInput
    {
        public string id { get; set; }
    }

    public class WiegandIo
    {
        public WiegandTamper wiegand_tamper { get; set; }
        public object wiegand_leds { get; set; }
        public WiegandBuzzer wiegand_buzzer { get; set; }
    }

    public class WiegandTamper
    {
        public IdStringDto device_id { get; set; }
        public string input_index { get; set; }
        public string switch_type { get; set; }
    }

    public class Wlan
    {
        public string enabled { get; set; }
        public string operation_mode { get; set; }
        public string auth_type { get; set; }
        public string encryption_type { get; set; }
    }

