using UnityEngine;

public class HealthCollectible : MonoBehaviour 
{

    [SerializeField] private int healAmount = 20;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("HealthCollectible Collected");

            Health playerHealth = other.GetComponent<Health>();

            if (playerHealth != null)
            {
                playerHealth.Heal(healAmount);
            }
            else
            {
                Debug.Log("HealthCollectible collected, but no Health component found on player.");
            }

            Destroy(gameObject);
        }
    }
}
