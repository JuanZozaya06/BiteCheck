using System;
using BiteCheck.Data;

namespace BiteCheck.Systems
{
    public enum DecisionType
    {
        Admit,
        Quarantine
    }

    public enum DecisionResultType
    {
        CorrectAdmitHuman,
        CorrectQuarantineInfected,
        WrongAdmitInfected,
        WrongQuarantineHuman
    }

    [Serializable]
    public class DecisionResult
    {
        public DecisionResultType ResultType;
        public bool Correct;
        public string FeedbackMessage;
        public int ResourceDelta;
        public int SecurityDelta;
        public int MoraleDelta;

        public DecisionResult(
            DecisionResultType resultType,
            bool correct,
            string feedbackMessage,
            int resourceDelta,
            int securityDelta,
            int moraleDelta)
        {
            ResultType = resultType;
            Correct = correct;
            FeedbackMessage = feedbackMessage;
            ResourceDelta = resourceDelta;
            SecurityDelta = securityDelta;
            MoraleDelta = moraleDelta;
        }
    }

    public static class DecisionResolver
    {
        public static DecisionResult Resolve(SurvivorCase survivorCase, DecisionType decision)
        {
            if (survivorCase == null)
            {
                throw new ArgumentNullException(nameof(survivorCase));
            }

            if (decision == DecisionType.Admit)
            {
                return survivorCase.Infected
                    ? WrongAdmitInfected(survivorCase)
                    : CorrectAdmitHuman(survivorCase);
            }

            return survivorCase.Infected
                ? CorrectQuarantineInfected(survivorCase)
                : WrongQuarantineHuman(survivorCase);
        }

        private static DecisionResult CorrectAdmitHuman(SurvivorCase survivorCase)
        {
            return new DecisionResult(
                DecisionResultType.CorrectAdmitHuman,
                true,
                $"{survivorCase.DisplayName} was human. Shelter resources increased.",
                survivorCase.Reward,
                0,
                0);
        }

        private static DecisionResult CorrectQuarantineInfected(SurvivorCase survivorCase)
        {
            return new DecisionResult(
                DecisionResultType.CorrectQuarantineInfected,
                true,
                $"{survivorCase.DisplayName} was infected. Quarantine worked.",
                survivorCase.Reward,
                0,
                0);
        }

        private static DecisionResult WrongAdmitInfected(SurvivorCase survivorCase)
        {
            return new DecisionResult(
                DecisionResultType.WrongAdmitInfected,
                false,
                $"{survivorCase.DisplayName} was infected. Security took a hit.",
                0,
                -survivorCase.SecurityPenalty,
                0);
        }

        private static DecisionResult WrongQuarantineHuman(SurvivorCase survivorCase)
        {
            return new DecisionResult(
                DecisionResultType.WrongQuarantineHuman,
                false,
                $"{survivorCase.DisplayName} was human. Morale dropped.",
                0,
                0,
                -survivorCase.MoralePenalty);
        }
    }
}
