using System.Collections.Generic;
using UnityEngine;

public class ShurikenProjectile : ProjectileBase
{
    [Header("Movement")]
    [SerializeField] private float speed = 18f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 720f;

    [Header("Hit")]
    [SerializeField] private float targetHitDistance = 0.1f;


    // =========================================================
    // TARGET
    // =========================================================

    private Transform currentTarget;


    // =========================================================
    // CHAIN
    // =========================================================

    private int chainHits = 0;


    // =========================================================
    // STATE
    // =========================================================

    private bool initialized = false;


    // =========================================================
    // TRAFIENI PRZECIWNICY
    // =========================================================

    private readonly List<GameObject> hitEnemies =
        new List<GameObject>();


    // =========================================================
    // INITIALIZE
    // =========================================================

    public override void Initialize(
        SkillInstance instance)
    {
        base.Initialize(
            instance
        );


        if (skillInstance == null)
            return;


        chainHits = 0;

        hitEnemies.Clear();

        currentTarget = null;

        initialized = true;
    }


    // =========================================================
    // PIERWSZY CEL
    // =========================================================

    public void SetFirstTarget(
        Transform target)
    {
        currentTarget =
            target;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!initialized)
            return;


        // =====================================================
        // BRAK CELU
        // =====================================================

        if (currentTarget == null)
        {
            FindNextTarget();


            if (currentTarget == null)
            {
                Destroy(gameObject);
                return;
            }
        }


        // =====================================================
        // RUCH
        // =====================================================

        MoveTowardsTarget();
    }


    // =========================================================
    // RUCH DO CELU
    // =========================================================

    private void MoveTowardsTarget()
    {
        if (currentTarget == null)
            return;


        Vector3 targetPosition =
            currentTarget.position;


        Vector3 direction =
            targetPosition -
            transform.position;


        float distance =
            direction.magnitude;


        // =====================================================
        // TRAFIENIE
        // =====================================================

        if (distance <= targetHitDistance)
        {
            Hit(
                currentTarget.gameObject
            );

            return;
        }


        // =====================================================
        // RUCH
        // =====================================================

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                targetPosition,
                speed * Time.deltaTime
            );


        // =====================================================
        // OBRÓT
        // =====================================================

        transform.Rotate(
            0f,
            rotationSpeed * Time.deltaTime,
            0f,
            Space.Self
        );
    }


    // =========================================================
    // HIT
    // =========================================================

    public override void Hit(
        GameObject target)
    {
        if (target == null)
        {
            currentTarget = null;

            FindNextTarget();

            return;
        }


        // =====================================================
        // TEN WRÓG JUŻ BYŁ TRAFIONY
        // =====================================================

        if (hitEnemies.Contains(target))
        {
            currentTarget = null;

            FindNextTarget();

            return;
        }


        // =====================================================
        // ZAPISZ TRAFIENIE
        // =====================================================

        hitEnemies.Add(
            target
        );


        // =====================================================
        // OBRAŻENIA
        // =====================================================

        EnemyHealth enemyHealth =
            target.GetComponent<EnemyHealth>();


        if (enemyHealth != null &&
            !enemyHealth.IsDying)
        {
            enemyHealth.TakeDamage(
                skillInstance.damage
            );
        }


        // =====================================================
        // LICZNIK
        // =====================================================

        chainHits++;


        // =====================================================
        // KONIEC ŁAŃCUCHA
        // =====================================================

        if (chainHits >=
            skillInstance.chainCount)
        {
            Destroy(gameObject);

            return;
        }


        // =====================================================
        // SZUKAMY NASTĘPNEGO
        // =====================================================

        currentTarget = null;

        FindNextTarget();


        // =====================================================
        // BRAK NASTĘPNEGO CELU
        // =====================================================

        if (currentTarget == null)
        {
            Destroy(gameObject);
        }
    }


    // =========================================================
    // SZUKANIE KOLEJNEGO CELU
    // =========================================================

    private void FindNextTarget()
    {
        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );


        float bestDistance =
            Mathf.Infinity;


        GameObject bestEnemy =
            null;


        // =====================================================
        // PUNKT STARTOWY SZUKANIA
        // =====================================================

        Vector3 searchPosition =
            transform.position;


        if (hitEnemies.Count > 0)
        {
            GameObject lastHit =
                hitEnemies[
                    hitEnemies.Count - 1
                ];


            if (lastHit != null)
            {
                searchPosition =
                    lastHit.transform.position;
            }
        }


        // =====================================================
        // SZUKANIE
        // =====================================================

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null)
                continue;


            // Nie trafiaj ponownie
            if (hitEnemies.Contains(enemy))
                continue;


            EnemyHealth enemyHealth =
                enemy.GetComponent<EnemyHealth>();


            if (enemyHealth == null)
                continue;


            // Nie wybieraj umierających
            if (enemyHealth.IsDying)
                continue;


            float distance =
                Vector3.Distance(
                    searchPosition,
                    enemy.transform.position
                );


            // =================================================
            // CHAIN RANGE
            // =================================================

            if (distance >
                skillInstance.chainRange)
            {
                continue;
            }


            // =================================================
            // NAJBLIŻSZY
            // =================================================

            if (distance <
                bestDistance)
            {
                bestDistance =
                    distance;

                bestEnemy =
                    enemy;
            }
        }


        // =====================================================
        // USTAWIENIE CELU
        // =====================================================

        if (bestEnemy != null)
        {
            currentTarget =
                bestEnemy.transform;
        }
        else
        {
            currentTarget =
                null;
        }
    }


    // =========================================================
    // GIZMO
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (skillInstance == null)
            return;


        Gizmos.color =
            Color.yellow;


        Gizmos.DrawWireSphere(
            transform.position,
            skillInstance.chainRange
        );
    }
}