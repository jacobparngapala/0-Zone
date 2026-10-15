using System.Collections.Generic;
using UnityEngine;

// One full set of gun sprites (used twice: "equipped" and "attack")
[System.Serializable]
public class GunPoseSprites
{
    [Header("Standing")]
    public Sprite frontIdle; // S
    public Sprite backIdle;  // W
    public Sprite leftIdle;  // A
    public Sprite rightIdle; // D

    [Header("Walking (played in the order you list them)")]
    public Sprite[] frontWalk;
    public Sprite[] backWalk;
    public Sprite[] leftWalk;
    public Sprite[] rightWalk;

    public Sprite GetIdle(KeyCode k)
    {
        switch (k)
        {
            case KeyCode.W: return backIdle;
            case KeyCode.A: return leftIdle;
            case KeyCode.D: return rightIdle;
            default: return frontIdle;
        }
    }

    public Sprite[] GetWalk(KeyCode k)
    {
        switch (k)
        {
            case KeyCode.W: return backWalk;
            case KeyCode.A: return leftWalk;
            case KeyCode.D: return rightWalk;
            default: return frontWalk;
        }
    }
}

public class PlayerController : MonoBehaviour
{
    // ---------------- ORIGINAL FIELDS (unchanged) ----------------
    public float moveSpeed = 5f;

    public Sprite frontSprite; // S
    public Sprite backSprite;  // W
    public Sprite leftSprite;  // A
    public Sprite rightSprite; // D
    public Transform flashlight;

    public AudioClip footstepSound;
    public float footstepInterval = 0.3f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private float footstepTimer;

