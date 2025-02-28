using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float speed = 3f;
    public float health = 100f;
    public float damage = 10f;
    public float detectionRadius = 10f;
    public float shootRadius = 15f; // Larger than detectionRadius
    public GameObject projectilePrefab;
    public float shootInterval = 2f;
    public float jumpForce = 5f;
    public float jumpInterval = 2f; // Configurable jump interval
    public bool moveAwayFromPlayer = false;
    public bool canShoot = false;
    public bool canJump = false;
    public bool canFloat = false; // Toggle for floating or grounded movement
    public float damageCooldown = 1f; // Cooldown time before taking damage again
    public float pushBackForce = 5f; // Force applied when taking damage

    private Transform player;
    private Rigidbody rb;
    private float nextShootTime;
    private float nextJumpTime;
    private bool isTouchingPlayer = false;
    private bool canTakeDamage = true;
    public float moldTime; 

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = this.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (player == null) return;
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRadius && !isTouchingPlayer)
        {
            Vector3 direction = (moveAwayFromPlayer ? transform.position - player.position : player.position - transform.position).normalized;

            if (canFloat)
            {
                transform.position += direction * speed * Time.deltaTime;
            }
            else
            {
                direction.y = 0; // Keep movement grounded
                rb.MovePosition(transform.position + direction * speed * Time.deltaTime);
            }
        }
        else if (canFloat)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (canShoot && distance <= shootRadius && Time.time >= nextShootTime)
        {
            Shoot();
            nextShootTime = Time.time + shootInterval;
        }

        if (canJump && Time.time >= nextJumpTime)
        {
            Jump();
            nextJumpTime = Time.time + jumpInterval;
        }

        KeepUpright();
    }

    void Shoot()
    {
        if (projectilePrefab)
        {
            Instantiate(projectilePrefab, transform.position + transform.forward, Quaternion.identity);
        }
    }

    void Jump()
    {
        if (rb)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void KeepUpright()
    {
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }

    public void TakeDamage(float amount)
    {
        if (canTakeDamage)
        {
            health -= amount;
            canTakeDamage = false;
            Invoke(nameof(ResetDamageCooldown), damageCooldown);

            // Push back enemy away from the player
            if (player != null && rb != null)
            {
                Vector3 pushDirection = (transform.position - player.position).normalized;
                rb.AddForce(pushDirection * pushBackForce, ForceMode.Impulse);
            }
        }
    }

    public void TakeProjectileDamage(float amount)
    {
        health -= amount;

        // Push back enemy away from the player
        if (player != null && rb != null)
        {
            Vector3 pushDirection = (transform.position - player.position).normalized;
            rb.AddForce(pushDirection * pushBackForce, ForceMode.Impulse);
        }
    }

    private void ResetDamageCooldown()
    {
        canTakeDamage = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isTouchingPlayer = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isTouchingPlayer = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, shootRadius);
    }
}
