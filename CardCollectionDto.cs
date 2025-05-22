using System;
using System.Collections.Generic;
using System.Text;

namespace Bonzer.Propman.App.Suprema
{
    public class CardCollectionDto
    {
        public List<CardDto> rows { get; set; }
        public string total { get; set; }
    }

    public class CardDto
    {
        public string id { get; set; }
        public string card_id { get; set; }
        public string display_card_id { get; set; }
        public string status { get; set; }
        public string is_blocked { get; set; }
        public string is_assigned { get; set; }
        public CardType card_type { get; set; }
        public string mobile_card { get; set; }
        public string issue_count { get; set; }
        public string card_slot { get; set; }
        public string card_mask { get; set; }
        public WiegandFormatId wiegand_format_id { get; set; }
        public UserId user_id { get; set; }
    }
}