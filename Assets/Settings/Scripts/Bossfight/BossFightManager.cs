using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

// Runs the whole Level 3 boss fight. Put it on the "Level3Manager" object (or a new empty "BossFightManager").
//
//  PHASE 1  StarterWorm wanders harmlessly. Killing it splits it into four LittleWorms.
//  PHASE 2  Hitting the little worms enough times triggers the boss event:
//             lights out -> black screen flicker x3 -> BOSS WARNING -> worms vanish in a puff
//             -> lights back -> boss music starts -> boss spawns
//  PHASE 3  BossController: 300 HP, 25 damage, boss health bar, death animation.
//           When the boss is defeated the music fades out and the win area is enabled.
public class BossFightManager : MonoBehaviour
{
    public enum Phase { Phase1, Phase2, BossEvent, Phase3, Defeated }

    [Header("Phase 1")]
    [SerializeField] private StarterWorm starterWorm;

    [Header("Phase 2")]
    [SerializeField] private LittleWorm littleWormPrefab;
    [SerializeField] private int wormCount = 4;
    [SerializeField] private float splitRadius = 0.6f;
    [Tooltip("Total hits on the little worms (all four together) that trigger the boss event")]
    [SerializeField] private int hitsToTriggerBoss = 12;

    [Header("Boss event - lights")]
    [Tooltip("The 'Global Light 2D' object in the scene")]
    [SerializeField] private Light2D globalLight;
    [Tooltip("Optional extra lights to switch off during the blackout (e.g. the player's Flashlight / Glow)")]
    [SerializeField] private Light2D[] extraLightsToDisable;
    [SerializeField] private float lightsOutFadeTime = 0.6f;
    [SerializeField] private float lightsBackFadeTime = 1f;
    [Tooltip("Global light intensity after the blackout. -1 = same as before the blackout")]
    [SerializeField] private float lightsBackIntensity = -1f;

    [Header("Boss event - UI (all start disabled)")]
    [Tooltip("Full-screen black Image")]
    [SerializeField] private GameObject blackScreen;
    [Tooltip("Image using 3timesflicker_TasMagpapakitaSaBlackScreen")]
    [SerializeField] private GameObject flickerSprite;
    [SerializeField] private int flickerCount = 3;
    [SerializeField] private float flickerOnTime = 0.18f;
    [SerializeField] private float flickerOffTime = 0.28f;
    [Tooltip("Panel with the BOSS WARNING text/graphic. A CanvasGroup on it makes it pulse, otherwise it blinks")]
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private float warningDuration = 3f;
    [SerializeField] private float warningPulseSpeed = 6f;

    [Header("Boss event - vanishing enemies")]
    [Tooltip("Sprite Untitled385_20261011172933 (the smoke puff)")]
    [SerializeField] private Sprite puffSprite;
    [SerializeField] private float puffDuration = 0.7f;
    [SerializeField] private float puffGrowScale = 1.5f;
    [Tooltip("Also make every other EnemyController2D in the level vanish")]
    [SerializeField] private bool clearOtherEnemies = false;

    [Header("Boss event - sounds")]
    [SerializeField] private AudioClip blackoutSfx;      // e.g. Thud / Microwave
    [SerializeField] private AudioClip flickerSfx;       // played on each flicker, e.g. Buzzer Done
    [SerializeField] private AudioClip warningSfx;       // e.g. Buzzer Done
    [SerializeField] private AudioClip puffSfx;          // e.g. DeathSoundMonster
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;

    [Header("Phase 3 - boss")]
    [SerializeField] private BossController bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private BossHealthBarUI bossHealthBar;

    [Header("Music")]
    [Tooltip("The AudioSource on GameBGMusic")]
    [SerializeField] private AudioSource levelMusic;
    [Tooltip("Boss music.wav")]
    [SerializeField] private AudioClip bossMusicClip;
    [Range(0f, 1f)] [SerializeField] private float bossMusicVolume = 0.8f;
    [SerializeField] private float musicFadeTime = 1f;

    [Header("After the boss")]
    [Tooltip("Optional: object (e.g. WinArea) that is switched on once the boss is dead. Leave it disabled in the scene")]
    [SerializeField] private GameObject enableWhenDefeated;
    public UnityEvent onBossDefeated;

    public Phase CurrentPhase { get; private set; } = Phase.Phase1;
    public bool BossEventStarted => CurrentPhase >= Phase.BossEvent;

