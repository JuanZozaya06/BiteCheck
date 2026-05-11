using System;
using System.Collections;
using BiteCheck.Characters;
using BiteCheck.Data;
using BiteCheck.Input;
using BiteCheck.Systems;
using UnityEngine;

namespace BiteCheck.Core
{
    public class RoundManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SwipeInputController swipeInput;
        [SerializeField] private SurvivorCharacter survivorPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform decisionPoint;

        [Header("Timing")]
        [SerializeField] private float throwForce = 5.5f;
        [SerializeField] private float nextSurvivorDelay = 1f;
        [SerializeField] private float decisionTime = 5f;

        private StatsManager statsManager;
        private SurvivorCharacter currentSurvivor;
        private bool dayActive;
        private bool waitingForDecision;
        private float timerRemaining;
        private Coroutine nextSurvivorRoutine;

        public event Action<SurvivorCase> OnSurvivorSpawned;
        public event Action<SurvivorCase> OnSurvivorReadyForDecision;
        public event Action<DecisionResult> OnDecisionResolved;
        public event Action<float, float> OnDecisionTimerChanged;

        private void Awake()
        {
            if (swipeInput == null)
            {
                swipeInput = FindFirstObjectByType<SwipeInputController>();
            }

            EnsureDefaultPoints();
        }

        private void Update()
        {
            if (!dayActive || !waitingForDecision)
            {
                return;
            }

            timerRemaining = Mathf.Max(0f, timerRemaining - Time.deltaTime);
            OnDecisionTimerChanged?.Invoke(timerRemaining, decisionTime);

            if (timerRemaining <= 0f)
            {
                HandleDecisionTimeout();
            }
        }

        private void OnEnable()
        {
            if (swipeInput == null)
            {
                return;
            }

            swipeInput.OnSwipeLeft += HandleSwipeLeft;
            swipeInput.OnSwipeRight += HandleSwipeRight;
            swipeInput.LockInput();
        }

        private void OnDisable()
        {
            if (swipeInput == null)
            {
                return;
            }

            swipeInput.OnSwipeLeft -= HandleSwipeLeft;
            swipeInput.OnSwipeRight -= HandleSwipeRight;
        }

        public void Initialize(StatsManager runStatsManager)
        {
            statsManager = runStatsManager != null
                ? runStatsManager
                : throw new ArgumentNullException(nameof(runStatsManager));
        }

        public void BeginDay()
        {
            if (statsManager == null)
            {
                Debug.LogError("RoundManager needs Initialize(statsManager) before BeginDay.", this);
                return;
            }

            dayActive = true;
            waitingForDecision = false;
            timerRemaining = 0f;
            OnDecisionTimerChanged?.Invoke(timerRemaining, decisionTime);
            swipeInput?.LockInput();
            SpawnNextSurvivor();
        }

        public void StopRound()
        {
            dayActive = false;
            waitingForDecision = false;
            timerRemaining = 0f;
            OnDecisionTimerChanged?.Invoke(timerRemaining, decisionTime);
            swipeInput?.LockInput();

            if (nextSurvivorRoutine != null)
            {
                StopCoroutine(nextSurvivorRoutine);
                nextSurvivorRoutine = null;
            }
        }

        private void SpawnNextSurvivor()
        {
            if (!dayActive || statsManager.IsGameOver() || statsManager.IsDayComplete())
            {
                return;
            }

            SurvivorCase survivorCase = CaseDatabase.GetRandomCase(statsManager.Day);
            currentSurvivor = CreateSurvivor();
            currentSurvivor.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            currentSurvivor.Initialize(survivorCase);
            currentSurvivor.OnReachedDecisionPoint += HandleSurvivorReachedDecisionPoint;
            currentSurvivor.WalkToDecisionPoint(decisionPoint);

            OnSurvivorSpawned?.Invoke(survivorCase);
            Debug.Log($"Survivor {statsManager.CurrentSurvivorIndex + 1}/{statsManager.SurvivorsPerDay}: {survivorCase.DisplayName}, age {survivorCase.Age}. {survivorCase.Dialogue}");
            Debug.Log($"Symptoms: {string.Join(", ", survivorCase.Symptoms)}");
        }

        private SurvivorCharacter CreateSurvivor()
        {
            SurvivorCharacter survivor = survivorPrefab != null
                ? Instantiate(survivorPrefab)
                : new GameObject("Survivor").AddComponent<SurvivorCharacter>();

            if (survivor.GetComponent<RagdollController>() == null)
            {
                survivor.gameObject.AddComponent<RagdollController>();
            }

            return survivor;
        }

