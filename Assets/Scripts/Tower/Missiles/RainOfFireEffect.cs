using System.Collections.Generic;
using UnityEngine;

public class RainOfFireEffect : MonoBehaviour
{
    // =========================================================
    // SKILL
    // =========================================================

    private SkillInstance skill;


    // =========================================================
    // POZYCJA
    // =========================================================

    private Vector3 lockedPosition;


    // =========================================================
    // BAZOWA SKALA
    // =========================================================

    private Vector3 baseScale;
    private bool baseScaleInitialized = false;


    // =========================================================
    // PRZECIWNICY
    // =========================================================

    private HashSet<EnemyMovement> enemiesInside =
        new HashSet<EnemyMovement>();


    // =========================================================
    // COLLIDER
    // =========================================================

    private SphereCollider rainOfFireCollider;


    // =========================================================
    // CZAS
    // =========================================================

    private float durationTimer;
    private float tickTimer;

    private bool initialized = false;


    // =========================================================
    // CREATE RAIN OF FIRE
    // =========================================================

    public void CreateRainOfFire(
        SkillInstance skillInstance,
        Vector3 position)
    {
        if (skillInstance == null)
        {
            Destroy(gameObject);
            return;
        }


        if (skillInstance.data == null)
        {
            Destroy(gameObject);
            return;
        }


        // =====================================================
        // ZAPAMIĘTUJEMY POZYCJĘ
        // =====================================================

        lockedPosition =
            position;

        transform.position =
            lockedPosition;


        // =====================================================
        // INITIALIZE
        // =====================================================

        Initialize(
            skillInstance
        );
    }


    // =========================================================
    // INITIALIZE
    // =========================================================

    public void Initialize(
        SkillInstance skillInstance)
    {
        if (skillInstance == null)
        {
            Destroy(gameObject);
            return;
        }


        if (skillInstance.data == null)
        {
            Destroy(gameObject);
            return;
        }


        skill =
            skillInstance;


        // =====================================================
        // ZAPIS POZYCJI
        // =====================================================

        if (!initialized)
        {
            lockedPosition =
                transform.position;
        }


        transform.position =
            lockedPosition;


        // =====================================================
        // BAZOWA SKALA PREFABU
        // =====================================================

        if (!baseScaleInitialized)
        {
            baseScale =
                transform.localScale;

            baseScaleInitialized =
                true;
        }


        // =====================================================
        // COLLIDER
        // =====================================================

        rainOfFireCollider =
            GetComponent<SphereCollider>();


        if (rainOfFireCollider == null)
        {
            rainOfFireCollider =
                gameObject.AddComponent<SphereCollider>();
        }


        rainOfFireCollider.isTrigger =
            true;


        // =====================================================
        // SKALA VFX + COLLIDER
        // =====================================================

        UpdateAoEScale();


        // =====================================================
        // CZAS
        // =====================================================

        durationTimer =
            Mathf.Max(
                0.01f,
                skill.rainOfFireDuration
            );


        tickTimer =
            Mathf.Max(
                0.01f,
                skill.rainOfFireTickInterval
            );


        initialized =
            true;
    }


    // =========================================================
    // AOE SCALE
    // =========================================================

    private void UpdateAoEScale()
    {
        if (!baseScaleInitialized)
            return;


        if (skill == null ||
            skill.data == null)
        {
            return;
        }


        float baseRadius =
            skill.data.rainOfFireRadius;


        if (baseRadius <= 0f)
            return;


        // =====================================================
        // MULTIPLIER
        // =====================================================

        float scaleMultiplier =
            skill.rainOfFireRadius /
            baseRadius;


        scaleMultiplier =
            Mathf.Max(
                0f,
                scaleMultiplier
            );


        // =====================================================
        // SKALA VFX
        // =====================================================

        transform.localScale =
            baseScale *
            scaleMultiplier;


        // =====================================================
        // COLLIDER
        // =====================================================

        if (rainOfFireCollider != null)
        {
            rainOfFireCollider.radius =
                baseRadius;
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!initialized)
            return;


        // =====================================================
        // BLOKADA POZYCJI
        // =====================================================

        transform.position =
            lockedPosition;


        // =====================================================
        // CZAS DZIAŁANIA
        // =====================================================

        durationTimer -=
            Time.deltaTime;


        // =====================================================
        // TICK DAMAGE
        // =====================================================

        tickTimer -=
            Time.deltaTime;


        if (tickTimer <= 0f)
        {
            DamageEnemiesInside();


            tickTimer +=
                Mathf.Max(
                    0.01f,
                    skill.rainOfFireTickInterval
                );
        }


        // =====================================================
        // CLEANUP MARTWYCH PRZECIWNIKÓW
        // =====================================================

        CleanupEnemies();


        // =====================================================
        // KONIEC CZASU
        // =====================================================

        if (durationTimer <= 0f)
        {
            DestroyRainOfFire();
        }
    }


