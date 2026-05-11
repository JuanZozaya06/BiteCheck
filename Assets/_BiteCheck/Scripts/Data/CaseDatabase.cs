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
            Case("human-obvious-001", "Marta Soup", 41, "Soup for the shelter. Zero fingers.", false, 1, 4, 10, 8, "valid shelter pass", "normal pulse", "warm hands"),
            Case("human-obvious-002", "Owen Battery", 29, "Flashlight dead. Still breathing.", false, 1, 4, 10, 8, "clear eyes", "normal pulse", "spare batteries"),
            Case("human-obvious-003", "Priya Bandage", 36, "Roof fall. Not chewing related.", false, 1, 4, 10, 8, "clean bandage", "normal temperature", "valid shelter pass"),
            Case("human-obvious-004", "Gus Clipboard", 52, "I filled every form twice.", false, 1, 4, 10, 8, "valid shelter pass", "annoyed by paperwork", "normal pulse"),
            Case("human-obvious-005", "Nina Sneakers", 18, "Ran here. Zombies hate cardio.", false, 1, 4, 10, 8, "clear speech", "fast breathing", "untorn sneakers"),
            Case("human-obvious-006", "Theo Toolbox", 47, "I can fix that gate.", false, 1, 4, 10, 8, "steady hands", "normal temperature", "toolbox"),
            Case("human-obvious-007", "Lena Homework", 15, "Is math canceled forever?", false, 1, 4, 10, 8, "school backpack", "normal pulse", "worried about algebra"),
            Case("human-obvious-008", "Barry Beans", 63, "Beans, yes. Biting, no.", false, 1, 4, 10, 8, "canned food", "clear eyes", "normal pulse"),
            Case("human-obvious-009", "Tara Blanket", 34, "This blanket is for warmth, not hiding bites.", false, 1, 4, 10, 8, "warm skin", "valid shelter pass", "no wounds"),
            Case("human-obvious-010", "Miguel Radio", 40, "Radio says shelter snacks are real.", false, 1, 4, 10, 8, "normal pulse", "working radio", "clear eyes"),
            Case("human-obvious-011", "June Helmet", 26, "Helmet saved me from a billboard.", false, 1, 4, 10, 8, "helmet dent", "normal temperature", "clear speech"),
            Case("human-obvious-012", "Otis Mop", 58, "I brought cleaning supplies. Big day.", false, 1, 4, 10, 8, "normal pulse", "mop bucket", "valid shelter pass"),
            Case("human-obvious-013", "Rina Snacks", 22, "I trade chips for not dying.", false, 1, 4, 10, 8, "snack bag", "warm hands", "clear eyes"),
            Case("human-obvious-014", "Caleb Map", 31, "This map only lied twice.", false, 1, 4, 10, 8, "paper map", "normal pulse", "no bite marks"),
            Case("human-obvious-015", "Bess Kettle", 69, "Tea helps during monster paperwork.", false, 1, 4, 10, 8, "tea kettle", "normal temperature", "valid shelter pass"),

            Case("infected-obvious-001", "Carl Chomps", 34, "I always drool at gates.", true, 1, 5, 14, 6, "fresh bite mark", "black veins", "drooling"),
            Case("infected-obvious-002", "Sandra Shamble", 27, "Walking straight is rude.", true, 1, 5, 14, 6, "staggering walk", "gray skin", "high fever"),
            Case("infected-obvious-003", "Mort Fencebit", 58, "The fence bit me first.", true, 1, 5, 14, 6, "fresh bite mark", "black veins", "claims it was a dog bite"),
            Case("infected-obvious-004", "Tina Growls", 31, "Grrr means hello.", true, 1, 5, 14, 6, "keeps growling quietly", "black tongue", "high fever"),
            Case("infected-obvious-005", "Edgar Elbow", 44, "My arm has opinions.", true, 1, 5, 14, 6, "arm twitching", "cold skin", "fresh bite mark"),
            Case("infected-obvious-006", "Pamela Pulse", 39, "My pulse is shy.", true, 1, 5, 14, 6, "no pulse", "cloudy eyes", "low groan"),
            Case("infected-obvious-007", "Kevin Snack", 22, "Nice handshake-sized hand.", true, 1, 5, 14, 6, "lunges at hands", "black veins", "broken shelter pass"),
            Case("infected-obvious-008", "Doris Doorchewer", 70, "The door was crunchy.", true, 1, 5, 14, 6, "wood splinters in teeth", "gray skin", "fresh bite mark"),
            Case("infected-obvious-009", "Milo Moans", 19, "Mooooon? I mean morning.", true, 1, 5, 14, 6, "constant moaning", "blank stare", "cold skin"),
            Case("infected-obvious-010", "Greta Gums", 55, "These teeth are decorative.", true, 1, 5, 14, 6, "bloody gums", "black veins", "snaps at air"),
            Case("infected-obvious-011", "Ned Neckbite", 42, "Scarf fashion. Ignore leaks.", true, 1, 5, 14, 6, "neck bite", "high fever", "dirty scarf"),
            Case("infected-obvious-012", "Opal Twitch", 61, "My leg is dancing.", true, 1, 5, 14, 6, "leg twitching", "cloudy eyes", "gray skin"),
            Case("infected-obvious-013", "Vera Windowlick", 37, "Glass tastes like hope.", true, 1, 5, 14, 6, "licks glass", "no pulse", "fresh bite mark"),
            Case("infected-obvious-014", "Brock Brains", 48, "Any brain-free snacks?", true, 1, 5, 14, 6, "says brains twice", "black veins", "drooling"),
            Case("infected-obvious-015", "Lulu Limp", 30, "My foot left early.", true, 1, 5, 14, 6, "dragging foot", "cold skin", "keeps growling quietly"),

            Case("human-ambiguous-001", "Felix Feverfew", 33, "Pollen, panic, or plague. Pick one.", false, 2, 6, 11, 12, "nervous sweating", "mild fever", "normal pulse"),
            Case("human-ambiguous-002", "Ivy Dogpark", 24, "It really was a dog bite.", false, 2, 6, 11, 12, "claims it was a dog bite", "clean wound", "normal pulse"),
            Case("human-ambiguous-003", "Rocco Gravel", 46, "Growling stomach. Three cracker days.", false, 2, 6, 11, 12, "keeps growling quietly", "hungry", "clear eyes"),
            Case("human-ambiguous-004", "Mina Makeup", 28, "Gray foundation. Bad sale.", false, 2, 6, 11, 12, "gray face powder", "valid shelter pass", "normal pulse"),
            Case("human-ambiguous-005", "Stanley Stumble", 55, "Twisted ankle. Bad clown chase.", false, 2, 6, 11, 12, "staggering walk", "swollen ankle", "clear speech"),
            Case("human-ambiguous-006", "Jae Static", 19, "Radio dental ads broke me.", false, 2, 6, 11, 12, "trembling hands", "nervous sweating", "normal temperature"),
            Case("human-ambiguous-007", "Agnes Picklejar", 67, "Pickle breath is a lifestyle.", false, 2, 7, 12, 14, "strange breath", "carries pickles", "normal pulse"),
            Case("human-ambiguous-008", "Dante Sleepless", 38, "Three nights awake. Still human-ish.", false, 2, 6, 11, 12, "red eyes", "normal pulse", "exhausted"),
            Case("human-ambiguous-009", "Kira Bandit", 21, "This mask is for dust, not biting.", false, 2, 6, 11, 12, "face mask", "nervous sweating", "valid shelter pass"),
            Case("human-ambiguous-010", "Moira Coughs", 50, "Smoke cough. Burned toast bunker.", false, 2, 6, 11, 12, "dry cough", "normal temperature", "clear eyes"),
            Case("human-ambiguous-011", "Eli Paint", 17, "Black veins? Marker prank.", false, 3, 7, 12, 14, "marker on arms", "normal pulse", "embarrassed"),
            Case("human-ambiguous-012", "Tomas Biteycat", 44, "Cat bite. Tiny criminal.", false, 3, 7, 12, 14, "small bite mark", "normal temperature", "cat scratches"),
            Case("human-ambiguous-013", "Sasha Whisper", 29, "I whisper because sirens hurt.", false, 3, 7, 12, 14, "quiet voice", "normal pulse", "valid shelter pass"),
            Case("human-ambiguous-014", "Nolan Coldhands", 57, "Cold hands. Warm conscience.", false, 3, 7, 12, 14, "cold hands", "steady pulse", "clear speech"),
            Case("human-ambiguous-015", "Pia Perfume", 35, "Perfume hides bunker socks.", false, 3, 7, 12, 14, "too much perfume", "normal pulse", "no wounds"),

            Case("infected-ambiguous-001", "Victor Veins", 37, "Fresh tattoos. Very itchy.", true, 2, 7, 16, 7, "black veins", "normal-looking pass", "high fever"),
            Case("infected-ambiguous-002", "Cassie Coughdrop", 26, "I cough near brain thoughts.", true, 2, 7, 16, 7, "wet cough", "cloudy eyes", "keeps sniffing people"),
            Case("infected-ambiguous-003", "Bruno Backpack", 49, "Moving bag? Laundry ambition.", true, 2, 7, 16, 7, "bag twitching", "black veins", "forced smile"),
            Case("infected-ambiguous-004", "Elsa Elevator", 32, "Elevator biter. Awkward ride.", true, 2, 7, 16, 7, "covered bite mark", "nervous sweating", "high fever"),
            Case("infected-ambiguous-005", "Neil Nightshift", 43, "I work nights. Slightly dead.", true, 2, 8, 18, 8, "cold skin", "slow pulse", "gray skin"),
            Case("infected-ambiguous-006", "Poppy Perfume", 35, "Perfume is for confidence.", true, 2, 8, 18, 8, "too much perfume", "black veins under sleeve", "avoids flashlight"),
            Case("infected-ambiguous-007", "Hank Helmet", 60, "Helmet stays on. Safety.", true, 2, 8, 18, 8, "helmet taped shut", "keeps growling quietly", "normal-looking pass"),
            Case("infected-ambiguous-008", "Cora Smile", 28, "Smiling is required, right?", true, 2, 7, 16, 7, "forced smile", "high fever", "snaps at birds"),
            Case("infected-ambiguous-009", "Leon Gloves", 46, "Gloves are fashionable now.", true, 2, 7, 16, 7, "gloves hiding bite", "nervous sweating", "cloudy eyes"),
            Case("infected-ambiguous-010", "Ruth Blanket", 72, "Blanket is not moving.", true, 2, 7, 16, 7, "blanket twitching", "cold skin", "low groan"),
            Case("infected-ambiguous-011", "Omar Sunglasses", 33, "Sun is bright at night.", true, 3, 8, 18, 8, "sunglasses at night", "cloudy eyes", "slow pulse"),
            Case("infected-ambiguous-012", "Tess Tea", 52, "Tea fixes everything. Maybe.", true, 3, 8, 18, 8, "shaking cup", "black veins", "high fever"),
            Case("infected-ambiguous-013", "Walt Whistle", 64, "Whistling covers the groan.", true, 3, 8, 18, 8, "polite groaning", "fresh bite mark", "valid shelter pass"),
            Case("infected-ambiguous-014", "Yara Yoga", 25, "This bend is wellness.", true, 3, 8, 18, 8, "wrong-way elbow", "gray skin", "calm voice"),
            Case("infected-ambiguous-015", "Zed Delivery", 39, "Package says urgent brains.", true, 3, 8, 18, 8, "sealed crate rattles", "black veins", "hungry stare"),
        };

        public static SurvivorCase GetRandomCase(int day)
        {
            return GetRandomCaseForDifficulty(day);
        }

        public static SurvivorCase GetRandomCaseForDifficulty(int day)
        {
            CaseMix mix = GetCaseMix(day);
            List<SurvivorCase> candidates = new List<SurvivorCase>();

            AddWeightedCandidates(candidates, false, true, mix.ObviousHuman);
            AddWeightedCandidates(candidates, true, true, mix.ObviousInfected);
            AddWeightedCandidates(candidates, false, false, mix.AmbiguousHuman);
            AddWeightedCandidates(candidates, true, false, mix.AmbiguousInfected);

            if (candidates.Count == 0)
            {
                return Cases[Random.Next(Cases.Count)];
            }

            return candidates[Random.Next(candidates.Count)];
        }

        public static List<SurvivorCase> GetCasesForDay(int day)
        {
            CaseMix mix = GetCaseMix(day);
            List<SurvivorCase> dayCases = new List<SurvivorCase>(CasesPerDay);

            AddRandomCases(dayCases, false, true, mix.ObviousHuman);
            AddRandomCases(dayCases, true, true, mix.ObviousInfected);
            AddRandomCases(dayCases, false, false, mix.AmbiguousHuman);
            AddRandomCases(dayCases, true, false, mix.AmbiguousInfected);

            return dayCases
                .OrderBy(_ => Random.Next())
                .Take(CasesPerDay)
                .ToList();
        }

        private static CaseMix GetCaseMix(int day)
        {
            int normalizedDay = Math.Max(1, day);

            if (normalizedDay == 1)
            {
                return new CaseMix(4, 4, 1, 1);
            }

            if (normalizedDay == 2)
            {
                return new CaseMix(3, 3, 2, 2);
            }

            if (normalizedDay < 5)
            {
                return new CaseMix(2, 3, 2, 3);
            }

            return new CaseMix(1, 3, 3, 3);
        }

        private static void AddWeightedCandidates(List<SurvivorCase> target, bool infected, bool obvious, int weight)
        {
            if (weight <= 0)
            {
                return;
            }

            List<SurvivorCase> source = GetCases(infected, obvious);
            for (int i = 0; i < weight; i++)
            {
                target.AddRange(source);
            }
        }

        private static void AddRandomCases(List<SurvivorCase> target, bool infected, bool obvious, int count)
        {
            List<SurvivorCase> source = GetCases(infected, obvious)
                .OrderBy(_ => Random.Next())
                .Take(count)
                .ToList();

            target.AddRange(source);
        }

        private static List<SurvivorCase> GetCases(bool infected, bool obvious)
        {
            string category = obvious ? "-obvious-" : "-ambiguous-";
            return Cases
                .Where(survivorCase => survivorCase.Infected == infected && survivorCase.Id.Contains(category))
                .ToList();
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

        private struct CaseMix
        {
            public CaseMix(int obviousHuman, int obviousInfected, int ambiguousHuman, int ambiguousInfected)
            {
                ObviousHuman = obviousHuman;
                ObviousInfected = obviousInfected;
                AmbiguousHuman = ambiguousHuman;
                AmbiguousInfected = ambiguousInfected;
            }

            public int ObviousHuman { get; }
            public int ObviousInfected { get; }
            public int AmbiguousHuman { get; }
            public int AmbiguousInfected { get; }
        }
    }
}
