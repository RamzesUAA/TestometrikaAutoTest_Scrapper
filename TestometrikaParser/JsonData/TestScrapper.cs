using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestometrikaParser.JsonData
{
    public class TestScrapper
    {
        [JsonProperty("results_blog")]
        public Dictionary<string, ResultBlog> Blogs { get; set; } = new Dictionary<string, ResultBlog>();
        [JsonProperty("tests")]
        public List<Test> Tests { get; set; } = new List<Test>();
    }
}