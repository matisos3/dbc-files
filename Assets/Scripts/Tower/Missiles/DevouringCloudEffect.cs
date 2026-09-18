using System.Collections.Generic;
using UnityEngine;

public class DevouringCloudEffect : MonoBehaviour
{
    private SkillInstance skill;


    private float remainingTime;
    private float tickTimer;


    // =========================================================
    // INITIALIZE
    // =========================================================

    public void Initialize(
        SkillInstance skillInstance)
    {
        skill =
            skillInstance;


        if (skill == null)
        {
            Destroy(gameObject);
            return;
        }


        remainingTime =
            skill.cloudDuration;


        tickTimer =
            0f;


        // =====================================================
        // SKALA CHMURY
        // =====================================================

        Vector3 currentScale =
            transform.localScale;


        transform.localScale =
            new Vector3(
                skill.cloudRadius * 2f,
                currentScale.y,
                skill.cloudRadius * 2f
            );
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (skill == null)
        {
            Destroy(gameObject);
            return;
        }


        remainingTime -=
            Time.deltaTime;


        tickTimer -=
            Time.deltaTime;


        if (tickTimer <= 0f)
        {
            DealDamage();


            tickTimer =
                skill.cloudTickInterval;
        }


        if (remainingTime <= 0f)
        {
            Destroy(gameObject);
        }
    }


    // =========================================================
    // DAMAGE
    // =========================================================

    private void DealDamage()
    {
        Collider[] colliders =
            Physics.OverlapSphere(
                transform.position,
                skill.cloudRadius
            );


        HashSet<EnemyHealth> enemies =
            new HashSet<EnemyHealth>();


        foreach (Collider collider in colliders)
        {
            EnemyHealth enemy =
                collider.GetComponentInParent<EnemyHealth>();


            if (enemy == null)
                continue;


            if (enemy.IsDying)
                continue;


            enemies.Add(
                enemy
            );
        }


        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null)
                continue;


            if (enemy.IsDying)
                continue;


            enemy.TakeDamage(
                skill.damage
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


        Gizmos.color =
            Color.green;


        Gizmos.DrawWireSphere(
            transform.position,
            skill.cloudRadius
        );
    }
}