using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class JumpWithBButtonNoRB : MonoBehaviour
{
	public float jumpHeight = 2f;      // How high the object will jump.
	public float jumpDuration = 0.5f;   // How long the jump takes.
	public float gravity = -9.81f;       // Gravity value (can adjust for feel).

	private bool isJumping = false;
	private float jumpStartTime;
	public Vector3 initialPosition;
	private Vector3 targetPosition; //Store the target position
	private float verticalVelocity;

	public LayerMask groundLayer;
	public float groundCheckDistance = 0.15f;
	private bool isGrounded;

	private XRNode controllerNode = XRNode.RightHand;
	private List<InputDevice> devices = new List<InputDevice>();
	private InputDevice device;

	void GetDevice()
	{
		InputDevices.GetDevicesAtXRNode(controllerNode, devices);
		if (devices.Count > 0)
		{
			device = devices[0];
		}
	}

	void Start()
	{
		GetDevice();
		initialPosition = transform.position; // Initialize on Start, not in Update
	}

	void Update()
	{
		if (!device.isValid || devices.Count == 0)
		{
			GetDevice();
		}

		//Ground check
		isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
		Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);

		// Input Check
		if (device.isValid && isGrounded && !isJumping)
		{
			if (device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryButtonValue) && secondaryButtonValue)
			{
				StartJump();
			}
		}

		// Jump Logic (if currently jumping)
		if (isJumping)
		{
			float timeSinceJumpStart = Time.time - jumpStartTime;
			float jumpProgress = timeSinceJumpStart / jumpDuration;

			if (jumpProgress <= 1f)
			{
				// Calculate the vertical position using a parabolic motion.
				// y(t) = y0 + v0*t + 0.5*a*t^2
				float yOffset = initialPosition.y + (verticalVelocity * timeSinceJumpStart) + (0.5f * gravity * timeSinceJumpStart * timeSinceJumpStart);

				// Set new position
				transform.position = new Vector3(transform.position.x, yOffset, transform.position.z);
			}
			else
			{
				// End the jump.
				isJumping = false;
				transform.position = targetPosition; // Snap to the final ground position
			}
		}
		else if (!isGrounded)
		{ //Apply gravity at all times when not grounded.
		  // Apply gravity. This happens every frame *after* the jump, so it pulls the object back down.
		  //y(t) = y_0 + v*t + 0.5*a*t^2
			float time = Time.deltaTime;
			verticalVelocity += gravity * time;
			float yOffset = (verticalVelocity * time) + (0.5f * gravity * time * time); //use current verticalVelocity
			transform.position += new Vector3(0, yOffset, 0); //apply the change.


		}
	}

	void StartJump()
	{
		isJumping = true;
		jumpStartTime = Time.time;
		initialPosition = transform.position; // Set initialPosition here

		// Calculate the initial upward velocity needed to reach the jumpHeight.
		// v^2 = u^2 + 2as   (where v = final velocity (0 at peak), u = initial velocity, a = acceleration (gravity), s = displacement (jumpHeight))
		// 0 = u^2 + 2*gravity*jumpHeight
		// u = sqrt(-2 * gravity * jumpHeight)

		verticalVelocity = Mathf.Sqrt(-2f * gravity * jumpHeight);

		// Set target position
		targetPosition = transform.position;  // The target *horizontal* position is the same.
		targetPosition.y = initialPosition.y;  // The target *vertical* position should be back on the ground, *not* at the peak height.  Important.
	}
	// Optional:  OnDrawGizmos for visual debugging of the ground check
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = isGrounded ? Color.green : Color.red;
		Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
	}
}