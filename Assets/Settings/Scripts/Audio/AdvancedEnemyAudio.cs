using UnityEngine;

// Per-enemy audio. Put this on the enemy itself (e.g. AdvancedEnemy1).
// Plays its own looping sound while the player is within the enemy's hearingRange,
// independent of the global EnemyAudioManager.
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(EnemyController2D))]
public class AdvancedEnemyAudio : MonoBehaviour
{
    public AudioClip loopClip;
    [Range(0f, 1f)] public float maxVolume = 1f;
    [Tooltip("Louder as the player gets closer")]
    public bool fadeWithDistance = true;

    private AudioSource audioSource;
    private EnemyController2D enemy;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        enemy = GetComponent<EnemyController2D>();

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;   // 2D game: plain stereo, volume handled below
        audioSource.clip = loopClip;

        // Stop the global EnemyAudioManager from also reacting to this enemy
        enemy.useGlobalAudio = false;
    }

    void Update()
    {
        if (loopClip == null) return;

        if (enemy.IsPlayerNearby)
        {
            if (fadeWithDistance && enemy.HearingRange > 0f)
            {
                float t = 1f - Mathf.Clamp01(enemy.DistanceToPlayer / enemy.HearingRange);
                audioSource.volume = maxVolume * t;
            }
            else
            {
                audioSource.volume = maxVolume;
            }

            if (!audioSource.isPlaying) audioSource.Play();
        }
        else if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}