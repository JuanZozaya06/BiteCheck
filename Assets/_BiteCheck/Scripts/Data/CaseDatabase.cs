using System;
using System.Collections.Generic;
using System.Linq;

namespace BiteCheck.Data
{
    public static class CaseDatabase
    {
        private const int CasesPerDay = 10;

        private static readonly Random Random = new Random();

        private static readonly List<SurvivorCase> Cases = new List<SurvivorCase>
        {
            Case("human-obvious-001", "Marta Lunchbox", 41, "I brought soup for everyone. It is normal soup. No fingers.", false, 1, 4, 10, 8,
                "valid shelter pass", "normal pulse", "warm hands", "offers soup"),
            Case("human-obvious-002", "Owen Battery", 29, "My flashlight died, but my breathing is still premium human quality.", false, 1, 4, 10, 8,
                "normal pulse", "clear eyes", "carrying spare batteries", "complains about paperwork"),
            Case("human-obvious-003", "Priya Bandage", 36, "This bandage is from falling off a roof, not from anyone chewing me.", false, 1, 4, 10, 8,
                "clean bandage", "valid shelter pass", "normal temperature", "recites shelter rules"),
            Case("human-obvious-004", "Gus Clipboard", 52, "I filled out form B-12 twice because the first one looked haunted.", false, 1, 4, 10, 8,
                "valid shelter pass", "normal pulse", "annoyed by bureaucracy", "no visible wounds"),
            Case("human-obvious-005", "Nina Sneakers", 18, "I ran here from the mall. The zombies still owe me a refund.", false, 1, 4, 10, 8,
                "normal pulse", "fast breathing", "untorn sneakers", "clear speech"),
            Case("human-obvious-006", "Theo Toolbox", 47, "I can fix the gate if someone stops screaming at the gate.", false, 1, 4, 10, 8,
                "normal temperature", "grease stains", "steady hands", "toolbox full of tools"),
            Case("human-obvious-007", "Lena Homework", 15, "If the world ended to cancel math class, I accept shelter immediately.", false, 1, 4, 10, 8,
                "normal pulse", "school backpack", "valid shelter pass", "worried about algebra"),
            Case("human-obvious-008", "Barry Beans", 63, "I have canned beans and exactly zero cravings for ankle meat.", false, 1, 4, 10, 8,
                "normal pulse", "canned food", "clear eyes", "smells like beans"),

            Case("infected-obvious-001", "Carl Chomps", 34, "No bite here. I always drool this much at checkpoints.", true, 1, 5, 14, 6,
                "fresh bite mark", "black veins", "keeps growling quietly", "drooling"),
            Case("infected-obvious-002", "Sandra Shamble", 27, "Walking straight is elitist. Let me in sideways.", true, 1, 5, 14, 6,
                "staggering walk", "gray skin", "high fever", "blank stare"),
            Case("infected-obvious-003", "Mort Fencebit", 58, "That fence bit me first. Very aggressive fence.", true, 1, 5, 14, 6,
                "fresh bite mark", "claims it was a dog bite", "black veins", "missing shoe"),
            Case("infected-obvious-004", "Tina Tonsils", 31, "Grrr means hello in my old neighborhood.", true, 1, 5, 14, 6,
                "keeps growling quietly", "black tongue", "high fever", "snaps at flies"),
            Case("infected-obvious-005", "Edgar Elbow", 44, "Ignore the arm. It has been independent since Tuesday.", true, 1, 5, 14, 6,
                "arm twitching", "black veins", "cold skin", "fresh bite mark"),
            Case("infected-obvious-006", "Pamela Pulse", 39, "My pulse is shy. It hides during inspections.", true, 1, 5, 14, 6,
                "no pulse", "cloudy eyes", "low groan", "smells like grave dirt"),
            Case("infected-obvious-007", "Kevin Snack", 22, "Why does your hand look so handshake-sized?", true, 1, 5, 14, 6,
                "lunges at hands", "high fever", "black veins", "broken shelter pass"),
            Case("infected-obvious-008", "Doris Doorchewer", 70, "The door was crunchy before I got here.", true, 1, 5, 14, 6,
                "wood splinters in teeth", "keeps growling quietly", "gray skin", "fresh bite mark"),

            Case("human-ambiguous-001", "Felix Feverfew", 33, "It is pollen season, rubble season, and panic season. Pick one.", false, 2, 6, 11, 12,
                "nervous sweating", "mild fever", "normal pulse", "valid shelter pass"),
            Case("human-ambiguous-002", "Ivy Dogpark", 24, "It really was a dog bite. The dog was also having a bad week.", false, 2, 6, 11, 12,
                "claims it was a dog bite", "normal pulse", "clean wound", "afraid of needles"),
            Case("human-ambiguous-003", "Rocco Gravel", 46, "The growling is my stomach. I have eaten crackers for three days.", false, 2, 6, 11, 12,
                "keeps growling quietly", "hungry", "normal temperature", "clear eyes"),
            Case("human-ambiguous-004", "Mina Makeup", 28, "The gray skin is bargain-bin foundation. Apocalypse sales are brutal.", false, 2, 6, 11, 12,
                "gray face powder", "normal pulse", "valid shelter pass", "embarrassed"),
            Case("human-ambiguous-005", "Stanley Stumble", 55, "I twisted my ankle escaping a birthday clown with a stapler.", false, 2, 6, 11, 12,
                "staggering walk", "swollen ankle", "normal pulse", "clear speech"),
            Case("human-ambiguous-006", "Jae Static", 19, "I am shaking because the radio keeps playing dental ads.", false, 2, 6, 11, 12,
                "trembling hands", "nervous sweating", "normal temperature", "valid shelter pass"),
            Case("human-ambiguous-007", "Agnes Picklejar", 67, "My breath is pickles. That is a lifestyle, not a symptom.", false, 3, 7, 12, 14,
                "strange breath", "normal pulse", "carries pickles", "clear eyes"),

            Case("infected-ambiguous-001", "Victor Veins", 37, "These black veins are tattoos. Very fresh, very itchy tattoos.", true, 2, 7, 16, 7,
                "black veins", "normal-looking pass", "nervous sweating", "high fever"),
            Case("infected-ambiguous-002", "Cassie Coughdrop", 26, "I only cough when I think about brains. Which is rarely. Hourly.", true, 2, 7, 16, 7,
                "wet cough", "cloudy eyes", "normal pulse at first", "keeps sniffing people"),
            Case("infected-ambiguous-003", "Bruno Backpack", 49, "The moving thing in my bag is laundry with ambition.", true, 2, 7, 16, 7,
                "bag twitching", "black veins", "claims it was a dog bite", "forced smile"),
            Case("infected-ambiguous-004", "Elsa Elevator", 32, "I got stuck in an elevator with a biter. We are no longer friends.", true, 2, 7, 16, 7,
                "covered bite mark", "nervous sweating", "high fever", "valid shelter pass"),
            Case("infected-ambiguous-005", "Neil Nightshift", 43, "I am pale because I work nights. Also because I might be slightly dead.", true, 3, 8, 18, 8,
                "cold skin", "slow pulse", "gray skin", "polite groaning"),
            Case("infected-ambiguous-006", "Poppy Perfume", 35, "The perfume is to calm everyone, not hide corpse smell.", true, 3, 8, 18, 8,
                "too much perfume", "black veins under sleeve", "high fever", "avoids flashlight"),
            Case("infected-ambiguous-007", "Hank Helmet", 60, "Helmet stays on for safety. Definitely not hiding forehead teeth.", true, 3, 8, 18, 8,
                "helmet taped shut", "keeps growling quietly", "fresh bite mark", "normal-looking pass"),
        };

