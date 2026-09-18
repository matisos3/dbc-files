using System.Collections.Generic;
using UnityEngine;

public class ElectricShockProjectile : ProjectileBase
{
    // =========================================================
    // RUCH
    // =========================================================

    private float speed;
    private Vector3 direction;
    private Vector3 startPosition;
    private float maxDistance;

    private bool initialized = false;


    // =========================================================
    // AOE / SKALA
    // =========================================================

    private Vector3 baseScale;
    private bool baseScaleInitialized = false;


    // =========================================================
    // OBRAŻENIA
    // =========================================================

    private const float DAMAGE_TICK_INTERVAL = 0.2f;


    // Każdy przeciwnik ma własny timer kolejnego obrażenia.
    private readonly Dictionary<EnemyHealth, float> damageTimers =
        new Dictionary<EnemyHealth, float>();


    // Przeciwnicy aktualnie znajdujący się w kolizji.
    private readonly HashSet<EnemyHealth> targetsInCollision =
        new HashSet<EnemyHealth>();


    // =========================================================
    // PIERCING
    // =========================================================

    // Przeciwnicy, których Electric Shock już zaliczył
    // jako wykorzystane przebicie.
    private readonly HashSet<EnemyHealth> piercedTargets =
        new HashSet<EnemyHealth>();


    // Liczba pozostałych przebić.
    private int remainingPiercing;


    // =========================================================
    // INICJALIZACJA
    // =========================================================

    public void InitializeElectricShock(
        SkillInstance skill,
        EnemyHealth target)
    {
        if (skill == null ||
            skill.data == null)
        {
            return;
        }

        if (target == null)
        {
            return;
        }

        base.Initialize(skill);


        // =====================================================
        // PIERCING
        // =====================================================

        remainingPiercing =
            Mathf.Max(
                1,
                skill.piercingCount
            );


        piercedTargets.Clear();


        // =====================================================
        // ZAPIS BAZOWEJ SKALI
        // =====================================================

        if (!baseScaleInitialized)
        {
            baseScale =
                transform.localScale;

            baseScaleInitialized =
                true;
        }


        // =====================================================
        // AOE / SKALOWANIE POCISKU
        // =====================================================

        UpdateAoEScale(
            skill
        );


        // =====================================================
        // PRĘDKOŚĆ
        // =====================================================

        speed =
            Mathf.Max(
                0.1f,
                skill.electricShockProjectileSpeed
            );


        // =====================================================
        // MAKSYMALNY DYSTANS
        // =====================================================

        maxDistance =
            Mathf.Max(
                0.1f,
                skill.range
            );


        // =====================================================
        // POZYCJA STARTOWA
        // =====================================================

        startPosition =
            transform.position;


        // =====================================================
        // KIERUNEK DO PIERWSZEGO WROGA
        // =====================================================

        direction =
            target.transform.position -
            transform.position;


        // Ruch odbywa się poziomo.
        direction.y = 0f;


        // Jeśli kierunek jest praktycznie zerowy,
        // używamy kierunku forward pocisku.
        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction =
                transform.forward;

            direction.y = 0f;
        }


        direction.Normalize();


        // =====================================================
        // OBRÓT POCISKU
        // =====================================================

        if (direction.sqrMagnitude > 0.0001f)
        {
            transform.rotation =
                Quaternion.LookRotation(
                    direction
                );
        }


