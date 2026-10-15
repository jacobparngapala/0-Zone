using UnityEngine;

// Put this on the Player. Plays a sound whenever the player LOSES health
// (healing does not trigger it). A minimum interval keeps continuous small
// damage from spamming the sound.
[RequireComponent(typeof(Health))]
public class PlayerHurtSound : MonoBehaviour
{
    public AudioClip hurtClip;
    [Range(0f, 1f)] public float volume = 1f;
    [Tooltip("Minimum seconds between hurt sounds, so continuous damage doesn't spam it")]
    public float minInterval = 1.5f;

    private Health health;
    private AudioSource sfx;
    private int lastHealth;
    private float nextAllowedTime;

    void Awake()
    {
        health = GetComponent<Health>();
        lastHealth = health.MaxHealth;

        // Separate child AudioSource so it never clashes with the footstep source
        GameObject go = new GameObject("HurtSFX");
        go.transform.SetParent(transform, false);
        sfx = go.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.spatialBlend = 0f;

        health.OnHealthChanged.AddListener(OnHealthChanged);
    }

    void OnHealthChanged(int current, int max)
    {
        bool tookDamage = current < lastHealth;
        lastHealth = current;

        if (!tookDamage || hurtClip == null) return;
        if (Time.time < nextAllowedTime) return;

        sfx.PlayOneShot(hurtClip, volume);
        nextAllowedTime = Time.time + minInterval;
    }
}
