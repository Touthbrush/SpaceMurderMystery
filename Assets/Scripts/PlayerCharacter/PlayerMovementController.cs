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
	//[SerializeField]
	//bool canMove = true;
	[SerializeField]
	bool enableSprint = true;
	[SerializeField]
	bool enableCrouch = true;

	[Header("Jump Settings")]
	[SerializeField]
	float jumpHeight;
	[SerializeField]
	float jumpTime;

	[Header("Movement Speed Settings")]
	[SerializeField]
	float baseMovementSpeed = 4;
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
	public float verticalSpeed;
	public float jumpForce;

	//Mouse Look
	[Header("Camera Values")]
	//public Quaternion charTargetRot;
	//public Quaternion camTargetRot;
	public Quaternion origPlayerRot;
	public Quaternion origCameraRot;

	//Cursor Lock
	private bool cursorLocked;
	#endregion Functionality Variables

	#region Player Initialisation
	void Awake()
	{
		controller = GetComponent<CharacterController>();
		camRotator = GetComponent<PlayerCameraRotator>();

		origPlayerRot = transform.localRotation;
		origCameraRot = (Quaternion)playerCam?.transform.localRotation;

		jumpForce = Mathf.Sqrt(2 * baseGravity * jumpHeight);
		
		if (!playerCam) { Debug.Log("Player camera is missing."); }
	}

	void Start()
	{
		//InitCameraTransforms();
	}
	#endregion Player Initialisation

	void Update ()
	{
		UpdatePlayerMovement();
		UpdateCameraRotation();
	}

	#region Player Movement
	/// <summary>
	/// Handles movement of the player, including movement input and gravity. Also handles ground sticking for slopes and angled surfaces.
	/// </summary>
	void UpdatePlayerMovement()
	{
		Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
		float xMoveSpeed = (moveInput.x * baseMovementSpeed);
		float zMoveSpeed = (moveInput.y * baseMovementSpeed);
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

	#endregion Helper Methods
}
