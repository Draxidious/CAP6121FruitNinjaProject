using UnityEngine;
using UnityEngine.InputSystem; // If you're using the new Input System
using UnityEngine.XR;

[RequireComponent(typeof(InputData))]
public class LeanMovement : MonoBehaviour
{
	public Transform headTransform; // Assign the VR headset transform
	public CharacterController characterController;
	public float maxSpeed = 3.0f;
	public float acceleration = 2.0f;
	public float leanThreshold = 0.1f;

	public float gravity = 9.81f;
	public LayerMask groundLayer; // Assign ground layer in Inspector (for isGrounded)

	public float initialX;
	public float initialZ;
	public float currentSpeedX = 0.0f;
	public float currentSpeedZ = 0.0f;
	private bool movementEnabled = false;
	private bool buttonPressed = false;
	private bool Bpressed = false;
	private bool Xpressed = false;
	float verticalVelocity = 0f;

	float groundHeight;


	private InputData inputData;

	float initialYPosition;  // Corrected: Store *CharacterController's* Y position
	bool isJumping = false;
	public float jumpHeight = 1.0f; // Set your desired jump height
	public float jumpTime = 0.8f; // Total jump time (up and down)
	private float elapsedTime = 0f;
	private float initialJumpVelocity;

	private bool isFlying = false;
	public float flyHeight = 3.0f; // Set your desired jump height
	public float timetoFlyHeight = 1.0f;
	public float flyTime = 0.8f; // Total jump time (up and down)
	private float elapsedFlyTime = 0f;
	private float initialFlyVelocity;
	private bool isFlyingUp = false;
	private bool isFlyingAtHeight = false;


	bool isDucked = false;
	float duckTime = 3f;
	float elapsedDuckTime = 0f;
	bool Ypressed = false;
	float duckHeight = 0.2f;
	public GameObject unicycle;


	float initialHeight;

	void Start()
	{
		groundHeight = headTransform.position.y;
		inputData = GetComponent<InputData>();
		if (headTransform == null)
			headTransform = Camera.main.transform;
		initialYPosition = headTransform.position.y;

		initialZ = headTransform.localPosition.z;
		initialHeight = characterController.height;

	}

	void Update()
	{
		// --- Input and Movement Enabling (Your existing code) ---
		if (IsAButtonPressed() && !isFlying && !isJumping && !isDucked)
		{
			if (!buttonPressed)
			{
				buttonPressed = true;
				movementEnabled = !movementEnabled;
				if (movementEnabled)
				{
					initialZ = headTransform.localPosition.z;
					initialX = headTransform.localPosition.x;
				}
			}
		}
		else
		{
			buttonPressed = false;
		}

		if (!movementEnabled)
		{
			currentSpeedX = 0.0f;
			currentSpeedZ = 0.0f;
			return;
		}

		// --- Jump Input and State ---
		if (!isJumping && !isFlying) // VERY IMPORTANT: Only allow jump if grounded
		{
			//Reset values.
			elapsedTime = 0;
			if (IsBButtonPressed())
			{
				if (!Bpressed)
				{
					Debug.LogWarning("jump started");
					Bpressed = true;
					isJumping = true;
					//initialYPosition = headTransform.position.y;// Store CharacterController's Y
					initialJumpVelocity = CalculateInitialJumpVelocity(jumpHeight, jumpTime / 2f); // Calculate *once*
					elapsedTime = 0f; // Reset the jump timer
				}
			}
			else
			{
				Bpressed = false;
			}
		}
		if (!isFlying && !isJumping) // IMPORTANT: Only allow Flying if grounded. Reset bools
		{
			elapsedFlyTime = 0;
			isFlyingUp = false;
			isFlyingAtHeight = false;

			if (IsXButtonPressed())
			{
				if (!Xpressed)
				{
					Debug.LogWarning("fly started");
					Xpressed = true;
					isFlying = true;
				
					initialFlyVelocity = CalculateInitialJumpVelocity(flyHeight, timetoFlyHeight); // Use jump velocity calc
					elapsedFlyTime = 0f; // Reset the fly timer
					isFlyingUp = true; // Start in the "flying up" phase
					isFlyingAtHeight = false;
				}
			}
			else
			{
				Xpressed = false;
			}
		}
		if (!isDucked && !isFlying && !isJumping)
		{
			elapsedDuckTime = 0;
			isDucked = false;


			if (IsYButtonPressed())
			{
				if (!Ypressed)
				{
					Debug.LogWarning("duck started");
					Ypressed = true;
					isDucked = true;
					//initialYPosition = headTransform.position.y;
					headTransform.position = new Vector3(headTransform.position.x, 0, headTransform.position.z);
					characterController.height = duckHeight;

				}
			}
			else
			{
				Ypressed = false;
			}

		}
		// --- Lean Movement Calculation (Your existing code) ---
		float xOffset = headTransform.localPosition.x - initialX;
		float zOffset = headTransform.localPosition.z - initialZ;
		float targetSpeedX = 0.0f;
		float targetSpeedZ = 0.0f;

		if (xOffset > leanThreshold) targetSpeedX = maxSpeed;
		else if (xOffset < -leanThreshold) targetSpeedX = -maxSpeed;

		if (zOffset > leanThreshold) targetSpeedZ = maxSpeed;
		else if (zOffset < -leanThreshold) targetSpeedZ = -maxSpeed;

		currentSpeedX = Mathf.Lerp(currentSpeedX, targetSpeedX, acceleration * Time.deltaTime);
		currentSpeedZ = Mathf.Lerp(currentSpeedZ, targetSpeedZ, acceleration * Time.deltaTime);

		// --- Jump Calculation and Movement ---


		if (isJumping)
		{
			verticalVelocity = CalculateJumpYVelocity(initialJumpVelocity, jumpTime, ref elapsedTime);

		}
		// --- Flying Calculation and Movement ---
		else if (isFlying)
		{
			if (isFlyingUp)
			{
				verticalVelocity = CalculateJumpYVelocity(initialFlyVelocity, timetoFlyHeight, ref elapsedFlyTime);
				if (elapsedFlyTime >= timetoFlyHeight)  // Check for reaching fly height using >= for precision
				{
					isFlyingUp = false;
					isFlyingAtHeight = true;
					elapsedFlyTime = 0f; // Reset timer for the "at height" phase
					verticalVelocity = 0; // Stop upward movement
				}
			}
			else if (isFlyingAtHeight)
			{

				elapsedFlyTime += Time.deltaTime; // Keep track of time at height
				if (elapsedFlyTime >= flyTime)
				{
					isFlyingAtHeight = false;
					isFlying = false; // End flying
					verticalVelocity = 0; //Ensure we don't keep flying
				}
			}

		}
		if(!isJumping && !isFlying && headTransform.position.y > initialYPosition)
		{ //Handles falling
		  //Apply default gravity when not on the ground and not jumping
			verticalVelocity = -9.8f;
			float heightDif = Mathf.Abs(headTransform.position.y - groundHeight);
			//characterController.transform.position = new Vector3(characterController.transform.position.x, characterController.transform.position.y - heightDif, characterController.transform.position.z);
		}
		//else
		//{
		//	verticalVelocity = 0f;
		//}

		if(!isJumping &&  !isFlying && isDucked)
		{
			elapsedDuckTime += Time.deltaTime;
			if(elapsedDuckTime >= duckTime)
			{
				headTransform.position = new Vector3(headTransform.position.x, initialHeight, headTransform.position.z);
				unicycle.SetActive(true);
				characterController.height = initialHeight;
				isDucked = false;
			}
		} 

		Vector3 moveDirection = (characterController.transform.right * currentSpeedX) +
								(characterController.transform.forward * currentSpeedZ) + (Vector3.up * verticalVelocity);
		characterController.Move(moveDirection * Time.deltaTime);  // Apply horizontal movement
	}

