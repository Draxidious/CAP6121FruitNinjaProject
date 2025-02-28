using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    [SerializeField] private float swingThreshold = 2.0f; // Minimum speed to count as a swing
    [SerializeField] private float damage = 20f; // Damage dealt to enemies

    private Vector3 previousPosition;
    private float currentSpeed;

    void Start()
    {
        previousPosition = transform.position;
    }

    void Update()
    {
        // Calculate sword movement speed
        currentSpeed = (transform.position - previousPosition).magnitude / Time.deltaTime;
        previousPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentSpeed > swingThreshold)
        {
            if (other.CompareTag("Enemy"))
            {
                EnemyAI enemyAI = other.GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    enemyAI.TakeDamage(damage);
                }
                else
                {
                    Debug.LogWarning("EnemyAI component not found on enemy object.");
                }
            }
        }
    }
}
