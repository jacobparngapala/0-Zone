using System.Collections;
using UnityEngine;

// Put this on any enemy (needs Health + SpriteRenderer).
// When a hit doesn't kill the enemy: plays a hit sound and blinks the sprite red.
// A killing hit is handled by EnemyDeathSound instead, so the two never overlap.
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyHitFeedback : MonoBehaviour
{
    [Header("Sound")]
    public AudioClip hitClip;
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Flash")]
    public Color flashColor = new Color(1f, 0.15f, 0.15f, 1f);
    public float flashDuration = 1f;
    public float blinkInterval = 0.1f;

    private SpriteRenderer sr;
    private Health health;
    private AudioSource sfx;
    private Color originalColor;
    private Coroutine flashRoutine;
    private int lastHealth;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        health = GetComponent<Health>();
        originalColor = sr.color;
        lastHealth = health.MaxHealth;

        GameObject go = new GameObject("HitSFX");
        go.transform.SetParent(transform, false);
        sfx = go.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.spatialBlend = 0f;   // 2D: same loudness at any distance

        health.OnHealthChanged.AddListener(OnHealthChanged);
    }

    void OnHealthChanged(int current, int max)
    {
        bool tookDamage = current < lastHealth;
        lastHealth = current;

        if (!tookDamage || current <= 0) return;   // dying: death sound takes over

        if (hitClip != null) sfx.PlayOneShot(hitClip, volume);

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        sr.color = originalColor;
        flashRoutine = StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        float end = Time.time + flashDuration;
        bool red = true;
        while (Time.time < end)
        {
            sr.color = red ? flashColor : originalColor;
            red = !red;
            yield return new WaitForSeconds(blinkInterval);
        }
        sr.color = originalColor;
        flashRoutine = null;
    }

    void OnDisable()
    {
        if (sr != null) sr.color = originalColor;
    }
}
