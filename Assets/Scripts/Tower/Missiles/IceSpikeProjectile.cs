using System.Collections.Generic;
using UnityEngine;

public class IceSpikeProjectile : ProjectileBase
{
    private Vector3 direction;

    private float speed;
    private float lifetime;
    private float damage;

    private bool initializedProjectile = false;


    // =========================================================
    // PIERCING
    // =========================================================

    // Liczba pozostałych przebić dla TEGO konkretnego odłamka.
    private int remainingPiercing;

    // Lista przeciwników trafionych przez TEN odłamek.
    // Dzięki temu ten sam przeciwnik nie zostanie trafiony
    // ponownie przez ten sam odłamek.
    private readonly HashSet<GameObject> hitEnemies =
        new HashSet<GameObject>();


    // =========================================================
    // INITIALIZE
    // =========================================================

    public void Initialize(
        SkillInstance skill,
        Vector3 newDirection)
    {
        base.Initialize(
            skill
        );


        if (skill == null)
        {
            Destroy(
                gameObject
            );

            return;
        }


        if (skill.data == null)
        {
            Destroy(
                gameObject
            );

            return;
        }


        // =====================================================
        // KIERUNEK
        // =====================================================

        direction =
            newDirection.normalized;


        // =====================================================
        // PARAMETRY
        // =====================================================

        speed =
            Mathf.Max(
                0.1f,
                skill.iceSpikeSpeed
            );


        lifetime =
            Mathf.Max(
                0.1f,
                skill.iceSpikeLifetime
            );


        damage =
            skill.damage *
            skill.iceSpikeDamageMultiplier;


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
        // GOTOWY
        // =====================================================

        initializedProjectile =
            true;


        // =====================================================
        // ROTACJA
        // =====================================================

        if (direction.sqrMagnitude >
            0.0001f)
        {
            transform.rotation =
                Quaternion.LookRotation(
                    direction
                );
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!initializedProjectile)
            return;


        // =====================================================
        // RUCH
        // =====================================================

        transform.position +=
            direction *
            speed *
            Time.deltaTime;


        // =====================================================
        // CZAS ŻYCIA
        // =====================================================

        lifetime -=
            Time.deltaTime;


        if (lifetime <= 0f)
        {
            Destroy(
                gameObject
            );
        }
    }


    // =========================================================
    // HIT
    // =========================================================

    public override void Hit(
        GameObject enemy)
    {
        if (!initializedProjectile)
            return;


        if (enemy == null)
            return;


        // =====================================================
        // ENEMY HEALTH
        // =====================================================

        EnemyHealth health =
            enemy.GetComponent<EnemyHealth>();


        if (health == null)
        {
            health =
                enemy.GetComponentInParent<EnemyHealth>();
        }


        if (health == null)
            return;


        if (health.IsDying)
            return;


        // =====================================================
        // USTALAMY GŁÓWNY OBIEKT WROGA
        // =====================================================

        GameObject enemyObject =
            health.gameObject;


        // =====================================================
        // TEN WRÓG JUŻ ZOSTAŁ TRAFIONY
        // PRZEZ TEN ODŁAMEK
        // =====================================================

        if (hitEnemies.Contains(
                enemyObject))
        {
            return;
        }


        // =====================================================
        // BRAK PRZEBIĆ
        // =====================================================

        if (remainingPiercing <= 0)
        {
            Destroy(
                gameObject
            );

            return;
        }


        // =====================================================
        // ZAPISUJEMY TRAFIENIE
        // =====================================================

        hitEnemies.Add(
            enemyObject
        );


        // =====================================================
        // OBRAŻENIA
        // =====================================================

        health.TakeDamage(
            damage
        );


        // =====================================================
        // ZUŻYCIE JEDNEGO PRZEBICIA
        // =====================================================

        remainingPiercing--;


        // =====================================================
        // KONIEC PRZEBIĆ
        // =====================================================

        if (remainingPiercing <= 0)
        {
            Destroy(
                gameObject
            );
        }
    }


    // =========================================================
    // TRIGGER
    // =========================================================

    private void OnTriggerEnter(
        Collider other)
    {
        if (!initializedProjectile)
            return;


        EnemyHealth health =
            other.GetComponentInParent<EnemyHealth>();


        if (health == null)
            return;


        if (health.IsDying)
            return;


        Hit(
            health.gameObject
        );
    }


    // =========================================================
    // COLLISION
    // =========================================================

    private void OnCollisionEnter(
        Collision collision)
    {
        if (!initializedProjectile)
            return;


        EnemyHealth health =
            collision.gameObject
                .GetComponentInParent<EnemyHealth>();


        if (health == null)
            return;


        if (health.IsDying)
            return;


        Hit(
            health.gameObject
        );
    }
}