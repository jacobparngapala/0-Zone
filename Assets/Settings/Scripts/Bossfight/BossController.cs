using System;
using System.Collections;
using UnityEngine;

// PHASE 3 - the boss.
// 300 health (set Max Health = 300 on the Health component), deals 25 damage on contact.
// Chases the player with front / side / back sprites, telegraphs and lunges now and then,
// uses the boss sound effects, drives the boss health bar, and plays the DEATH sprites when it dies.
// Raises OnDefeated once the death animation has finished.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Health))]
public class BossController : MonoBehaviour
{
    public event Action OnDefeated;

    private enum State { Intro, Chase, Telegraph, Lunge, Dead }
    private enum Facing { Front, Side, Back }

    [Header("Stats")]
    public string bossName = "THE WORM";
    [Tooltip("Damage dealt to the player on contact")]
    public int contactDamage = 25;
    public float contactInterval = 1f;
    [Tooltip("Only used to warn you in the Console if the Health component isn't set to this")]
    public int expectedMaxHealth = 300;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float stopRange = 0.5f;

    [Header("Lunge attack")]
    public bool enableLunge = true;
    public float lungeRange = 5f;
    public float lungeCooldown = 4f;
    public float telegraphTime = 0.6f;
    public float lungeSpeed = 7f;
    public float lungeDuration = 0.5f;
    public Color telegraphColor = new Color(1f, 0.5f, 0.5f, 1f);

    [Header("Sprites (sliced from the spritesheets, in order)")]
    public Sprite[] frontFrames;   // front_boss_spritesheet_0..3
    public Sprite[] sideFrames;    // sideview_boss_spritesheet_0..3  (art faces RIGHT)
    public Sprite[] backFrames;    // backview_boss_spritesheet_0..3
    public Sprite[] deathFrames;   // Death1..Death4 (or bossfight_death_spritesheet_0..3)
    public float frameRate = 6f;
    public float deathFrameRate = 5f;
    [Tooltip("Tick if the side-view art faces LEFT instead of right")]
    public bool sideArtFacesLeft = false;
    [Tooltip("Death frames are 768px, boss frames 1280px. Raise if the death animation looks too small")]
    public float deathScaleMultiplier = 1f;

    [Header("Intro / Death timing")]
    public float introDuration = 1.5f;
    public float deathHoldTime = 1f;
    public float deathFadeTime = 1f;

    [Header("Hit feedback")]
    public Color hitFlashColor = new Color(1f, 0.15f, 0.15f, 1f);
    public float hitFlashDuration = 0.12f;

    [Header("Boss sound effects")]
    public AudioClip spawnClip;        // e.g. DeathSoundMonster / Spooky
    public AudioClip moveLoopClip;     // wormmovement
    public AudioClip attackClip;       // WormAttack
    public AudioClip hitClip;          // hit
    public AudioClip deathClip;        // WormDeath
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float moveLoopVolume = 0.6f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Health health;
    private Collider2D[] colliders;
    private AudioSource sfx;
    private AudioSource moveSource;

    private Transform player;
    private Health playerHealth;
    private BossHealthBarUI healthBar;

    private State state = State.Intro;
    private Facing facing = Facing.Front;
    private bool begun;
    private Vector2 moveDir = Vector2.down;
    private Vector2 lungeDir;
    private float nextLungeTime;
    private float nextDamageTime;
    private float frameTimer;
    private int frameIndex;
    private float hitFlashTimer;
    private int lastHealth;
    private Color baseColor;
    private Vector3 baseScale;

    // Called by BossFightManager right after the boss is spawned
    public void Begin(Transform playerTransform, BossHealthBarUI bar)
    {
        if (begun) return;
        begun = true;

        player = playerTransform;
        healthBar = bar;
        if (player != null) playerHealth = player.GetComponent<Health>();

        StartCoroutine(IntroRoutine());
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        health = GetComponent<Health>();
        colliders = GetComponents<Collider2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;   // contact damage keeps ticking on a still player

        baseColor = sr.color;
        baseScale = transform.localScale;
        lastHealth = health.MaxHealth;

        sfx = CreateSource("BossSFX", false);
        moveSource = CreateSource("BossMoveLoop", true);
        moveSource.clip = moveLoopClip;
        moveSource.volume = moveLoopVolume;

        health.OnHealthChanged.AddListener(HandleHealthChanged);
        health.OnDeath.AddListener(HandleDeath);

        if (health.MaxHealth != expectedMaxHealth)
            Debug.LogWarning("BossController: Health.Max Health is " + health.MaxHealth
                + " but the boss should have " + expectedMaxHealth + ". Set it on the Health component.");
    }

