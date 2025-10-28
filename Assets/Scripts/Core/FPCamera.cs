using UnityEngine;
using UnityEngine.InputSystem;

namespace BioWarfare.Core
{
    /// <summary>
    /// First-person camera controller with mouse look.
    /// Attach to Main Camera as child of player.
    /// </summary>
    public class FirstPersonCamera : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private float mouseSensitivity = 100f;
        [SerializeField] private float minVerticalAngle = -90f;
        [SerializeField] private float maxVerticalAngle = 90f;
        
        [Header("Input Actions")]
        [SerializeField] private InputActionAsset inputActions;
        
        private InputAction lookAction;
        private float xRotation = 0f;
        private Transform playerBody;
        
        private void Awake()
        {
            // Get player body (parent transform)
            playerBody = transform.parent;
            
            // Get look action
            var playerMap = inputActions.FindActionMap("Player");
            lookAction = playerMap.FindAction("Look");
        }
        
        private void OnEnable()
        {
            lookAction.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        private void OnDisable()
        {
            lookAction.Disable();
        }
        
        private void Update()
        {
            HandleMouseLook();
        }
        
        /// <summary>
        /// Handles first-person mouse look.
        /// Camera looks up/down, player body rotates left/right.
        /// </summary>
        private void HandleMouseLook()
        {
            Vector2 lookInput = lookAction.ReadValue<Vector2>();
            
            float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
            float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;
            
            // Rotate camera up/down (pitch)
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            
            // Rotate player body left/right (yaw)
            if (playerBody != null)
            {
                playerBody.Rotate(Vector3.up * mouseX);
            }
        }
    }
}