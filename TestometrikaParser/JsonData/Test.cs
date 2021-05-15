using System;
using System.Collections.Generic;
using System.Text;

namespace TestometrikaParser
{
    public class Test
    {
        public string name { get; set; }
        public string description { get; set; }
        public Question Questions { get; set; } = new Question();
    }
}
