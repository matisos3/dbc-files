using UnityEngine;

public class SearingShotProjectile : ProjectileBase
{
    private bool hitProcessed = false;


    // =========================================================
    // HIT
    // =========================================================

    public override void Hit(
        GameObject enemy)
    {
        if (hitProcessed)
            return;


        if (enemy == null)
            return;


        if (skillInstance == null)
            return;


        EnemyHealth enemyHealth =
            enemy.GetComponent<EnemyHealth>();


        if (enemyHealth == null)
        {
            enemyHealth =
                enemy.GetComponentInParent<EnemyHealth>();
        }


        if (enemyHealth == null)
            return;


        if (enemyHealth.IsDying)
            return;


        hitProcessed =
            true;


        // =====================================================
        // OBRAŻENIA POCZĄTKOWE
        // =====================================================

        enemyHealth.TakeDamage(
            skillInstance.searingShotDamage
        );


        // =====================================================
        // JEŚLI WRÓG UMARŁ OD PIERWSZEGO HIT
        // =====================================================

        if (enemyHealth.IsDying)
        {
            return;
        }


        // =====================================================
        // BURN EFFECT
        // =====================================================

        BurnEffect burn =
            enemyHealth.GetComponent<BurnEffect>();


        if (burn == null)
        {
            burn =
                enemyHealth.gameObject
                    .AddComponent<BurnEffect>();
        }


        if (burn == null)
        {
            return;
        }


        // =====================================================
        // SEARING SHOT BURN
        // =====================================================

        burn.ApplySearingShotBurn(
            skillInstance.searingShotBurnDamage,
            skillInstance.searingShotBurnDuration,
            skillInstance.searingShotBurnTickInterval,
            skillInstance.searingShotExplosionDamage,
            skillInstance.searingShotExplosionRadius,
            skillInstance.data != null
                ? skillInstance.data.explosionEffectPrefab
                : null
        );
    }
}