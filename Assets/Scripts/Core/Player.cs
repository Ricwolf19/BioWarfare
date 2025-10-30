using UnityEngine;
using UnityEngine.InputSystem;

namespace BioWarfare.Core
{
	[RequireComponent(typeof(FPController))]
	public class Player : MonoBehaviour
	{
		[Header("Components")]
		public FPController FPController;

		private void OnValidate()
		{
			if (FPController == null)
				FPController = GetComponent<FPController>();
		}

		private void Start()
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}

		// Called by PlayerInput when Move action is triggered
		public void OnMove(InputValue value)
		{
			Vector2 moveVec = value.Get<Vector2>();
			FPController.MoveInput = moveVec;
		}

		// Called by PlayerInput when Look action is triggered
		public void OnLook(InputValue value)
		{
			Vector2 lookVec = value.Get<Vector2>();
			FPController.LookInput = lookVec;
		}

		// Called by PlayerInput when Sprint action is triggered
		public void OnSprint(InputValue value)
		{
			bool sprint = value.isPressed;
			FPController.SprintInput = sprint;
		}

		// Called by PlayerInput when Jump action is triggered
		public void OnJump(InputValue value)
		{
			if (value.isPressed)
			{
				FPController.TryJump();
			}
		}
	}
}
