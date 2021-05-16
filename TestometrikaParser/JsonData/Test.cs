using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TestometrikaParser.JsonData;

namespace TestometrikaParser
{
    public class Test
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("questions")]
        public Dictionary<int, Questions> Questions { get; set; } = new Dictionary<int, Questions>();
        [JsonProperty("questions")]
        public Dictionary<int, Result> Results { get; set; } = new Dictionary<int, Result>();

    }
}
