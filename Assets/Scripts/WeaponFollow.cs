using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponFollow : MonoBehaviour
{
    [Header("Weapon Settings")]
    public Vector2 weaponOffset = new Vector2(0.5f, -0.2f);

    [Header("Swing Settings")]
    public float swingAngle = 90f;
    public float swingSpeed = 10f;
    public int damage = 10;

    [Header("Sprites")]
    public Sprite[] idleFrames;
    public Sprite[] swingFrames;
    public float frameRate = 8f;

    private SpriteRenderer weaponRenderer;
    private Vector2 lastMoveDirection = Vector2.right;
    private bool isSwinging = false;
    private bool hasDealtDamage = false; // ensures damage is dealt once per swing
    private float currentAngle = 0f;
    private float targetAngle = 0f;
    private int swingDirection = 1;
    private float frameTimer = 0f;
    private int currentFrame = 0;
    private float swingTimer = 0f;
    public float swingDuration = 0.4f;

    void Start()
    {
        weaponRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public bool IsSwinging()
    {
        return isSwinging;
    }

    void Update()
    {
        Vector2 input = new Vector2(
            Keyboard.current.dKey.isPressed ? 1 : Keyboard.current.aKey.isPressed ? -1 : 0,
            Keyboard.current.wKey.isPressed ? 1 : Keyboard.current.sKey.isPressed ? -1 : 0
        );

        if (input != Vector2.zero)
            lastMoveDirection = input.normalized;

        if (Mouse.current.leftButton.wasPressedThisFrame && !isSwinging)
        {
            isSwinging     = true;
            hasDealtDamage = false;
            swingTimer     = swingDuration;
            currentFrame   = 0;
            frameTimer     = 0f;
            targetAngle    = swingAngle * swingDirection;
            swingDirection *= -1;
        }

        if (isSwinging)
        {
            swingTimer -= Time.deltaTime;

            // Deal damage once at the midpoint of the swing
            if (!hasDealtDamage && swingTimer <= swingDuration / 2f)
            {
                HitEnemies();
                hasDealtDamage = true;
            }

            if (swingTimer <= 0f)
            {
                isSwinging   = false;
                currentFrame = 0;
                currentAngle = 0f;
                transform.localRotation = Quaternion.identity;
            }
        }

        AnimateSprite();
        HandleSwing();
        UpdateWeaponPosition();
    }

    void HitEnemies()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                enemyHealth eh = hit.GetComponent<enemyHealth>();
                if (eh != null)
                    eh.TakeDamage(damage);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }

    void AnimateSprite()
    {
        if (weaponRenderer == null) return;

        Sprite[] frames = isSwinging ? swingFrames : idleFrames;
        if (frames == null || frames.Length == 0) return;

        frameTimer += Time.deltaTime;
        if (frameTimer >= 1f / frameRate)
        {
            frameTimer = 0f;
            currentFrame = (currentFrame + 1) % frames.Length;
        }

        weaponRenderer.sprite = frames[currentFrame];
    }

    void HandleSwing()
    {
        if (!isSwinging) return;

        currentAngle = Mathf.MoveTowards(
            currentAngle, targetAngle, swingSpeed * 100f * Time.deltaTime
        );
        transform.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
    }

    void UpdateWeaponPosition()
    {
        bool facingLeft = lastMoveDirection.x < 0;
        transform.localScale = new Vector3(facingLeft ? -1f : 1f, 1f, 1f);
        transform.localPosition = new Vector2(weaponOffset.x, weaponOffset.y);
    }
}
