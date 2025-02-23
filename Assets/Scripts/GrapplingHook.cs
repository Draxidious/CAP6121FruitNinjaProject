using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingHook : MonoBehaviour
{
	[Header("Grappling Hook Settings")]
	public float maxDistance = 50f;
	public float pullForce = 300f; // Force applied to the object
	public LayerMask grappleLayer;
	public LineRenderer lineRenderer;

	[Header("Input (New Input System)")]
	public InputActionReference grappleAction;

	[Header("Debug Options")] // Keep the debug options!
	public bool showDebugRay = true;
	public Color debugRayColor = Color.green;

	private bool isGrappling = false;
	private Vector3 grapplePoint;
	private Rigidbody grappledObjectRigidbody; // Store the Rigidbody
	private FixedJoint fixedJoint; // Use a FixedJoint

	private void Start()
	{
		grappleAction.action.Enable();
		grappleAction.action.performed += OnGrappleInput;
		grappleAction.action.canceled += OnGrappleRelease;

		if (lineRenderer == null)
		{
			lineRenderer = gameObject.AddComponent<LineRenderer>();
		}
		lineRenderer.enabled = false;
		lineRenderer.startWidth = 0.05f;
		lineRenderer.endWidth = 0.05f;
		lineRenderer.material.color = Color.white;
	}
	private void OnGrappleInput(InputAction.CallbackContext context)
	{
		if (!isGrappling)
		{
			StartGrapple();
		}
	}
	private void OnGrappleRelease(InputAction.CallbackContext context)
	{
		StopGrapple();
	}

	private void Update()
	{
		if (isGrappling)
		{
			lineRenderer.SetPosition(0, transform.position);
			//No need to update position 2, the joint handles that
		}
	}

	void StartGrapple()
	{
		RaycastHit hit;
		Vector3 startPosition = transform.position;
		Vector3 forwardDirection = transform.forward;


		if (Physics.Raycast(startPosition, forwardDirection, out hit, maxDistance, grappleLayer))
		{
			// Check if the hit object has a Rigidbody
			if (hit.rigidbody != null)
			{
				grappledObjectRigidbody = hit.rigidbody;
				grapplePoint = hit.point; //World Space
				isGrappling = true;

				lineRenderer.enabled = true;
				lineRenderer.SetPosition(0, transform.position);
				lineRenderer.SetPosition(1, grapplePoint);

				// Create the FixedJoint
				fixedJoint = grappledObjectRigidbody.gameObject.AddComponent<FixedJoint>();
				fixedJoint.connectedBody = this.GetComponent<Rigidbody>(); // Connect to *this* controller's Rigidbody
																		   //If your controllers don't have rigidbodies, add a kinematic rigidbody.
				fixedJoint.anchor = grappledObjectRigidbody.transform.InverseTransformPoint(grapplePoint); //Local Space
				fixedJoint.connectedAnchor = this.transform.InverseTransformPoint(transform.position); //Local Space
				fixedJoint.enableCollision = false; // Prevent unwanted collisions
				fixedJoint.breakForce = Mathf.Infinity; // Make the joint strong
				fixedJoint.breakTorque = Mathf.Infinity;



				if (showDebugRay)
				{
					Debug.DrawLine(startPosition, hit.point, debugRayColor, 0.5f);
				}
			}
			else
			{
				// Optionally handle the case where the object doesn't have a Rigidbody
				Debug.LogWarning("Grappled object does not have a Rigidbody!");
				if (showDebugRay) // Still draw the ray even if no Rigidbody
				{
					Debug.DrawLine(startPosition, startPosition + forwardDirection * maxDistance, debugRayColor, 0.5f);
				}

			}
		}
		else
		{
			if (showDebugRay)
			{
				Debug.DrawLine(startPosition, startPosition + forwardDirection * maxDistance, debugRayColor, 0.5f);
			}
		}
	}


	void StopGrapple()
	{
		isGrappling = false;
		lineRenderer.enabled = false;

		if (fixedJoint != null)
		{
			Destroy(fixedJoint); // Clean up the joint
		}
		grappledObjectRigidbody = null; // Clear the reference
	}

	private void OnDestroy()
	{
		grappleAction.action.performed -= OnGrappleInput;
		grappleAction.action.canceled -= OnGrappleRelease;
	}
}