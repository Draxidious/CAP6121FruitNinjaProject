using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 5f;
    public float damage = 12f;

    private Transform player;
    private Vector3 direction;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("MainCamera")?.transform;
        if (player != null)
        {
            direction = (player.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0); // Adjusting for capsule orientation
        }
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
        
    }
}