        public static SurvivorCase GetRandomCase(int day)
        {
            List<SurvivorCase> casesForDay = GetCasesForDay(day);
            return casesForDay[Random.Next(casesForDay.Count)];
        }

        public static List<SurvivorCase> GetCasesForDay(int day)
        {
            int normalizedDay = Math.Max(1, day);
            int maxDifficulty = Math.Min(3, 1 + ((normalizedDay - 1) / 2));

            List<SurvivorCase> eligibleCases = Cases
                .Where(survivorCase => survivorCase.Difficulty <= maxDifficulty)
                .OrderBy(_ => Random.Next())
                .Take(CasesPerDay)
                .ToList();

            if (eligibleCases.Count < CasesPerDay)
            {
                eligibleCases.AddRange(Cases
                    .Where(survivorCase => !eligibleCases.Contains(survivorCase))
                    .OrderBy(_ => Random.Next())
                    .Take(CasesPerDay - eligibleCases.Count));
            }

            return eligibleCases;
        }

        private static SurvivorCase Case(
            string id,
            string displayName,
            int age,
            string dialogue,
            bool infected,
            int difficulty,
            int reward,
            int securityPenalty,
            int moralePenalty,
            params string[] symptoms)
        {
            return new SurvivorCase(
                id,
                displayName,
                age,
                dialogue,
                symptoms.ToList(),
                infected,
                difficulty,
                reward,
                securityPenalty,
                moralePenalty);
        }
    }
}
