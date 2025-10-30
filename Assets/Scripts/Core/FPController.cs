using UnityEngine;

namespace BioWarfare.Core
{
	[RequireComponent(typeof(CharacterController))]
	public class FPController : MonoBehaviour
	{
		[Header("Movement Parameters")]
		public float WalkSpeed = 3.5f;
		public float SprintSpeed = 8f;
		public float Acceleration = 15f;

		[Header("Jump & Gravity Parameters")]
		public float JumpHeight = 2f;       // how high the character jumps
		public float GravityScale = 3f;     // multiplier for downward gravity
		public float GroundStick = -3f;     // small downward force to keep grounded

		[Header("Looking Parameters")]
		public Vector2 LookSensitivity = new Vector2(0.1f, 0.1f);
		public float PitchLimit = 85f;

		private float currentPitch = 0f;
		public float CurrentPitch
		{
			get => currentPitch;
			set => currentPitch = Mathf.Clamp(value, -PitchLimit, PitchLimit);
		}

		[Header("State")]
		public Vector3 CurrentVelocity { get; private set; }
		public float CurrentSpeed { get; private set; }
		public float VerticalVelocity;        // vertical movement component due to jump/gravity
		public bool SprintInput;              // read from input
		public bool JumpInput;                // read from input

		[Header("Input")]
		public Vector2 MoveInput;              // read from input
		public Vector2 LookInput;

		[Header("Components")]
		public Camera MainCamera;
		public CharacterController CharacterController;

		// Determine actual movement speed depending on sprint input
		private float MaxSpeed => SprintInput ? SprintSpeed : WalkSpeed;

        #region Unity Methods

		private void OnValidate()
		{
			if (CharacterController == null)
				CharacterController = GetComponent<CharacterController>();
		}

		private void Update()
		{
			HandleLook();
			HandleJumpInput();
		}

		private void FixedUpdate()
		{
			HandleMovement();
		}

        #endregion

        #region Controller Methods

		// Called by input system (e.g., Player script) when jump button pressed
		public void TryJump()
		{
			if (!CharacterController.isGrounded)
				return;

			// Calculate initial upward velocity for jump using physics formula
			VerticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Physics.gravity.y * GravityScale);
		}

		// Process jump input each frame
		private void HandleJumpInput()
		{
			if (JumpInput)
			{
				TryJump();
				JumpInput = false;  // consume the jump input
			}
		}

		private void HandleMovement()
		{
			// Build horizontal motion based on MoveInput
			Vector3 motion = transform.forward * MoveInput.y + transform.right * MoveInput.x;
			motion.y = 0f;
			motion.Normalize();

			// Smooth the velocity toward target speed
			if (motion.sqrMagnitude >= 0.01f)
			{
				CurrentVelocity = Vector3.MoveTowards(CurrentVelocity, motion * MaxSpeed, Acceleration * Time.deltaTime);
			}
			else
			{
				CurrentVelocity = Vector3.MoveTowards(CurrentVelocity, Vector3.zero, Acceleration * Time.deltaTime);
			}

			// Handle vertical movement (jump/gravity)
			if (CharacterController.isGrounded && VerticalVelocity < 0f)
			{
				VerticalVelocity = GroundStick;
			}
			else
			{
				VerticalVelocity += Physics.gravity.y * GravityScale * Time.deltaTime;
			}

			// Combine horizontal and vertical and move the controller
			Vector3 fullVelocity = new Vector3(CurrentVelocity.x, VerticalVelocity, CurrentVelocity.z);
			CharacterController.Move(fullVelocity * Time.deltaTime);

			// Update current speed (horizontal magnitude)
			CurrentSpeed = new Vector3(CurrentVelocity.x, 0f, CurrentVelocity.z).magnitude;
		}

		private void HandleLook()
		{
			// Calculate yaw (horizontal) and pitch (vertical) based on LookInput
			float yaw = LookInput.x * LookSensitivity.x;
			float pitch = LookInput.y * LookSensitivity.y;

			CurrentPitch -= pitch;

			// Rotate camera (for pitch) and player body (for yaw)
			MainCamera.transform.localRotation = Quaternion.Euler(CurrentPitch, 0f, 0f);
			transform.Rotate(Vector3.up * yaw);
		}

        #endregion
	}
}
