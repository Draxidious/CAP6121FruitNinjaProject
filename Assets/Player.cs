using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Player Settings")]
    public float health = 100f;
    private float maxHealth;
    public GameObject playerStart; // Reference to the PlayerStart object
    public float damageCooldown = 1f; // Cooldown time between taking damage in seconds

    [Header("Health Bar Settings")]
    public RectTransform healthBarTransform;

    private bool canTakeDamage = true;
    private float initialHealthBarWidth;

    
    

    void Start()
    {
        maxHealth = health;
        initialHealthBarWidth = healthBarTransform.sizeDelta.x; // Store initial width
    }

    void Update()
    {
      
    }

    // Function to teleport the player to the PlayerStart position
    void TeleportToStart()
    {
        if (playerStart != null)
        {
            // Teleport the player to the position of the PlayerStart object
            transform.position = playerStart.transform.position;
            // Optionally, reset health or handle death logic here
            health = 100f; // Resetting health to full
            healthBarTransform.sizeDelta = new Vector2(initialHealthBarWidth, healthBarTransform.sizeDelta.y);
        }
        else
        {
            Debug.LogWarning("PlayerStart object is not assigned.");
        }
    }

    public bool TakeDamage(float damage)
    {
        if (canTakeDamage)
        {
            canTakeDamage = false;
            Invoke(nameof(ResetDamageCooldown), damageCooldown);
            health -= damage;
            if (healthBarTransform != null)
            {
                float healthPercentage = Mathf.Clamp01((float)health / maxHealth);
                healthBarTransform.sizeDelta = new Vector2(healthPercentage * initialHealthBarWidth, healthBarTransform.sizeDelta.y);
            }
            if (health <= 0)
            {
                TeleportToStart();
            }
            return true;
        }
        return false;
    }

    private void ResetDamageCooldown()
    {
        canTakeDamage = true;
    }
}
