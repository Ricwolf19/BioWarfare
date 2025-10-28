using UnityEngine;
using UnityEngine.InputSystem;

namespace BioWarfare.Core
{
    /// <summary>
    /// Third-person player movement with New Input System.
    /// Simple, camera-relative controls for survival horror gameplay.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 3.5f;
        [SerializeField] private float sprintSpeed = 6.0f;
        [SerializeField] private float rotationSpeed = 10f;
        
        [Header("Physics")]
        [SerializeField] private float gravity = -15f;
        
        [Header("Input Actions")]
        [SerializeField] private InputActionAsset inputActions;
        
        private CharacterController controller;
        private InputAction moveAction;
        private InputAction sprintAction;
        private Vector2 moveInput;
        private Vector3 velocity;
        private Transform mainCamera;
        
        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            mainCamera = Camera.main.transform;
            
            // Get input actions from the asset
            var playerMap = inputActions.FindActionMap("Player");
            moveAction = playerMap.FindAction("Move");
            sprintAction = playerMap.FindAction("Sprint");
        }
        
        private void OnEnable()
        {
            moveAction.Enable();
            sprintAction.Enable();
        }
        
        private void OnDisable()
        {
            moveAction.Disable();
            sprintAction.Disable();
        }
        
        private void Update()
        {
            HandleMovement();
            ApplyGravity();
        }
        
        /// <summary>
        /// Handles first-person movement.
        /// Movement is relative to where player is facing (camera controls rotation).
        /// </summary>
        private void HandleMovement()
        {
            // Read input
            moveInput = moveAction.ReadValue<Vector2>();
            
            if (moveInput.sqrMagnitude < 0.01f)
                return;
            
            // Calculate movement direction relative to player's forward
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            
            // Get movement direction (already on horizontal plane)
            Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;
            
            // Determine speed
            bool isSprinting = sprintAction.IsPressed();
            float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
            
            // Move
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);
        }
        
        /// <summary>
        /// Applies gravity to player.
        /// </summary>
        private void ApplyGravity()
        {
            if (controller.isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
            
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
        
        // Public API for other systems
        public bool IsMoving() => moveInput.sqrMagnitude > 0.01f;
        public bool IsGrounded() => controller.isGrounded;
    }
}