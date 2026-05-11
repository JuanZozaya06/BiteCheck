using System;
using UnityEngine;

namespace BiteCheck.Systems
{
    public class StatsManager : MonoBehaviour
    {
        [SerializeField] private int day = 1;
        [SerializeField] private int security = 100;
        [SerializeField] private int morale = 100;
        [SerializeField] private int resources;
        [SerializeField] private int correctDecisions;
        [SerializeField] private int wrongDecisions;
        [SerializeField] private int currentSurvivorIndex;
        [SerializeField] private int survivorsPerDay = 10;

        private bool gameOverRaised;
        private bool dayCompleteRaised;

        public event Action OnStatsChanged;
        public event Action OnGameOver;
        public event Action OnDayComplete;

        public int Day => day;
        public int Security => security;
        public int Morale => morale;
        public int Resources => resources;
        public int CorrectDecisions => correctDecisions;
        public int WrongDecisions => wrongDecisions;
        public int CurrentSurvivorIndex => currentSurvivorIndex;
        public int SurvivorsPerDay => survivorsPerDay;

        private void Awake()
        {
            ClampStats();
        }

        public void ResetRun()
        {
            day = 1;
            security = 100;
            morale = 100;
            resources = 0;
            correctDecisions = 0;
            wrongDecisions = 0;
            currentSurvivorIndex = 0;
            survivorsPerDay = Mathf.Max(1, survivorsPerDay);
            gameOverRaised = false;
            dayCompleteRaised = false;

            RaiseStatsChanged();
        }

        public void StartNewDay()
        {
            day++;
            currentSurvivorIndex = 0;
            dayCompleteRaised = false;

            RaiseStatsChanged();
        }

        public void RegisterCorrectDecision(int reward)
        {
            resources += Mathf.Max(0, reward);
            correctDecisions++;

            RaiseStatsChanged();
            CheckEndStates();
        }

        public bool CanSpendResources(int amount)
        {
            return resources >= Mathf.Max(0, amount);
        }

        public bool TrySpendResources(int amount)
        {
            int normalizedAmount = Mathf.Max(0, amount);

            if (resources < normalizedAmount)
            {
                return false;
            }

            resources -= normalizedAmount;
            RaiseStatsChanged();
            return true;
        }

        public void RegisterAdmittedInfected(int penalty)
        {
            security -= Mathf.Max(0, penalty);
            wrongDecisions++;
            ClampStats();

            RaiseStatsChanged();
            CheckEndStates();
        }

        public void RegisterQuarantinedHuman(int penalty)
        {
            morale -= Mathf.Max(0, penalty);
            wrongDecisions++;
            ClampStats();

            RaiseStatsChanged();
            CheckEndStates();
        }

        public void AdvanceSurvivor()
        {
            currentSurvivorIndex++;

            RaiseStatsChanged();
            CheckEndStates();
        }

        public bool IsDayComplete()
        {
            return currentSurvivorIndex >= survivorsPerDay;
        }

        public bool IsGameOver()
        {
            return security <= 0 || morale <= 0;
        }

        private void CheckEndStates()
        {
            if (IsGameOver())
            {
                RaiseGameOver();
                return;
            }

            if (IsDayComplete())
            {
                RaiseDayComplete();
            }
        }

        private void RaiseStatsChanged()
        {
            OnStatsChanged?.Invoke();
        }

        private void RaiseGameOver()
        {
            if (gameOverRaised)
            {
                return;
            }

            gameOverRaised = true;
            OnGameOver?.Invoke();
        }

        private void RaiseDayComplete()
        {
            if (dayCompleteRaised)
            {
                return;
            }

            dayCompleteRaised = true;
            OnDayComplete?.Invoke();
        }

        private void ClampStats()
        {
            security = Mathf.Clamp(security, 0, 100);
            morale = Mathf.Clamp(morale, 0, 100);
            resources = Mathf.Max(0, resources);
            survivorsPerDay = Mathf.Max(1, survivorsPerDay);
            currentSurvivorIndex = Mathf.Max(0, currentSurvivorIndex);
        }
    }
}
