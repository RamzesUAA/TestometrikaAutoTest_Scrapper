using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TestometrikaParser.JsonData
{
    public class Result
    {
        [JsonProperty("text")]
        public string Text { get;set; }
        [JsonProperty("blog_description")]
        public string BlogDescription { get; set; }
    }
}
