using System;
using System.Collections.Generic;
using System.Text;

namespace TestometrikaParser.JsonData
{
    public class Results
    {
        public List<ResultData> Result { get; set; } = new List<ResultData>();
    }

    public class ResultData
    {
        public string text { get; set; }
        public string description { get; set; }
        public string blog_description { get; set; }
    }
}