        initialized = true;
    }


    // =========================================================
    // AOE / ROZMIAR ELECTRIC SHOCK
    // =========================================================

    private void UpdateAoEScale(
        SkillInstance skill)
    {
        if (skill == null ||
            skill.data == null)
        {
            return;
        }


        // =====================================================
        // BAZOWY ROZMIAR Z SKILL DATA
        // =====================================================

        float baseRadius =
            skill.data.electricShockRadius;


        if (baseRadius <= 0f)
        {
            return;
        }


        // =====================================================
        // MULTIPLIER
        // =====================================================

        float scaleMultiplier =
            skill.electricShockRadius /
            baseRadius;


        scaleMultiplier =
            Mathf.Max(
                0f,
                scaleMultiplier
            );


        // =====================================================
        // SKALOWANIE CAŁEGO PREFABU
        // =====================================================

        transform.localScale =
            baseScale *
            scaleMultiplier;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!initialized)
            return;


        // =====================================================
        // RUCH
        // =====================================================

        transform.position +=
            direction *
            speed *
            Time.deltaTime;


        // =====================================================
        // SPRAWDZAMY DYSTANS
        // =====================================================

        float distance =
            Vector3.Distance(
                startPosition,
                transform.position
            );

        if (distance >= maxDistance)
        {
            Destroy(
                gameObject
            );

            return;
        }


        // =====================================================
        // TICK OBRAŻEŃ
        // =====================================================

        UpdateDamageTimers();
    }


    // =========================================================
    // TICK OBRAŻEŃ
    // =========================================================

    private void UpdateDamageTimers()
    {
        if (targetsInCollision.Count == 0)
            return;


        SkillInstance skill =
            GetSkillInstance();


        if (skill == null)
            return;


        // Kopia listy jest potrzebna, ponieważ podczas
        // zadawania obrażeń przeciwnik może umrzeć.
        List<EnemyHealth> targets =
            new List<EnemyHealth>(
                targetsInCollision
            );


        foreach (EnemyHealth enemy in targets)
        {
            if (enemy == null)
                continue;


            if (enemy.IsDying)
                continue;


            // Jeśli przeciwnik nie został jeszcze zaliczony
            // do piercingu, tutaj nic nie robimy.
            //
            // Pierwsze trafienie jest obsługiwane
            // w OnTriggerEnter / Hit.
            if (!piercedTargets.Contains(enemy))
                continue;


            if (!damageTimers.ContainsKey(enemy))
                damageTimers[enemy] = DAMAGE_TICK_INTERVAL;


            damageTimers[enemy] -=
                Time.deltaTime;


            if (damageTimers[enemy] <= 0f)
            {
                DealDamage(
                    enemy,
                    skill
                );


                damageTimers[enemy] =
                    DAMAGE_TICK_INTERVAL;
            }
        }
    }


    // =========================================================
    // WEJŚCIE W KOLIZJĘ
    // =========================================================

    private void OnTriggerEnter(Collider other)
    {
        if (!initialized)
            return;


        EnemyHealth enemy =
            GetEnemyHealth(other);


        if (enemy == null)
            return;


        if (enemy.IsDying)
            return;


        // Jeśli ten przeciwnik został już zaliczony
        // jako przebicie, nic więcej tutaj nie robimy.
        if (piercedTargets.Contains(enemy))
            return;


        // Jeśli nie ma już wolnego piercingu,
        // nie można trafić kolejnego przeciwnika.
        if (remainingPiercing <= 0)
        {
            Destroy(
                gameObject
            );

            return;
        }


        // =====================================================
        // ZUŻYCIE PIERCINGU
        // =====================================================

        piercedTargets.Add(
            enemy
        );


        remainingPiercing--;


        // =====================================================
        // DODANIE DO AKTUALNEJ KOLIZJI
        // =====================================================

        targetsInCollision.Add(
            enemy
        );


        // =====================================================
        // PIERWSZE OBRAŻENIE
        // =====================================================

        DealDamage(
            enemy,
            GetSkillInstance()
        );


        // =====================================================
        // NASTĘPNY TICK
        // =====================================================

        damageTimers[enemy] =
            DAMAGE_TICK_INTERVAL;


        // =====================================================
        // KONIEC PIERCINGU
        // =====================================================

        if (remainingPiercing <= 0)
        {
            Destroy(
                gameObject
            );
        }
    }


    // =========================================================
    // POZOSTAWANIE W KOLIZJI
    // =========================================================

    private void OnTriggerStay(Collider other)
    {
        if (!initialized)
            return;


        EnemyHealth enemy =
            GetEnemyHealth(other);


        if (enemy == null)
            return;


        if (enemy.IsDying)
            return;


        // Jeśli już wykorzystaliśmy na nim piercing,
        // może dalej otrzymywać ticki obrażeń.
        if (piercedTargets.Contains(enemy))
        {
            if (!targetsInCollision.Contains(enemy))
            {
                targetsInCollision.Add(
                    enemy
                );


                if (!damageTimers.ContainsKey(enemy))
                {
                    damageTimers[enemy] =
                        DAMAGE_TICK_INTERVAL;
                }
            }

            return;
        }


        // Jeśli nadal mamy piercing,
        // próbujemy zaliczyć nowego przeciwnika.
        if (remainingPiercing > 0)
        {
            piercedTargets.Add(
                enemy
            );


            remainingPiercing--;


            targetsInCollision.Add(
                enemy
            );


            DealDamage(
                enemy,
                GetSkillInstance()
            );


            damageTimers[enemy] =
                DAMAGE_TICK_INTERVAL;


            if (remainingPiercing <= 0)
            {
                Destroy(
                    gameObject
                );
            }
        }
    }


    // =========================================================
    // WYJŚCIE Z KOLIZJI
    // =========================================================

    private void OnTriggerExit(Collider other)
    {
        if (!initialized)
            return;


        EnemyHealth enemy =
            GetEnemyHealth(other);


        if (enemy == null)
            return;


        targetsInCollision.Remove(
            enemy
        );


        damageTimers.Remove(
            enemy
        );
    }


    // =========================================================
    // ZADAWANIE OBRAŻEŃ
    // =========================================================

    private void DealDamage(
        EnemyHealth enemy,
        SkillInstance skill)
    {
        if (enemy == null)
            return;


        if (skill == null)
            return;


        if (enemy.IsDying)
            return;


        // =====================================================
        // OBRAŻENIA
        // =====================================================

        enemy.TakeDamage(
            skill.electricShockDamage
        );


        // =====================================================
        // STUN
        // =====================================================

        ApplyStun(
            enemy,
            skill.electricShockStunDuration
        );
    }


    // =========================================================
    // WYSZUKIWANIE ENEMYHEALTH
    // =========================================================

    private EnemyHealth GetEnemyHealth(
        Collider collider)
    {
        if (collider == null)
            return null;


        EnemyHealth enemy =
            collider.GetComponent<EnemyHealth>();


        if (enemy == null)
        {
            enemy =
                collider.GetComponentInParent<
                    EnemyHealth
                >();
        }


        return enemy;
    }


    // =========================================================
    // HIT
    // =========================================================

    public override void Hit(
        GameObject target)
    {
        if (!initialized)
            return;


        if (target == null)
            return;


        EnemyHealth enemy =
            target.GetComponent<EnemyHealth>();


        if (enemy == null)
        {
            enemy =
                target.GetComponentInParent<
                    EnemyHealth
                >();
        }


        if (enemy == null)
            return;


        if (enemy.IsDying)
            return;


        // Jeśli ten przeciwnik został już trafiony,
        // Hit nie zużywa kolejnego piercingu.
        if (piercedTargets.Contains(enemy))
            return;


        if (remainingPiercing <= 0)
            return;


        SkillInstance skill =
            GetSkillInstance();


        if (skill == null)
            return;


        // =====================================================
        // ZUŻYCIE PIERCINGU
        // =====================================================

        piercedTargets.Add(
            enemy
        );


        remainingPiercing--;


        // =====================================================
        // DODANIE DO AKTUALNEJ KOLIZJI
        // =====================================================

        targetsInCollision.Add(
            enemy
        );


        // =====================================================
        // PIERWSZE OBRAŻENIE
        // =====================================================

        DealDamage(
            enemy,
            skill
        );


        damageTimers[enemy] =
            DAMAGE_TICK_INTERVAL;


        // =====================================================
        // KONIEC PIERCINGU
        // =====================================================

        if (remainingPiercing <= 0)
        {
            Destroy(
                gameObject
            );
        }
    }


    // =========================================================
    // STUN
    // =========================================================

    private void ApplyStun(
        EnemyHealth enemy,
        float duration)
    {
        if (enemy == null)
            return;


        if (duration <= 0f)
            return;


        EnemyMovement movement =
            enemy.GetComponent<EnemyMovement>();


        if (movement == null)
        {
            movement =
                enemy.GetComponentInChildren<
                    EnemyMovement
                >();
        }


        if (movement == null)
            return;


        movement.ApplyStun(
            duration,
            null,
            0f
        );
    }
}