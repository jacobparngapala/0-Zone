using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController2D : MonoBehaviour
{
    public bool IsChasingPlayer => currentState == State.Idle;
    private enum State { Idle, Walk, Run }

    // Frames
    public Sprite[] idleFrames;
    public Sprite[] walkFrames;
    public Sprite[] runFrames;
    public float frameRate = 8f;

    // Range
    public float detectionRange = 5f;
    public float runRange = 2f;
    public float stopRange = 0.6f;

    // Speed
    public float walkSpeed = 1.5f;
    public float runSpeed = 3.5f;
    public Transform player;

    // Tick this if the enemy's art faces LEFT by default (fixes "walks backwards")
    public bool spriteFacesLeft = false;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    // State
    private State currentState = State.Idle;
    private int frameIndex;
    private float frameTimer;

    // Audio Detector
    public float hearingRange = 3f;

    // The global EnemyAudio manager skips enemies that have their own audio
    // (AdvancedEnemyAudio sets this to false automatically).
    [HideInInspector] public bool useGlobalAudio = true;

    public float HearingRange => hearingRange;
    public float DistanceToPlayer => player == null ? float.MaxValue : Vector2.Distance(transform.position, player.position);
    public bool IsPlayerNearby
    {
        get
        {
            if (player == null) return false;
            float distance = Vector2.Distance(transform.position, player.position);
            return distance <= hearingRange;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    private void Update()
    {
        UpdateState();
        Animate();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void UpdateState()
    {
        if (player == null)
        {
            currentState = State.Idle;
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= stopRange)
        {
            currentState = State.Idle;
        }
        else if (distance <= runRange)
        {
            currentState = State.Run;
        }
        else if (distance <= detectionRange)
        {
            currentState = State.Walk;
        }
        else
        {
            currentState = State.Idle; // player out of range 
        }
    }

    private void Move()
    {
        if (currentState == State.Idle || player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = ((Vector2)player.position - rb.position).normalized;
        float speed = currentState == State.Run ? runSpeed : walkSpeed;
        rb.linearVelocity = direction * speed;

        // Flip sprite to face movement direction.
        // Default assumes the art faces right; spriteFacesLeft inverts that.
        if (Mathf.Abs(direction.x) > 0.01f)
        {
            bool movingLeft = direction.x < 0f;
            sr.flipX = spriteFacesLeft ? !movingLeft : movingLeft;
        }
    }

    private void Animate()
    {
        Sprite[] frames = currentState switch
        {
            State.Walk => walkFrames,
            State.Run => runFrames,
            _ => idleFrames,
        };

        if (frames == null || frames.Length == 0) return;

        frameTimer += Time.deltaTime;
        float frameDuration = 1f / Mathf.Max(frameRate, 0.01f);

        if (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            frameIndex = (frameIndex + 1) % frames.Length;
            sr.sprite = frames[frameIndex];
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, runRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, stopRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
    }
}