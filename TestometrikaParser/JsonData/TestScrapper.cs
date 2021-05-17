using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestometrikaParser.JsonData
{
    public class TestScrapper
    {
        [JsonProperty("results_blog")]
        public ResultBlog ResultBlog { get; set; } = new ResultBlog();
        [JsonProperty("tests")]
        public List<Test> Tests { get; set; } = new List<Test>();
    }
}