using System;
using System.Collections.Generic;
using System.Text;

namespace Bonzer.Propman.App.Suprema
{
    public class ResponseJsonDto
    {
        public ResponseDto Response { get; set; }
    }

    public class ResponseDto
    {
        public string code { get; set; }
        public string link { get; set; }
        public string message { get; set; }
    }
}