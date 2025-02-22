using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
    [SerializeField] private float speedThreshold = 5.0f; // Minimum speed to deal damage
    [SerializeField] private float damage = 20f; // Damage dealt to enemies
    [SerializeField] private bool destroyOnImpact = true; // Destroy the projectile on hit

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("ProjectileDamage requires a Rigidbody component.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (rb.linearVelocity.magnitude > speedThreshold) // Check if projectile is moving fast enough
        {
            if (other.CompareTag("Enemy"))
            {
                EnemyAI enemyAI = other.GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    enemyAI.TakeProjectileDamage(damage);
                }
            }

            if (destroyOnImpact)
            {
                Destroy(gameObject); // Destroy the projectile after impact
            }
        }
    }
}
