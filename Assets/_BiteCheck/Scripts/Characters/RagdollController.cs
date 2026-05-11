using UnityEngine;

namespace BiteCheck.Characters
{
    public class RagdollController : MonoBehaviour
    {
        [Header("Fallback Physics")]
        [SerializeField] private Rigidbody fallbackBody;
        [SerializeField] private Collider fallbackCollider;

        [Header("Future Ragdoll")]
        [SerializeField] private Rigidbody[] ragdollBodies;
        [SerializeField] private Collider[] ragdollColliders;

        [Header("Throw")]
        [SerializeField] private float upwardForce = 0.12f;
        [SerializeField] private float torqueForce = 8f;
        [SerializeField] private float despawnDelay = 2.5f;

        private SurvivorCharacter survivorCharacter;
        private bool thrown;

        private void Awake()
        {
            survivorCharacter = GetComponent<SurvivorCharacter>();
            EnsureFallbackPhysics();
            SetAnimatedMode();
        }

        public void SetAnimatedMode()
        {
            thrown = false;

            SetBodyMode(fallbackBody, false);
            SetColliderMode(fallbackCollider, false);

            SetBodiesMode(ragdollBodies, false);
            SetCollidersMode(ragdollColliders, false);

            if (survivorCharacter != null)
            {
                survivorCharacter.enabled = true;
            }
        }

        public void SetRagdollMode()
        {
            SetBodyMode(fallbackBody, true);
            SetColliderMode(fallbackCollider, true);

            SetBodiesMode(ragdollBodies, true);
            SetCollidersMode(ragdollColliders, true);
        }

        public void Throw(Vector3 direction, float force)
        {
            if (thrown)
            {
                return;
            }

            thrown = true;

            if (survivorCharacter != null)
            {
                survivorCharacter.StopAtDecisionPoint();
                survivorCharacter.enabled = false;
            }

            SetRagdollMode();

            Vector3 throwDirection = direction.sqrMagnitude > 0.001f
                ? direction.normalized
                : transform.right;

            Vector3 impulse = (throwDirection + Vector3.up * upwardForce).normalized * force;

            if (HasRagdollBodies())
            {
                ApplyImpulseToBodies(ragdollBodies, impulse);
            }
            else if (fallbackBody != null)
            {
                ApplyImpulse(fallbackBody, impulse);
            }

            Destroy(gameObject, despawnDelay);
        }

        private void EnsureFallbackPhysics()
        {
            if (fallbackBody == null)
            {
                fallbackBody = GetComponent<Rigidbody>();
            }

            if (fallbackBody == null)
            {
                fallbackBody = gameObject.AddComponent<Rigidbody>();
            }

            if (fallbackCollider == null)
            {
                fallbackCollider = GetComponent<Collider>();
            }

            if (fallbackCollider == null)
            {
                fallbackCollider = gameObject.AddComponent<CapsuleCollider>();
                CapsuleCollider capsuleCollider = (CapsuleCollider)fallbackCollider;
                capsuleCollider.center = new Vector3(0f, 1.1f, 0f);
                capsuleCollider.height = 2.2f;
                capsuleCollider.radius = 0.4f;
            }
        }

        private bool HasRagdollBodies()
        {
            if (ragdollBodies == null)
            {
                return false;
            }

            for (int i = 0; i < ragdollBodies.Length; i++)
            {
                if (ragdollBodies[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void SetBodiesMode(Rigidbody[] bodies, bool physicsEnabled)
        {
            if (bodies == null)
            {
                return;
            }

            for (int i = 0; i < bodies.Length; i++)
            {
                SetBodyMode(bodies[i], physicsEnabled);
            }
        }

        private void SetBodyMode(Rigidbody body, bool physicsEnabled)
        {
            if (body == null)
            {
                return;
            }

            if (!physicsEnabled)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            body.isKinematic = !physicsEnabled;
            body.useGravity = physicsEnabled;
        }

        private void SetCollidersMode(Collider[] colliders, bool enabledState)
        {
            if (colliders == null)
            {
                return;
            }

            for (int i = 0; i < colliders.Length; i++)
            {
                SetColliderMode(colliders[i], enabledState);
            }
        }

        private void SetColliderMode(Collider targetCollider, bool enabledState)
        {
            if (targetCollider == null)
            {
                return;
            }

            targetCollider.enabled = enabledState;
        }

        private void ApplyImpulseToBodies(Rigidbody[] bodies, Vector3 impulse)
        {
            for (int i = 0; i < bodies.Length; i++)
            {
                ApplyImpulse(bodies[i], impulse);
            }
        }

        private void ApplyImpulse(Rigidbody body, Vector3 impulse)
        {
            if (body == null)
            {
                return;
            }

            body.AddForce(impulse, ForceMode.Impulse);
            body.AddTorque(Random.insideUnitSphere * torqueForce, ForceMode.Impulse);
        }
    }
}
