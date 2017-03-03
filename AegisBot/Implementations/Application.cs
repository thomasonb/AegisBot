using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AegisBot.Implementations
{
    public class Application
    {
        public enum State
        {
            New, InProgress, Finished, Any, Change, Approved, Denied, NeedsInvestigation, Submitted
        }
        public List<QA> QAs { get; set; } = new List<QA>();
        public UInt64 UserID { get; private set; }
        public UInt64 ChannelID { get; set; }
        public string ApplicationTitle { get; set; }
        public State CurrentState;
        public int CurrentQuestionID;
        [JsonProperty(PropertyName = "ApplicationPath")]
        private string ApplicationPath;
        [JsonProperty(PropertyName = "ApplicationID")]
        public string ApplicationID { get; private set; }
        [JsonProperty(PropertyName = "StateDescription")]
        public string StateDescription => GetStateDescription();
        [JsonProperty(PropertyName = "LastUpdateDate")]
        private DateTime LastUpdateDate;
        [JsonProperty(PropertyName = "CreateDate")]
        private DateTime CreateDate;

        public string GetStateDescription()
        {
            return CurrentState.ToString();
        }

        public Application(UInt64 userID)
        {
            CreateDate = DateTime.Now;
            ApplicationID = Guid.NewGuid().ToString();
            LoadQuestions();
            UserID = userID;
            CurrentState = State.New;
        }

        private void LoadQuestions()
        {
            QAs.Add(new QA(1, "Your Real Name*"));
            QAs.Add(new QA(2, "Preferred Name/Handle"));
            QAs.Add(new QA(3, "Date of Birth"));
            QAs.Add(new QA(4, "Gender"));
            QAs.Add(new QA(5, "Are you a decent person?"));
            QAs.Add(new QA(6, "You wanna meet some cool people to play games with?"));
            QAs.Add(new QA(7, "Preferred Gaming Platform"));
            QAs.Add(new QA(8, "List Some Favorite Games"));
            QAs.Add(new QA(9, "Do you play Rocket League?"));
            QAs.Add(new QA(10, "What is your Standard Rank (3v3)? (Type \"N/A\" if you DONT play Rocket League)"));
            QAs.Add(new QA(11, "Doubles Rank (2v2)? (Type \"N/A\" if you DONT play Rocket League)"));
            QAs.Add(new QA(12, "Are you interested in playing Rocket League at a Competitive Level (Semi-Pro/Pro)?"));
            QAs.Add(new QA(13, "Have you participated in a \"Legitimate\" Rocket League Tournament before? (ESL, ModkIT, RLCS)?"));
            QAs.Add(new QA(14, "SteamCommunity URL*"));
            QAs.Add(new QA(15, "Battle.NET BattleTag*"));
            QAs.Add(new QA(16, "Origin ID*"));
            QAs.Add(new QA(17, "LoL Username"));
            QAs.Add(new QA(18, "Do you acknowledge that if you're a douche, we can kick/ban you with or without prior warning?"));
            QAs.Add(new QA(19, "Anyting else you wanna say or add?"));
        }

        public async Task SaveApplication(string solutionPath)
        {
            LastUpdateDate = DateTime.Now;
            ApplicationPath = solutionPath;
            if (!Directory.Exists(solutionPath))
            {
                Directory.CreateDirectory(solutionPath);
            }
            using (StreamWriter sw = new StreamWriter(solutionPath + $"\\{UserID}.Application.json", false))
            {
                await sw.WriteAsync(JsonConvert.SerializeObject(this, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Formatting = Formatting.Indented }));
            }
        }

        public async Task AnswerQuestion(int questionID, string answer)
        {
            QAs.First(x => x.QuestionID == questionID).Answer = answer;
            if (CurrentState == State.InProgress)
            {
                if (questionID == QAs.Last().QuestionID)
                {
                    CurrentState = State.Finished;
                }
                else
                {
                    CurrentQuestionID += 1;
                }
            }
            else if (CurrentState == State.Change)
            {
                CurrentState = State.Finished;
            }
            await SaveApplication(ApplicationPath);
        }

        public string GetApplication(bool includeApplicationID)
        {
            List<string> temp = QAs.Select(x => $"{Environment.NewLine}{x.QuestionID}. {x.Question}: {x.Answer}").ToList();
            return $"```{Environment.NewLine}{ApplicationTitle}{Environment.NewLine}{(includeApplicationID ? $"ApplicationID: {ApplicationID}" : "")}{string.Join(Environment.NewLine, temp)}{Environment.NewLine}```";
        }
    }
}
