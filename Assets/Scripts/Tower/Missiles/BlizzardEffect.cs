using System.Collections.Generic;
using UnityEngine;

public class BlizzardEffect : MonoBehaviour
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

    private SphereCollider blizzardCollider;


    // =========================================================
    // CZAS
    // =========================================================

    private float durationTimer;
    private float tickTimer;

    private bool initialized = false;


    // =========================================================
    // CREATE BLIZZARD
    // =========================================================

    public void CreateBlizzard(
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
        // POZYCJA
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
        // JEŚLI Initialize() ZOSTAŁO WYWOŁANE BEZ
        // CreateBlizzard(), ZAPAMIĘTUJEMY AKTUALNĄ POZYCJĘ
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

        blizzardCollider =
            GetComponent<SphereCollider>();


        if (blizzardCollider == null)
        {
            blizzardCollider =
                gameObject.AddComponent<SphereCollider>();
        }


        blizzardCollider.isTrigger =
            true;


        // =====================================================
        // SKALA VFX + COLLIDER
        // =====================================================

        UpdateAoEScale();


        // =====================================================
        // CZAS
        // =====================================================

        durationTimer =
            skill.blizzardDuration;


        tickTimer =
            Mathf.Max(
                0.01f,
                skill.blizzardTickInterval
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
            skill.data.blizzardRadius;


        if (baseRadius <= 0f)
            return;


        // =====================================================
        // MULTIPLIER
        // =====================================================

        float scaleMultiplier =
            skill.blizzardRadius /
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
        //
        // Root jest skalowany, więc collider również zostałby
        // automatycznie powiększony.
        //
        // Dlatego ustawiamy collider na promień BAZOWY.
        //
        // Efektywnie:
        //
        // base radius × transform scale
        //
        // = aktualny radius AoE.
        //
        // =====================================================

        blizzardCollider.radius =
            baseRadius;
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
                    skill.blizzardTickInterval
                );
        }


        // =====================================================
        // CLEANUP
        // =====================================================

        CleanupEnemies();


        // =====================================================
        // KONIEC
        // =====================================================

        if (durationTimer <= 0f)
        {
            DestroyBlizzard();
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


        if (health.IsDying)
            return;


        if (enemiesInside.Contains(enemy))
            return;


        enemiesInside.Add(
            enemy
        );


        // =====================================================
        // SLOW
        // =====================================================

        ApplySlow(
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


            if (health.IsDying)
                continue;


            health.TakeDamage(
                skill.blizzardDamage
            );
        }
    }


    // =========================================================
    // APPLY SLOW
    // =========================================================

    private void ApplySlow(
        EnemyMovement enemy)
    {
        if (enemy == null)
            return;


        if (skill == null)
            return;


        float slow =
            Mathf.Clamp01(
                skill.blizzardSlowPercent
            );


        enemy.ApplySlow(skill.blizzardSlowPercent, skill.blizzardDuration);
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
            if (enemy == null)
            {
                toRemove.Add(
                    enemy
                );

                continue;
            }


            EnemyHealth health =
                enemy.GetComponent<EnemyHealth>();


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
    // DESTROY BLIZZARD
    // =========================================================

    private void DestroyBlizzard()
    {
        foreach (
            EnemyMovement enemy
            in enemiesInside)
        {
            if (enemy == null)
                continue;
        }


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


        foreach (
            EnemyMovement enemy
            in enemiesInside)
        {
            if (enemy == null)
                continue;
        }


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
            Color.cyan;


        Gizmos.DrawWireSphere(
            lockedPosition,
            skill.blizzardRadius
        );
    }
}