    private void Start()
    {
        // Lets you also drop the boss straight into the scene for testing, without the manager
        if (!begun)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            Begin(p != null ? p.transform : null, FindFirstObjectByType<BossHealthBarUI>());
        }
    }

    private AudioSource CreateSource(string objName, bool loop)
    {
        GameObject go = new GameObject(objName);
        go.transform.SetParent(transform, false);
        AudioSource src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = loop;
        src.spatialBlend = 0f;   // 2D game: same loudness everywhere
        return src;
    }

    // ---------------- INTRO ----------------
    private IEnumerator IntroRoutine()
    {
        state = State.Intro;
        SetColliders(false);
        SetAlpha(0f);

        if (spawnClip != null) sfx.PlayOneShot(spawnClip, sfxVolume);
        if (healthBar != null) healthBar.Show(bossName, health.CurrentHealth, health.MaxHealth);

        // Appear on the first front frame, fading in
        if (frontFrames != null && frontFrames.Length > 0) sr.sprite = frontFrames[0];

        float t = 0f;
        while (t < introDuration)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Clamp01(t / Mathf.Max(introDuration, 0.01f)));
            Animate();
            yield return null;
        }

        SetAlpha(1f);
        SetColliders(true);
        nextLungeTime = Time.time + lungeCooldown;
        state = State.Chase;
    }

    // ---------------- UPDATE ----------------
    private void Update()
    {
        if (state == State.Dead) return;

        if (hitFlashTimer > 0f)
        {
            hitFlashTimer -= Time.deltaTime;
            if (hitFlashTimer <= 0f && state != State.Telegraph) sr.color = baseColor;
        }

        if (state == State.Chase && enableLunge && CanSeePlayer()
            && Time.time >= nextLungeTime && DistanceToPlayer() <= lungeRange)
        {
            StartCoroutine(LungeRoutine());
        }

        if (state == State.Chase || state == State.Lunge) UpdateFacing(moveDir);
        Animate();
        UpdateMoveSound();
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case State.Chase:
                if (!CanSeePlayer() || DistanceToPlayer() <= stopRange)
                {
                    rb.linearVelocity = Vector2.zero;
                    break;
                }
                moveDir = ((Vector2)player.position - rb.position).normalized;
                rb.linearVelocity = moveDir * moveSpeed;
                break;

            case State.Lunge:
                rb.linearVelocity = lungeDir * lungeSpeed;
                break;

            default:
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    private bool CanSeePlayer()
    {
        return player != null && (playerHealth == null || playerHealth.CurrentHealth > 0);
    }

    private float DistanceToPlayer()
    {
        return player == null ? float.MaxValue : Vector2.Distance(transform.position, player.position);
    }

    // ---------------- LUNGE ----------------
    private IEnumerator LungeRoutine()
    {
        state = State.Telegraph;
        rb.linearVelocity = Vector2.zero;
        sr.color = telegraphColor;
        if (attackClip != null) sfx.PlayOneShot(attackClip, sfxVolume);

        // Wind-up: stand still, keep looking at the player
        float t = 0f;
        while (t < telegraphTime)
        {
            t += Time.deltaTime;
            if (player != null)
            {
                moveDir = ((Vector2)player.position - rb.position).normalized;
                UpdateFacing(moveDir);
            }
            yield return null;
        }

        sr.color = baseColor;
        lungeDir = moveDir;
        state = State.Lunge;

        yield return new WaitForSeconds(lungeDuration);

        if (state == State.Lunge)
        {
            state = State.Chase;
            nextLungeTime = Time.time + lungeCooldown;
        }
    }

    // ---------------- SPRITES ----------------
    private void UpdateFacing(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) facing = Facing.Side;
        else facing = dir.y < 0f ? Facing.Front : Facing.Back;

        if (facing == Facing.Side)
        {
            bool movingLeft = dir.x < 0f;
            sr.flipX = sideArtFacesLeft ? !movingLeft : movingLeft;
        }
        else
        {
            sr.flipX = false;
        }
    }

    private void Animate()
    {
        Sprite[] frames = facing == Facing.Side ? sideFrames
                        : facing == Facing.Back ? backFrames
                        : frontFrames;
        if (frames == null || frames.Length == 0) return;

        frameTimer += Time.deltaTime;
        float step = 1f / Mathf.Max(frameRate, 0.01f);
        if (frameTimer >= step)
        {
            frameTimer -= step;
            frameIndex++;
        }
        sr.sprite = frames[frameIndex % frames.Length];
    }

    private void UpdateMoveSound()
    {
        if (moveLoopClip == null) return;

        bool moving = (state == State.Chase || state == State.Lunge) && rb.linearVelocity.sqrMagnitude > 0.05f;
        if (moving && !moveSource.isPlaying) moveSource.Play();
        else if (!moving && moveSource.isPlaying) moveSource.Pause();
    }

    private void SetAlpha(float a)
    {
        Color c = baseColor;
        c.a = a;
        sr.color = c;
    }

    private void SetColliders(bool on)
    {
        foreach (Collider2D c in colliders) c.enabled = on;
    }

    // ---------------- DAMAGE IN / OUT ----------------
    private void HandleHealthChanged(int current, int max)
    {
        bool tookDamage = current < lastHealth;
        lastHealth = current;

        if (healthBar != null) healthBar.UpdateBar(current, max);

        if (!tookDamage || current <= 0 || state == State.Dead) return;

        if (hitClip != null) sfx.PlayOneShot(hitClip, sfxVolume);
        if (state != State.Telegraph && state != State.Intro)
        {
            sr.color = hitFlashColor;
            hitFlashTimer = hitFlashDuration;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) nextDamageTime = 0f;   // first touch hurts immediately
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (state == State.Dead || state == State.Intro) return;
        if (Time.time < nextDamageTime) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        Health target = collision.gameObject.GetComponent<Health>();
        if (target == null) return;

        target.TakeDamage(contactDamage);   // 25
        nextDamageTime = Time.time + contactInterval;
    }

    // ---------------- DEATH ----------------
    private void HandleDeath()
    {
        if (state == State.Dead) return;
        state = State.Dead;

        StopAllCoroutines();
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        SetColliders(false);
        moveSource.Stop();

        if (healthBar != null) healthBar.Hide();
        PlayDeathSound();

        StartCoroutine(DeathRoutine());
    }

    private void PlayDeathSound()
    {
        if (deathClip == null) return;

        // Own temporary object so the sound survives even if the boss is destroyed early
        GameObject go = new GameObject("BossDeathSound");
        AudioSource src = go.AddComponent<AudioSource>();
        src.clip = deathClip;
        src.volume = sfxVolume;
        src.spatialBlend = 0f;
        src.priority = 0;
        src.Play();
        Destroy(go, deathClip.length + 0.1f);
    }

    private IEnumerator DeathRoutine()
    {
        sr.color = baseColor;
        sr.flipX = false;
        transform.localScale = baseScale * deathScaleMultiplier;

        if (deathFrames != null && deathFrames.Length > 0)
        {
            float step = 1f / Mathf.Max(deathFrameRate, 0.01f);
            foreach (Sprite s in deathFrames)
            {
                sr.sprite = s;
                yield return new WaitForSeconds(step);
            }
        }

        yield return new WaitForSeconds(deathHoldTime);

        float t = 0f;
        while (t < deathFadeTime)
        {
            t += Time.deltaTime;
            SetAlpha(1f - Mathf.Clamp01(t / Mathf.Max(deathFadeTime, 0.01f)));
            yield return null;
        }

        OnDefeated?.Invoke();
        Destroy(gameObject);
    }
}
