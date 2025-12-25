#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitySimpleLiquid
{
    /// <summary>
    /// Dispenses liquid from a container controlled by a Hinge Joint lever
    /// Transfers liquid to containers positioned below the spout
    /// </summary>
    public class LiquidDispenser : MonoBehaviour
    {
        [Header("Hinge Joint Settings")]
        [SerializeField]
        private HingeJoint hingeJoint;
        [SerializeField]
        [Tooltip("Hinge joint angle threshold to trigger dispensing")]
        private float dispenseAngleThreshold = 0f;

        [Header("Dispenser Settings")]
        [SerializeField]
        [Tooltip("Maximum liquid amount the dispenser can hold")]
        private float maxLiquidAmount = 10f;
        [SerializeField]
        [Tooltip("If enabled, the dispenser has unlimited liquid")]
        private bool unlimitedLiquid = true;
        [SerializeField]
        [Tooltip("How fast liquid is dispensed from the dispenser")]
        public float dispenseSpeed = 1f;
        [SerializeField]
        [Tooltip("Color of the liquid being dispensed")]
        public Color dispensedLiquidColor = Color.cyan;

        private float currentLiquidAmount;

        [Header("Particle System")]
        [SerializeField]
        private ParticleSystem particleSystem;
        [SerializeField]
        [Tooltip("Scale of particle emission based on dispense amount")]
        [Range(0.1f, 2f)]
        public float particleScale = 1f;

        private bool isDispensing;
        private float lastJointAngle;
        private ParticleSystem.CollisionModule collisionModule;
        private Dictionary<Collider, LiquidContainer> collidingContainers = new Dictionary<Collider, LiquidContainer>();

        public bool IsDispensing
        {
            get { return isDispensing; }
            private set { isDispensing = value; }
        }

        private void Awake()
        {
            if (!hingeJoint)
                hingeJoint = GetComponent<HingeJoint>();

            // Find particle system if not assigned
            if (!particleSystem)
                particleSystem = GetComponentInChildren<ParticleSystem>();

            if (particleSystem)
            {
                collisionModule = particleSystem.collision;
                Debug.Log($"Found ParticleSystem: {particleSystem.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("No ParticleSystem found!");
            }

            // Initialize with full liquid amount
            currentLiquidAmount = maxLiquidAmount;
        }

        private void Start()
        {
            lastJointAngle = hingeJoint.angle;
        }

        private void Update()
        {
            if (!hingeJoint)
                return;

            // Check if hinge joint has been pulled beyond threshold
            float currentAngle = hingeJoint.angle;
            float angleDifference = Mathf.Abs(currentAngle - lastJointAngle);

            // Determine if we should be dispensing based on joint angle
            bool shouldDispense = Mathf.Abs(currentAngle) > dispenseAngleThreshold;

            // Update dispensing state
            if (shouldDispense && (unlimitedLiquid || currentLiquidAmount > 0f))
            {
                Debug.Log("Dispensing liquid...");

                if (!isDispensing)
                {
                    StartDispensing();
                }
                DispenseLiquid();
            }
            else
            {
                if (isDispensing)
                {
                    StopDispensing();
                }
            }

            lastJointAngle = currentAngle;
        }

        private void StartDispensing()
        {
            isDispensing = true;
            Debug.Log($"Particle system: {particleSystem}");

            if (particleSystem)
            {
                var main = particleSystem.main;
                main.startColor = dispensedLiquidColor;
                particleSystem.Play();
            }
        }

        private void StopDispensing()
        {
            isDispensing = false;

            if (particleSystem)
                particleSystem.Stop();
        }

        private void DispenseLiquid()
        {
            // Calculate flow rate based on current joint angle
            float flowScale = Mathf.Clamp01(Mathf.Abs(hingeJoint.angle) / 90f);
            float liquidStep = dispenseSpeed * Time.deltaTime * flowScale;

            // Remove liquid from dispenser (unless unlimited)
            if (liquidStep > 0f)
            {
                if (!unlimitedLiquid)
                {
                    currentLiquidAmount -= liquidStep;
                    if (currentLiquidAmount < 0f)
                        currentLiquidAmount = 0f;
                }

                // Update particle scale based on flow
                if (particleSystem)
                {
                    var main = particleSystem.main;
                    main.startSize = particleScale * flowScale;
                }

                // Transfer liquid to any containers particles are colliding with
                TransferLiquidToCollidingContainers(liquidStep, flowScale);
            }
        }

        private void TransferLiquidToCollidingContainers(float liquidStep, float flowScale)
        {
            // Transfer liquid to each container that particles are currently hitting
            foreach (var kvp in collidingContainers)
            {
                LiquidContainer container = kvp.Value;
                if (container)
                {
                    Debug.Log($"Transferring {liquidStep} to {container.gameObject.name}, current fill: {container.FillAmountPercent}");
                    container.FillAmount += liquidStep;
                    MixLiquidColor(container);
                }
            }
            
            if (collidingContainers.Count == 0)
                Debug.Log("No containers in collision dictionary");
        }

        private LiquidContainer FindContainerBelow(Vector3 spoutPos)
        {
            // This method is kept for compatibility but is no longer used
            // Collision detection is now handled by OnParticleTrigger()
            return null;
        }

        private void TransferLiquid(LiquidContainer targetContainer, float liquidAmount, float flowScale)
        {
            // Transfer to target container
            targetContainer.FillAmount += liquidAmount;

            // Mix colors
            MixLiquidColor(targetContainer);
        }

        private void MixLiquidColor(LiquidContainer targetContainer)
        {
            // Simple color mixing based on dispenser liquid color
            float mixSpeed = dispenseSpeed * Time.deltaTime;
            targetContainer.LiquidColor = Color.Lerp(
                targetContainer.LiquidColor,
                dispensedLiquidColor,
                mixSpeed * 0.5f
            );
        }


        private void OnParticleCollision(GameObject other)
        {
            Debug.Log($"OnParticleCollision called with: {other.name}");
            
            if (!particleSystem || other == null)
                return;

            Debug.Log($"Particle collision detected with: {other.name}");

            // Check if the colliding object has a LiquidContainer
            LiquidContainer container = other.GetComponent<LiquidContainer>();
            if (container)
            {
                Debug.Log($"Found LiquidContainer on {other.name}");
                Collider col = other.GetComponent<Collider>();
                if (col && !collidingContainers.ContainsKey(col))
                {
                    collidingContainers[col] = container;
                    Debug.Log($"Added {other.name} to colliding containers. Total: {collidingContainers.Count}");
                }
                return;
            }

            // Check if the colliding object has a SplitController (which has a LiquidContainer)
            SplitController splitController = other.GetComponent<SplitController>();
            if (splitController && splitController.liquidContainer)
            {
                Debug.Log($"Found SplitController on {other.name}");
                Collider col = other.GetComponent<Collider>();
                if (col && !collidingContainers.ContainsKey(col))
                {
                    collidingContainers[col] = splitController.liquidContainer;
                    Debug.Log($"Added {other.name} to colliding containers. Total: {collidingContainers.Count}");
                }
                return;
            }

            Debug.Log($"No LiquidContainer found on {other.name}");
        }

        #region Gizmos
        private void OnDrawGizmosSelected()
        {
            // No gizmos needed for particle collision-based detection
        }
        #endregion
    }
}
