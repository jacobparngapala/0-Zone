using System;
using UnityEngine;

// PHASE 1 - the harmless worm.
// Wanders around slowly, never damages the player. Needs a Health component.
// When its Health reaches 0 it raises OnKilled(position) and removes itself.
// BossFightManager listens to that event and splits it into four little worms (phase 2).
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Health))]
public class StarterWorm : MonoBehaviour
{
    public event Action<Vector3> OnKilled;

    [Header("Animation (Bossfight1, Bossfight2)")]
    public Sprite[] frames;
    public float frameRate = 4f;
    [Tooltip("Tick if the art faces LEFT by default")]
    public bool spriteFacesLeft = false;

    [Header("Wandering")]
    public float wanderSpeed = 0.8f;
    public Vector2 changeDirectionTime = new Vector2(1f, 2.5f);
    [Tooltip("Pause between walks")]
    public Vector2 restTime = new Vector2(0.3f, 1f);

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Health health;

    private Vector2 dir;
    private float stateTimer;
    private bool resting;
    private int frameIndex;
    private float frameTimer;
    private bool dead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        health = GetComponent<Health>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        health.OnDeath.AddListener(HandleDeath);
        PickNewState();
    }

    private void Update()
    {
        if (dead) return;

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f) PickNewState();

        if (frames != null && frames.Length > 0)
        {
            frameTimer += Time.deltaTime;
            float step = 1f / Mathf.Max(frameRate, 0.01f);
            if (frameTimer >= step)
            {
                frameTimer -= step;
                frameIndex = (frameIndex + 1) % frames.Length;
                sr.sprite = frames[frameIndex];
            }
        }
    }

    private void FixedUpdate()
    {
        if (dead) { rb.linearVelocity = Vector2.zero; return; }

        rb.linearVelocity = resting ? Vector2.zero : dir * wanderSpeed;

        if (!resting && Mathf.Abs(dir.x) > 0.01f)
        {
            bool movingLeft = dir.x < 0f;
            sr.flipX = spriteFacesLeft ? !movingLeft : movingLeft;
        }
    }

    // Bounce off walls by picking a new direction on any collision
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (dead || collision.gameObject.CompareTag("Player")) return;
        PickNewState();
    }

    private void PickNewState()
    {
        resting = !resting && UnityEngine.Random.value < 0.35f;
        if (resting)
        {
            stateTimer = UnityEngine.Random.Range(restTime.x, restTime.y);
        }
        else
        {
            dir = UnityEngine.Random.insideUnitCircle.normalized;
            stateTimer = UnityEngine.Random.Range(changeDirectionTime.x, changeDirectionTime.y);
        }
    }

    private void HandleDeath()
    {
        if (dead) return;
        dead = true;

        rb.linearVelocity = Vector2.zero;
        foreach (Collider2D c in GetComponents<Collider2D>()) c.enabled = false;

        OnKilled?.Invoke(transform.position);
        Destroy(gameObject);
    }
}
