using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TestometrikaParser.JsonData
{
    public class Way
    {
        [JsonProperty("answers_road")]
        public List<AnswerRoad> AnswerRoads { get; set; } = new List<AnswerRoad>();
        [JsonProperty("result")]
        public string ResultId { get; set; }
    }

    public class AnswerRoad
    {
        [JsonProperty("question_id")]
        public int QuestionId { get; set; }
        [JsonProperty("answer_id")]
        public int AnswerId { get; set; }
    }
}
