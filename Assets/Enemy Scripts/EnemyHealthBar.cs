using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EnemyAI))]
public class EnemyHealthBar : MonoBehaviour
{
    public Vector3 healthBarOffset = new Vector3(0, 2f, 0); // Offset above the enemy
    private EnemyAI enemyAI;
    private Image healthBarImage;
    private RectTransform healthBarTransform;
    private float maxHealth;
    private float initialHealthBarWidth;

    void Start()
    {
        enemyAI = GetComponent<EnemyAI>();
        maxHealth = enemyAI.health;
        Transform canvasTransform = transform.Find("HealthBarCanvas");
        if (canvasTransform != null)
        {
            Transform bgTransform = canvasTransform.Find("HealthBarBG");
            if (bgTransform != null)
            {
                Transform healthBar = bgTransform.Find("HealthBar");
                if (healthBar != null)
                {
                    healthBarImage = healthBar.GetComponent<Image>();
                    healthBarTransform = healthBar.GetComponent<RectTransform>();
                    initialHealthBarWidth = healthBarTransform.sizeDelta.x; // Store initial width
                }
                else
                {
                    Debug.LogWarning("HealthBar not found.");
                }
            }
            else
            {
                Debug.LogWarning("HealthBarBG not found.");
            }
        }
        else
        {
            Debug.LogWarning("HealthBarCanvas not found.");
        }
    }

    void Update()
    {
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (enemyAI.health <= 0)
        {
            Destroy(gameObject); // Destroy enemy when health reaches 0
            return;
        }

        if (healthBarTransform != null)
        {
            float healthPercentage = Mathf.Clamp01((float)enemyAI.health / maxHealth);
            healthBarTransform.sizeDelta = new Vector2(healthPercentage * initialHealthBarWidth, healthBarTransform.sizeDelta.y);
        }
    }
}
