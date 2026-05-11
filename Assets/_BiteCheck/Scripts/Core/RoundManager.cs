using System;
using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private Transform quarantineZone;
        [SerializeField] private Transform shelterGate;
        [SerializeField] private UpgradeSystem upgradeSystem;

        [Header("Timing")]
        [SerializeField] private float nextSurvivorDelay = 1.15f;
        [SerializeField] private float decisionTime = 5f;

        [Header("Throw")]
        [SerializeField] private float throwForce = 5.8f;
        [SerializeField] private float upwardForce = 0.08f;
        [SerializeField] private float torqueForce = 1.5f;
        [SerializeField] private float admitExitWalkSpeed = 4.8f;
        [SerializeField] private float admitExitDespawnDelay = 0.15f;

        private StatsManager statsManager;
        private SurvivorCharacter currentSurvivor;
        private bool dayActive;
        private bool waitingForDecision;
        private float timerRemaining;
        private Coroutine nextSurvivorRoutine;
        private List<SurvivorCase> currentDayCases;

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

            if (upgradeSystem == null)
            {
                upgradeSystem = FindFirstObjectByType<UpgradeSystem>();
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
            currentDayCases = CaseDatabase.GetCasesForDay(statsManager.Day);
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

        public void ClearActiveSurvivor()
        {
            if (currentSurvivor == null)
            {
                return;
            }

            currentSurvivor.OnReachedDecisionPoint -= HandleSurvivorReachedDecisionPoint;
            Destroy(currentSurvivor.gameObject);
            currentSurvivor = null;
        }

        private void SpawnNextSurvivor()
        {
            if (!dayActive || statsManager.IsGameOver() || statsManager.IsDayComplete())
            {
                return;
            }

            SurvivorCase survivorCase = GetNextCaseForCurrentDay();
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

        private SurvivorCase GetNextCaseForCurrentDay()
        {
            if (currentDayCases == null || currentDayCases.Count == 0)
            {
                currentDayCases = CaseDatabase.GetCasesForDay(statsManager.Day);
            }

            int caseIndex = Mathf.Clamp(statsManager.CurrentSurvivorIndex, 0, currentDayCases.Count - 1);
            return currentDayCases[caseIndex];
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
            ResolveCharacterExit(decision);

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
                    int securityPenalty = Mathf.Abs(result.SecurityDelta);
                    if (upgradeSystem != null)
                    {
                        securityPenalty = upgradeSystem.ApplySecurityPenaltyReduction(securityPenalty);
                    }

                    statsManager.RegisterAdmittedInfected(securityPenalty);
                    break;
                case DecisionResultType.WrongQuarantineHuman:
                    int moralePenalty = Mathf.Abs(result.MoraleDelta);
                    if (upgradeSystem != null)
                    {
                        moralePenalty = upgradeSystem.ApplyMoralePenaltyReduction(moralePenalty);
                    }

                    statsManager.RegisterQuarantinedHuman(moralePenalty);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ResolveCharacterExit(DecisionType decision)
        {
            if (decision == DecisionType.Admit)
            {
                WalkCurrentSurvivorToShelter();
                return;
            }

            ThrowCurrentSurvivor(decision);
        }

        private void WalkCurrentSurvivorToShelter()
        {
            if (currentSurvivor == null)
            {
                return;
            }

            currentSurvivor.OnReachedDecisionPoint -= HandleSurvivorReachedDecisionPoint;

            if (shelterGate != null)
            {
                currentSurvivor.WalkToExitPoint(shelterGate, admitExitWalkSpeed, admitExitDespawnDelay);
                return;
            }

            currentSurvivor.WalkToExitPoint(currentSurvivor.transform.position + Vector3.right * 5f, admitExitWalkSpeed, admitExitDespawnDelay);
        }

        private void ThrowCurrentSurvivor(DecisionType decision)
        {
            RagdollController ragdollController = currentSurvivor.GetComponent<RagdollController>();
            Vector3 throwDirection = GetThrowDirection(decision);

            if (ragdollController != null)
            {
                ragdollController.Throw(throwDirection, throwForce, upwardForce, torqueForce);
            }
        }

        private Vector3 GetThrowDirection(DecisionType decision)
        {
            Transform targetZone = decision == DecisionType.Admit ? shelterGate : quarantineZone;

            if (targetZone == null || currentSurvivor == null)
            {
                return decision == DecisionType.Admit ? Vector3.right : Vector3.left;
            }

            Vector3 direction = targetZone.position - currentSurvivor.transform.position;
            direction.y = 0f;
            return direction.sqrMagnitude > 0.001f
                ? direction.normalized
                : decision == DecisionType.Admit ? Vector3.right : Vector3.left;
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

            if (quarantineZone == null)
            {
                GameObject zone = new GameObject("LeftQuarantineZone");
                zone.transform.SetParent(transform, false);
                zone.transform.localPosition = new Vector3(-3.5f, 0f, -1.5f);
                quarantineZone = zone.transform;
            }

            if (shelterGate == null)
            {
                GameObject gate = new GameObject("RightShelterGate");
                gate.transform.SetParent(transform, false);
                gate.transform.localPosition = new Vector3(3.5f, 0f, -1.5f);
                shelterGate = gate.transform;
            }
        }
    }
}
