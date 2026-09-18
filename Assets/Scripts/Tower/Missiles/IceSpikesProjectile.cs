using UnityEngine;

public class IceSpikesProjectile : ProjectileBase
{
    private bool initializedProjectile = false;


    // =========================================================
    // ODLEGŁOŚĆ STARTU ODŁAMKÓW OD WROGA
    // =========================================================

    [Header("Ice Spikes")]
    [SerializeField]
    private float spikeSpawnDistance = 3f;


    // =========================================================
    // INITIALIZE
    // =========================================================

    public override void Initialize(
        SkillInstance instance)
    {
        base.Initialize(
            instance
        );


        if (instance == null)
        {
            return;
        }


        if (instance.data == null)
        {
            return;
        }


        initializedProjectile =
            true;
    }


    // =========================================================
    // HIT
    // =========================================================

    public override void Hit(
        GameObject enemy)
    {
        if (!initializedProjectile)
        {
            return;
        }


        if (enemy == null)
            return;


        SkillInstance skill =
            GetSkillInstance();


        if (skill == null)
        {
            return;
        }


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
        {
            return;
        }


        if (health.IsDying)
            return;


        // =====================================================
        // GŁÓWNE OBRAŻENIA
        // =====================================================

        float mainDamage =
            skill.damage;

        health.TakeDamage(
            mainDamage
        );


        // =====================================================
        // POZYCJA STARTOWA ODŁAMKÓW
        // =====================================================

        Vector3 spawnCenter =
            enemy.transform.position;


        if (health.hitPoint != null)
        {
            spawnCenter =
                health.hitPoint.position;
        }


        // =====================================================
        // LICZBA ODŁAMKÓW
        // =====================================================

        int spikeCount =
            Mathf.Max(
                1,
                skill.iceSpikeCount
            );


        float angleStep =
            360f /
            spikeCount;


        // =====================================================
        // TWORZENIE ODŁAMKÓW
        // =====================================================

        for (
            int i = 0;
            i < spikeCount;
            i++)
        {
            // =================================================
            // KĄT
            // =================================================

            float angle =
                angleStep * i;


            // =================================================
            // KIERUNEK
            // =================================================

            Vector3 direction =
                Quaternion.Euler(
                    0f,
                    angle,
                    0f
                ) *
                Vector3.forward;


            direction.y = 0f;


            if (direction.sqrMagnitude <=
                0.0001f)
            {
                direction =
                    Vector3.forward;
            }


            direction.Normalize();


            // =================================================
            // NOWA POZYCJA STARTOWA
            //
            // KLUCZOWA ZMIANA:
            // odłamek NIE powstaje w przeciwniku.
            // =================================================

            Vector3 spawnPosition =
                spawnCenter +
                direction *
                spikeSpawnDistance;


            // =================================================
            // ROTACJA
            // =================================================

            Quaternion rotation =
                Quaternion.LookRotation(
                    direction
                );


            // =================================================
            // SPAWN
            // =================================================

            GameObject spikeObject =
                Instantiate(
                    skill.iceSpikeProjectilePrefab,
                    spawnPosition,
                    rotation
                );


            if (spikeObject == null)
                continue;


            // =================================================
            // COMPONENT
            // =================================================

            IceSpikeProjectile spike =
                spikeObject.GetComponent<
                    IceSpikeProjectile
                >();


            if (spike == null)
            {
                Destroy(
                    spikeObject
                );


                continue;
            }


            // =================================================
            // INITIALIZE
            // =================================================

            spike.Initialize(
                skill,
                direction
            );
        }


        // =====================================================
        // GŁÓWNY POCISK ZNIKA
        // =====================================================

        Destroy(
            gameObject
        );
    }
}