using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AegisBot.Implementations
{
    public class QA
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public int QuestionID { get; set; }

        public QA(int questionID, string question, string answer = null)
        {
            QuestionID = questionID;
            Question = question;
            Answer = answer;
        }
    }
}
