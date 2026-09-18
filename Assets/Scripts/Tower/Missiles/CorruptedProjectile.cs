using System.Collections.Generic;
using UnityEngine;

public class CorruptedProjectile : ProjectileBase
{
    [Header("Ruch między celami")]
    [SerializeField] private float nextTargetDelay = 0.02f;

    private HashSet<EnemyHealth> hitEnemies =
        new HashSet<EnemyHealth>();

    private ProjectileMovement movement;

    private int targetsHit = 0;
    private bool isSearchingNextTarget = false;


    // =========================================================
    // INITIALIZE
    // =========================================================

    public override void Initialize(
        SkillInstance instance)
    {
        base.Initialize(instance);

        hitEnemies.Clear();

        targetsHit = 0;
        isSearchingNextTarget = false;

        movement =
            GetComponent<ProjectileMovement>();
    }


    // =========================================================
    // HIT
    // =========================================================

    public override void Hit(
        GameObject target)
    {
        if (target == null)
            return;


        if (skillInstance == null ||
            skillInstance.data == null)
        {
            Destroy(gameObject);

            return;
        }


        EnemyHealth enemy =
            target.GetComponentInParent<EnemyHealth>();


        if (enemy == null)
            return;


        if (enemy.IsDying)
            return;


        // =====================================================
        // TEN WRÓG JUŻ BYŁ TRAFIONY
        // =====================================================

        if (hitEnemies.Contains(enemy))
        {
            FindNextTarget(
                enemy.transform.position
            );

            return;
        }


        // =====================================================
        // ZAPISUJEMY CEL
        // =====================================================

        hitEnemies.Add(enemy);

        targetsHit++;


        // =====================================================
        // NAKŁADAMY DEBUFF
        // =====================================================

        ApplyCorruptedDebuff(
            enemy
        );


        // =====================================================
        // EFEKT WIZUALNY NA PRZECIWNIKU
        // =====================================================

        ApplyCorruptedVisualEffect(
            enemy
        );

        // =====================================================
        // LIMIT CELÓW
        // =====================================================

        if (targetsHit >=
            skillInstance.corruptedMaxTargets)
        {
            Destroy(gameObject);
            return;
        }


        // =====================================================
        // SZUKAMY KOLEJNEGO
        // =====================================================

        FindNextTarget(
            enemy.transform.position
        );
    }


    // =========================================================
    // CORRUPTED DEBUFF
    // =========================================================

    private void ApplyCorruptedDebuff(
        EnemyHealth enemy)
    {
        if (enemy == null)
            return;


        CorruptedDebuff debuff =
            enemy.GetComponent<CorruptedDebuff>();


        // =====================================================
        // PIERWSZY STACK
        // =====================================================

        if (debuff == null)
        {
            debuff =
                enemy.gameObject.AddComponent<
                    CorruptedDebuff
                >();


            debuff.Initialize(
                skillInstance
            );
        }
        else
        {
            // =================================================
            // KOLEJNY STACK
            // =================================================

            debuff.Refresh(
                skillInstance
            );
        }
    }


    // =========================================================
    // EFEKT WIZUALNY SPACZENIA
    // =========================================================

    private void ApplyCorruptedVisualEffect(
        EnemyHealth enemy)
    {
        if (enemy == null)
            return;


        if (skillInstance == null ||
            skillInstance.data == null)
            return;


        GameObject effectPrefab =
            skillInstance.hitEffectPrefab;


        if (effectPrefab == null)
        {
            return;
        }


        // =====================================================
        // SPRAWDZAMY CZY WRÓG JUŻ MA TEN EFEKT
        // =====================================================

        CorruptedVisualEffect existingEffect =
            enemy.GetComponentInChildren<
                CorruptedVisualEffect
            >();


        if (existingEffect != null)
        {
            existingEffect.Refresh(
                skillInstance.corruptedDuration
            );

            return;
        }


        // =====================================================
        // TWORZYMY EFEKT
        // =====================================================

        GameObject effectObject =
            Instantiate(
                effectPrefab,
                enemy.transform.position,
                Quaternion.identity,
                enemy.transform
            );


        // =====================================================
        // KOMPONENT EFEKTU
        // =====================================================

        CorruptedVisualEffect visualEffect =
            effectObject.GetComponent<
                CorruptedVisualEffect
            >();


        if (visualEffect == null)
        {
            visualEffect =
                effectObject.AddComponent<
                    CorruptedVisualEffect
                >();
        }


        visualEffect.Initialize(
            skillInstance.corruptedDuration
        );
    }


    // =========================================================
    // SZUKANIE KOLEJNEGO CELU
    // =========================================================

    private void FindNextTarget(
        Vector3 currentPosition)
    {
        if (isSearchingNextTarget)
            return;


        isSearchingNextTarget = true;


        // =====================================================
        // ZASIĘG CHAIN
        // =====================================================

        Collider[] colliders =
            Physics.OverlapSphere(
                currentPosition,
                skillInstance.corruptedChainRange
            );


        EnemyHealth closestEnemy =
            null;


        float closestDistance =
            Mathf.Infinity;


        foreach (Collider collider in colliders)
        {
            if (collider == null)
                continue;


            EnemyHealth enemy =
                collider.GetComponentInParent<EnemyHealth>();


            if (enemy == null)
                continue;


            if (enemy.IsDying)
                continue;


            // =================================================
            // NIE TRAFIAJ TEGO SAMEGO WROGA
            // =================================================

            if (hitEnemies.Contains(enemy))
                continue;


            float distance =
                Vector3.Distance(
                    currentPosition,
                    enemy.transform.position
                );


            if (distance <
                closestDistance)
            {
                closestDistance =
                    distance;

                closestEnemy =
                    enemy;
            }
        }


        // =====================================================
        // BRAK KOLEJNEGO CELU
        // =====================================================

        if (closestEnemy == null)
        {
            isSearchingNextTarget = false;

            Destroy(gameObject);

            return;
        }


        // =====================================================
        // POCISK LECI DO KOLEJNEGO
        // =====================================================

        if (movement == null)
        {
            movement =
                GetComponent<ProjectileMovement>();
        }


        if (movement == null)
        {
            Destroy(gameObject);

            return;
        }


        movement.Initialize(
            closestEnemy.transform
        );


        // =====================================================
        // RESET BLOKADY
        // =====================================================

        Invoke(
            nameof(ResetSearchState),
            nextTargetDelay
        );
    }


    // =========================================================
    // RESET SEARCH
    // =========================================================

    private void ResetSearchState()
    {
        isSearchingNextTarget = false;
    }


    // =========================================================
    // GIZMO
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (skillInstance == null ||
            skillInstance.data == null)
            return;


        Gizmos.color =
            Color.magenta;


        Gizmos.DrawWireSphere(
            transform.position,
            skillInstance.corruptedChainRange
        );
    }
}