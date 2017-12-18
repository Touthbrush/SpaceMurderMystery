using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(PlayerCameraRotator))]
public class PlayerMovementController : MonoBehaviour
{
	#region GameObject References
	CharacterController controller;
	PlayerCameraRotator camRotator;
	[SerializeField]
	Camera playerCam;
	#endregion GameObject References

	#region BasePlayerSettings
	[Header("Movement Settings")]
	[SerializeField]
	bool enableSprint = true;
	[SerializeField]
	bool enableCrouch = true;

	[Header("Jump Settings")]
	[SerializeField]
	float jumpHeight;

	[Header("Crouch Settings")]
	[SerializeField]
	float crouchedCameraHeight;
	float standingCameraHeight;
	[SerializeField]
	float crouchedColliderHeight;
	float standingColliderHeight;

	[Header("Movement Speed Settings")]
	[SerializeField]
	float baseMovementSpeed = 4;
	[SerializeField]
	[Range(0, 1)]
	float backSpeedMultiplier = 0.5f;
	[SerializeField]
	float sprintMovementSpeed = 6;
	[SerializeField]
	float crouchMovementSpeed = 3;

	[Header("Gravity Settings")]
	[SerializeField]
	float baseGravity = 9.5f;
	float baseMaxGravity = 9.5f;
	#endregion BasePlayerSettings

	#region GameplaySettings
	[Header("Control Locks")]
	public bool canMove = true;
	public bool canCamera = true;
	public bool canSprint = true;
	public bool canCrouch = true;

	[Header("Status Checks")]
	public bool isSprinting = false;
	public bool isCrouching = false;
	#endregion GameplaySettings

	#region Functionality Variables
	//Player Movement
	[Header("Movement Values")]
	public Vector2 currentVelocity; //implement later
	public Vector2 targetVelocity; //implement later
	float verticalSpeed;
	float jumpForce;

	Quaternion origPlayerRot;
	Quaternion origCameraRot;

	//Cursor Lock
	bool cursorLocked;
	#endregion Functionality Variables

	#region Player Initialisation
	void Awake()
	{
		controller = GetComponent<CharacterController>();
		camRotator = GetComponent<PlayerCameraRotator>();

		if (!playerCam) { Debug.Log("Player camera is missing."); }

		origPlayerRot = transform.localRotation;
		origCameraRot = playerCam.transform.localRotation;
		standingCameraHeight = playerCam.transform.localPosition.y;
		standingColliderHeight = controller.height;

		jumpForce = Mathf.Sqrt(2 * baseGravity * jumpHeight);
	}
	#endregion Player Initialisation

	void Update ()
	{
		//Check for status changes
		isSprinting = UpdatePlayerSprint();
		isCrouching = UpdatePlayerCrouch();

		//Apply movement and camera updates
		UpdatePlayerMovement();
		UpdateCameraRotation();
	}

	#region Player Movement
	/// <summary>
	/// Handles movement of the player, including movement input and gravity. Also handles ground sticking for slopes and angled surfaces.
	/// </summary>
	void UpdatePlayerMovement()
	{
		//Get Player Input
		Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
		isSprinting = canSprint && enableSprint ? Input.GetKey(KeyCode.LeftShift) : false;

		float moveSpeed = GetMovementSpeed(moveInput);

		float xMoveSpeed = (moveInput.x * moveSpeed);
		float zMoveSpeed = (moveInput.y * moveSpeed);
		Vector3 velocity = transform.forward * zMoveSpeed + transform.right * xMoveSpeed;

		//Get vertical velocity
		if (controller.isGrounded)
		{
			verticalSpeed = -baseGravity * Time.deltaTime;

			if (Input.GetKeyDown("space"))
			{
				verticalSpeed = jumpForce;
				AirborneMove(new Vector3(velocity.x, verticalSpeed, velocity.z));
			}
			else
			{
				GroundMove(new Vector3(velocity.x, verticalSpeed, velocity.z));
			}

			
		}
		else
		{
			verticalSpeed -= baseGravity * Time.deltaTime;
			verticalSpeed = Mathf.Max(verticalSpeed, -baseMaxGravity);

			AirborneMove(new Vector3(velocity.x, verticalSpeed, velocity.z));
		}
	}

	void AirborneMove(Vector3 velocity)
	{
		controller.Move(velocity * Time.deltaTime);
	}

	void GroundMove(Vector3 velocity)
	{
		Vector3 displacement = Vector3.zero;
		displacement.x = velocity.x * Time.deltaTime;
		displacement.z = velocity.z * Time.deltaTime;
		displacement.y = velocity.y - Mathf.Abs((displacement.magnitude * Mathf.Tan(controller.slopeLimit * Mathf.Deg2Rad)) * Time.deltaTime);

		controller.Move(displacement);
	}
	#endregion Player Movement

	#region Camera Rotation
	void UpdateCameraRotation()
	{
		Quaternion playerRot;
		Quaternion cameraRot;

		//Get movement speed of mouse
		Vector2 mouseInput = Vector2.zero;
		mouseInput.x = Input.GetAxisRaw("Mouse X");
		mouseInput.y = Input.GetAxisRaw("Mouse Y");

		camRotator.GetCameraRotations(mouseInput, out playerRot, out cameraRot);

		transform.localRotation = origPlayerRot * playerRot;
		playerCam.transform.localRotation = origCameraRot * cameraRot;
	}
	#endregion Camera Rotation

	#region Helper Methods
	bool UpdatePlayerSprint()
	{
		if (!canSprint || !enableSprint) { return false; }

		return Input.GetKey(KeyCode.LeftShift);
	}

	bool UpdatePlayerCrouch()
	{
		if (!canCrouch || !enableCrouch) { return false; }

		if (!isCrouching && Input.GetKey(KeyCode.LeftControl))
		{
			//Switch to crouch stance
			controller.height = crouchedColliderHeight;
			controller.center = new Vector3(0, crouchedColliderHeight / 2, 0);
			playerCam.transform.localPosition = new Vector3(0, crouchedCameraHeight, 0);
		}
		else if (isCrouching && !Input.GetKey(KeyCode.LeftControl))
		{
			//Switch to stand stance
			controller.height = standingColliderHeight;
			controller.center = new Vector3(0, standingColliderHeight / 2, 0);
			playerCam.transform.localPosition = new Vector3(0, standingCameraHeight, 0);
		}

		return Input.GetKey(KeyCode.LeftControl);
	}

	bool CheckStandingSpace()
	{
		return true;
	}

	float GetMovementSpeed(Vector2 input)
	{
		if (isCrouching) //Crouched Movement
		{
			return crouchMovementSpeed;
		}
		else if (input.y > float.Epsilon && isSprinting) //Sprint Forward Movement
		{
			return sprintMovementSpeed;
		}
		else if (Mathf.Sign(input.y) >= 0) //Forward/Strafe Movement
		{
			return baseMovementSpeed;
		}
		else //Backward Movement
		{
			return baseMovementSpeed * backSpeedMultiplier;
		}
	}
	#endregion Helper Methods
}
