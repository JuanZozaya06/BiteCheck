using System;
using BiteCheck.Data;
using UnityEngine;

namespace BiteCheck.Characters
{
    public class SurvivorCharacter : MonoBehaviour
    {
        [SerializeField] private float walkSpeed = 3.2f;
        [SerializeField] private float stoppingDistance = 0.05f;
        [SerializeField] private bool createPlaceholderVisuals = true;
        [SerializeField] private Color humanSkinColor = new Color(0.95f, 0.72f, 0.55f);
        [SerializeField] private Color infectedSkinColor = new Color(0.52f, 0.78f, 0.46f);
        [SerializeField] private Color shirtColor = new Color(0.25f, 0.55f, 0.95f);
        [SerializeField] private Color pantsColor = new Color(0.18f, 0.2f, 0.28f);
        [SerializeField] private Color infectedShirtColor = new Color(0.42f, 0.25f, 0.62f);
        [SerializeField] private float bobAmplitude = 0.08f;
        [SerializeField] private float bobSpeed = 9f;
        [SerializeField] private float limbSwingAngle = 18f;

        private SurvivorCase currentCase;
        private Transform targetDecisionPoint;
        private Vector3 exitTargetPosition;
        private bool walkingToDecisionPoint;
        private bool reachedDecisionPoint;
        private bool walkingToExitPoint;
        private float exitWalkSpeed = 7f;
        private float exitDespawnDelay = 0.15f;
        private Transform visualRoot;
        private Transform body;
        private Transform head;
        private Transform leftArm;
        private Transform rightArm;
        private Transform leftLeg;
        private Transform rightLeg;
        private Transform leftEye;
        private Transform rightEye;
        private Transform[] veinMarks;
        private float walkAnimTime;
        private bool infectedVisuals;

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
            if (walkingToExitPoint)
            {
                AnimatePlaceholder();
                MoveTowardExitPoint();
                return;
            }

            if (!walkingToDecisionPoint || targetDecisionPoint == null)
            {
                return;
            }

            AnimatePlaceholder();
            MoveTowardDecisionPoint();
        }

        public void Initialize(SurvivorCase survivorCase)
        {
            currentCase = survivorCase ?? throw new ArgumentNullException(nameof(survivorCase));
            gameObject.name = $"Survivor_{currentCase.Id}";
            infectedVisuals = currentCase.Infected;
            ApplyCaseVisuals();
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
            ResetPose();
        }

        public void WalkToExitPoint(Transform exitPoint, float speed, float despawnDelay)
        {
            if (exitPoint == null)
            {
                WalkToExitPoint(transform.position + Vector3.right * 5f, speed, despawnDelay);
                return;
            }

            WalkToExitPoint(exitPoint.position, speed, despawnDelay);
        }

