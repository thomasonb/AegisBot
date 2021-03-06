﻿using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Reflection;

namespace AegisBotV2.Implementations
{
    public class Application
    {
        public enum State
        {
            New, InProgress, Finished, Any, Change, Approved, Denied, NeedsInvestigation, Submitted, Review
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

        private static string saveDir = new DirectoryInfo(Assembly.GetEntryAssembly().Location).Parent?.Parent?.Parent?.Parent?.Parent?.FullName + "\\ApplicationConfig";


        public string GetStateDescription()
        {
            return CurrentState.ToString();
        }

        public Application(UInt64 userID, bool LoadQAs = false)
        {
            CreateDate = DateTime.Now;
            ApplicationID = Guid.NewGuid().ToString();
            if (LoadQAs)
            {
                LoadQuestions();
            }
            UserID = userID;
            CurrentState = State.New;
        }

        public void LoadQuestions()
        {
            //TODO: this doesn't work when Questions.json doesn't exist
            //if (!Directory.Exists(saveDir))
            //{
            //    Directory.CreateDirectory(saveDir);
            //}
            //using (StreamReader sr = new StreamReader(new FileStream(saveDir + "\\Questions.json", FileMode.Open)))
            //{
            //    QAs = JsonConvert.DeserializeObject<List<QA>>(sr.ReadToEnd());
            //}

            if (!QAs.Any())
            {
                QAs.Add(new QA(1, "Steam Name/GT/PSN", false, null, null, QuestionRoleType.None));
                QAs.Add(new QA(2, "Current Highest Rank", true, new List<string>
                {
                    "Grand Champion",
                    "Champion 3", "Champion 2", "Champion 1",
                    "Diamond 3", "Diamond 2", "Diamond 1",
                    "Platinum 3", "Platinum 2", "Platinum 1",
                    "Gold 3", "Gold 2", "Gold 1",
                    "Silver 3", "Silver 2", "Silver 1",
                    "Bronze 3", "Bronze 2", "Bronze 1",
                    "Unranked"
                }, null, QuestionRoleType.Rank));
                QAs.Add(new QA(3, "Preferred Region", true, new List<string>
                {
                    "NA West", "NA East", "EU", "Asia", "Oceania", "Middle East", "SAM"
                }, null, QuestionRoleType.Region));
                QAs.Add(new QA(4, "Preferred Platform", true, new List<string>
                {
                    "Steam", "PS4", "Xbox"
                }, null, QuestionRoleType.Platform));
                SaveQuestions();
            }
        }

        private async void SaveQuestions()
        {
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }
            using (StreamWriter sw = new StreamWriter(new FileStream(saveDir + $"\\Questions.json", FileMode.Create)))
            {
                await sw.WriteAsync(JsonConvert.SerializeObject(QAs, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Formatting = Formatting.Indented }));
            }
        }

        public async Task SaveApplication(string solutionPath)
        {
            LastUpdateDate = DateTime.Now;
            ApplicationPath = solutionPath;
            if (!Directory.Exists(solutionPath))
            {
                Directory.CreateDirectory(solutionPath);
            }
            using (StreamWriter sw = new StreamWriter(new FileStream(solutionPath + $"\\{UserID}.Application.json", FileMode.Create)))
            {
                await sw.WriteAsync(JsonConvert.SerializeObject(this, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Formatting = Formatting.Indented }));
            }
        }

        public async Task AnswerQuestion(int questionID, string answer)
        {
            QAs.First(x => x.QuestionID == questionID).Answer = answer;
            if (CurrentState == State.InProgress || CurrentState == State.New)
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
