using UnityEngine;

// PHASE 2 - one of the four little worms.
// Skitters after the player. Every hit it takes is reported to BossFightManager,
// which starts the boss event once enough hits have landed on the little worms.
//
// While the boss event is still pending the worms cannot die (immortalUntilBossEvent),
// so the player can't accidentally clear phase 2 without triggering the boss.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Health))]
public class LittleWorm : MonoBehaviour
{
    [Header("Movement")]
    public float chaseSpeed = 2.4f;
    [Tooltip("How long the outward burst lasts right after the split")]
    public float burstTime = 0.35f;
    public float burstSpeed = 4f;
    [Tooltip("Random side-to-side wobble so they skitter instead of gliding")]
    public float wobbleAmount = 0.6f;
    public float wobbleSpeed = 6f;
    [Tooltip("Tick if the art faces LEFT by default")]
    public bool spriteFacesLeft = false;

    [Header("Damage to player")]
    public int contactDamage = 3;
    public float contactInterval = 0.5f;

    [Header("Hits")]
    public bool immortalUntilBossEvent = true;
    [Range(0.1f, 0.9f)] public float healBackBelow = 0.4f;

    [Header("Hit feedback")]
    public Color hitFlashColor = new Color(1f, 0.15f, 0.15f, 1f);
    public float hitFlashDuration = 0.12f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Health health;
    private Transform player;
    private BossFightManager manager;

    private Vector2 burstDir;
    private float burstTimer;
    private float wobblePhase;
    private float nextDamageTime;
    private float flashTimer;
    private Color baseColor;
    private int lastHealth;
    private bool frozen;

    public void Init(BossFightManager owner, Transform playerTransform, Vector2 outwardDirection)
    {
        manager = owner;
        player = playerTransform;
        burstDir = outwardDirection.normalized;
        burstTimer = burstTime;
    }

    // Called by the manager when the boss event begins: stops moving and attacking
    public void Freeze()
    {
        frozen = true;
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        health = GetComponent<Health>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;   // keeps contact damage ticking

        baseColor = sr.color;
        lastHealth = health.MaxHealth;
        wobblePhase = Random.value * 10f;

        health.OnHealthChanged.AddListener(HandleHealthChanged);
        health.OnDeath.AddListener(HandleDeath);
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Update()
    {
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f) sr.color = baseColor;
        }
    }

    private void FixedUpdate()
    {
        if (frozen || player == null) { rb.linearVelocity = Vector2.zero; return; }

        Vector2 velocity;
        if (burstTimer > 0f)
        {
            burstTimer -= Time.fixedDeltaTime;
            velocity = burstDir * burstSpeed * Mathf.Clamp01(burstTimer / Mathf.Max(burstTime, 0.01f));
        }
        else
        {
            Vector2 toPlayer = ((Vector2)player.position - rb.position).normalized;
            Vector2 side = new Vector2(-toPlayer.y, toPlayer.x);
            float wobble = Mathf.Sin(Time.time * wobbleSpeed + wobblePhase) * wobbleAmount;
            velocity = (toPlayer + side * wobble).normalized * chaseSpeed;
        }

        rb.linearVelocity = velocity;

        if (Mathf.Abs(velocity.x) > 0.01f)
        {
            bool movingLeft = velocity.x < 0f;
            sr.flipX = spriteFacesLeft ? !movingLeft : movingLeft;
        }
    }

    private void HandleHealthChanged(int current, int max)
    {
        bool tookDamage = current < lastHealth;
        lastHealth = current;

        if (!tookDamage) return;

        manager?.RegisterLittleWormHit();

        sr.color = hitFlashColor;
        flashTimer = hitFlashDuration;

        // Keep the worms alive until the boss event fires
        if (immortalUntilBossEvent && manager != null && !manager.BossEventStarted
            && current <= max * healBackBelow)
        {
            lastHealth = max;
            health.Heal(max);
        }
    }

    private void HandleDeath()
    {
        // Only reachable when immortalUntilBossEvent is off, or after the event started
        foreach (Collider2D c in GetComponents<Collider2D>()) c.enabled = false;
        Destroy(gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (frozen || contactDamage <= 0 || Time.time < nextDamageTime) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        Health target = collision.gameObject.GetComponent<Health>();
        if (target == null) return;

        target.TakeDamage(contactDamage);
        nextDamageTime = Time.time + contactInterval;
    }
}
