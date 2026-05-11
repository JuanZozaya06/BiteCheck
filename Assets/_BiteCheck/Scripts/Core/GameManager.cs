using System;
using BiteCheck.Systems;
using UnityEngine;

namespace BiteCheck.Core
{
    public class DaySummary
    {
        public DaySummary(
            int dayCompleted,
            int correctDecisions,
            int wrongDecisions,
            int security,
            int morale,
            int resourcesEarned,
            int totalResources)
        {
            DayCompleted = dayCompleted;
            CorrectDecisions = correctDecisions;
            WrongDecisions = wrongDecisions;
            Security = security;
            Morale = morale;
            ResourcesEarned = resourcesEarned;
            TotalResources = totalResources;
        }

        public int DayCompleted { get; }
        public int CorrectDecisions { get; }
        public int WrongDecisions { get; }
        public int Security { get; }
        public int Morale { get; }
        public int ResourcesEarned { get; }
        public int TotalResources { get; }
    }

    public class GameManager : MonoBehaviour
    {
        [SerializeField] private StatsManager statsManager;
        [SerializeField] private RoundManager roundManager;
        [SerializeField] private UpgradeSystem upgradeSystem;
        [SerializeField] private bool startOnAwake;

        private bool gameOver;
        private bool waitingForContinue;
        private int dayStartCorrectDecisions;
        private int dayStartWrongDecisions;
        private int dayStartResources;

        public event Action<DaySummary> OnDaySummaryReady;
        public event Action OnRunStarted;
        public event Action<int> OnDayStarted;

        private void Awake()
        {
            if (statsManager == null)
            {
                statsManager = FindFirstObjectByType<StatsManager>();
            }

            if (roundManager == null)
            {
                roundManager = FindFirstObjectByType<RoundManager>();
            }

            if (upgradeSystem == null)
            {
                upgradeSystem = FindFirstObjectByType<UpgradeSystem>();
            }
        }

        private void OnEnable()
        {
            if (statsManager == null)
            {
                return;
            }

            statsManager.OnGameOver += HandleGameOver;
            statsManager.OnDayComplete += HandleDayComplete;
        }

        private void OnDisable()
        {
            if (statsManager == null)
            {
                return;
            }

            statsManager.OnGameOver -= HandleGameOver;
            statsManager.OnDayComplete -= HandleDayComplete;
        }

        private void Start()
        {
            if (startOnAwake)
            {
                StartRun();
            }
        }

        public void StartRun()
        {
            if (statsManager == null || roundManager == null)
            {
                Debug.LogError("GameManager needs StatsManager and RoundManager references.", this);
                return;
            }

            gameOver = false;
            waitingForContinue = false;
            roundManager.StopRound();
            roundManager.ClearActiveSurvivor();
            statsManager.ResetRun();
            upgradeSystem?.ResetUpgrades();
            roundManager.Initialize(statsManager);

            Debug.Log("Bite Check run started.");
            OnRunStarted?.Invoke();
            StartCurrentDay();
        }

        public void RestartRun()
        {
            StartRun();
        }

        private void StartCurrentDay()
        {
            if (gameOver || waitingForContinue)
            {
                return;
            }

            dayStartCorrectDecisions = statsManager.CorrectDecisions;
            dayStartWrongDecisions = statsManager.WrongDecisions;
            dayStartResources = statsManager.Resources;

            Debug.Log($"Starting day {statsManager.Day}.");
            OnDayStarted?.Invoke(statsManager.Day);
            roundManager.BeginDay();
        }

        public void ContinueToNextDay()
        {
            if (gameOver || !waitingForContinue)
            {
                return;
            }

            waitingForContinue = false;
            statsManager.StartNewDay();
            StartCurrentDay();
        }

        private void HandleGameOver()
        {
            if (gameOver)
            {
                return;
            }

            gameOver = true;

            waitingForContinue = false;
            roundManager.StopRound();
            Debug.Log($"Game over on day {statsManager.Day}. Security={statsManager.Security}, Morale={statsManager.Morale}, Resources={statsManager.Resources}");
        }

        private void HandleDayComplete()
        {
            if (gameOver || waitingForContinue)
            {
                return;
            }

            waitingForContinue = true;
            roundManager.StopRound();

            DaySummary summary = new DaySummary(
                statsManager.Day,
                statsManager.CorrectDecisions - dayStartCorrectDecisions,
                statsManager.WrongDecisions - dayStartWrongDecisions,
                statsManager.Security,
                statsManager.Morale,
                statsManager.Resources - dayStartResources,
                statsManager.Resources);

            Debug.Log($"Day {summary.DayCompleted} complete. Correct={summary.CorrectDecisions}, Wrong={summary.WrongDecisions}, Resources earned={summary.ResourcesEarned}");
            OnDaySummaryReady?.Invoke(summary);
        }
    }
}
