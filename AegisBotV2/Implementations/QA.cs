using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AegisBotV2.Implementations
{
    public enum QuestionRoleType
    {
        Rank, Platform, Region, None
    }
    public class QA
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public int QuestionID { get; set; }
        public List<string> ValidAnswers { get; set; }
        public bool SetRoleToAnswer { get; set; }
        public QuestionRoleType RoleType { get; set; }
        public QA(int questionID, string question, bool setRoleToAnswer, List<string> validAnswers = null,  string answer = null, QuestionRoleType roleType = QuestionRoleType.None)
        {
            QuestionID = questionID;
            Question = question;
            Answer = answer;
            SetRoleToAnswer = setRoleToAnswer;
            ValidAnswers = validAnswers ?? new List<string>();
            RoleType = roleType;
        }
    }
}
