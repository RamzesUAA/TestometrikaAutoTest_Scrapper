using System;
using System.Collections.Generic;
using System.Text;
using TestometrikaParser.JsonData;

namespace TestometrikaParser
{
    public class Questions
    {
        public string title { get; set; }
        public Answer answer { get; set; } = new Answer();
        //public Dictionary<int, Question> QuestionList = new Dictionary<int, Question>();
    }

    //public class Question
    //{
       
    //}

}
