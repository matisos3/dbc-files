using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class EnergyOrbProjectile : ProjectileBase
{
    // =========================================================
    // ENERGY ORB
    // =========================================================

    private SkillInstance skill;
    private EnemyHealth target;

    private Vector3 lastKnownTargetPosition;
    private Vector3 lastMoveDirection = Vector3.forward;

    private bool initialized = false;
    private bool hasHit = false;


    // =========================================================
    // BAZOWA SKALA
    // =========================================================

    private Vector3 baseScale;
    private bool baseScaleInitialized = false;


    // =========================================================
    // KOMPONENTY
    // =========================================================

    private SphereCollider sphereCollider;
    private Rigidbody rb;


    // =========================================================
    // USTAWIENIA COLLIDERA
    // =========================================================

    [Header("Energy Orb Collider")]

    [Tooltip("Promień fizycznego hitboxa pocisku przed skalowaniem.")]
    [SerializeField]
    private float collisionRadius = 0.25f;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        // =====================================================
        // SPHERE COLLIDER
        // =====================================================

        sphereCollider =
            GetComponent<SphereCollider>();


        sphereCollider.isTrigger =
            true;


        sphereCollider.radius =
            collisionRadius;


        // =====================================================
        // ZAPAMIĘTUJEMY BAZOWĄ SKALĘ PREFABU
        // =====================================================

        baseScale =
            transform.localScale;

        baseScaleInitialized =
            true;


        // =====================================================
        // RIGIDBODY
        // =====================================================

        rb =
            GetComponent<Rigidbody>();


        rb.useGravity =
            false;


        rb.isKinematic =
            true;


        // =====================================================
        // WAŻNE DLA SZYBKIEGO POCISKU
        // =====================================================

        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousSpeculative;


        rb.interpolation =
            RigidbodyInterpolation.Interpolate;
    }


    // =========================================================
    // INICJALIZACJA
    // =========================================================

    public void InitializeEnergyOrb(
        SkillInstance skillInstance,
        EnemyHealth targetEnemy)
    {
        if (skillInstance == null)
        {
            return;
        }


        if (skillInstance.data == null)
        {
            return;
        }


        if (targetEnemy == null)
        {
            return;
        }


        skill =
            skillInstance;


        target =
            targetEnemy;


        lastKnownTargetPosition =
            target.transform.position;


        // =====================================================
        // AOE / SKALA KULI
        // =====================================================

        UpdateAoEScale();


        initialized =
            true;


        hasHit =
            false;
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


        // =====================================================
        // BAZOWY PROMIEŃ ENERGY ORB
        // =====================================================

        float baseHitRadius =
            skill.data.energyOrbHitRadius;


        if (baseHitRadius <= 0f)
            return;


        // =====================================================
        // AKTUALNY PROMIEŃ / BAZOWY PROMIEŃ
        // =====================================================

        float scaleMultiplier =
            skill.energyOrbHitRadius /
            baseHitRadius;


        scaleMultiplier =
            Mathf.Max(
                0f,
                scaleMultiplier
            );


        // =====================================================
        // SKALA KULI
        // =====================================================

        transform.localScale =
            baseScale *
            scaleMultiplier;


        // =====================================================
        // COLLIDER
        // =====================================================
        //
        // Collider zachowuje swój bazowy radius.
        //
        // Root transform jest skalowany razem z kulą,
        // więc fizyczny hitbox skaluje się automatycznie.
        //
        // =====================================================

        sphereCollider.radius =
            collisionRadius;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!initialized)
            return;


        if (hasHit)
            return;


        // =====================================================
        // AKTUALIZUJEMY POZYCJĘ CELU
        // =====================================================

        if (target != null &&
            !target.IsDying)
        {
            lastKnownTargetPosition =
                target.transform.position;
        }


        // =====================================================
        // JEŚLI CEL ZNIKNĄŁ / UMARŁ
        // =====================================================

        if (target == null ||
            target.IsDying)
        {
            transform.position =
                lastKnownTargetPosition;


            Impact(
                null
            );


            return;
        }


        // =====================================================
        // PRĘDKOŚĆ
        // =====================================================

        float speed =
            skill.energyOrbProjectileSpeed;


        if (speed <= 0f)
        {
            Impact(
                target
            );

            return;
        }


        // =====================================================
        // KIERUNEK DO CELU
        // =====================================================

        Vector3 direction =
            target.transform.position -
            transform.position;


        direction.y =
            0f;


        if (direction.sqrMagnitude >
            0.0001f)
        {
            lastMoveDirection =
                direction.normalized;
        }


        // =====================================================
        // ODLEGŁOŚĆ
        // =====================================================

        float distance =
            direction.magnitude;


        float movementThisFrame =
            speed *
            Time.deltaTime;


        // =====================================================
        // JESTEŚMY JUŻ PRZY CELU
        // =====================================================

        if (distance <= movementThisFrame)
        {
            transform.position =
                target.transform.position;


            // =================================================
            // AWARYJNE TRAFIENIE
            // =================================================
            // Jeśli z jakiegoś powodu OnTriggerEnter
            // nie został wywołany, nie pozwalamy pociskowi
            // przelecieć przez wybrany cel.

            Impact(
                target
            );


            return;
        }


        // =====================================================
        // RUCH
        // =====================================================

        Vector3 movementDirection =
            direction.normalized;


        transform.position =
            Vector3.MoveTowards(
                transform.position,
                target.transform.position,
                movementThisFrame
            );


        // =====================================================
        // OBRÓT KULI
        // =====================================================

        if (movementDirection.sqrMagnitude >
            0.0001f)
        {
            transform.rotation =
                Quaternion.LookRotation(
                    movementDirection
                );
        }
    }


    // =========================================================
    // KOLIZJA Z ENEMY
    // =========================================================

    private void OnTriggerEnter(
        Collider other)
    {
        if (!initialized)
            return;


        if (hasHit)
            return;


        // =====================================================
        // SZUKAMY ENEMY HEALTH
        // =====================================================

        EnemyHealth enemy =
            other.GetComponent<EnemyHealth>();


        if (enemy == null)
        {
            enemy =
                other.GetComponentInParent<EnemyHealth>();
        }


        if (enemy == null)
            return;


        // =====================================================
        // NIE TRAFIAJ ZABITEGO ENEMY
        // =====================================================

        if (enemy.IsDying)
            return;


        // =====================================================
        // PIERWSZY FAKTYCZNIE DOTKNIĘTY WRÓG
        // =====================================================

        target =
            enemy;


        lastKnownTargetPosition =
            enemy.transform.position;


        // =====================================================
        // TRAFIENIE
        // =====================================================

        Impact(
            enemy
        );
    }


    // =========================================================
    // TRAFIENIE
    // =========================================================

    private void Impact(
        EnemyHealth impactTarget)
    {
        if (hasHit)
            return;


        hasHit =
            true;


        // =====================================================
        // POZYCJA UDERZENIA
        // =====================================================

        Vector3 impactPosition;


        if (impactTarget != null)
        {
            impactPosition =
                impactTarget.transform.position;
        }
        else if (target != null)
        {
            impactPosition =
                target.transform.position;
        }
        else
        {
            impactPosition =
                lastKnownTargetPosition;
        }


        impactPosition.y =
            transform.position.y;


        // =====================================================
        // PROMIEŃ
        // =====================================================

        float hitRadius =
            skill.energyOrbHitRadius;


        if (hitRadius < 0f)
        {
            hitRadius =
                0f;
        }


        // =====================================================
        // OBRAŻENIA
        // =====================================================

        float damage =
            skill.damage;


        if (damage < 0f)
        {
            damage =
                0f;
        }


        // =====================================================
        // SIŁA ODRZUTU
        // =====================================================

        float knockback =
            skill.energyOrbKnockback;


        if (knockback < 0f)
        {
            knockback =
                0f;
        }


        // =====================================================
        // SZUKAMY WSZYSTKICH PRZECIWNIKÓW
        // W PROMIENIU UDERZENIA
        // =====================================================

        GameObject[] enemyObjects =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );


        List<EnemyHealth> affectedEnemies =
            new List<EnemyHealth>();


        foreach (
            GameObject enemyObject
            in enemyObjects)
        {
            if (enemyObject == null)
                continue;


            // =================================================
            // ENEMY HEALTH
            // =================================================

            EnemyHealth enemy =
                enemyObject.GetComponent<EnemyHealth>();


            if (enemy == null)
            {
                enemy =
                    enemyObject.GetComponentInParent<EnemyHealth>();
            }


            if (enemy == null)
                continue;


            if (enemy.IsDying)
                continue;


            // =================================================
            // NIE DUBLUJEMY TEGO SAMEGO ENEMY
            // =================================================

            if (affectedEnemies.Contains(enemy))
                continue;


            // =================================================
            // ODLEGŁOŚĆ OD MIEJSCA UDERZENIA
            // =================================================

            Vector3 enemyPosition =
                enemy.transform.position;


            Vector3 knockbackDirection =
                enemyPosition -
                impactPosition;


            knockbackDirection.y =
                0f;


            float enemyDistance =
                knockbackDirection.magnitude;


            if (enemyDistance > hitRadius)
                continue;


            // =================================================
            // KIERUNEK ODRZUTU
            // =================================================

            if (knockbackDirection.sqrMagnitude <=
                0.0001f)
            {
                knockbackDirection =
                    lastMoveDirection;


                knockbackDirection.y =
                    0f;


                if (knockbackDirection.sqrMagnitude <=
                    0.0001f)
                {
                    knockbackDirection =
                        Vector3.forward;
                }
            }


            knockbackDirection.Normalize();


            // =================================================
            // DAMAGE
            // =================================================

            if (damage > 0f)
            {
                enemy.TakeDamage(
                    damage
                );
            }


            // =================================================
            // ENEMY MOVEMENT
            // =================================================

            EnemyMovement enemyMovement =
                enemyObject.GetComponent<EnemyMovement>();


            if (enemyMovement == null)
            {
                enemyMovement =
                    enemy.GetComponent<EnemyMovement>();
            }


            if (enemyMovement == null)
            {
                enemyMovement =
                    enemy.GetComponentInParent<EnemyMovement>();
            }


            // =================================================
            // KNOCKBACK
            // =================================================

            if (enemyMovement != null &&
                knockback > 0f)
            {
                enemyMovement.ApplyKnockback(
                    knockbackDirection,
                    knockback
                );
            }


            // =================================================
            // ZAPISUJEMY TRAFIONEGO
            // =================================================

            affectedEnemies.Add(
                enemy
            );
        }

        // =====================================================
        // NISZCZYMY POCISK
        // =====================================================

        Destroy(
            gameObject
        );
    }


    // =========================================================
    // PROJECTILE BASE
    // =========================================================

    public override void Hit(
        GameObject hitTarget)
    {
        if (hasHit)
            return;


        EnemyHealth enemy =
            null;


        if (hitTarget != null)
        {
            enemy =
                hitTarget.GetComponent<EnemyHealth>();


            if (enemy == null)
            {
                enemy =
                    hitTarget.GetComponentInParent<EnemyHealth>();
            }
        }


        if (enemy != null &&
            !enemy.IsDying)
        {
            target =
                enemy;
        }


        Impact(
            enemy
        );
    }
}