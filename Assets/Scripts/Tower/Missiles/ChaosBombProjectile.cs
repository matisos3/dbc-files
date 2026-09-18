using System.Collections.Generic;
using UnityEngine;

public class ChaosBombProjectile : ProjectileBase
{
    public override void Hit(GameObject target)
    {
        if (target == null)
            return;


        // =====================================================
        // EFEKT TRAFIENIA
        // =====================================================

        if (skillInstance.hitEffectPrefab != null)
        {
            Instantiate(
                skillInstance.hitEffectPrefab,
                target.transform.position,
                Quaternion.identity
            );
        }


        // =====================================================
        // EFEKT OBSZARU EKSPLOZJI
        // =====================================================

        if (skillInstance.explosionEffectPrefab != null)
        {
            GameObject explosion =
                Instantiate(
                    skillInstance.explosionEffectPrefab,
                    target.transform.position,
                    Quaternion.identity
                );


            // explosionRadius = PROMIEŃ
            // prefab pokazuje CAŁY obszar
            // dlatego średnica = radius * 2

            float diameter =
                skillInstance.explosionRadius * 2f;


            explosion.transform.localScale =
                Vector3.one * diameter;
        }


        // =====================================================
        // SZUKAMY WROGÓW W PROMIENIU
        // =====================================================

        Collider[] colliders =
            Physics.OverlapSphere(
                target.transform.position,
                skillInstance.explosionRadius
            );


        HashSet<EnemyHealth> hitEnemies =
            new HashSet<EnemyHealth>();


        // =====================================================
        // GŁÓWNY CEL
        // =====================================================

        EnemyHealth mainTarget =
            target.GetComponentInParent<EnemyHealth>();


        if (mainTarget != null)
        {
            hitEnemies.Add(
                mainTarget
            );
        }


        // =====================================================
        // POZOSTALI WROGOWIE
        // =====================================================

        foreach (Collider collider in colliders)
        {
            EnemyHealth enemy =
                collider.GetComponentInParent<EnemyHealth>();


            if (enemy == null)
                continue;


            hitEnemies.Add(
                enemy
            );
        }


        // =====================================================
        // OBSŁUGA WROGÓW
        // =====================================================

        foreach (EnemyHealth enemy in hitEnemies)
        {
            if (enemy == null)
                continue;


            if (enemy.IsDying)
                continue;


            HandleChaosEffect(
                enemy
            );
        }


        // =====================================================
        // USUWAMY POCISK
        // =====================================================

        Destroy(
            gameObject
        );
    }


    // =========================================================
    // CHAOS EFFECT
    // =========================================================

    private void HandleChaosEffect(
        EnemyHealth enemy)
    {
        ChaosDamageOverTime chaosDot =
            enemy.GetComponent<ChaosDamageOverTime>();


        // =====================================================
        // BRAK CHAOS DOT
        // =====================================================

        if (chaosDot == null)
        {
            chaosDot =
                enemy.gameObject.AddComponent<
                    ChaosDamageOverTime
                >();


            chaosDot.Initialize(
                enemy,
                skillInstance.dotDamage,
                skillInstance.dotDuration,
                skillInstance.dotTickInterval
            );


            return;
        }


        // =====================================================
        // CHAOS DOT JUŻ ISTNIEJE
        // =====================================================

        chaosDot.Detonate(
            skillInstance.damage
        );


        // =====================================================
        // ODŚWIEŻAMY DOT
        // =====================================================

        chaosDot.Refresh(
            skillInstance.dotDamage,
            skillInstance.dotDuration,
            skillInstance.dotTickInterval
        );
    }
}