    private readonly List<KeyCode> heldKeys = new List<KeyCode>();
    private readonly KeyCode[] moveKeys = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };

    public AudioClip footstepClip;

    [Header("Walk Animation (flashlight)")]
    public Sprite[] frontWalk; // FlashWalk1, FlashWalk2
    public Sprite[] backWalk;  // FlashBackWalk1, FlashBackWalk2
    public Sprite[] leftWalk;  // FlashLeftWalk1, FlashLeftWalk2
    public Sprite[] rightWalk; // FlashRightWalk1, FlashRightWalk2
    public float walkFps = 6f;

    private float animTimer;
    private KeyCode lastFacing = KeyCode.S;

    // ---------------- NEW: GUN ANIMATION ----------------
    [Header("Gun Animation")]
    public GunPoseSprites equipped;   // default pose while holding the gun
    public GunPoseSprites attack;     // pose after a shot
    public float gunWalkFps = 8f;
    [Tooltip("Seconds the attack pose lasts after each shot")]
    public float attackPoseDuration = 4f;

    private InventoryManager inventory;
    private float attackTimer;

    // ---------------- NEW: PUBLIC API FOR THE GUN ----------------
    public bool HoldingGun => inventory != null && inventory.IsHolding(ItemType.Gun);
    public bool IsInAttackPose => attackTimer > 0f;

    public Vector2 FacingDirection
    {
        get
        {
            switch (lastFacing)
            {
                case KeyCode.W: return Vector2.up;
                case KeyCode.A: return Vector2.left;
                case KeyCode.D: return Vector2.right;
                default: return Vector2.down;
            }
        }
    }

    // Same angles your flashlight already uses: W=0, A=90, S=180, D=270
    public float FacingAngle
    {
        get
        {
            switch (lastFacing)
            {
                case KeyCode.W: return 0f;
                case KeyCode.A: return 90f;
                case KeyCode.D: return 270f;
                default: return 180f;
            }
        }
    }

    public void TriggerAttackPose()
    {
        attackTimer = attackPoseDuration;
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        inventory = GetComponent<InventoryManager>();
        if (rb != null) rb.gravityScale = 0f;
    }

    void OnEnable()
    {
        if (inventory != null) inventory.OnSelectionChanged += OnItemChanged;
    }

    void OnDisable()
    {
        if (inventory != null) inventory.OnSelectionChanged -= OnItemChanged;
    }

    void OnItemChanged(int index, ItemData item)
    {
        attackTimer = 0f;   // switching items always resets the pose
        animTimer = 0f;
    }

    void Update()
    {
        UpdateHeldKeys();
        if (attackTimer > 0f) attackTimer -= Time.deltaTime;
        UpdateSprite();
        UpdateFootstepAudio();
    }

    void UpdateFootstep()
    {
        if (heldKeys.Count > 0)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                if (audioSource != null && footstepSound != null)
                    audioSource.PlayOneShot(footstepSound);

                footstepTimer = footstepInterval;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    void FixedUpdate()
    {
        Vector2 move = GetMoveVector();

        if (rb != null)
        {
            rb.MovePosition(rb.position + move * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            transform.position += (Vector3)(move * moveSpeed * Time.deltaTime);
        }
    }

    void UpdateFootstepAudio()
    {
        if (audioSource == null) return;

        bool isMoving = heldKeys.Count > 0;

        if (isMoving && !audioSource.isPlaying)
        {
            audioSource.clip = footstepClip;
            audioSource.loop = true;
            audioSource.Play();
        }
        else if (!isMoving && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    void UpdateHeldKeys()
    {
        foreach (var key in moveKeys)
        {
            if (Input.GetKeyDown(key) && !heldKeys.Contains(key))
            {
                heldKeys.Add(key);
            }
            else if (Input.GetKeyUp(key))
            {
                heldKeys.Remove(key);
            }
        }
    }

    Vector2 GetMoveVector()
    {
        Vector2 dir = Vector2.zero;
        if (heldKeys.Contains(KeyCode.W)) dir.y += 1f;
        if (heldKeys.Contains(KeyCode.S)) dir.y -= 1f;
        if (heldKeys.Contains(KeyCode.A)) dir.x -= 1f;
        if (heldKeys.Contains(KeyCode.D)) dir.x += 1f;
        return dir.normalized;
    }

    // ---------------- SPRITES ----------------
    void UpdateSprite()
    {
        bool moving = heldKeys.Count > 0;

        if (moving)
        {
            KeyCode facing = heldKeys[0];
            if (facing != lastFacing) animTimer = 0f; // restart cycle on direction change
            lastFacing = facing;
            animTimer += Time.deltaTime;
            SetFlashlightForFacing(facing);
        }
        else
        {
            animTimer = 0f;
        }

        if (HoldingGun) UpdateGunSprite(moving);
        else UpdateFlashlightSprite(moving);
    }

    // Your original flashlight logic, unchanged
    void UpdateFlashlightSprite(bool moving)
    {
        if (!moving)
        {
            Sprite idle = GetStandingSprite(lastFacing);
            if (idle != null) spriteRenderer.sprite = idle;
            return;
        }

        Sprite[] frames = GetWalkFrames(lastFacing);
        if (frames != null && frames.Length > 0)
        {
            int index = Mathf.FloorToInt(animTimer * walkFps) % frames.Length;
            spriteRenderer.sprite = frames[index];
        }
        else
        {
            Sprite s = GetStandingSprite(lastFacing);
            if (s != null) spriteRenderer.sprite = s;
        }
    }

    // Gun: attack sprites while the timer runs, equipped sprites otherwise
    void UpdateGunSprite(bool moving)
    {
        GunPoseSprites pose = attackTimer > 0f ? attack : equipped;
        if (pose == null) return;

        Sprite s = null;
        if (moving)
        {
            Sprite[] frames = pose.GetWalk(lastFacing);
            if (frames != null && frames.Length > 0)
                s = frames[Mathf.FloorToInt(animTimer * gunWalkFps) % frames.Length];
        }
        if (s == null) s = pose.GetIdle(lastFacing);
        if (s != null) spriteRenderer.sprite = s;
    }

    Sprite[] GetWalkFrames(KeyCode k)
    {
        switch (k)
        {
            case KeyCode.W: return backWalk;
            case KeyCode.S: return frontWalk;
            case KeyCode.A: return leftWalk;
            case KeyCode.D: return rightWalk;
        }
        return null;
    }

    Sprite GetStandingSprite(KeyCode k)
    {
        switch (k)
        {
            case KeyCode.W: return backSprite;
            case KeyCode.S: return frontSprite;
            case KeyCode.A: return leftSprite;
            case KeyCode.D: return rightSprite;
        }
        return frontSprite;
    }

    void SetFlashlightForFacing(KeyCode k)
    {
        switch (k)
        {
            case KeyCode.W: SetFlashlightAngle(0f); break;
            case KeyCode.S: SetFlashlightAngle(180f); break;
            case KeyCode.A: SetFlashlightAngle(90f); break;
            case KeyCode.D: SetFlashlightAngle(270f); break;
        }
    }

    void SetFlashlightAngle(float zAngle)
    {
        if (flashlight != null)
        {
            flashlight.rotation = Quaternion.Euler(0f, 0f, zAngle);
        }
    }
}