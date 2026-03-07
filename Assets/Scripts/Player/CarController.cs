using System.Collections;
using System.Collections.Generic;
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

        [Header("移动性能")]
        public float maxSpeed = 30f;
        public float acceleration = 20f;
        public float turnStrength = 5f;

        private float moveInput;
        private float turnInput;

        [Header("漂移性能")]
        public float driftTurnStrength = 8f;
        public float driftSlideFactor = 0.3f;
        public float minDriftTime = 0.5f;
        public float boostDuration = 1.0f;
        public float boostForce = 15f;

        private bool isDrifting = false;
        private float driftStartTime = 0f;
        private float boostTimeRemaining = 0f;
        private float driftInput = 1f;

        [Header("地面检测")]
        // 检测和地面的角度
        public Transform groundCheckPoint; // 车底的检测点
        public float groundCheckLength = 1.0f;
        public LayerMask groundLayer; // 地面层
        private bool isGrounded;
        private Vector3 groundNormal; // 地面法线

        [Header("视觉效果")]
        public Transform visualModel;
        public float driftSquashScale = 0.8f;
        public float driftStretchScale = 1.2f;
        public float turnTiltAngle = 15f; // 正常移动时倾斜的最大角度
        public float turnTiltSpeed = 10f; // 正常移动时倾斜变换的速度
        public float driftTiltAngle = 15f; // 漂移时倾斜的最大角度
        public float driftTiltSpeed = 10f; // 倾斜变换的速度


        private void Start()
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            HandleInput();

            if (Keyboard.current.shiftKey.wasPressedThisFrame && isGrounded && rb.velocity.magnitude > 0f && !isDrifting)
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

                if (boostTimeRemaining > 0)
                {
                    rb.AddForce(transform.forward * boostForce, ForceMode.Acceleration);
                    boostTimeRemaining -= Time.fixedDeltaTime;
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
            }

            LimitSpeed();
        }

        #region BaseMove

        private void Accelerate()
        {
            var force = transform.forward * moveInput * acceleration;
            rb.AddForce(force, ForceMode.Acceleration);
        }

        private void Turn()
        {
            //float turnAmount = turnInput * turnStrength * rb.velocity.magnitude / maxSpeed;
            float turnAmount = turnInput * turnStrength;
            transform.Rotate(0, turnAmount, 0);

            // 视觉效果
            float targetTilt = -turnInput * turnTiltAngle;
            var targetRotation = Quaternion.Euler(0, 0, targetTilt);
            visualModel.localRotation = Quaternion.Slerp(visualModel.localRotation, targetRotation, Time.fixedDeltaTime * turnTiltSpeed);
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

            visualModel.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            visualModel.DOLocalRotate(Vector3.zero, 0.4f).SetEase(Ease.OutElastic);

            if (driftDuration >= minDriftTime)
            {
                boostTimeRemaining = boostDuration;
            }
        }

        private void Drifting()
        {

            // 漂移有点问题
            var localVelocity = transform.InverseTransformDirection(rb.velocity);

            float driftTurn = driftInput * driftTurnStrength;
            transform.Rotate(0, driftTurn, 0);

            // 视觉效果
            float targetTilt = -driftInput * driftTiltAngle;
            var targetRotation = Quaternion.Euler(0, 0, targetTilt);
            visualModel.localRotation = Quaternion.Slerp(visualModel.localRotation, targetRotation, Time.fixedDeltaTime * driftTiltSpeed);

            var forwardVelocity = transform.forward * localVelocity.z;
            var sidewayVelocity = driftSlideFactor * localVelocity.x * transform.right;
            var verticalSpeed = rb.velocity.y;

            float currentAccelerate = moveInput * acceleration;

            var targetVelocity = forwardVelocity + sidewayVelocity + currentAccelerate * Time.fixedDeltaTime * transform.forward;
            targetVelocity.y = verticalSpeed;

            rb.velocity = targetVelocity;

            //if (Mathf.Abs(driftInput) > 0.1f)
            //{
            //    rb.AddForce(transform.right * turnInput * 5f, ForceMode.Acceleration);
            //}
        }


        #endregion


        #region Handle

        private void HandleGroundCheck()
        {
            // 向下发射射线检测地面存在以及地面的法线
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

        #region DEBUG

        private void OnDrawGizmosSelected()
        {
            if (groundCheckPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(groundCheckPoint.position, groundCheckPoint.position - transform.up * groundCheckLength);
            }
        }

        #endregion
    }
}