	// --- Input Functions (Your existing code) ---
	private bool IsAButtonPressed()
	{
		bool isPressed = false;
		if (inputData._rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool value) && value)
		{
			isPressed = true;
		}

		return isPressed;
	}

	private bool IsBButtonPressed()
	{
		bool isPressed = false;
		if (inputData._rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out bool value) && value)
		{
			isPressed = true;
		}

		return isPressed;
	}

	private bool IsYButtonPressed()
	{
		bool isPressed = false;
		if (inputData._leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out bool value) && value)
		{
			isPressed = true;
		}

		return isPressed;
	}

	private bool IsXButtonPressed()
	{
		bool isPressed = false;
		if (inputData._leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool value) && value)
		{
			isPressed = true;
		}

		return isPressed;
	}

	// --- Jump Calculation Functions (From previous examples) ---
	public float CalculateInitialJumpVelocity(float jumpHeight, float timeToJumpApex)
	{
		if (timeToJumpApex <= 0)
		{
			Debug.LogWarning("TimeToJumpApex must be greater than zero.");
			return 0f;
		}

		float initialVelocity = (2f * jumpHeight) / timeToJumpApex;
		return initialVelocity;
	}


	public float CalculateJumpYVelocity(float initialYVelocity, float initialYPosition, ref float elapsedTime)
	{
		if (characterController == null)
		{
			Debug.LogError("CharacterController is null.  Ensure Start() has run.");
			return 0f;
		}


		if (!isFlying && elapsedTime > jumpTime)
		{
			// Jump is complete.
			isJumping = false;
			return 0f;

		}

		//Basic kinematic equation v_f = v_i + a*t.
		float currentYVelocity = initialYVelocity + Physics.gravity.y * elapsedTime;

		elapsedTime += Time.deltaTime;

		return currentYVelocity;
	}

	/// <summary>
	/// Calculates and updates the character's Y velocity over time during a jump.
	/// </summary>
	/// <param name="jumpHeight">The height of the jump</param>
	/// <param name="jumpTime">The *total* duration of the jump.</param>
	/// <param name="elapsedTime">The time elapsed since the jump started.</param>
	///  <param name="initialYPosition"> starting y position</param>
	/// <returns>The current Y position.</returns>
	public float CalculateJumpYPosition(float jumpHeight, float jumpTime, ref float elapsedTime)
	{
		if (characterController == null)
		{
			Debug.LogError("CharacterController is null.  Ensure Start() has run.");
			return 0f;
		}

		if (elapsedTime > jumpTime)
		{
			isJumping = false;
			elapsedTime = jumpTime;  // Cap at jumpTime.
		}


		float initialVelocity = 0;

		// Only calculate the initial velocity if we are starting the jump (elapsed time is 0 or a very small value)
		if (elapsedTime < 0.001f)  // Use a small tolerance to account for floating-point imprecision
		{
			initialVelocity = CalculateInitialJumpVelocity(jumpHeight, jumpTime / 2); // timeToJumpApex is half jumpTime
		}

		// Kinematic equation:  d = v_i*t + 0.5*a*t^2

		float currentYPosition = initialYPosition + (initialVelocity * elapsedTime) + (0.5f * Physics.gravity.y * elapsedTime * elapsedTime);

		//Clamp above ground level, accounting for charactercontroller center and height
		float groundLevel = initialYPosition - characterController.height / 2f - characterController.center.y;

		elapsedTime += Time.deltaTime;

		return Mathf.Max(currentYPosition, groundLevel);  // Prevent going below ground.
	}
}