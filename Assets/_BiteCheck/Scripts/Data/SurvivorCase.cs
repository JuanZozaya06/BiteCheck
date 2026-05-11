using System;
using System.Collections.Generic;

namespace BiteCheck.Data
{
    [Serializable]
    public class SurvivorCase
    {
        public string Id;
        public string DisplayName;
        public int Age;
        public string Dialogue;
        public List<string> Symptoms;
        public bool Infected;
        public int Difficulty;
        public int Reward;
        public int SecurityPenalty;
        public int MoralePenalty;

        public SurvivorCase(
            string id,
            string displayName,
            int age,
            string dialogue,
            List<string> symptoms,
            bool infected,
            int difficulty,
            int reward,
            int securityPenalty,
            int moralePenalty)
        {
            Id = id;
            DisplayName = displayName;
            Age = age;
            Dialogue = dialogue;
            Symptoms = symptoms;
            Infected = infected;
            Difficulty = difficulty;
            Reward = reward;
            SecurityPenalty = securityPenalty;
            MoralePenalty = moralePenalty;
        }
    }
}