    // =========================================================
    // TRIGGER ENTER
    // =========================================================

    private void OnTriggerEnter(
        Collider other)
    {
        if (!initialized)
            return;


        EnemyMovement enemy =
            other.GetComponentInParent<EnemyMovement>();


        if (enemy == null)
            return;


        EnemyHealth health =
            enemy.GetComponent<EnemyHealth>();


        if (health == null)
            return;


        // Martwy przeciwnik nie jest dodawany.
        if (health.IsDying)
            return;


        if (enemiesInside.Contains(enemy))
            return;


        enemiesInside.Add(
            enemy
        );
    }


    // =========================================================
    // TRIGGER EXIT
    // =========================================================

    private void OnTriggerExit(
        Collider other)
    {
        if (!initialized)
            return;


        EnemyMovement enemy =
            other.GetComponentInParent<EnemyMovement>();


        if (enemy == null)
            return;


        if (!enemiesInside.Contains(enemy))
            return;


        enemiesInside.Remove(
            enemy
        );
    }


    // =========================================================
    // DAMAGE
    // =========================================================

    private void DamageEnemiesInside()
    {
        if (skill == null)
            return;


        if (enemiesInside.Count == 0)
            return;


        foreach (
            EnemyMovement enemy
            in enemiesInside)
        {
            if (enemy == null)
                continue;


            EnemyHealth health =
                enemy.GetComponent<EnemyHealth>();


            if (health == null)
                continue;


            // =================================================
            // MARTWY WRÓG - NIC NIE ROBIMY
            // =================================================

            if (health.IsDying)
                continue;


            // =================================================
            // RAIN OF FIRE DAMAGE
            // =================================================

            health.TakeDamage(
                skill.rainOfFireDamage
            );


            // =================================================
            // BURN
            // =================================================

            BurnEffect burn =
                health.GetComponent<BurnEffect>();


            if (burn == null)
            {
                burn =
                    health.gameObject.AddComponent<BurnEffect>();
            }


            burn.ApplyBurn(
                skill.rainOfFireBurnDamage,
                skill.rainOfFireBurnDuration,
                skill.rainOfFireBurnTickInterval
            );
        }
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void CleanupEnemies()
    {
        if (enemiesInside.Count == 0)
            return;


        List<EnemyMovement> toRemove =
            new List<EnemyMovement>();


        foreach (
            EnemyMovement enemy
            in enemiesInside)
        {
            // Obiekt przeciwnika został zniszczony.
            if (enemy == null)
            {
                toRemove.Add(
                    enemy
                );

                continue;
            }


            EnemyHealth health =
                enemy.GetComponent<EnemyHealth>();


            // Przeciwnik został zabity.
            if (health == null ||
                health.IsDying)
            {
                toRemove.Add(
                    enemy
                );
            }
        }


        foreach (
            EnemyMovement enemy
            in toRemove)
        {
            enemiesInside.Remove(
                enemy
            );
        }
    }


    // =========================================================
    // DESTROY RAIN OF FIRE
    // =========================================================

    private void DestroyRainOfFire()
    {
        enemiesInside.Clear();


        Destroy(
            gameObject
        );
    }


    // =========================================================
    // ON DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (enemiesInside == null)
            return;


        enemiesInside.Clear();
    }


    // =========================================================
    // GIZMO
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (skill == null)
            return;


        Gizmos.color =
            Color.red;


        Gizmos.DrawWireSphere(
            lockedPosition,
            skill.rainOfFireRadius
        );
    }
}