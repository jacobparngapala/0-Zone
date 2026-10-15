using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float invincibilityDuration = 0.5f;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;

    private bool isInvincible;
    private float invincibilityTimer;

    public UnityEvent<int, int> OnHealthChanged;
    public UnityEvent OnDeath;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    private void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f) isInvincible = false;
        }
    }

    public void TakeDamage(int amount)
    {
        if (isInvincible || CurrentHealth <= 0) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);

        Debug.Log("TakeDamage called. New CurrentHealth: " + CurrentHealth
            + " | Listener count: " + (OnHealthChanged == null ? "OnHealthChanged is NULL" : OnHealthChanged.GetPersistentEventCount().ToString()));

        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0)
        {
            Die();
        }
        else
        {
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
        }
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    private void Die()
    {
        OnDeath?.Invoke();
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
} 