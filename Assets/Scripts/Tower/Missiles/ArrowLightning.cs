using UnityEngine;

public class ArrowLightning : ProjectileBase
{
    [Header("Stun Effect")]
    [SerializeField] private GameObject stunEffectPrefab;

    [Tooltip("Wysokość, na której pojawi się efekt nad przeciwnikiem.")]
    [SerializeField] private float stunEffectYOffset = 1.5f;

    public override void Hit(GameObject enemy)
    {
        if (enemy == null)
            return;

        EnemyHealth enemyHealth =
            enemy.GetComponentInParent<EnemyHealth>();

        if (enemyHealth == null)
            return;

        if (enemyHealth.IsDying)
            return;

        // =========================================================
        // PODSTAWOWE OBRAŻENIA
        // =========================================================

        float baseDamage =
            skillInstance.damage;

        enemyHealth.TakeDamage(
            baseDamage
        );

        // Jeżeli podstawowe obrażenia zabiły przeciwnika,
        // nie wykonujemy już rzutu na stun.
        if (enemyHealth.IsDying)
            return;

        // =========================================================
        // SZANSA NA PORAŻENIE
        // =========================================================

        float stunChance =
            Mathf.Clamp01(
                skillInstance.lightningStunChance
            );

        float roll =
            Random.value;

        if (roll > stunChance)
        {
            return;
        }

        // =========================================================
        // BONUSOWE OBRAŻENIA
        // =========================================================

        float bonusDamage =
            Mathf.Max(
                0f,
                skillInstance.lightningBonusDamage
            );

        if (bonusDamage > 0f)
        {
            enemyHealth.TakeDamage(
                bonusDamage
            );
        }

        // Jeżeli bonusowe obrażenia zabiły przeciwnika,
        // nie uruchamiamy efektu ani stuna.
        if (enemyHealth.IsDying)
            return;

        // =========================================================
        // OGŁUSZENIE
        // =========================================================

        EnemyMovement enemyMovement =
            enemy.GetComponentInParent<EnemyMovement>();

        if (enemyMovement != null)
        {
            enemyMovement.ApplyStun(
                skillInstance.lightningStunDuration,
                stunEffectPrefab,
                stunEffectYOffset
            );
        }
    }
}