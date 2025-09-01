using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerHealth : LivingEntity
{
    private PlayerMovement playerMove;
    private Gun playerGun;
    private PlayerAnimation playerAnim;

    [Header("FX")]
    public Flash screenHitFlash;

    [Header("Health Display")]
    public UnityEngine.UI.Text healthText; // UI Text 컴포넌트 (선택사항)

    private void Awake()
    {
        playerMove = GetComponent<PlayerMovement>();
        playerGun = GetComponentInChildren<Gun>(true);
        playerAnim = GetComponentInChildren<PlayerAnimation>(true);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (playerMove) playerMove.enabled = true;
        if (playerGun) playerGun.enabled = true;
        if (playerAnim) playerAnim.isDead = false;

        UpdateHealthUI();
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (isDead) return;

        Debug.Log($"[PlayerHealth] Taking {damage} damage. Health: {health:0.1f} -> {health - damage:0.1f}");

        base.OnDamage(damage, hitPoint, hitNormal);

        UpdateHealthUI();

        if (screenHitFlash)
        {
            float ratio = maxHealth > 0f ? damage / maxHealth : 1f;
            screenHitFlash.Pulse(ratio);
        }
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {health:0.0f}/{maxHealth:0.0f}";
        }
    }

    protected override void Die()
    {
        if (isDead) return;

        Debug.Log("[PlayerHealth] Player died!");
        base.Die();

        if (playerMove) playerMove.enabled = false;
        if (playerGun) playerGun.enabled = false;
        if (playerAnim) playerAnim.PlayDeath();

        UpdateHealthUI();
    }
}