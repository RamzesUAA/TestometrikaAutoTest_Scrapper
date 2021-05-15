using System;
using System.Collections.Generic;
using System.Text;

namespace TestometrikaParser.JsonData
{
    public class Ways
    {
        public List<AnswerRoad> answerRoads { get; set; } = new List<AnswerRoad>();
    }

    public class AnswerRoad
    {
        public int id { get; set; }
        public int aswer_id { get; set; }
    }
}
