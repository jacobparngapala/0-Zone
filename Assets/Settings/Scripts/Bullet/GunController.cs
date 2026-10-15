using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(InventoryManager))]
public class GunController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform shotPoint;            // the existing ShotPoint
    [SerializeField] private GameObject bulletPrefab;        // Bullet.prefab
    [SerializeField] private PlayerLightController lights;   // the Player itself

    [Header("Gun Stats")]
    [SerializeField] private float fireCooldown = 1.25f;     // slow fire rate (seconds between shots)
    [SerializeField] private float bulletSpeed = 30f;        // fast travel
    [SerializeField] private int damage = 50;                // large damage
    [SerializeField] private float bulletLifetime = 2f;

    [Header("Muzzle Position (local to Player, per facing)")]
    [SerializeField] private Vector2 muzzleUp = new Vector2(0f, 0.15f);
    [SerializeField] private Vector2 muzzleDown = new Vector2(0f, -0.12f);
    [SerializeField] private Vector2 muzzleLeft = new Vector2(-0.23f, -0.03f);
    [SerializeField] private Vector2 muzzleRight = new Vector2(0.23f, -0.03f);

    [Header("Muzzle Flame")]
    [SerializeField] private SpriteRenderer flameRenderer;   // child of ShotPoint
    [SerializeField] private Sprite[] flameFrames;           // Flame_1 (burst), then Flame_0 (flame)
    [SerializeField] private float flameFrameTime = 0.04f;

    [Header("Audio")]
    [SerializeField] private AudioClip shotSound;
    [SerializeField, Range(0f, 1f)] private float shotVolume = 1f;

    private PlayerController player;
    private InventoryManager inventory;
    private AudioSource sfx;
    private float nextFireTime;
    private Coroutine flameRoutine;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        inventory = GetComponent<InventoryManager>();

        GameObject go = new GameObject("GunSFX");
        go.transform.SetParent(transform, false);
        sfx = go.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.spatialBlend = 0f;

        if (flameRenderer != null)
        {
            flameRenderer.enabled = false;
            SpriteRenderer playerSprite = GetComponent<SpriteRenderer>();
            if (playerSprite != null)
            {
                flameRenderer.sortingLayerID = playerSprite.sortingLayerID;
                flameRenderer.sortingOrder = playerSprite.sortingOrder + 1;
            }
        }
    }

    private void Update()
    {
        if (shotPoint == null || bulletPrefab == null) return;
        if (!inventory.IsHolding(ItemType.Gun)) return;   // flashlight in hand = cannot shoot

        AimShotPoint();

        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            Fire();
        }
    }

    // Puts ShotPoint at the muzzle and points its local "up" the way the player faces
    private void AimShotPoint()
    {
        Vector2 m = MuzzleFor(player.FacingDirection);
        shotPoint.localPosition = new Vector3(m.x, m.y, shotPoint.localPosition.z);
        shotPoint.rotation = Quaternion.Euler(0f, 0f, player.FacingAngle);
    }

    private Vector2 MuzzleFor(Vector2 dir)
    {
        if (dir.y > 0.5f) return muzzleUp;
        if (dir.y < -0.5f) return muzzleDown;
        return dir.x < 0f ? muzzleLeft : muzzleRight;
    }

    private void Fire()
    {
        nextFireTime = Time.time + fireCooldown;
        AimShotPoint();

        GameObject bulletObj = Instantiate(bulletPrefab, shotPoint.position, shotPoint.rotation);

        BulletScript bullet = bulletObj.GetComponent<BulletScript>();
        if (bullet != null) bullet.Launch(shotPoint.up, bulletSpeed, damage, bulletLifetime);

        // The bullet must not collide with the player or anything attached to the player
        Collider2D bulletCol = bulletObj.GetComponent<Collider2D>();
        if (bulletCol != null)
        {
            foreach (Collider2D c in GetComponentsInChildren<Collider2D>())
                Physics2D.IgnoreCollision(bulletCol, c);
        }

        player.TriggerAttackPose();                      // attack sprites for the next 4 seconds
        if (lights != null) lights.Flash(player.FacingAngle);   // bright flash, glow stays
        PlayFlame();
        if (shotSound != null) sfx.PlayOneShot(shotSound, shotVolume);
    }

    private void PlayFlame()
    {
        if (flameRenderer == null || flameFrames == null || flameFrames.Length == 0) return;
        if (flameRoutine != null) StopCoroutine(flameRoutine);
        flameRoutine = StartCoroutine(FlameRoutine());
    }

    private IEnumerator FlameRoutine()
    {
        flameRenderer.enabled = true;
        foreach (Sprite frame in flameFrames)
        {
            flameRenderer.sprite = frame;
            yield return new WaitForSeconds(flameFrameTime);
        }
        flameRenderer.enabled = false;
        flameRoutine = null;
    }
}