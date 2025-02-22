using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;


public class TakePlayerDamage : MonoBehaviour
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
}
