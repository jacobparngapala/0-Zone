using UnityEngine;

// Keep this class name: the scene references it by this script file.
// Damages the target repeatedly while the two colliders stay in contact.
public class ContactDamage : MonoBehaviour
{
    [SerializeField] private int damage = 3;                 // damage PER TICK, keep it small
    [SerializeField] private float damageInterval = 0.5f;    // seconds between ticks
    [SerializeField] private string targetTag = "Player";    // set to "Enemy" on the player's script if it deals damage back

    private float nextDamageTime;

    private void Awake()
    {
        // A resting body goes to sleep and Unity then stops calling OnCollisionStay2D,
        // so an idle enemy touching a still player would stop damaging. Prevent that.
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Reset so the first touch always hurts immediately
        if (collision.gameObject.CompareTag(targetTag)) nextDamageTime = 0f;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (Time.time < nextDamageTime) return;
        if (!collision.gameObject.CompareTag(targetTag)) return;

        Health targetHealth = collision.gameObject.GetComponent<Health>();
        if (targetHealth == null) return;

        targetHealth.TakeDamage(damage);
        nextDamageTime = Time.time + damageInterval;
    }
}