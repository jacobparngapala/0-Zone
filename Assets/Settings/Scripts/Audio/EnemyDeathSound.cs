using UnityEngine;

// Put this on any enemy that has a Health component.
// Plays a one-shot sound when Health.OnDeath fires, at full volume
// no matter how far the enemy is from the player.
[RequireComponent(typeof(Health))]
public class EnemyDeathSound : MonoBehaviour
{
    public AudioClip deathClip;
    [Range(0f, 1f)] public float volume = 1f;
    [Tooltip("Prints a line to the Console when the sound fires (to check it triggers)")]
    public bool logWhenPlayed = true;

    void Awake()
    {
        GetComponent<Health>().OnDeath.AddListener(PlayDeathSound);
    }

    void PlayDeathSound()
    {
        if (logWhenPlayed)
            Debug.Log("EnemyDeathSound: " + name + " died, clip = " + (deathClip != null ? deathClip.name : "NONE ASSIGNED"));

        if (deathClip == null) return;

        // The enemy is about to be destroyed, so the sound can't live on it.
        // Spawn a temporary AudioSource at the listener (the camera), fully 2D,
        // so distance to the enemy can't affect volume.
        AudioListener listener = FindFirstObjectByType<AudioListener>();
        Vector3 pos = listener != null ? listener.transform.position : transform.position;

        GameObject go = new GameObject("EnemyDeathSound");
        go.transform.position = pos;

        AudioSource src = go.AddComponent<AudioSource>();
        src.clip = deathClip;
        src.volume = volume;
        src.spatialBlend = 0f;
        src.priority = 0;        // highest priority, never culled by other sounds
        src.Play();

        Destroy(go, deathClip.length + 0.1f);
    }
}