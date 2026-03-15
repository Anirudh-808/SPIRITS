using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Bar")]
    [SerializeField] private Slider healthSlider;

    //[SerializeField] private string nextScene;
    
    public int nextScene = 2;

    private void Start()
    {
        // Prefab slider is min:0 max:10 — start full
        healthSlider.value = healthSlider.maxValue;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            healthSlider.value -= 1;

            if (healthSlider.value <= 0)
            {
                Debug.Log("Player is dead.");
                Die();
            }
        }
        if (other.gameObject.CompareTag("boss"))
        {
            healthSlider.value -= 2;

            if (healthSlider.value <= 0)
            {
                Debug.Log("Player is dead.");
                Die();
            }
        }
    }

    void Die()
    {
        SceneManager.LoadScene(nextScene);
    }
}
