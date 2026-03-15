using UnityEngine;

public class PlayerCombatAnimation : MonoBehaviour
{
    [Header("Player Animation")]
    [SerializeField] private SpriteRenderer playerRenderer;
    [SerializeField] private Sprite[] idleSprites;
    [SerializeField] private float idleFrameRate = 4f;
    
    [Header("Weapon Reference")]
    [SerializeField] private WeaponFollow weaponScript;
    
    private int currentIdleFrame = 0;
    private float idleFrameTimer = 0f;

    private void Start()
    {
        // Auto-find sprite renderer if not assigned
        if (playerRenderer == null)
            playerRenderer = GetComponent<SpriteRenderer>();
            
        // Auto-find weapon script in children
        if (weaponScript == null)
            weaponScript = GetComponentInChildren<WeaponFollow>();
            
        // Safety check - log if missing references
        if (playerRenderer == null)
            Debug.LogError("No SpriteRenderer found on " + gameObject.name);
            
        if (weaponScript == null)
            Debug.LogError("No WeaponFollow script found in children of " + gameObject.name);
            
        if (idleSprites.Length == 0)
            Debug.LogWarning("No idle sprites assigned to " + gameObject.name);
    }

    private void Update()
    {
        // Check if weapon is swinging
        bool weaponIsSwinging = weaponScript != null && weaponScript.IsSwinging();
        
        if (!weaponIsSwinging)
        {
            // Only play idle animation when not swinging
            PlayIdleAnimation();
        }
        // When swinging, we don't update the player sprite
        // The weapon has its own animation during swing
    }

    private void PlayIdleAnimation()
    {
        if (idleSprites.Length == 0 || playerRenderer == null) return;
        
        idleFrameTimer += Time.deltaTime;
        if (idleFrameTimer >= 1f / idleFrameRate)
        {
            idleFrameTimer = 0f;
            currentIdleFrame = (currentIdleFrame + 1) % idleSprites.Length;
            playerRenderer.sprite = idleSprites[currentIdleFrame];
        }
    }
}