        private void HandleSurvivorReachedDecisionPoint(SurvivorCharacter survivor)
        {
            if (survivor != currentSurvivor || !dayActive)
            {
                return;
            }

            waitingForDecision = true;
            timerRemaining = Mathf.Max(0.1f, decisionTime);
            OnDecisionTimerChanged?.Invoke(timerRemaining, decisionTime);
            swipeInput?.UnlockInput();

            SurvivorCase survivorCase = survivor.GetCurrentCase();
            OnSurvivorReadyForDecision?.Invoke(survivorCase);
            Debug.Log($"{survivorCase.DisplayName} reached the checkpoint. Swipe right to admit, left to quarantine.");
        }

        private void HandleSwipeLeft()
        {
            SubmitDecision(DecisionType.Quarantine);
        }

        private void HandleSwipeRight()
        {
            SubmitDecision(DecisionType.Admit);
        }

        private void HandleDecisionTimeout()
        {
            if (currentSurvivor == null)
            {
                return;
            }

            SurvivorCase survivorCase = currentSurvivor.GetCurrentCase();
            DecisionType missedDecision = survivorCase.Infected
                ? DecisionType.Admit
                : DecisionType.Quarantine;

            SubmitDecision(missedDecision, true);
        }

        private void SubmitDecision(DecisionType decision, bool timedOut = false)
        {
            if (!dayActive || !waitingForDecision || currentSurvivor == null)
            {
                return;
            }

            waitingForDecision = false;
            timerRemaining = 0f;
            OnDecisionTimerChanged?.Invoke(timerRemaining, decisionTime);
            swipeInput?.LockInput();

            SurvivorCase survivorCase = currentSurvivor.GetCurrentCase();
            DecisionResult result = DecisionResolver.Resolve(survivorCase, decision);
            if (timedOut)
            {
                result.FeedbackMessage = $"Time ran out. {result.FeedbackMessage}";
            }

            ApplyDecisionResult(result);
            OnDecisionResolved?.Invoke(result);

            Debug.Log(result.FeedbackMessage);
            ThrowCurrentSurvivor(decision);

            if (statsManager.IsGameOver())
            {
                return;
            }

            statsManager.AdvanceSurvivor();

            if (statsManager.IsDayComplete())
            {
                return;
            }

            nextSurvivorRoutine = StartCoroutine(SpawnNextSurvivorAfterDelay());
        }

        private void ApplyDecisionResult(DecisionResult result)
        {
            switch (result.ResultType)
            {
                case DecisionResultType.CorrectAdmitHuman:
                case DecisionResultType.CorrectQuarantineInfected:
                    statsManager.RegisterCorrectDecision(result.ResourceDelta);
                    break;
                case DecisionResultType.WrongAdmitInfected:
                    statsManager.RegisterAdmittedInfected(Mathf.Abs(result.SecurityDelta));
                    break;
                case DecisionResultType.WrongQuarantineHuman:
                    statsManager.RegisterQuarantinedHuman(Mathf.Abs(result.MoraleDelta));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ThrowCurrentSurvivor(DecisionType decision)
        {
            RagdollController ragdollController = currentSurvivor.GetComponent<RagdollController>();
            Vector3 throwDirection = decision == DecisionType.Admit ? Vector3.right : Vector3.left;

            if (ragdollController != null)
            {
                ragdollController.Throw(throwDirection, throwForce);
            }
        }

        private IEnumerator SpawnNextSurvivorAfterDelay()
        {
            yield return new WaitForSeconds(nextSurvivorDelay);
            nextSurvivorRoutine = null;
            SpawnNextSurvivor();
        }

        private void EnsureDefaultPoints()
        {
            if (spawnPoint == null)
            {
                GameObject spawn = new GameObject("SurvivorSpawnPoint");
                spawn.transform.SetParent(transform, false);
                spawn.transform.localPosition = new Vector3(0f, 0f, 6f);
                spawnPoint = spawn.transform;
            }

            if (decisionPoint == null)
            {
                GameObject point = new GameObject("DecisionPoint");
                point.transform.SetParent(transform, false);
                point.transform.localPosition = new Vector3(0f, 0f, -1.5f);
                decisionPoint = point.transform;
            }
        }
    }
}
