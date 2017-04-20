using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AegisBotV2.Implementations
{
    public class QA
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public int QuestionID { get; set; }
        public List<string> ValidAnswers { get; set; }
        public QA(int questionID, string question, List<string> validAnswers = null,  string answer = null)
        {
            QuestionID = questionID;
            Question = question;
            Answer = answer;
            ValidAnswers = validAnswers ?? new List<string>();
        }
    }
}
