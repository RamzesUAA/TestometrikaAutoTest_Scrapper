using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TestometrikaParser.JsonData
{
    public class ResultBlog
    {
        [JsonProperty("images")]
        public List<string> ImagesList { get; set; } = new List<string>();
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
