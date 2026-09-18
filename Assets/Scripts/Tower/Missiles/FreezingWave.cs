using System.Collections.Generic;
using UnityEngine;

public class FreezingWave : MonoBehaviour
{
    // =========================================================
    // STAN
    // =========================================================

    private SkillInstance skill;

    private Vector3 direction;

    private Vector3 startPosition;

    private float traveledDistance;

    private bool initialized = false;


    // =========================================================
    // TRAFIENI PRZECIWNICY
    // =========================================================

    private HashSet<EnemyHealth> hitEnemies =
        new HashSet<EnemyHealth>();


    // =========================================================
    // PIERCING
    // =========================================================

    // Liczba pozostałych możliwych trafień.
    private int remainingPiercing;


    // =========================================================
    // INITIALIZE
    // =========================================================

    public void Initialize(
        SkillInstance instance,
        Vector3 moveDirection)
    {
        skill = instance;

        if (skill == null)
        {
            return;
        }


        // =====================================================
        // PIERCING
        // =====================================================

        remainingPiercing =
            Mathf.Max(
                1,
                skill.piercingCount
            );

        hitEnemies.Clear();


        // =====================================================
        // SKALA
        // =====================================================

        float radius =
            skill.freezingWaveRadius;

        transform.localScale =
            Vector3.one * radius;


        // =====================================================
        // KIERUNEK
        // =====================================================

        direction =
            moveDirection;

        direction.y = 0f;

        if (direction == Vector3.zero)
        {
            direction =
                transform.forward;

            direction.y = 0f;
        }

        direction.Normalize();


        // =====================================================
        // POZYCJA STARTOWA
        // =====================================================

        startPosition =
            transform.position;

        traveledDistance = 0f;


        // =====================================================
        // ROTACJA
        // =====================================================

        transform.rotation =
            Quaternion.LookRotation(
                direction
            );


        initialized = true;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!initialized)
            return;


        if (skill == null)
        {
            Destroy(gameObject);
            return;
        }


        // =====================================================
        // RUCH
        // =====================================================

        float movement =
            skill.freezingWaveSpeed *
            Time.deltaTime;


        transform.position +=
            direction *
            movement;


        traveledDistance +=
            movement;


        // =====================================================
        // SPRAWDZANIE TRAFIEŃ
        // =====================================================

        CheckForEnemies();


        // =====================================================
        // KONIEC FALI
        // =====================================================

        if (traveledDistance >=
            skill.freezingWaveDistance)
        {
            Destroy(
                gameObject
            );
        }
    }


    // =========================================================
    // CHECK FOR ENEMIES
    // =========================================================

    private void CheckForEnemies()
    {
        if (remainingPiercing <= 0)
            return;


        Collider[] colliders =
            Physics.OverlapSphere(
                transform.position,
                skill.freezingWaveRadius
            );


        foreach (
            Collider collider
            in colliders)
        {
            if (collider == null)
                continue;


            // =================================================
            // ENEMY HEALTH
            // =================================================

            EnemyHealth enemyHealth =
                collider.GetComponent<EnemyHealth>();


            if (enemyHealth == null)
            {
                enemyHealth =
                    collider.GetComponentInParent<EnemyHealth>();
            }


            if (enemyHealth == null)
                continue;


            // =================================================
            // NIE TRAFIAMY TEGO SAMEGO WROGA PONOWNIE
            // =================================================

            if (hitEnemies.Contains(
                enemyHealth))
            {
                continue;
            }


            // =================================================
            // MARTWY / UMIERAJĄCY
            // =================================================

            if (enemyHealth.IsDying)
                continue;


            // =================================================
            // BRAK PIERCINGU
            // =================================================

            if (remainingPiercing <= 0)
            {
                return;
            }


            // =================================================
            // ZAPISUJEMY TRAFIENIE
            // =================================================

            hitEnemies.Add(
                enemyHealth
            );


            // =================================================
            // ZUŻYCIE PIERCINGU
            // =================================================

            remainingPiercing--;


            // =================================================
            // DAMAGE
            // =================================================

            enemyHealth.TakeDamage(
                skill.freezingWaveDamage
            );


            // =================================================
            // FROZEN
            // =================================================

            EnemyMovement movement =
                enemyHealth.GetComponent<EnemyMovement>();


            if (movement == null)
            {
                movement =
                    enemyHealth.GetComponentInParent<EnemyMovement>();
            }


            if (movement != null)
            {
                movement.ApplyFrozen(
                    skill.freezingWaveFreezeDuration
                );
            }


            // =================================================
            // KONIEC PIERCINGU
            // =================================================

            if (remainingPiercing <= 0)
            {
                Destroy(
                    gameObject
                );

                return;
            }
        }
    }


    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (skill == null)
            return;


        Gizmos.DrawWireSphere(
            transform.position,
            skill.freezingWaveRadius
        );
    }
}