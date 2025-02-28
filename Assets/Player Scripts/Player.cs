using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour
{
    [Header("Player Settings")]
    public float health = 100f;
    public float maxHealth = 100f;
    public GameObject playerStart; // Reference to the PlayerStart object
    public float damageCooldown = 1f; // Cooldown time between taking damage in seconds
    public int sweetTreats = 0; // Number of sweet treats collected
    public int sweetMax = 10; // Number of sweet treats collected

    [Header("UI Settings")]
    public RectTransform healthBarTransform;
    public TMP_Text sweetTreatsCount;

    private bool canTakeDamage = true;
    private float initialHealthBarWidth;
    


    void Start()
    {
        initialHealthBarWidth = healthBarTransform.sizeDelta.x; // Store initial width
    }

    void Update()
    {
        sweetTreatsCount.text = sweetTreats.ToString() + " / " + sweetMax;
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

    public void HealPlayer(float healAmount)
    {
        health += healAmount;
        health = Mathf.Clamp(health, 0, maxHealth);
        if (healthBarTransform != null)
        {
            float healthPercentage = Mathf.Clamp01((float)health / maxHealth);
            healthBarTransform.sizeDelta = new Vector2(healthPercentage * initialHealthBarWidth, healthBarTransform.sizeDelta.y);
        }
    }

    private void ResetDamageCooldown()
    {
        canTakeDamage = true;
    }
}
