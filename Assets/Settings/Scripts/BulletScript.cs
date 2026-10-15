using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public int damage = 15;
    public float lifetime = 2f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Called by GunController right after Instantiate
    public void Launch(Vector2 direction, float speed, int damageAmount, float lifeSeconds)
    {
        damage = damageAmount;
        lifetime = lifeSeconds;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.linearVelocity = direction.normalized * speed;   // Unity 6. On older Unity use rb.velocity
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) return;

        Health targetHealth = collision.gameObject.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}