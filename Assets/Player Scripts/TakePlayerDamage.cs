using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;


public class OnPlayerCollision : MonoBehaviour
{
    public Player player;
    public float duration = 0.5f;

    [SerializeField] public DamageVignette vignette;
    
   

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("collision tag: " + other.gameObject.tag);
        if (other.gameObject.CompareTag("Enemy"))
        {
            if (player.TakeDamage(other.gameObject.GetComponent<EnemyAI>().damage))
            {
                vignette.TriggerVignetteEffect(duration);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("collision tag: " + other.gameObject.tag);
        if (other.gameObject.CompareTag("Projectile"))
        {
            if (player.TakeDamage(other.gameObject.GetComponent<EnemyProjectile>().damage))
            {
                vignette.TriggerVignetteEffect(duration);
            }
        }

        if (other.gameObject.CompareTag("Collectible"))
        {
            player.sweetTreats++;
            player.HealPlayer(player.maxHealth/2);
            Destroy(other.gameObject);
        }
    }
}
