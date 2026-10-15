using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyAudio : MonoBehaviour
{
    public AudioClip creepyClip;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        bool anyEnemyChasing = false;

        EnemyController2D[] enemies = FindObjectsByType<EnemyController2D>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            if (!enemy.useGlobalAudio) continue;
            if (enemy.IsPlayerNearby)
            {
                anyEnemyChasing = true;
                break;
            }
        }

        if (anyEnemyChasing && !audioSource.isPlaying)
        {
            audioSource.clip = creepyClip;
            audioSource.loop = true;
            audioSource.Play();
        }
        else if (!anyEnemyChasing && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}