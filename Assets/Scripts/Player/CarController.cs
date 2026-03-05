using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

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

        // 检测和地面的角度
        public Transform groundCheckPoint; // 车底的检测点
        public float groundCheckLength = 1.0f;
        public LayerMask groundLayer; // 地面层
        private bool isGrounded;
        private Vector3 groundNormal; // 地面法线

        private float moveInput;
        private float turnInput;

        private void Start()
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            HandleInput();
        }

        private void FixedUpdate()
        {
            HandleGroundCheck();

            if (isGrounded)
            {
                AlignWithGround();
                Accelerate();
                Turn();
            }
            else
            {
                AlignWithPlane();
                rb.AddForce(Physics.gravity * 2f, ForceMode.Acceleration);
            }

            LimitSpeed();
        }

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
        }

        private void AlignWithGround()
        {
            var targetRotation = Quaternion.FromToRotation(transform.up, groundNormal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        private void AlignWithPlane()
        {
            var rotation = transform.eulerAngles;
            var targetRotation = Quaternion.Euler(rotation.x, rotation.y, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        private void LimitSpeed()
        {
            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }
        }

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
        }

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

