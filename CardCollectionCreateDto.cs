using System;
using System.Collections.Generic;
using System.Text;

namespace Bonzer.Propman.App.Suprema
{
    public class CardCollectionCreateDto
    {
        public CardCollectionCreate CardCollection { get; set; }
    }

    public class CardCollectionCreate
    {
        public List<CardCreateDto> rows { get; set; }
    }

    public class CardType
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string mode { get; set; }
    }

    public class CardCreateDto
    {
        public CardType card_type { get; set; }
        public WiegandFormatId wiegand_format_id { get; set; }
        public string card_id { get; set; }
        public string display_card_id { get; set; }
        public bool isDel { get; set; }
    }

    public class WiegandFormatId
    {
        public string id { get; set; }
        public string name { get; set; }
    }
}