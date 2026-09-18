using System.Collections.Generic;
using UnityEngine;

public class BreathOfFire : MonoBehaviour
{
    private Tower tower;
    private Transform firePoint;
    private SkillInstance skill;

    private GameObject fireEffect;
    private Transform breathTransform;

    private float damageTimer;
    private bool initialized;

    private const float BASE_RANGE = 5f;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        tower = GetComponent<Tower>();

        if (tower != null)
            firePoint = tower.firePoint;
    }


    // =========================================================
    // INITIALIZE
    // =========================================================

    public void Initialize(SkillInstance instance)
    {
        if (instance == null || instance.data == null)
            return;

        if (tower == null)
            tower = GetComponent<Tower>();

        if (firePoint == null && tower != null)
            firePoint = tower.firePoint;

        skill = instance;

        if (!initialized)
        {
            initialized = true;
            damageTimer = 0f;
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!initialized ||
            skill == null ||
            skill.data == null)
            return;

        if (firePoint == null)
        {
            if (tower != null)
                firePoint = tower.firePoint;

            if (firePoint == null)
                return;
        }

        EnemyHealth nearestEnemy =
            FindNearestEnemy();

        if (nearestEnemy == null)
        {
            DestroyFireEffect();
            return;
        }


        // =====================================================
        // KIERUNEK DO CELU
        // =====================================================

        Vector3 direction =
            nearestEnemy.transform.position -
            firePoint.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction =
                transform.forward;

            direction.y = 0f;
        }

        direction.Normalize();


        // =====================================================
        // FIRE EFFECT
        // =====================================================

        CreateOrUpdateFireEffect(direction);


        // =====================================================
        // DAMAGE TIMER
        // =====================================================

        damageTimer -= Time.deltaTime;

        if (damageTimer <= 0f)
        {
            DamageEnemiesInCone(direction);

            damageTimer =
                Mathf.Max(
                    0.01f,
                    skill.breathOfFireTickInterval
                );
        }
    }


    // =========================================================
    // FIND NEAREST ENEMY
    // =========================================================

    private EnemyHealth FindNearestEnemy()
    {
        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag("Enemy");

        EnemyHealth closest = null;

        float closestDistance =
            Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null)
                continue;

            EnemyHealth enemyHealth =
                enemy.GetComponent<EnemyHealth>();

            if (enemyHealth == null)
            {
                enemyHealth =
                    enemy.GetComponentInParent<EnemyHealth>();
            }

            if (enemyHealth == null)
                continue;

            if (enemyHealth.IsDying)
                continue;


            float distance =
                Vector3.Distance(
                    firePoint.position,
                    enemyHealth.transform.position
                );

            if (distance >
                skill.breathOfFireRange)
                continue;


            if (distance <
                closestDistance)
            {
                closestDistance =
                    distance;

                closest =
                    enemyHealth;
            }
        }

        return closest;
    }


    // =========================================================
    // DAMAGE - HITBOX OBRAŻEŃ
    // =========================================================

    private void DamageEnemiesInCone(
        Vector3 direction)
    {
        Collider[] colliders =
            Physics.OverlapSphere(
                firePoint.position,
                skill.breathOfFireRange
            );

        HashSet<EnemyHealth> affectedEnemies =
            new HashSet<EnemyHealth>();


        foreach (Collider collider in colliders)
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

            if (enemyHealth.IsDying)
                continue;


            // Jeden enemy tylko raz,
            // nawet jeśli posiada kilka colliderów.
            if (!affectedEnemies.Add(enemyHealth))
                continue;


            // =================================================
            // ODLEGŁOŚĆ
            // =================================================

            Vector3 toEnemy =
                enemyHealth.transform.position -
                firePoint.position;

            toEnemy.y = 0f;

            float distance =
                toEnemy.magnitude;

            if (distance >
                skill.breathOfFireRange)
                continue;


            if (toEnemy.sqrMagnitude <=
                0.0001f)
                continue;

            toEnemy.Normalize();


            // =================================================
            // KĄT
            // =================================================

            float angle =
                Vector3.Angle(
                    direction,
                    toEnemy
                );


            if (angle >
                skill.breathOfFireAngle / 2f)
                continue;


            // =================================================
            // DAMAGE
            // =================================================

            enemyHealth.TakeDamage(
                skill.breathOfFireDamage
            );


            // =================================================
            // BURN
            // =================================================

            BurnEffect burn =
                enemyHealth.GetComponent<BurnEffect>();

            if (burn == null)
            {
                burn =
                    enemyHealth.gameObject
                        .AddComponent<BurnEffect>();
            }

            burn.ApplyBurn(
                skill.breathOfFireBurnDamage,
                skill.breathOfFireBurnDuration,
                skill.breathOfFireBurnTickInterval
            );
        }
    }


    // =========================================================
    // CREATE / UPDATE FIRE EFFECT
    // =========================================================

    private void CreateOrUpdateFireEffect(
        Vector3 direction)
    {
        if (skill.breathOfFireEffectPrefab == null)
            return;


        if (fireEffect == null)
        {
            fireEffect =
                Instantiate(
                    skill.breathOfFireEffectPrefab,
                    firePoint.position,
                    Quaternion.LookRotation(direction)
                );

            if (fireEffect == null)
                return;


            breathTransform =
                FindChildRecursive(
                    fireEffect.transform,
                    "Breath"
                );
        }


        // =====================================================
        // POSITION
        // =====================================================

        fireEffect.transform.position =
            firePoint.position;


        // =====================================================
        // ROTATION GŁÓWNEGO EFEKTU
        // =====================================================

        fireEffect.transform.rotation =
            Quaternion.LookRotation(direction);


        // =====================================================
        // RANGE SCALE
        // =====================================================

        float rangeMultiplier =
            skill.breathOfFireRange /
            BASE_RANGE;

        rangeMultiplier =
            Mathf.Max(
                0.01f,
                rangeMultiplier
            );

        fireEffect.transform.localScale =
            Vector3.one *
            rangeMultiplier;


        // =====================================================
        // PARTICLE ARC + BREATH ROTATION
        // =====================================================

        UpdateBreathAoE();
    }


    // =========================================================
    // UPDATE BREATH AOE
    // =========================================================

    private void UpdateBreathAoE()
    {
        if (fireEffect == null)
            return;


        if (breathTransform == null)
        {
            breathTransform =
                FindChildRecursive(
                    fireEffect.transform,
                    "Breath"
                );
        }

        if (breathTransform == null)
            return;


        ParticleSystem particleSystem =
            breathTransform
                .GetComponentInChildren<ParticleSystem>(
                    true
                );

        if (particleSystem == null)
            return;


        var shape =
            particleSystem.shape;


        // =====================================================
        // PARTICLE ARC
        // =====================================================

        shape.arc =
            Mathf.Clamp(
                skill.breathOfFireAngle,
                0f,
                360f
            );


        // =====================================================
        // BREATH ROTATION
        // =====================================================
        //
        // Arc 60  -> Y -60
        // Arc 90  -> Y -45
        // Arc 120 -> Y -30
        //
        // Aktualna formuła:
        // Y = (Arc / 2) - 90
        //
        // Przykładowo:
        // 60  -> -60
        // 90  -> -45
        // 120 -> -30

        float arc =
            shape.arc;

        Vector3 localEuler =
            breathTransform.localEulerAngles;

        localEuler.y =
            (arc / 2f) - 90f;

        breathTransform.localEulerAngles =
            localEuler;
    }


    // =========================================================
    // FIND CHILD RECURSIVELY
    // =========================================================

    private Transform FindChildRecursive(
        Transform parent,
        string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;


            Transform result =
                FindChildRecursive(
                    child,
                    childName
                );

            if (result != null)
                return result;
        }

        return null;
    }


    // =========================================================
    // DESTROY FIRE EFFECT
    // =========================================================

    private void DestroyFireEffect()
    {
        if (fireEffect == null)
            return;

        Destroy(fireEffect);

        fireEffect = null;
        breathTransform = null;
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        DestroyFireEffect();
    }
}