    private Transform player;
    private AudioSource bossMusicSource;
    private AudioSource sfxSource;
    private readonly List<LittleWorm> worms = new List<LittleWorm>();
    private int littleWormHits;
    private float originalLightIntensity = 1f;
    private float levelMusicVolume = 1f;

    private void Awake()
    {
        if (globalLight != null) originalLightIntensity = globalLight.intensity;
        if (levelMusic != null) levelMusicVolume = levelMusic.volume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;

        if (bossMusicClip != null)
        {
            bossMusicSource = gameObject.AddComponent<AudioSource>();
            bossMusicSource.clip = bossMusicClip;
            bossMusicSource.loop = true;
            bossMusicSource.playOnAwake = false;
            bossMusicSource.spatialBlend = 0f;
            bossMusicSource.volume = 0f;
        }

        SetUI(blackScreen, false);
        SetUI(flickerSprite, false);
        SetUI(warningPanel, false);
        if (enableWhenDefeated != null) enableWhenDefeated.SetActive(false);
    }

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        if (starterWorm != null) starterWorm.OnKilled += HandleStarterWormKilled;
        else Debug.LogWarning("BossFightManager: no Starter Worm assigned.");
    }

    // ---------------- PHASE 1 -> 2 ----------------
    private void HandleStarterWormKilled(Vector3 position)
    {
        if (CurrentPhase != Phase.Phase1) return;
        CurrentPhase = Phase.Phase2;

        if (littleWormPrefab == null)
        {
            Debug.LogError("BossFightManager: Little Worm Prefab is not assigned.");
            return;
        }

        for (int i = 0; i < wormCount; i++)
        {
            float angle = (360f / wormCount) * i + 45f;              // diagonals for 4 worms
            Vector2 outward = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            LittleWorm w = Instantiate(littleWormPrefab, position + (Vector3)(outward * splitRadius), Quaternion.identity);
            w.Init(this, player, outward);
            worms.Add(w);
        }
    }

    // Called by every LittleWorm each time it takes damage
    public void RegisterLittleWormHit()
    {
        if (CurrentPhase != Phase.Phase2) return;

        littleWormHits++;
        if (littleWormHits >= hitsToTriggerBoss)
            StartCoroutine(BossEventRoutine());
    }

    // ---------------- PHASE 2 -> BOSS EVENT -> 3 ----------------
    private IEnumerator BossEventRoutine()
    {
        CurrentPhase = Phase.BossEvent;

        foreach (LittleWorm w in worms) if (w != null) w.Freeze();

        // 1. LIGHTS OUT
        PlaySfx(blackoutSfx);
        yield return FadeGlobalLight(globalLight != null ? globalLight.intensity : 0f, 0f, lightsOutFadeTime);
        SetExtraLights(false);

        // 2. FLICKER SPRITE x3 on a black screen
        SetUI(blackScreen, true);
        yield return new WaitForSeconds(0.4f);
        for (int i = 0; i < flickerCount; i++)
        {
            SetUI(flickerSprite, true);
            PlaySfx(flickerSfx);
            yield return new WaitForSeconds(flickerOnTime);
            SetUI(flickerSprite, false);
            yield return new WaitForSeconds(flickerOffTime);
        }

        // 3. BOSS WARNING UI
        PlaySfx(warningSfx);
        SetUI(warningPanel, true);
        CanvasGroup warningGroup = warningPanel != null ? warningPanel.GetComponent<CanvasGroup>() : null;
        float t = 0f;
        while (t < warningDuration)
        {
            t += Time.deltaTime;
            float wave = Mathf.Sin(t * warningPulseSpeed) * 0.5f + 0.5f;
            if (warningGroup != null) warningGroup.alpha = Mathf.Lerp(0.25f, 1f, wave);
            else SetUI(warningPanel, wave > 0.35f);
            yield return null;
        }
        SetUI(warningPanel, false);
        if (warningGroup != null) warningGroup.alpha = 1f;
        SetUI(blackScreen, false);

        // 4. ENEMIES DISAPPEAR with the puff sprite (lights come back so the puff can be seen)
        SetExtraLights(true);
        float target = lightsBackIntensity >= 0f ? lightsBackIntensity : originalLightIntensity;
        StartCoroutine(FadeGlobalLight(0f, target, lightsBackFadeTime));

        PlaySfx(puffSfx);
        foreach (LittleWorm w in worms) if (w != null) StartCoroutine(PuffAway(w.gameObject));
        if (clearOtherEnemies)
        {
            foreach (EnemyController2D e in FindObjectsByType<EnemyController2D>(FindObjectsSortMode.None))
                StartCoroutine(PuffAway(e.gameObject));
        }
        worms.Clear();
        yield return new WaitForSeconds(puffDuration + 0.2f);

        // 5. BOSS FIGHT STARTS - phase 3 + boss music
        StartPhase3();
    }

    private IEnumerator PuffAway(GameObject target)
    {
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        if (sr == null) { Destroy(target); yield break; }

        // Stop everything on the enemy and swap in the puff sprite
        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
        if (rb != null) { rb.linearVelocity = Vector2.zero; rb.simulated = false; }
        foreach (Collider2D c in target.GetComponents<Collider2D>()) c.enabled = false;
        foreach (MonoBehaviour m in target.GetComponents<MonoBehaviour>()) m.enabled = false;
        foreach (AudioSource a in target.GetComponentsInChildren<AudioSource>()) a.Stop();

        sr.sprite = puffSprite;
        sr.flipX = false;
        sr.color = Color.white;

        Vector3 startScale = target.transform.localScale;
        float t = 0f;
        while (t < puffDuration && target != null)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / Mathf.Max(puffDuration, 0.01f));
            target.transform.localScale = Vector3.Lerp(startScale, startScale * puffGrowScale, k);
            Color c = sr.color;
            c.a = 1f - k * k;
            sr.color = c;
            yield return null;
        }

        if (target != null) Destroy(target);
    }

    private void StartPhase3()
    {
        CurrentPhase = Phase.Phase3;

        StartCoroutine(SwapToBossMusic());

        if (bossPrefab == null)
        {
            Debug.LogError("BossFightManager: Boss Prefab is not assigned.");
            return;
        }

        Vector3 pos = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position;
        BossController boss = Instantiate(bossPrefab, pos, Quaternion.identity);
        boss.OnDefeated += HandleBossDefeated;
        boss.Begin(player, bossHealthBar);
    }

    // ---------------- DEFEAT ----------------
    private void HandleBossDefeated()
    {
        CurrentPhase = Phase.Defeated;
        StartCoroutine(EndBossMusic());

        if (enableWhenDefeated != null) enableWhenDefeated.SetActive(true);
        onBossDefeated?.Invoke();
    }

    // ---------------- MUSIC ----------------
    private IEnumerator SwapToBossMusic()
    {
        if (bossMusicSource != null)
        {
            bossMusicSource.volume = bossMusicVolume;
            bossMusicSource.Play();
        }

        if (levelMusic != null)
        {
            float start = levelMusic.volume;
            float t = 0f;
            while (t < musicFadeTime)
            {
                t += Time.deltaTime;
                levelMusic.volume = Mathf.Lerp(start, 0f, t / Mathf.Max(musicFadeTime, 0.01f));
                yield return null;
            }
            levelMusic.Pause();
            levelMusic.volume = levelMusicVolume;
        }
    }

    private IEnumerator EndBossMusic()
    {
        if (bossMusicSource != null)
        {
            float start = bossMusicSource.volume;
            float t = 0f;
            while (t < musicFadeTime)
            {
                t += Time.deltaTime;
                bossMusicSource.volume = Mathf.Lerp(start, 0f, t / Mathf.Max(musicFadeTime, 0.01f));
                yield return null;
            }
            bossMusicSource.Stop();
        }

        if (levelMusic != null) levelMusic.UnPause();   // the normal level music returns
    }

    // ---------------- HELPERS ----------------
    private IEnumerator FadeGlobalLight(float from, float to, float time)
    {
        if (globalLight == null) yield break;

        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            globalLight.intensity = Mathf.Lerp(from, to, t / Mathf.Max(time, 0.01f));
            yield return null;
        }
        globalLight.intensity = to;
    }

    private void SetExtraLights(bool on)
    {
        if (extraLightsToDisable == null) return;
        foreach (Light2D l in extraLightsToDisable) if (l != null) l.enabled = on;
    }

    private void SetUI(GameObject go, bool on)
    {
        if (go != null) go.SetActive(on);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip != null) sfxSource.PlayOneShot(clip, sfxVolume);
    }
}
