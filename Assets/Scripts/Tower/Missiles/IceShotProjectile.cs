using System.Collections.Generic;
using UnityEngine;

public class IceShotProjectile : ProjectileBase
{
    // =========================================================
    // PRZEBICIA
    // =========================================================

    private int piercingCount = 1;

    private HashSet<EnemyHealth> hitEnemies =
        new HashSet<EnemyHealth>();


    // =========================================================
    // BAZOWA SKALA MODELU
    // =========================================================

    private Vector3 baseModelScale;

    private bool modelScaleInitialized = false;


    // =========================================================
    // MOVEMENT
    // =========================================================

    private ProjectileMovement projectileMovement;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        projectileMovement =
            GetComponent<ProjectileMovement>();
    }


    // =========================================================
    // INITIALIZE
    // =========================================================

    public override void Initialize(SkillInstance instance)
    {
        base.Initialize(instance);

        if (skillInstance == null)
            return;


        // =====================================================
        // LICZBA PRZEBIĆ
        // =====================================================

        piercingCount =
            Mathf.Max(
                1,
                skillInstance.piercingCount
            );


        hitEnemies.Clear();


        // =====================================================
        // ZAPISZ BAZOWĄ SKALĘ MODELU
        // =====================================================

        if (!modelScaleInitialized)
        {
            if (projectileMovement != null)
            {
                projectileMovement.InitializeIceShotModelScale();
            }

            modelScaleInitialized = true;
        }


        // =====================================================
        // USTAW ROZMIAR ICE SHOT
        // =====================================================

        UpdateAoEScale();
    }


    // =========================================================
    // AOE / ROZMIAR ICE SHOT
    // =========================================================

    private void UpdateAoEScale()
    {
        if (skillInstance == null ||
            skillInstance.data == null)
        {
            return;
        }


        // =====================================================
        // BAZOWY ROZMIAR Z SKILL DATA
        // =====================================================

        float baseRadius =
            skillInstance.data.iceShotRadius;


        if (baseRadius <= 0f)
            return;


        // =====================================================
        // OBLICZ MULTIPLIER
        // =====================================================

        float scaleMultiplier =
            skillInstance.iceShotRadius /
            baseRadius;


        scaleMultiplier =
            Mathf.Max(
                0f,
                scaleMultiplier
            );


        // =====================================================
        // SKALOWANIE WIZUALNEGO MODELU
        // =====================================================

        if (projectileMovement != null)
        {
            projectileMovement.SetIceShotModelScale(
                scaleMultiplier
            );
        }
        else
        {
            // Awaryjnie, jeżeli ProjectileMovement
            // nie znajduje się na prefabie.

            transform.localScale =
                Vector3.one *
                scaleMultiplier;
        }
    }


    // =========================================================
    // GET ICE SHOT RADIUS
    // =========================================================

    public float GetIceShotRadius()
    {
        if (skillInstance == null)
            return 0f;


        return skillInstance.iceShotRadius;
    }


    // =========================================================
    // HIT
    // =========================================================

    public override void Hit(GameObject enemy)
    {
        if (enemy == null ||
            skillInstance == null)
        {
            return;
        }


        // =====================================================
        // ZNAJDŹ ENEMY HEALTH
        // =====================================================

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


        // =====================================================
        // TEN WRÓG JUŻ ZOSTAŁ TRAFIONY
        // =====================================================

        if (hitEnemies.Contains(enemyHealth))
            return;


        // =====================================================
        // LIMIT PRZEBIĆ
        // =====================================================

        if (hitEnemies.Count >= piercingCount)
        {
            Destroy(
                gameObject
            );

            return;
        }


        // =====================================================
        // ZAPISZ TRAFIENIE
        // =====================================================

        hitEnemies.Add(
            enemyHealth
        );


        // =====================================================
        // OBRAŻENIA
        // =====================================================

        enemyHealth.TakeDamage(
            skillInstance.iceShotDamage
        );


        // =====================================================
        // SLOW
        // =====================================================

        EnemyMovement enemyMovement =
            enemyHealth.GetComponent<EnemyMovement>();


        if (enemyMovement == null)
        {
            enemyMovement =
                enemyHealth.GetComponentInParent<EnemyMovement>();
        }


        if (enemyMovement != null)
        {
            enemyMovement.ApplySlow(
                skillInstance.iceShotSlowPercent / 100f,
                skillInstance.iceShotSlowDuration
            );
        }


        // =====================================================
        // HIT EFFECT
        // =====================================================

        if (skillInstance.data.hitEffectPrefab != null)
        {
            Vector3 hitPosition =
                enemyHealth.transform.position;


            if (enemyHealth.hitPoint != null)
            {
                hitPosition =
                    enemyHealth.hitPoint.position;
            }


            GameObject effect =
                Instantiate(
                    skillInstance.data.hitEffectPrefab,
                    hitPosition,
                    Quaternion.identity
                );


            Destroy(
                effect,
                3f
            );
        }


        // =====================================================
        // OSTATNIE PRZEBICIE
        // =====================================================

        if (hitEnemies.Count >= piercingCount)
        {
            Destroy(
                gameObject
            );
        }
    }
}