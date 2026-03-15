using UnityEngine;

public class enemyHealth : MonoBehaviour
{
    public int EnemyHealth = 100;

    void Start()
    {
        EnemyHealth = 100;
    }

    public void TakeDamage(int damage)
    {
        if (EnemyHealth <= 0) return;

        EnemyHealth -= damage;

        if (EnemyHealth <= 0)
            Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
