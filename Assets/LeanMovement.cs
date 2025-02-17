using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

[RequireComponent(typeof(InputData))]
public class LeanMovement : MonoBehaviour
{
    public Transform headTransform; // Assign the VR headset transform (e.g., Camera.main.transform)
    public Transform playerTransform; // Assign the player's root transform (e.g., XR Rig)
    public float maxSpeed = 3.0f; // Maximum movement speed
    public float acceleration = 2.0f; // How quickly the player reaches max speed
    public float leanThreshold = 0.1f; // How much the player needs to lean to start moving

    private float initialX; // Initial side-to-side position
    private float initialZ; // Initial forward-backward position
    private float currentSpeedX = 0.0f; // Current speed for left-right
    private float currentSpeedZ = 0.0f; // Current speed for forward-backward
    private bool movementEnabled = false; // Toggle for lean movement
    private bool buttonPressed = false;

    private InputData inputData;

    void Start()
    {
        inputData = GetComponent<InputData>();
        if (headTransform == null)
            headTransform = Camera.main.transform; // Default to the main camera if not assigned

        if (playerTransform == null)
            playerTransform = transform; // Default to this GameObject

        initialZ = headTransform.localPosition.z; // Store initial Z position
    }

    void Update()
    {
        // Check if the player presses the "A" button
        if (IsAButtonPressed())
        {
            if (!buttonPressed)
            {
                buttonPressed = true;
                movementEnabled = !movementEnabled; // Toggle movement state

                if (movementEnabled)
                    initialZ = headTransform.localPosition.z; // Reset initial position when enabling movement
            }

        }
        else
        {
            buttonPressed = false;
        }

        if (!movementEnabled)
        {
            currentSpeedX = 0.0f; // Stop movement when disabled
            currentSpeedZ = 0.0f; // Stop movement when disabled
            return;
        }

        float xOffset = headTransform.localPosition.x - initialX; // Left-right lean
        float zOffset = headTransform.localPosition.z - initialZ; // Forward-backward lean
        float targetSpeedX = 0.0f; // Default no left-right movement
        float targetSpeedZ = 0.0f; // Default no forward-backward movement

        // Left-Right Movement
        if (xOffset > leanThreshold) // Leaning right
            targetSpeedX = maxSpeed;
        else if (xOffset < -leanThreshold) // Leaning left
            targetSpeedX = -maxSpeed;

        // Forward-Backward Movement
        if (zOffset > leanThreshold) // Leaning forward
            targetSpeedZ = maxSpeed;
        else if (zOffset < -leanThreshold) // Leaning backward
            targetSpeedZ = -maxSpeed;

        // Smooth acceleration using Mathf.Lerp
        currentSpeedX = Mathf.Lerp(currentSpeedX, targetSpeedX, acceleration * Time.deltaTime);
        currentSpeedZ = Mathf.Lerp(currentSpeedZ, targetSpeedZ, acceleration * Time.deltaTime);

        // Apply movement separately for left/right and forward/backward
        playerTransform.position += playerTransform.right * currentSpeedX * Time.deltaTime;
        playerTransform.position += playerTransform.forward * currentSpeedZ * Time.deltaTime;
    }

    // Function to check if the "A" button is pressed
    private bool IsAButtonPressed()
    {
        bool isPressed = false;
        if (inputData._rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool value) && value)
        {
            isPressed = true;
        }

        return isPressed;
    }
}