        public void WalkToExitPoint(Vector3 targetPosition, float speed, float despawnDelay)
        {
            walkingToDecisionPoint = false;
            targetDecisionPoint = null;
            walkingToExitPoint = true;
            exitTargetPosition = targetPosition;
            exitWalkSpeed = Mathf.Max(0.1f, speed);
            exitDespawnDelay = Mathf.Max(0f, despawnDelay);
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

        private void MoveTowardExitPoint()
        {
            Vector3 currentPosition = transform.position;
            Vector3 nextPosition = Vector3.MoveTowards(
                currentPosition,
                exitTargetPosition,
                exitWalkSpeed * Time.deltaTime);

            transform.position = nextPosition;

            Vector3 flatDirection = exitTargetPosition - currentPosition;
            flatDirection.y = 0f;

            if (flatDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(flatDirection.normalized, Vector3.up);
            }

            if (Vector3.Distance(nextPosition, exitTargetPosition) <= stoppingDistance)
            {
                walkingToExitPoint = false;
                Destroy(gameObject, exitDespawnDelay);
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

            body = CreatePrimitivePart("Body", PrimitiveType.Capsule, visualRoot, new Vector3(0f, 1.05f, 0f), new Vector3(0.62f, 0.95f, 0.46f)).transform;
            head = CreatePrimitivePart("Head", PrimitiveType.Sphere, visualRoot, new Vector3(0f, 2.12f, 0.02f), Vector3.one * 0.58f).transform;
            leftArm = CreatePrimitivePart("LeftArm", PrimitiveType.Capsule, visualRoot, new Vector3(-0.47f, 1.13f, 0f), new Vector3(0.18f, 0.54f, 0.18f)).transform;
            rightArm = CreatePrimitivePart("RightArm", PrimitiveType.Capsule, visualRoot, new Vector3(0.47f, 1.13f, 0f), new Vector3(0.18f, 0.54f, 0.18f)).transform;
            leftLeg = CreatePrimitivePart("LeftLeg", PrimitiveType.Capsule, visualRoot, new Vector3(-0.2f, 0.34f, 0f), new Vector3(0.2f, 0.52f, 0.2f)).transform;
            rightLeg = CreatePrimitivePart("RightLeg", PrimitiveType.Capsule, visualRoot, new Vector3(0.2f, 0.34f, 0f), new Vector3(0.2f, 0.52f, 0.2f)).transform;
            leftEye = CreatePrimitivePart("LeftEye", PrimitiveType.Sphere, head, new Vector3(-0.16f, 0.08f, 0.45f), Vector3.one * 0.12f).transform;
            rightEye = CreatePrimitivePart("RightEye", PrimitiveType.Sphere, head, new Vector3(0.16f, 0.08f, 0.45f), Vector3.one * 0.12f).transform;
            veinMarks = new[]
            {
                CreatePrimitivePart("BlackVeinA", PrimitiveType.Cube, head, new Vector3(-0.25f, -0.04f, 0.44f), new Vector3(0.04f, 0.22f, 0.02f)).transform,
                CreatePrimitivePart("BlackVeinB", PrimitiveType.Cube, head, new Vector3(0.27f, -0.09f, 0.44f), new Vector3(0.04f, 0.18f, 0.02f)).transform,
                CreatePrimitivePart("BlackVeinC", PrimitiveType.Cube, body, new Vector3(0.28f, 0.18f, 0.52f), new Vector3(0.04f, 0.28f, 0.02f)).transform,
            };

            ApplyCaseVisuals();
        }

        private GameObject CreatePrimitivePart(string partName, PrimitiveType primitiveType, Transform parent, Vector3 localPosition, Vector3 localScale)
        {
            GameObject part = GameObject.CreatePrimitive(primitiveType);
            part.name = partName;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;

            Collider partCollider = part.GetComponent<Collider>();
            if (partCollider != null)
            {
                Destroy(partCollider);
            }

            return part;
        }

        private void ApplyCaseVisuals()
        {
            SetColor(body, infectedVisuals ? infectedShirtColor : shirtColor);
            SetColor(head, infectedVisuals ? infectedSkinColor : humanSkinColor);
            SetColor(leftArm, infectedVisuals ? infectedSkinColor : humanSkinColor);
            SetColor(rightArm, infectedVisuals ? infectedSkinColor : humanSkinColor);
            SetColor(leftLeg, pantsColor);
            SetColor(rightLeg, pantsColor);
            SetColor(leftEye, infectedVisuals ? Color.red : Color.black);
            SetColor(rightEye, infectedVisuals ? Color.red : Color.black);

            if (veinMarks == null)
            {
                return;
            }

            for (int i = 0; i < veinMarks.Length; i++)
            {
                if (veinMarks[i] == null)
                {
                    continue;
                }

                veinMarks[i].gameObject.SetActive(infectedVisuals);
                SetColor(veinMarks[i], Color.black);
            }
        }

        private void AnimatePlaceholder()
        {
            if (visualRoot == null)
            {
                return;
            }

            walkAnimTime += Time.deltaTime * bobSpeed;
            float bob = Mathf.Abs(Mathf.Sin(walkAnimTime)) * bobAmplitude;
            float swing = Mathf.Sin(walkAnimTime) * limbSwingAngle;
            float twitch = infectedVisuals ? Mathf.Sin(Time.time * 18f) * 3f : 0f;

            visualRoot.localPosition = new Vector3(0f, bob, 0f);

            SetLocalXRotation(leftArm, swing + twitch);
            SetLocalXRotation(rightArm, -swing - twitch);
            SetLocalXRotation(leftLeg, -swing * 0.75f);
            SetLocalXRotation(rightLeg, swing * 0.75f);

            if (head != null && infectedVisuals)
            {
                head.localRotation = Quaternion.Euler(twitch, 0f, Mathf.Sin(Time.time * 11f) * 4f);
            }
        }

        private void ResetPose()
        {
            if (visualRoot != null)
            {
                visualRoot.localPosition = Vector3.zero;
            }

            SetLocalXRotation(leftArm, 0f);
            SetLocalXRotation(rightArm, 0f);
            SetLocalXRotation(leftLeg, 0f);
            SetLocalXRotation(rightLeg, 0f);

            if (head != null)
            {
                head.localRotation = Quaternion.identity;
            }
        }

        private void SetLocalXRotation(Transform target, float angle)
        {
            if (target != null)
            {
                target.localRotation = Quaternion.Euler(angle, 0f, 0f);
            }
        }

        private void SetColor(Transform target, Color color)
        {
            if (target == null)
            {
                return;
            }

            Renderer targetRenderer = target.GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                targetRenderer.material.color = color;
            }
        }
    }
}
