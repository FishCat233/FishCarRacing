using UnityEngine.InputSystem;
using UnityEngine;
using DG.Tweening;

namespace FishCarRacing.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class CarController : MonoBehaviour
    {
        [Header("引用")]
        public Rigidbody rb;

        [Header("地面检测")]
        public Transform groundCheckPoint; // 车底的检测点
        public float groundCheckLength = 1.0f;
        public LayerMask groundLayer; // 地面层
        private bool isGrounded;
        private Vector3 groundNormal; // 地面法线

        [Header("移动性能")]
        public float maxSpeed = 30f;
        public float maxReverseSpeed = 12f;
        public float acceleration = 20f;
        public float turnStrength = 5f;
        

        [Header("直行")]
        [Tooltip("向前移动的响应速度")]
        public float forwardResponse = 10f;

        [Tooltip("减速速度")]
        public float brakeDecel = 25f;

        [Tooltip("侧滑抑制")]
        public float sideFriction = 10f;

        [Tooltip("转向强度的缩放  1=不缩放")]
        public float turnBySpeedFactor = 0.6f;

        [Header("漂移性能")]
        public float driftTurnStrength = 8f;
        public float driftSlideFactor = 0.3f;
        public float minDriftTime = 0.5f;
        public float driftCooldown = 0.5f;
        public float boostDuration = 1.0f;

        [Header("漂移属性")]
        [Tooltip("向前漂移的响应速度")]
        public float driftForwardResponse = 12f;

        [Tooltip("侧向漂移的响应速度")]
        public float driftSideResponse = 18f;

        [Tooltip("向前漂移的速度加成")]
        public float boostSpeedBonus = 10f;

        [Tooltip("漂移的衰减速度")]
        public float boostDecay = 8f;

        [Header("视觉效果")]
        public Transform visualModel;
        public Transform visualModelBody;
        public Transform[] wheelMeshes;
        public WheelCollider[] wheelColliders;

        public float turnWheelAngle = 30f;
        public float driftWheelAngle = 60f;

        public float turnTiltAngle = 15f;
        public float turnTiltSpeed = 10f;
        public float driftTiltAngle = 15f;
        public float driftTiltSpeed = 10f;

        private float moveInput;
        private float turnInput;

        private bool isDrifting = false;
        private float driftStartTime = 0f;
        private float nextDriftCanStartTime = 0f;
        private float boostTimeRemaining = 0f;
        private float driftInput = 1f;

        private float boostSpeedCurrent = 0f;
        
        public float SpeedKmh { get; private set; }
        private float moveInput;
        private float turnInput;

        private Quaternion visualModelInitialRotation;
        private Vector3 visualModelInitialScale;
        

        private void Start()
        {
            if (rb == null) rb = GetComponent<Rigidbody>();

            if (visualModelBody != null)
            {
                visualModelInitialRotation = visualModelBody.localRotation;
                visualModelInitialScale = visualModelBody.localScale;
            }
        }

        private void Update()
        {
            HandleInput();

            if (Keyboard.current.shiftKey.wasPressedThisFrame &&
                isGrounded &&
                Time.time >= nextDriftCanStartTime &&
                rb.velocity.magnitude > 0.1f &&
                !isDrifting)
            {
                StartDrift();
            }

            if (!Keyboard.current.shiftKey.isPressed && isDrifting)
            {
                EndDrift();
            }
        }

        private void FixedUpdate()
        {
            HandleGroundCheck();

            if (isGrounded)
            {
                AlignWithGround();

                // boost 用速度重构
                if (boostTimeRemaining > 0)
                {
                    boostTimeRemaining -= Time.fixedDeltaTime;
                    boostSpeedCurrent = boostSpeedBonus;
                }
                else
                {
                    boostSpeedCurrent = Mathf.MoveTowards(boostSpeedCurrent, 0f, boostDecay * Time.fixedDeltaTime);
                }

                if (isDrifting)
                {
                    Drifting();
                }
                else
                {
                    Accelerate();
                    Turn();
                }
            }
            else
            {
                AlignWithPlane();
                rb.AddForce(Physics.gravity * 2f, ForceMode.Acceleration);
                
                // 空中不驱动
                wheelColliders[2].motorTorque = 0f;
                wheelColliders[3].motorTorque = 0f;
                
            }

            LimitSpeed();
            
            UpdateSpeedData();
        }

        private void LateUpdate()
        {
            int count = Mathf.Min(wheelMeshes.Length, wheelColliders.Length);
            for (int i = 0; i < count; i++)
            {
                wheelColliders[i].GetWorldPose(out Vector3 pos, out Quaternion _);
                if (wheelMeshes[i] != null) wheelMeshes[i].position = pos;
            }
            UpdateWheel();
        }

        #region BaseMove

        private void Accelerate()
        {
            Vector3 local = transform.InverseTransformDirection(rb.velocity);

            float currentForward = local.z;
            float targetForward;

            if (Mathf.Abs(moveInput) > 0.01f)
            {
                float desired = moveInput > 0f ? maxSpeed : -maxReverseSpeed;
                float maxDelta = acceleration * Time.fixedDeltaTime;
                targetForward = Mathf.MoveTowards(currentForward, desired, maxDelta * forwardResponse);
            }
            else
            {
                targetForward = Mathf.MoveTowards(currentForward, 0f, brakeDecel * Time.fixedDeltaTime);
            }

            // 侧向速度
            float targetSide = Mathf.MoveTowards(local.x, 0f, sideFriction * Time.fixedDeltaTime);

            Vector3 newLocal = new Vector3(targetSide, 0f, targetForward);
            Vector3 newWorld = transform.TransformDirection(newLocal);
            newWorld.y = rb.velocity.y;
            rb.velocity = newWorld;
        }

        private void Turn()
        {
            float speed01 = Mathf.Clamp01(rb.velocity.magnitude / Mathf.Max(0.01f, maxSpeed));
            float speedScale = Mathf.Lerp(0.3f, 1f, speed01);

            float turnAmount = turnInput * turnStrength * Mathf.Lerp(1f, speedScale, turnBySpeedFactor);
            transform.Rotate(0f, turnAmount, 0f);

            // 视觉效果
            float targetTilt = -turnInput * turnTiltAngle;
            var targetRotation = visualModelInitialRotation * Quaternion.Euler(0, 0, targetTilt);
            visualModelBody.localRotation = Quaternion.Slerp(visualModelBody.localRotation, targetRotation, Time.fixedDeltaTime * turnTiltSpeed);
        }

        private void AlignWithGround()
        {
            var targetRotation = Quaternion.FromToRotation(transform.up, groundNormal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        private void AlignWithPlane()
        {
            var rotation = transform.localEulerAngles;
            var targetRotation = Quaternion.Euler(rotation.x, rotation.y, 0);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * 10f);
        }

        private void LimitSpeed()
        {
            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }
        }

        #endregion

        #region Drift

        private void StartDrift()
        {
            isDrifting = true;
            driftStartTime = Time.time;

            //visualModel.DOScaleX(driftSquashScale, 0.2f).SetEase(Ease.OutQuad);
            //visualModel.DOScaleZ(driftStretchScale, 0.2f).SetEase(Ease.OutQuad);
            visualModel.DOLocalJump(Vector3.zero, 0.5f, 1, 0.3f);
        }

        private void EndDrift()
        {
            isDrifting = false;
            float driftDuration = Time.time - driftStartTime;

            nextDriftCanStartTime = Time.time + driftCooldown;

            visualModelBody.DOScale(visualModelInitialScale, 0.3f).SetEase(Ease.OutBack);
            visualModelBody.DOLocalRotate(visualModelInitialRotation.eulerAngles, 0.4f).SetEase(Ease.OutElastic);

            if (driftDuration >= minDriftTime)
            {
                boostTimeRemaining = boostDuration;
            }
        }

        private void Drifting()
        {
            var local = transform.InverseTransformDirection(rb.velocity);

            // 漂移
            float driftTurn = driftInput * driftTurnStrength;
            transform.Rotate(0, driftTurn, 0);

            // 视觉效果
            float targetTilt = -driftInput * driftTiltAngle;
            var targetRotation = visualModelInitialRotation * Quaternion.Euler(0, 0, targetTilt);
            visualModelBody.localRotation = Quaternion.Slerp(visualModelBody.localRotation, targetRotation, Time.fixedDeltaTime * driftTiltSpeed);
            

            // 前向
            float desiredForward = local.z;
            if (Mathf.Abs(moveInput) > 0.01f)
            {
                float desired = moveInput > 0f ? maxSpeed : -maxReverseSpeed;
                desiredForward = Mathf.MoveTowards(local.z, desired, acceleration * Time.fixedDeltaTime);
            }
            else
            {
                desiredForward = Mathf.MoveTowards(local.z, 0f, brakeDecel * Time.fixedDeltaTime);
            }

            desiredForward = Mathf.Clamp(desiredForward + boostSpeedCurrent, -maxReverseSpeed, maxSpeed);
            float targetForward = Mathf.MoveTowards(local.z, desiredForward, driftForwardResponse * Time.fixedDeltaTime);

            // 侧向
            float desiredSide = local.x * driftSlideFactor;
            float targetSide = Mathf.Lerp(local.x, desiredSide, 1f - Mathf.Exp(-driftSideResponse * Time.fixedDeltaTime));
            
            float y = rb.velocity.y;

            Vector3 newLocal = new Vector3(targetSide, 0f, targetForward);
            Vector3 newWorld = transform.TransformDirection(newLocal);
            newWorld.y = y;
            rb.velocity = newWorld;
        }

        #endregion

        #region Visual

        private void UpdateWheel()
        {
            float targetAngle = (isDrifting ? driftWheelAngle : turnWheelAngle) * turnInput;
            var localVelocity = transform.InverseTransformDirection(rb.velocity);
            float rollSpeed = localVelocity.z * Time.deltaTime * 360f;

            RotateWheel(wheelMeshes[0], targetAngle, rollSpeed);
            RotateWheel(wheelMeshes[1], targetAngle, rollSpeed);
            RotateWheel(wheelMeshes[2], 0f, rollSpeed);
            RotateWheel(wheelMeshes[3], 0f, rollSpeed);
        }

        private void RotateWheel(Transform wheel, float steeringAngle, float rollSpeed)
        {
            if (wheel == null) return;

            wheel.Rotate(Vector3.right, rollSpeed, Space.Self);

            Vector3 currentLocalEuler = wheel.localEulerAngles;
            wheel.localRotation = Quaternion.Euler(currentLocalEuler.x, steeringAngle, 0f);
        }

        #endregion

        #region Handle

        private void HandleGroundCheck()
        {
            isGrounded = false;
            groundNormal = Vector3.up;
            RaycastHit hit;
            if (Physics.Raycast(groundCheckPoint.position, -transform.up, out hit, groundCheckLength, groundLayer))
            {
                isGrounded = true;
                groundNormal = hit.normal;
            }
            else
            {
                isGrounded = false;
                groundNormal = Vector3.up;
            }
        }

        private void HandleInput()
        {
            var wKey = Keyboard.current.wKey.isPressed;
            var sKey = Keyboard.current.sKey.isPressed;
            if (wKey) moveInput = 1f;
            else if (sKey) moveInput = -1f;
            else moveInput = 0f;

            var aKey = Keyboard.current.aKey.isPressed;
            var dKey = Keyboard.current.dKey.isPressed;
            if (aKey) turnInput = -1f;
            else if (dKey) turnInput = 1f;
            else turnInput = 0f;

            driftInput = turnInput != 0f ? turnInput : driftInput;
        }

        #endregion

        #region Tools
        

        private void UpdateSpeedData()
        {
            SpeedKmh = rb.velocity.magnitude * 3.6f;
        }

        #endregion

        #region DEBUG

        private void OnDrawGizmosSelected()
        {

            bool hitGround = Physics.Raycast(
                groundCheckPoint.position,
                -transform.up,
                groundCheckLength,
                groundLayer,
                QueryTriggerInteraction.Ignore);

            Gizmos.color = hitGround ? Color.red : Color.yellow;
            Gizmos.DrawLine(
                groundCheckPoint.position,
                groundCheckPoint.position - transform.up * groundCheckLength);
        }

        #endregion
    }
}
