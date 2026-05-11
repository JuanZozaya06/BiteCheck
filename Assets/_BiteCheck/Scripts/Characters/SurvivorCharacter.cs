using System;
using BiteCheck.Data;
using UnityEngine;

namespace BiteCheck.Characters
{
    public class SurvivorCharacter : MonoBehaviour
    {
        [SerializeField] private float walkSpeed = 2.5f;
        [SerializeField] private float stoppingDistance = 0.05f;
        [SerializeField] private bool createPlaceholderVisuals = true;
        [SerializeField] private Color humanColor = new Color(0.35f, 0.75f, 0.45f);
        [SerializeField] private Color infectedColor = new Color(0.65f, 0.45f, 0.75f);

        private SurvivorCase currentCase;
        private Transform targetDecisionPoint;
        private bool walkingToDecisionPoint;
        private bool reachedDecisionPoint;
        private Transform visualRoot;

        public event Action<SurvivorCharacter> OnReachedDecisionPoint;

        private void Awake()
        {
            if (createPlaceholderVisuals && transform.childCount == 0)
            {
                CreatePlaceholderVisuals();
            }
        }

        private void Update()
        {
            if (!walkingToDecisionPoint || targetDecisionPoint == null)
            {
                return;
            }

            MoveTowardDecisionPoint();
        }

        public void Initialize(SurvivorCase survivorCase)
        {
            currentCase = survivorCase ?? throw new ArgumentNullException(nameof(survivorCase));
            gameObject.name = $"Survivor_{currentCase.Id}";
            ApplyDebugColor(currentCase.Infected ? infectedColor : humanColor);
        }

        public void WalkToDecisionPoint(Transform decisionPoint)
        {
            targetDecisionPoint = decisionPoint != null
                ? decisionPoint
                : throw new ArgumentNullException(nameof(decisionPoint));

            walkingToDecisionPoint = true;
            reachedDecisionPoint = false;
        }

        public void StopAtDecisionPoint()
        {
            walkingToDecisionPoint = false;
            targetDecisionPoint = null;
        }

        public SurvivorCase GetCurrentCase()
        {
            return currentCase;
        }

        private void MoveTowardDecisionPoint()
        {
            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = targetDecisionPoint.position;
            Vector3 nextPosition = Vector3.MoveTowards(
                currentPosition,
                targetPosition,
                walkSpeed * Time.deltaTime);

            transform.position = nextPosition;

            Vector3 flatDirection = targetPosition - currentPosition;
            flatDirection.y = 0f;

            if (flatDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(flatDirection.normalized, Vector3.up);
            }

            if (Vector3.Distance(nextPosition, targetPosition) <= stoppingDistance)
            {
                ReachDecisionPoint();
            }
        }

        private void ReachDecisionPoint()
        {
            if (reachedDecisionPoint)
            {
                return;
            }

            reachedDecisionPoint = true;
            StopAtDecisionPoint();
            OnReachedDecisionPoint?.Invoke(this);
        }

        private void CreatePlaceholderVisuals()
        {
            visualRoot = new GameObject("PlaceholderVisuals").transform;
            visualRoot.SetParent(transform, false);

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(visualRoot, false);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = new Vector3(0.65f, 1f, 0.65f);

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(visualRoot, false);
            head.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            head.transform.localScale = Vector3.one * 0.6f;

            ApplyDebugColor(humanColor);
        }

        private void ApplyDebugColor(Color color)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();

            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.color = color;
            }
        }
    }
}
