using System.Collections;
using BiteCheck.Systems;
using UnityEngine;

namespace BiteCheck.Core
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private StatsManager statsManager;
        [SerializeField] private RoundManager roundManager;
        [SerializeField] private bool startOnAwake = true;
        [SerializeField] private bool autoStartNextDay = true;
        [SerializeField] private float dayCompleteDelay = 2f;

        private bool gameOver;
        private Coroutine dayCompleteRoutine;

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
            statsManager.ResetRun();
            roundManager.Initialize(statsManager);

            Debug.Log("Bite Check run started.");
            StartCurrentDay();
        }

        private void StartCurrentDay()
        {
            if (gameOver)
            {
                return;
            }

            Debug.Log($"Starting day {statsManager.Day}.");
            roundManager.BeginDay();
        }

        private void HandleGameOver()
        {
            if (gameOver)
            {
                return;
            }

            gameOver = true;

            if (dayCompleteRoutine != null)
            {
                StopCoroutine(dayCompleteRoutine);
                dayCompleteRoutine = null;
            }

            roundManager.StopRound();
            Debug.Log($"Game over on day {statsManager.Day}. Security={statsManager.Security}, Morale={statsManager.Morale}, Resources={statsManager.Resources}");
        }

        private void HandleDayComplete()
        {
            if (gameOver || dayCompleteRoutine != null)
            {
                return;
            }

            dayCompleteRoutine = StartCoroutine(CompleteDayRoutine());
        }

        private IEnumerator CompleteDayRoutine()
        {
            roundManager.StopRound();

            Debug.Log($"Day {statsManager.Day} complete. Correct={statsManager.CorrectDecisions}, Wrong={statsManager.WrongDecisions}, Resources={statsManager.Resources}");

            yield return new WaitForSeconds(dayCompleteDelay);

            dayCompleteRoutine = null;

            if (!autoStartNextDay || statsManager.IsGameOver())
            {
                yield break;
            }

            statsManager.StartNewDay();
            StartCurrentDay();
        }
    }
}
