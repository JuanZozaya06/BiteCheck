using BiteCheck.Core;
using BiteCheck.Data;
using BiteCheck.Systems;
using UnityEngine;

namespace BiteCheck.UI
{
    public class FeedbackEffectsController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StatsManager statsManager;
        [SerializeField] private RoundManager roundManager;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private AudioSource audioSource;

        [Header("Screen Flash")]
        [SerializeField] private Color correctFlashColor = new Color(0.1f, 1f, 0.25f, 0.45f);
        [SerializeField] private Color admittedInfectedFlashColor = new Color(1f, 0.05f, 0.05f, 0.5f);
        [SerializeField] private Color quarantinedHumanFlashColor = new Color(1f, 0.55f, 0.05f, 0.5f);
        [SerializeField] private float flashDuration = 0.28f;

        [Header("Camera Shake")]
        [SerializeField] private float shakeDuration = 0.25f;
        [SerializeField] private float shakeMagnitude = 0.14f;

        [Header("Optional Audio")]
        [SerializeField] private AudioClip correctClip;
        [SerializeField] private AudioClip wrongClip;
        [SerializeField] private AudioClip admittedInfectedClip;
        [SerializeField] private AudioClip quarantinedHumanClip;
        [SerializeField] private AudioClip survivorReadyClip;
        [SerializeField] private AudioClip dayCompleteClip;
        [SerializeField] private AudioClip gameOverClip;
        [SerializeField] private float pitchVariation = 0.04f;

        private Color activeFlashColor;
        private float flashTimer;
        private float shakeTimer;
        private Vector3 cameraOriginalLocalPosition;
        private bool cameraShakeActive;

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

            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        private void OnEnable()
        {
            if (roundManager != null)
            {
                roundManager.OnSurvivorReadyForDecision += HandleSurvivorReadyForDecision;
                roundManager.OnDecisionResolved += HandleDecisionResolved;
            }

            if (statsManager != null)
            {
                statsManager.OnDayComplete += HandleDayComplete;
                statsManager.OnGameOver += HandleGameOver;
            }
        }

        private void OnDisable()
        {
            if (roundManager != null)
            {
                roundManager.OnSurvivorReadyForDecision -= HandleSurvivorReadyForDecision;
                roundManager.OnDecisionResolved -= HandleDecisionResolved;
            }

            if (statsManager != null)
            {
                statsManager.OnDayComplete -= HandleDayComplete;
                statsManager.OnGameOver -= HandleGameOver;
            }

            RestoreCamera();
        }

        private void Update()
        {
            if (flashTimer > 0f)
            {
                flashTimer = Mathf.Max(0f, flashTimer - Time.deltaTime);
            }

            UpdateCameraShake();
        }

        private void HandleSurvivorReadyForDecision(SurvivorCase survivorCase)
        {
            PlayClip(survivorReadyClip);
        }

        private void OnGUI()
        {
            if (flashTimer <= 0f || flashDuration <= 0f)
            {
                return;
            }

            Color previousColor = GUI.color;
            Color color = activeFlashColor;
            color.a *= flashTimer / flashDuration;
            GUI.color = color;
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previousColor;
        }

        private void HandleDecisionResolved(DecisionResult result)
        {
            bool wrongDecision = !result.Correct;

            if (result.Correct)
            {
                PlayFlash(correctFlashColor);
                PlayClip(correctClip);
                return;
            }

            if (result.ResultType == DecisionResultType.WrongAdmitInfected)
            {
                PlayFlash(admittedInfectedFlashColor);
                PlayClip(admittedInfectedClip != null ? admittedInfectedClip : wrongClip);
            }
            else if (result.ResultType == DecisionResultType.WrongQuarantineHuman)
            {
                PlayFlash(quarantinedHumanFlashColor);
                PlayClip(quarantinedHumanClip != null ? quarantinedHumanClip : wrongClip);
            }

            if (wrongDecision)
            {
                StartCameraShake();
            }
        }

        private void HandleDayComplete()
        {
            PlayClip(dayCompleteClip);
        }

        private void HandleGameOver()
        {
            PlayClip(gameOverClip != null ? gameOverClip : wrongClip);
        }

        private void PlayFlash(Color color)
        {
            activeFlashColor = color;
            flashTimer = flashDuration;
        }

        private void StartCameraShake()
        {
            if (targetCamera == null)
            {
                return;
            }

            if (!cameraShakeActive)
            {
                cameraOriginalLocalPosition = targetCamera.transform.localPosition;
            }

            cameraShakeActive = true;
            shakeTimer = shakeDuration;
        }

        private void UpdateCameraShake()
        {
            if (!cameraShakeActive || targetCamera == null)
            {
                return;
            }

            shakeTimer -= Time.deltaTime;

            if (shakeTimer <= 0f)
            {
                RestoreCamera();
                return;
            }

            Vector2 offset = Random.insideUnitCircle * shakeMagnitude;
            targetCamera.transform.localPosition = cameraOriginalLocalPosition + new Vector3(offset.x, offset.y, 0f);
        }

        private void RestoreCamera()
        {
            if (!cameraShakeActive || targetCamera == null)
            {
                cameraShakeActive = false;
                return;
            }

            targetCamera.transform.localPosition = cameraOriginalLocalPosition;
            cameraShakeActive = false;
            shakeTimer = 0f;
        }

        private void PlayClip(AudioClip clip)
        {
            if (audioSource == null || clip == null)
            {
                return;
            }

            audioSource.pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);
            audioSource.PlayOneShot(clip);
        }
    }
}
