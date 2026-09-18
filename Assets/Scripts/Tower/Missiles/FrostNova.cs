using System.Collections.Generic;
using UnityEngine;

public class FrostNova : MonoBehaviour
{
    private SkillInstance skill;

    private Vector3 baseScale;
    private bool baseScaleInitialized = false;


    // =========================================================
    // INITIALIZE
    // =========================================================

    public void Initialize(
        SkillInstance instance)
    {
        skill = instance;

        if (skill == null ||
            skill.data == null)
        {
            Destroy(gameObject);
            return;
        }


        // Zapamiętujemy oryginalną skalę prefabu.
        baseScale =
            transform.localScale;

        baseScaleInitialized = true;


        // Dopasowanie VFX do aktualnego AoE.
        UpdateVisualScale();


        Cast();
        SetupAutoDestroy();
    }


    // =========================================================
    // SKALA VFX
    // =========================================================

    private void UpdateVisualScale()
    {
        if (!baseScaleInitialized)
            return;

        if (skill == null ||
            skill.data == null)
            return;


        float baseRadius =
            skill.data.frostNovaRadius;


        if (baseRadius <= 0f)
            return;


        float scaleMultiplier =
            skill.frostNovaRadius /
            baseRadius;


        scaleMultiplier =
            Mathf.Max(
                0f,
                scaleMultiplier
            );


        transform.localScale =
            baseScale *
            scaleMultiplier;
    }


    // =========================================================
    // CAST
    // =========================================================

    private void Cast()
    {
        Collider[] hits =
            Physics.OverlapSphere(
                transform.position,
                skill.frostNovaRadius
            );


        // Zabezpieczenie przed zadaniem obrażeń
        // temu samemu przeciwnikowi kilka razy.
        HashSet<EnemyHealth> affectedEnemies =
            new HashSet<EnemyHealth>();


        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;


            EnemyHealth enemyHealth =
                hit.GetComponent<EnemyHealth>();


            if (enemyHealth == null)
            {
                enemyHealth =
                    hit.GetComponentInParent<EnemyHealth>();
            }


            if (enemyHealth == null)
                continue;


            if (enemyHealth.IsDying)
                continue;


            if (!affectedEnemies.Add(enemyHealth))
                continue;


            // =================================================
            // DAMAGE
            // =================================================

            enemyHealth.TakeDamage(
                skill.frostNovaDamage
            );


            // =================================================
            // FREEZE
            // =================================================

            EnemyMovement enemyMovement =
                enemyHealth.GetComponent<EnemyMovement>();


            if (enemyMovement == null)
            {
                enemyMovement =
                    enemyHealth.GetComponentInParent<EnemyMovement>();
            }


            if (enemyMovement != null)
            {
                enemyMovement.ApplyFrozen(
                    skill.frostNovaFreezeDuration
                );
            }
        }
    }


    // =========================================================
    // AUTO DESTROY
    // =========================================================

    private void SetupAutoDestroy()
    {
        ParticleSystem[] particleSystems =
            GetComponentsInChildren<ParticleSystem>();


        if (particleSystems.Length == 0)
            return;


        float longestLifetime = 0f;


        foreach (ParticleSystem particleSystem
                 in particleSystems)
        {
            if (particleSystem == null)
                continue;


            ParticleSystem.MainModule main =
                particleSystem.main;


            float duration =
                main.duration;


            float startLifetime = 0f;


            if (main.startLifetime.mode ==
                ParticleSystemCurveMode.Constant)
            {
                startLifetime =
                    main.startLifetime.constant;
            }
            else if (
                main.startLifetime.mode ==
                ParticleSystemCurveMode.TwoConstants)
            {
                startLifetime =
                    main.startLifetime.constantMax;
            }


            float lifetime =
                duration +
                startLifetime;


            if (lifetime > longestLifetime)
            {
                longestLifetime =
                    lifetime;
            }
        }


        if (longestLifetime > 0f)
        {
            Destroy(
                gameObject,
                longestLifetime + 0.25f
            );
        }
    }


    // =========================================================
    // GIZMO
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (skill == null)
            return;


        Gizmos.DrawWireSphere(
            transform.position,
            skill.frostNovaRadius
        );
    }
}