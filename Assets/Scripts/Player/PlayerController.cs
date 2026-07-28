using UnityEngine;

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
        [SerializeField] private float minPitch = -35.0f;
        [SerializeField] private float maxPitch = 60.0f;
        [SerializeField] private Vector3 cameraOffset = new Vector3(0, 1.8f, -3.5f);

        private CharacterController _controller;
        private Vector3 _velocity;
        private bool _isGrounded;
        private float _cameraPitch;
        private float _cameraYaw;
        private bool _controlsEnabled = true;

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

            _cameraYaw = transform.eulerAngles.y;
            _cameraPitch = 0f;
            ControlsEnabled = true;
        }

        private void Update()
        {
            if (!_controlsEnabled) return;

            HandleMouseLook();
            HandleMovement();
        }

        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            _cameraYaw += mouseX;
            _cameraPitch -= mouseY;
            _cameraPitch = Mathf.Clamp(_cameraPitch, minPitch, maxPitch);

            // Rotate camera around target
            Quaternion cameraRotation = Quaternion.Euler(_cameraPitch, _cameraYaw, 0f);
            if (cameraTransform != null)
            {
                cameraTransform.rotation = cameraRotation;
                cameraTransform.position = transform.position + cameraRotation * cameraOffset;
            }
        }

        private void HandleMovement()
        {
            _isGrounded = _controller.isGrounded;
            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;

            if (inputDir.magnitude >= 0.1f)
            {
                // Calculate move direction relative to camera facing yaw
                float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + _cameraYaw;
                float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, Time.deltaTime * rotationSpeed);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
                _controller.Move(moveDir.normalized * (speed * Time.deltaTime));
            }

            // Jump
            if (Input.GetButtonDown("Jump") && _isGrounded)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // Gravity
            _velocity.y += gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);
        }
    }
}
