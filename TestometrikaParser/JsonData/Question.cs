using System;
using System.Collections.Generic;
using System.Text;
using TestometrikaParser.JsonData;

namespace TestometrikaParser
{
    public class Question
    {
        public string title { get; set; }
        public Answer answer { get; set; } = new Answer();
    }
}
