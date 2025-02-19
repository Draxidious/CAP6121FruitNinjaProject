using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class UnicycleMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float maxSpeed;
    public float speed;
    public Transform rider;
    public LeanMovement lean;
    public float angle;
    public float wheelRadius = 1.4f;
    public GameObject wheel;
    public GameObject axel;
    public GameObject frame;
    public GameObject pedal1;
    public GameObject pedal2;
    Transform headTransform;
    Transform playerTransform;
	float xOffset;
    float zOffset;
    float currentSpeedX;
    float currentSpeedZ;
    public float CurrentSpeed;
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       transform.position = new Vector3(rider.position.x, transform.position.y, rider.position.z);
        leanVariables();

		movement();
        positioning();
    }

    public void movement()
    {
        speed = new Vector2(currentSpeedX, lean.currentSpeedZ).magnitude;
        CurrentSpeed = speed;
		frame.transform.rotation = Quaternion.Euler(angle, 0, 0); 
		float angularVelocity = speed/wheelRadius;
        wheel.transform.Rotate(angularVelocity,0, 0);
        pedals(pedal1.transform);
        pedals(pedal2.transform);
    }

    public void pedals(Transform p)
    {
		Quaternion currentRotation = p.rotation;
		currentRotation.eulerAngles = new Vector3(0f, currentRotation.eulerAngles.y, 0f); //For keeping upright around world up.
		p.rotation = currentRotation;

	}

    public void positioning()
    {
        angle = Mathf.Atan2(zOffset, xOffset);
		Quaternion headRotation = headTransform.rotation;
		float headYaw = headRotation.eulerAngles.y;
		Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles.x, headYaw, transform.eulerAngles.z);
		transform.rotation = targetRotation;
        frame.transform.rotation = targetRotation ;
        transform.position = new Vector3(headTransform.position.x, transform.position.y, headTransform.position.z);
        Vector2 dir = new Vector2 (lean.currentSpeedX, lean.currentSpeedZ);
        speed = dir.magnitude;

	}
    public void leanVariables()
    {
		headTransform = lean.headTransform;
		playerTransform = lean.headTransform;
		xOffset = headTransform.localPosition.x - lean.initialX; // Left-right lean
		zOffset = headTransform.localPosition.z - lean.initialZ; // Forward-backward lean
        currentSpeedX = lean.currentSpeedX;
        currentSpeedZ = lean.currentSpeedZ;
	}

}
