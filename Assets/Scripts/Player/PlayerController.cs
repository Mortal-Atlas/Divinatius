using UnityEngine;
using UnityEngine.InputSystem;
using Divinatius.Buffs;

namespace Divinatius.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 6.0f;
        [SerializeField] private float sprintSpeed = 10.0f;
        [SerializeField] private float rotationSpeed = 10.0f;
        [SerializeField] private float gravity = -19.62f;
        [SerializeField] private float jumpHeight = 1.2f;

        [Header("Camera & Mouse Look")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float mouseSensitivity = 2.0f;
        [SerializeField] private float minPitch = -25.0f;
        [SerializeField] private float maxPitch = 75.0f;
        [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 1.3f, -6.5f);
        [SerializeField] private float targetPivotHeight = 1.2f;
        [SerializeField] private float minZoomDistance = 2.5f;
        [SerializeField] private float maxZoomDistance = 14.0f;
        [SerializeField] private float zoomSpeed = 2.5f;

        private CharacterController _controller;
        private Vector3 _velocity;
        private Vector3 _currentHorizontalVelocity;
        private bool _isGrounded;
        private float _cameraPitch;
        private float _cameraYaw;
        private float _currentZoomDistance = 6.5f;
        private bool _controlsEnabled = true;
        private Camera _targetCamera;
        private float _baseFov = 60f;
        private float _sprintFov = 74f;

        public bool ControlsEnabled
        {
            get => _controlsEnabled;
            set
            {
                _controlsEnabled = value;
                Cursor.lockState = _controlsEnabled ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !_controlsEnabled;
            }
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }

            if (cameraTransform != null)
            {
                _targetCamera = cameraTransform.GetComponent<Camera>();
                if (_targetCamera != null)
                {
                    _baseFov = _targetCamera.fieldOfView;
                    _sprintFov = _baseFov + 14f;
                }
            }

            _cameraYaw = transform.eulerAngles.y;
            _cameraPitch = 18f; // Initial downward tilt to keep main character centered
            _currentZoomDistance = Mathf.Abs(cameraOffset.z);
            ControlsEnabled = true;

            EnsureBuffManager();
        }

        private void EnsureBuffManager()
        {
            if (PlayerBuffManager.Instance == null)
            {
                GameObject buffObj = new GameObject("PlayerBuffManager");
                buffObj.AddComponent<PlayerBuffManager>();
            }
        }

        private void Update()
        {
            if (!_controlsEnabled) return;

            HandleMouseLook();
            HandleMovement();
        }

        private void HandleMouseLook()
        {
            float mouseX = 0f;
            float mouseY = 0f;

            var mouse = Mouse.current;
            if (mouse != null)
            {
                Vector2 delta = mouse.delta.ReadValue();
                mouseX = delta.x * mouseSensitivity * 0.1f;
                mouseY = delta.y * mouseSensitivity * 0.1f;

                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    _currentZoomDistance -= Mathf.Sign(scroll) * zoomSpeed * 0.5f;
                    _currentZoomDistance = Mathf.Clamp(_currentZoomDistance, minZoomDistance, maxZoomDistance);
                }
            }

            _cameraYaw += mouseX;
            _cameraPitch -= mouseY;
            _cameraPitch = Mathf.Clamp(_cameraPitch, minPitch, maxPitch);

            Quaternion cameraRotation = Quaternion.Euler(_cameraPitch, _cameraYaw, 0f);
            if (cameraTransform != null)
            {
                Vector3 targetPivot = transform.position + Vector3.up * targetPivotHeight;
                Vector3 offsetDir = new Vector3(cameraOffset.x, cameraOffset.y, -_currentZoomDistance);
                cameraTransform.rotation = cameraRotation;
                cameraTransform.position = targetPivot + cameraRotation * offsetDir;
            }
        }

        private void HandleMovement()
        {
            _isGrounded = _controller.isGrounded;
            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }

            float horizontal = 0f;
            float vertical = 0f;
            bool isSprinting = false;
            bool jumpPressed = false;

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;

                isSprinting = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
                jumpPressed = keyboard.spaceKey.wasPressedThisFrame;
            }

            Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;
            Vector3 targetHorizontalVelocity = Vector3.zero;
            bool isMovingFast = false;

            if (inputDir.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + _cameraYaw;
                float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, Time.deltaTime * rotationSpeed);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                float speedMultiplier = 1.0f;
                if (PlayerBuffManager.Instance != null)
                {
                    speedMultiplier = PlayerBuffManager.Instance.GetSpeedMultiplier();
                }

                float speed = (isSprinting ? sprintSpeed : moveSpeed) * speedMultiplier;
                targetHorizontalVelocity = moveDir.normalized * speed;
                isMovingFast = isSprinting && speed > moveSpeed * 0.9f;
            }

            // Smooth momentum (acceleration / deceleration weight)
            float accelRate = targetHorizontalVelocity.magnitude > 0.1f ? 14.0f : 10.0f;
            _currentHorizontalVelocity = Vector3.Lerp(_currentHorizontalVelocity, targetHorizontalVelocity, Time.deltaTime * accelRate);
            _controller.Move(_currentHorizontalVelocity * Time.deltaTime);

            // Ensure camera reference
            if (_targetCamera == null && Camera.main != null)
            {
                _targetCamera = Camera.main;
                _baseFov = _targetCamera.fieldOfView;
                _sprintFov = _baseFov + 18f;
            }

            // Dynamic Sprint FOV Effect (60° -> 78°)
            if (_targetCamera != null)
            {
                bool isSprintingFast = isSprinting && _currentHorizontalVelocity.magnitude > moveSpeed * 0.8f;
                float targetFov = isSprintingFast ? 78f : 60f;
                _targetCamera.fieldOfView = Mathf.Lerp(_targetCamera.fieldOfView, targetFov, Time.deltaTime * 8.0f);
            }

            // Proximity NPC Pushing (triggers push force when walking into NPCs)
            if (_currentHorizontalVelocity.sqrMagnitude > 0.1f)
            {
                Collider[] hits = Physics.OverlapSphere(transform.position + Vector3.up * 0.9f, 1.25f);
                foreach (var h in hits)
                {
                    if (h.gameObject == gameObject) continue;
                    var npc = h.GetComponent<NPC.NPCWanderer>();
                    if (npc == null) npc = h.GetComponentInParent<NPC.NPCWanderer>();
                    if (npc != null)
                    {
                        Vector3 pushDir = (npc.transform.position - transform.position);
                        pushDir.y = 0;
                        if (pushDir.sqrMagnitude < 0.01f) pushDir = transform.forward;
                        npc.ReceivePush(pushDir.normalized * 8.0f);
                    }
                }
            }

            if (jumpPressed && _isGrounded)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            _velocity.y += gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.gameObject == null) return;

            var npc = hit.gameObject.GetComponent<NPC.NPCWanderer>();
            if (npc == null)
            {
                npc = hit.gameObject.GetComponentInParent<NPC.NPCWanderer>();
            }

            if (npc != null)
            {
                Vector3 pushDir = hit.moveDirection;
                pushDir.y = 0;
                if (pushDir.sqrMagnitude < 0.01f) pushDir = (npc.transform.position - transform.position);
                pushDir.y = 0;
                npc.ReceivePush(pushDir.normalized * 8.0f);
            }
        }
    }
}
