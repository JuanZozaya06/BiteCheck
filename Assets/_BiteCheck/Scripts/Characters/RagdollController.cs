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
        [SerializeField] private float upwardForce = 0.08f;
        [SerializeField] private float torqueForce = 1.5f;
        [SerializeField] private float directionVariation = 0.05f;
        [SerializeField] private float forceVariation = 0.08f;
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
            Throw(direction, force, upwardForce, torqueForce);
        }

        public void Throw(Vector3 direction, float force, float customUpwardForce, float customTorqueForce)
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

            throwDirection = ApplyDirectionVariation(throwDirection);
            float variedForce = Mathf.Max(0f, force * Random.Range(1f - forceVariation, 1f + forceVariation));
            Vector3 impulse = throwDirection * variedForce;
            impulse += Vector3.up * Mathf.Max(0f, customUpwardForce) * variedForce;

            if (HasRagdollBodies())
            {
                ApplyImpulseToBodies(ragdollBodies, impulse, customTorqueForce);
            }
            else if (fallbackBody != null)
            {
                ApplyImpulse(fallbackBody, impulse, customTorqueForce);
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
                body.isKinematic = false;
                body.useGravity = false;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = true;
                return;
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

        private Vector3 ApplyDirectionVariation(Vector3 direction)
        {
            Vector3 variedDirection = direction;
            variedDirection.x += Random.Range(-directionVariation, directionVariation);
            variedDirection.z += Random.Range(-directionVariation, directionVariation);
            return variedDirection.sqrMagnitude > 0.001f ? variedDirection.normalized : direction;
        }

        private void ApplyImpulseToBodies(Rigidbody[] bodies, Vector3 impulse, float appliedTorqueForce)
        {
            for (int i = 0; i < bodies.Length; i++)
            {
                ApplyImpulse(bodies[i], impulse, appliedTorqueForce);
            }
        }

        private void ApplyImpulse(Rigidbody body, Vector3 impulse, float appliedTorqueForce)
        {
            if (body == null)
            {
                return;
            }

            body.AddForce(impulse, ForceMode.Impulse);

            if (appliedTorqueForce > 0f)
            {
                body.AddTorque(Random.insideUnitSphere * appliedTorqueForce, ForceMode.Impulse);
            }
        }
    }
}
