using UnityEngine;

public class FloatingCollectible : MonoBehaviour
{
    public float floatSpeed = 1f; // Speed of floating up and down
    public float floatHeight = 0.5f; // Height of the floating motion
    public float rotationSpeed = 50f; // Speed of rotation

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Floating effect
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Rotating effect
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
