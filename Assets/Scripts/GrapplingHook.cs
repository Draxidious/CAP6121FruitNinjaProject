using UnityEngine;
using UnityEngine.XR;

public class GrapplingHook : MonoBehaviour
{
    public Transform rightRaycastOrigin;
    public float grabDistance = 2f; // Max distance to initiate grab
    public float objectPullSpeed = 10f; // Speed at which the object moves towards hand
    public float objectHoldDistance = 0.5f; // Distance to hold the object from the hand after pulling
    public LayerMask grabbableLayer;
    public Material redMaterial;
    public float reducedLinearDamping = 0f; // Value to set linear damping to during grab

    private GameObject rightGrabbedObject;
    private float rightGrabbedDistance; // Initial grab distance (not used for pulling anymore)
    private Vector3 objectGrabOffset; // Offset between controller and object at grab time
    private float originalLinearDamping; // Store original linear damping value

    private LineRenderer rightRayLine;
    private Material originalMaterial;

    private InputDevice rightController; // InputDevice for right controller

    private bool isPullingObject = false; // Flag to indicate if object is being pulled

    void Start()
    {
        // Get or Add LineRenderer components to raycast origin
        rightRayLine = rightRaycastOrigin.GetComponent<LineRenderer>();
        if (rightRayLine == null)
        {
            rightRayLine = rightRaycastOrigin.gameObject.AddComponent<LineRenderer>();
        }

        // Configure LineRenderer
        ConfigureLineRenderer(rightRayLine);
        DisableLineRenderer(rightRayLine);

        // Get right controller input device
        rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    void ConfigureLineRenderer(LineRenderer lineRenderer)
    {
        lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply")); // Default shader
        lineRenderer.widthMultiplier = 0.02f;
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
    }

    void EnableLineRenderer(LineRenderer lineRenderer)
    {
        lineRenderer.enabled = true;
    }

    void DisableLineRenderer(LineRenderer lineRenderer)
    {
        lineRenderer.enabled = false;
    }

    void Update()
    {
        // Update right controller input every frame
        rightController.TryGetFeatureValue(CommonUsages.triggerButton, out bool isRightTriggerPressed);

        if (isRightTriggerPressed)
        {
            RaycastHit hit;
            if (Physics.Raycast(rightRaycastOrigin.position, rightRaycastOrigin.forward, out hit, grabDistance, grabbableLayer))
            {
                // Raycast hit a grabbable object (layer filtered)
                EnableLineRenderer(rightRayLine);
                rightRayLine.SetPosition(0, rightRaycastOrigin.position);
                rightRayLine.SetPosition(1, hit.point);

                if (rightGrabbedObject == null) // No tag check needed, layer already filters
                {
                    rightGrabbedObject = hit.collider.gameObject;
                    rightGrabbedDistance = Vector3.Distance(rightRaycastOrigin.position, rightGrabbedObject.transform.position); // Store initial distance
                    isPullingObject = true; // Start pulling object
                    objectGrabOffset = rightGrabbedObject.transform.position - rightRaycastOrigin.position; // Calculate initial offset

                    // Store original linear damping and set to reduced value
                    Rigidbody rb = rightGrabbedObject.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        originalLinearDamping = rb.linearDamping;
                        rb.linearDamping = reducedLinearDamping;
                    }

                    // Change material to red temporarily
                    originalMaterial = rightGrabbedObject.GetComponent<Renderer>().material;
                    rightGrabbedObject.GetComponent<Renderer>().material = redMaterial;
                }

                if (rightGrabbedObject != null && isPullingObject) // If grabbing and pulling, move object towards hand
                {
                    Vector3 targetPosition = rightRaycastOrigin.position + rightRaycastOrigin.forward * objectHoldDistance; // Target position in front of hand
                    // Move object towards target position smoothly
                    rightGrabbedObject.GetComponent<Rigidbody>().MovePosition(Vector3.Lerp(rightGrabbedObject.transform.position, targetPosition, objectPullSpeed * Time.deltaTime));

                    // Stop pulling when object is close enough (optional, adjust threshold as needed)
                    if (Vector3.Distance(rightGrabbedObject.transform.position, targetPosition) < 0.1f)
                    {
                        isPullingObject = false; // Stop pulling once close
                    }
                }
                else if (rightGrabbedObject != null && !isPullingObject) // If grabbing and not pulling (object held), move with controller
                {
                    Vector3 targetPosition = rightRaycastOrigin.position + objectGrabOffset; // Maintain initial offset
                    rightGrabbedObject.GetComponent<Rigidbody>().MovePosition(targetPosition); // Move object with controller
                }
            }
            else
            {
                // Raycast did not hit a grabbable object (layer filtered), but trigger is pressed, draw line to max distance
                EnableLineRenderer(rightRayLine);
                rightRayLine.SetPosition(0, rightRaycastOrigin.position);
                rightRayLine.SetPosition(1, rightRaycastOrigin.position + rightRaycastOrigin.forward * grabDistance);

                // If we were targeting an object but no longer are, revert material and stop pulling
                if (rightGrabbedObject != null)
                {
                    RevertMaterial(rightGrabbedObject);
                    ResetLinearDamping(rightGrabbedObject); // Restore linear damping on loss of target
                    rightGrabbedObject = null; // Release object as raycast no longer hitting
                    isPullingObject = false; // Stop pulling if raycast lost
                }
            }
        }
        else if (rightGrabbedObject != null) // Trigger released, release object
        {
            DisableLineRenderer(rightRayLine);
            RevertMaterial(rightGrabbedObject);
            ResetLinearDamping(rightGrabbedObject); // Restore linear damping on release
            rightGrabbedObject = null;
            isPullingObject = false; // Stop pulling on release
        }
        else
        {
            // Trigger is not pressed and no object grabbed, disable ray line
            DisableLineRenderer(rightRayLine);
             if (rightGrabbedObject != null)
            {
                RevertMaterial(rightGrabbedObject);
                ResetLinearDamping(rightGrabbedObject); // Restore linear damping when no longer grabbing
                rightGrabbedObject = null; // Release object as trigger no longer pressed
                isPullingObject = false; // Ensure pulling flag is off
            }
        }
    }

    void RevertMaterial(GameObject obj)
    {
        if (obj != null && originalMaterial != null)
        {
            obj.GetComponent<Renderer>().material = originalMaterial;
            originalMaterial = null; // Clear original material after reverting
        }
    }

    void ResetLinearDamping(GameObject obj)
    {
        if (obj != null)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearDamping = originalLinearDamping; // Restore original linear damping
            }
        }
    }
}