using System.Collections.Generic;
using UnityEngine;

public class ArrowFire : ProjectileBase
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
        // ZADAJEMY OBRAŻENIA
        // =====================================================

        foreach (EnemyHealth enemy in hitEnemies)
        {
            if (enemy == null)
                continue;


            if (enemy.IsDying)
                continue;


            enemy.TakeDamage(
                skillInstance.damage
            );
        }


        // =====================================================
        // USUWAMY POCISK
        // =====================================================

        Destroy(
            gameObject
        );
    }
}