using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TestometrikaParser.JsonData;

namespace TestometrikaParser
{
    public class Questions
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("answer")]
        public Dictionary<int, string> Answer { get; set; } = new Dictionary<int, string>();
    }
}
