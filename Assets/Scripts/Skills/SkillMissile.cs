using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillMissile : MonoBehaviour
{
    private Tower tower;
    private Transform firePoint;

    // =========================================================
    // RAIN OF FIRE
    // =========================================================

    private RainOfFireEffect activeRainOfFire;


    // =========================================================
    // VOLCANIC SPHERE - BURST
    // =========================================================

    private bool volcanicSphereBurstActive = false;

    private const float VolcanicSphereShotDelay = 0.3f;


    // =========================================================
    // CHAIN LIGHTNING
    // =========================================================

    private bool chainLightningActive = false;


    // =========================================================
    // LIGHTNING STRIKE
    // =========================================================

    private bool lightningStrikeActive = false;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        tower =
            GetComponent<Tower>();


        if (tower == null)
        {
            return;
        }


        firePoint =
            tower.firePoint;


        if (firePoint == null)
        {
            return;
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (tower == null)
            return;


        foreach (SkillInstance skill in tower.activeSkills)
        {
            if (skill == null)
                continue;


            if (skill.data == null)
                continue;


            skill.cooldownTimer -=
                Time.deltaTime;


            // =================================================
            // BREATH OF FIRE
            // =================================================

            if (skill.data.skillID == "BreathOfFire")
            {
                BreathOfFire breath =
                    GetComponent<BreathOfFire>();


                if (breath == null)
                {
                    breath =
                        gameObject.AddComponent<BreathOfFire>();
                }


                breath.Initialize(
                    skill
                );


                continue;
            }


            // =================================================
            // FROST NOVA
            // =================================================

            if (skill.data.skillID == "FrostNova")
            {
                if (skill.cooldownTimer <= 0f)
                {
                    if (IsEnemyInFrostNovaRange(skill))
                    {
                        ShootFrostNova(
                            skill
                        );

                        skill.cooldownTimer =
                            skill.frostNovaCooldown;
                    }
                }


                continue;
            }


            // =================================================
            // SPINNING LASER
            // =================================================

            if (skill.data.skillID == "SpinningLaser")
            {
                ShootSpinningLaser(skill);
                continue;
            }


            // =================================================
            // VOLCANIC SPHERE
            // =================================================

            if (skill.data.skillID == "VolcanicSphere")
            {
                if (skill.cooldownTimer <= 0f &&
                    !volcanicSphereBurstActive)
                {
                    StartCoroutine(
                        ShootVolcanicSphereBurst(
                            skill
                        )
                    );
                }


                continue;
            }


            // =================================================
            // CHAIN LIGHTNING
            // =================================================

            if (skill.data.skillID == "ChainLightning")
            {
                if (skill.cooldownTimer <= 0f &&
                    !chainLightningActive)
                {
                    StartCoroutine(
                        ShootChainLightning(
                            skill
                        )
                    );
                }


                continue;
            }


            // =================================================
            // LIGHTNING STRIKE
            // =================================================

            if (skill.data.skillID == "LightningStrike")
            {
                if (skill.cooldownTimer <= 0f &&
                    !lightningStrikeActive)
                {
                    StartCoroutine(
                        ShootLightningStrike(
                            skill
                        )
                    );
                }


                continue;
            }


            // =================================================
            // NET
            // =================================================

            if (skill.data.skillID == "Net")
            {
                if (skill.cooldownTimer <= 0f)
                {
                    ShootNet(
                        skill
                    );


                    if (skill.fireRate > 0f)
                    {
                        skill.cooldownTimer =
                            skill.fireRate;
                    }
                }


                continue;
            }


            // =================================================
            // NORMALNE SKILLE
            // =================================================

            if (skill.cooldownTimer <= 0f)
            {
                // =================================================
                // RAIN OF FIRE
                // =================================================

                if (skill.data.skillID == "RainOfFire")
                {
                    if (activeRainOfFire != null)
                    {
                        continue;
                    }


                    Shoot(skill);


                    if (activeRainOfFire != null &&
                        skill.fireRate > 0f)
                    {
                        skill.cooldownTimer =
                            skill.fireRate;
                    }


                    continue;
                }


                // =================================================
                // POZOSTAŁE SKILLE
                // =================================================

                Shoot(
                    skill
                );


                if (skill.fireRate > 0f)
                {
                    skill.cooldownTimer =
                        skill.fireRate;
                }
            }
        }
    }


    // =========================================================
    // NET
    // =========================================================

    private void ShootNet(
        SkillInstance skill)
    {
        if (skill == null ||
            skill.data == null)
        {
            return;
        }


        if (firePoint == null)
        {
            return;
        }


        if (skill.data.projectilePrefab == null)
        {
            return;
        }


        GameObject[] enemyObjects =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );


        List<EnemyHealth> availableTargets =
            new List<EnemyHealth>();


        foreach (GameObject enemyObject in enemyObjects)
        {
            if (enemyObject == null)
                continue;


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


            float distance =
                Vector3.Distance(
                    transform.position,
                    enemy.transform.position
                );


            if (distance > skill.range)
                continue;


            if (!availableTargets.Contains(enemy))
            {
                availableTargets.Add(
                    enemy
                );
            }
        }


        if (availableTargets.Count == 0)
            return;


        // =====================================================
        // DYNAMICZNA LICZBA SIECI
        // =====================================================

        int netCount =
            skill.GetProjectileCount();


        int shotsToFire =
            Mathf.Min(
                netCount,
                availableTargets.Count
            );


        // =====================================================
        // WYSTRZELENIE SIECI
        // =====================================================

        for (int i = 0;
             i < shotsToFire;
             i++)
        {
            EnemyHealth target =
                FindClosestNetTarget(
                    availableTargets
                );


            if (target == null)
                break;


            availableTargets.Remove(
                target
            );


            GameObject projectileObject =
                Instantiate(
                    skill.data.projectilePrefab,
                    firePoint.position,
                    Quaternion.identity
                );


            if (projectileObject == null)
                continue;


            ProjectileBase projectile =
                projectileObject.GetComponent<ProjectileBase>();


            if (projectile == null)
            {
                Destroy(
                    projectileObject
                );


                continue;
            }


            projectile.Initialize(
                skill
            );


            ProjectileMovement movement =
                projectileObject.GetComponent<ProjectileMovement>();


            if (movement == null)
            {
                Destroy(
                    projectileObject
                );


                continue;
            }


            movement.Initialize(
                target.transform
            );
        }
    }


    // =========================================================
    // NET - NAJBLIŻSZY CEL
    // =========================================================

    private EnemyHealth FindClosestNetTarget(
        List<EnemyHealth> targets)
    {
        if (targets == null ||
            targets.Count == 0)
        {
            return null;
        }


        EnemyHealth closestTarget =
            null;


        float closestDistance =
            Mathf.Infinity;


        foreach (EnemyHealth enemy in targets)
        {
            if (enemy == null)
                continue;


            if (enemy.IsDying)
                continue;


            float distance =
                Vector3.Distance(
                    transform.position,
                    enemy.transform.position
                );


            if (distance < closestDistance)
            {
                closestDistance =
                    distance;


                closestTarget =
                    enemy;
            }
        }


        return closestTarget;
    }


    // =========================================================
    // SPINNING LASER
    // =========================================================

    private void ShootSpinningLaser(
        SkillInstance skill)
    {
        if (skill == null ||
            skill.data == null)
        {
            return;
        }


        SpinningLaserController controller =
            GetComponent<SpinningLaserController>();


        if (controller != null &&
            controller.IsInitialized)
        {
            return;
        }


        if (controller == null)
        {
            controller =
                gameObject.AddComponent<SpinningLaserController>();
        }


        controller.Initialize(
            skill,
            transform
        );
    }


    // =========================================================
    // LIGHTNING STRIKE
    // =========================================================

    private IEnumerator ShootLightningStrike(
        SkillInstance skill)
    {
        if (lightningStrikeActive)
            yield break;


        if (skill == null ||
            skill.data == null)
        {
            yield break;
        }


        lightningStrikeActive = true;


        EnemyHealth target =
            FindRandomLightningStrikeTarget(
                skill.range
            );


        if (target == null)
        {
            lightningStrikeActive = false;
            yield break;
        }


        float damage =
            skill.damage;


        int maxHits =
            Mathf.Max(
                1,
                skill.lightningStrikeMaxHits
            );


        float chance =
            Mathf.Clamp01(
                skill.lightningStrikeChance
            );


        float reduction =
            Mathf.Clamp01(
                skill.lightningStrikeDamageReduction
            );


        for (int hit = 0;
             hit < maxHits;
             hit++)
        {
            if (target == null)
                break;


            if (target.IsDying)
                break;


            CreateLightningStrikeEffect(
                target,
                skill
            );


            float lightningDamage =
                damage;


            target.TakeDamage(
                lightningDamage
            );


            if (target == null)
                break;


            if (target.IsDying)
                break;


            if (hit >= maxHits - 1)
                break;


            if (UnityEngine.Random.value > chance)
                break;


            damage *=
                1f - reduction;


            if (skill.lightningStrikeDelay > 0f)
            {
                yield return new WaitForSeconds(
                    skill.lightningStrikeDelay
                );
            }
        }


        lightningStrikeActive = false;


        if (skill != null)
        {
            skill.cooldownTimer =
                Mathf.Max(
                    0f,
                    skill.fireRate
                );
        }
    }


    // =========================================================
    // LIGHTNING STRIKE - LOSOWY CEL
    // =========================================================

    private EnemyHealth FindRandomLightningStrikeTarget(
        float range)
    {
        GameObject[] enemyObjects =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );


        List<EnemyHealth> validTargets =
            new List<EnemyHealth>();


        foreach (GameObject enemyObject in enemyObjects)
        {
            if (enemyObject == null)
                continue;


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


            float distance =
                Vector3.Distance(
                    transform.position,
                    enemy.transform.position
                );


            if (distance <= range)
            {
                if (!validTargets.Contains(enemy))
                {
                    validTargets.Add(
                        enemy
                    );
                }
            }
        }


        if (validTargets.Count == 0)
            return null;


        int randomIndex =
            Random.Range(
                0,
                validTargets.Count
            );


        return validTargets[
            randomIndex
        ];
    }


    // =========================================================
    // LIGHTNING STRIKE - EFEKT
    // =========================================================

    private void CreateLightningStrikeEffect(
        EnemyHealth target,
        SkillInstance skill)
    {
        if (target == null)
            return;


        if (skill == null ||
            skill.data == null)
        {
            return;
        }


        if (skill.lightningStrikeEffectPrefab == null)
        {
            return;
        }


        Transform followTarget =
            target.transform;


        Vector3 effectPosition =
            target.transform.position;


        if (target.hitPoint != null)
        {
            followTarget =
                target.hitPoint;


            effectPosition =
                target.hitPoint.position;
        }
        else
        {
            Renderer[] renderers =
                target.GetComponentsInChildren<Renderer>();


            if (renderers.Length > 0)
            {
                Bounds bounds =
                    renderers[0].bounds;


                for (int i = 1;
                     i < renderers.Length;
                     i++)
                {
                    bounds.Encapsulate(
                        renderers[i].bounds
                    );
                }


                effectPosition =
                    new Vector3(
                        bounds.center.x,
                        bounds.max.y,
                        bounds.center.z
                    );
            }
        }


        effectPosition.y = 0f;


        GameObject effect =
            Instantiate(
                skill.lightningStrikeEffectPrefab,
                effectPosition,
                Quaternion.identity
            );


        if (effect == null)
            return;


        StartCoroutine(
            FollowLightningStrikeAtGround(
                effect,
                followTarget
            )
        );


        ParticleSystem[] particles =
            effect.GetComponentsInChildren<ParticleSystem>();


        if (particles.Length > 0)
        {
            float maxDuration = 0f;


            foreach (ParticleSystem particle in particles)
            {
                if (particle == null)
                    continue;


                ParticleSystem.MainModule main =
                    particle.main;


                float duration =
                    main.duration;


                if (main.startLifetime.mode ==
                    ParticleSystemCurveMode.TwoConstants)
                {
                    duration +=
                        main.startLifetime.constantMax;
                }
                else
                {
                    duration +=
                        main.startLifetime.constant;
                }


                if (duration > maxDuration)
                {
                    maxDuration =
                        duration;
                }
            }


            Destroy(
                effect,
                Mathf.Max(
                    0.1f,
                    maxDuration
                )
            );
        }
        else
        {
            Destroy(
                effect,
                1f
            );
        }
    }


    // =========================================================
    // LIGHTNING STRIKE - ŚLEDZENIE X/Z
    // =========================================================

    private IEnumerator FollowLightningStrikeAtGround(
        GameObject effect,
        Transform target)
    {
        while (effect != null &&
               target != null)
        {
            Vector3 position =
                target.position;


            position.y = 0f;


            effect.transform.position =
                position;


            yield return null;
        }
    }


    // =========================================================
    // VOLCANIC SPHERE - BURST
    // =========================================================

    private IEnumerator ShootVolcanicSphereBurst(
        SkillInstance skill)
    {
        if (volcanicSphereBurstActive)
            yield break;


        if (skill == null ||
            skill.data == null)
        {
            yield break;
        }


        volcanicSphereBurstActive =
            true;


        List<EnemyHealth> enemies =
            GetVolcanicSphereTargets(
                skill
            );


        if (enemies.Count == 0)
        {
            volcanicSphereBurstActive =
                false;

            yield break;
        }


        List<EnemyHealth> availableTargets =
            new List<EnemyHealth>(
                enemies
            );


        int sphereCount =
            Mathf.Max(
                1,
                skill.volcanicSphereCount
            );


        for (int i = 0;
             i < sphereCount;
             i++)
        {
            if (availableTargets.Count == 0)
            {
                availableTargets =
                    new List<EnemyHealth>(
                        enemies
                    );
            }


            availableTargets.RemoveAll(
                enemy =>
                    enemy == null ||
                    enemy.IsDying
            );


            if (availableTargets.Count == 0)
                break;


            int randomIndex =
                Random.Range(
                    0,
                    availableTargets.Count
                );


            EnemyHealth target =
                availableTargets[
                    randomIndex
                ];


            availableTargets.RemoveAt(
                randomIndex
            );


            if (target == null ||
                target.IsDying ||
                !IsTargetInRange(
                    target.transform.position,
                    skill.range
                ))
            {
                i--;
                continue;
            }


            ShootVolcanicSphereProjectile(
                skill,
                target
            );


            if (i < sphereCount - 1)
            {
                yield return new WaitForSeconds(
                    VolcanicSphereShotDelay
                );
            }
        }


        volcanicSphereBurstActive =
            false;


        if (skill != null)
        {
            if (skill.fireRate > 0f)
            {
                skill.cooldownTimer =
                    skill.fireRate;
            }
            else
            {
                skill.cooldownTimer =
                    0f;
            }
        }
    }


    // =========================================================
    // VOLCANIC SPHERE - ZNAJDOWANIE CELÓW
    // =========================================================

    private List<EnemyHealth> GetVolcanicSphereTargets(
        SkillInstance skill)
    {
        List<EnemyHealth> enemies =
            new List<EnemyHealth>();


        if (skill == null ||
            skill.data == null)
        {
            return enemies;
        }


        GameObject[] enemyObjects =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );


        foreach (GameObject enemyObject in enemyObjects)
        {
            if (enemyObject == null)
                continue;


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


            if (!IsTargetInRange(
                    enemy.transform.position,
                    skill.range))
            {
                continue;
            }


            if (!enemies.Contains(enemy))
            {
                enemies.Add(
                    enemy
                );
            }
        }


        return enemies;
    }


    // =========================================================
    // VOLCANIC SPHERE - JEDEN POCISK
    // =========================================================

    private void ShootVolcanicSphereProjectile(
        SkillInstance skill,
        EnemyHealth target)
    {
        if (skill == null ||
            skill.data == null)
        {
            return;
        }


        if (target == null ||
            target.IsDying)
        {
            return;
        }


        if (!IsTargetInRange(
                target.transform.position,
                skill.range))
        {
            return;
        }


        if (firePoint == null)
        {
            return;
        }


        if (skill.data.volcanicSphereProjectilePrefab == null)
        {
            return;
        }


        GameObject projectileObject =
            Instantiate(
                skill.data.volcanicSphereProjectilePrefab,
                firePoint.position,
                Quaternion.identity
            );


        if (projectileObject == null)
            return;


        ProjectileBase projectileBase =
            projectileObject.GetComponent<ProjectileBase>();


        if (projectileBase == null)
        {
            Destroy(
                projectileObject
            );


            return;
        }


        projectileBase.Initialize(
            skill
        );


        ProjectileMovement movement =
            projectileObject.GetComponent<ProjectileMovement>();


        if (movement == null)
        {
            Destroy(
                projectileObject
            );


            return;
        }


        movement.SetSpeed(
            skill.volcanicSphereSpeed
        );


        movement.Initialize(
            target.transform
        );
    }


    // =========================================================
    // SPRAWDZANIE FROST NOVA
    // =========================================================

    private bool IsEnemyInFrostNovaRange(
        SkillInstance skill)
    {
        if (skill == null ||
            skill.data == null)
        {
            return false;
        }


        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );


        foreach (GameObject enemy in enemies)
        {
            if (enemy == null)
                continue;


            EnemyHealth enemyHealth =
                enemy.GetComponent<EnemyHealth>();


            if (enemyHealth == null)
                continue;


            if (enemyHealth.IsDying)
                continue;


            float distance =
                Vector3.Distance(
                    transform.position,
                    enemy.transform.position
                );


            if (distance <= skill.frostNovaRadius)
            {
                return true;
            }
        }


        return false;
    }


    // =========================================================
    // CHAIN LIGHTNING
    // =========================================================

    private IEnumerator ShootChainLightning(
        SkillInstance skill)
    {
        if (chainLightningActive)
            yield break;


        if (skill == null ||
            skill.data == null)
        {
            yield break;
        }


        chainLightningActive =
            true;


        EnemyHealth firstTarget =
            FindClosestChainLightningTarget(
                transform.position,
                skill.range,
                null,
                Vector3.forward,
                180f
            );


        if (firstTarget == null)
        {
            chainLightningActive =
                false;

            yield break;
        }


        CreateChainLightningEffectFromPoint(
            firePoint != null
                ? firePoint.position
                : transform.position,
            firstTarget.transform,
            skill
        );


        List<EnemyHealth> hitTargets =
            new List<EnemyHealth>();


        EnemyHealth currentTarget =
            firstTarget;


        float currentDamage =
            skill.damage;


        int maxTargets =
            Mathf.Max(
                1,
                skill.chainCount
            );


        for (int i = 0;
             i < maxTargets;
             i++)
        {
            if (currentTarget == null ||
                currentTarget.IsDying)
            {
                break;
            }


            if (hitTargets.Contains(
                    currentTarget))
            {
                break;
            }


            hitTargets.Add(
                currentTarget
            );


            currentTarget.TakeDamage(
                currentDamage
            );


            if (i >= maxTargets - 1)
                break;


            EnemyHealth nextTarget =
                FindNextChainLightningTarget(
                    currentTarget,
                    hitTargets,
                    skill
                );


            if (nextTarget == null)
                break;


            CreateChainLightningEffect(
                currentTarget.transform,
                nextTarget.transform,
                skill
            );


            currentDamage *=
                skill.chainDamageMultiplier;


            if (skill.chainLightningDelay > 0f)
            {
                yield return new WaitForSeconds(
                    skill.chainLightningDelay
                );
            }


            currentTarget =
                nextTarget;
        }


        chainLightningActive =
            false;


        if (skill != null)
        {
            if (skill.fireRate > 0f)
            {
                skill.cooldownTimer =
                    skill.fireRate;
            }
            else
            {
                skill.cooldownTimer =
                    0f;
            }
        }
    }


    // =========================================================
    // CHAIN LIGHTNING - PIERWSZY CEL
    // =========================================================

    private EnemyHealth FindClosestChainLightningTarget(
        Vector3 origin,
        float range,
        List<EnemyHealth> excludedTargets,
        Vector3 forwardDirection,
        float forwardAngle)
    {
        GameObject[] enemyObjects =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );


        EnemyHealth closestEnemy =
            null;


        float closestDistance =
            Mathf.Infinity;


        foreach (GameObject enemyObject in enemyObjects)
        {
            if (enemyObject == null)
                continue;


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


            if (excludedTargets != null &&
                excludedTargets.Contains(enemy))
            {
                continue;
            }


            float distance =
                Vector3.Distance(
                    origin,
                    enemy.transform.position
                );


            if (distance > range)
                continue;


            Vector3 direction =
                enemy.transform.position -
                origin;


            direction.y = 0f;


            if (direction.sqrMagnitude <= 0.0001f)
                continue;


            direction.Normalize();


            Vector3 flatForward =
                forwardDirection;


            flatForward.y = 0f;


            if (flatForward.sqrMagnitude <= 0.0001f)
            {
                flatForward =
                    Vector3.forward;
            }


            flatForward.Normalize();


            float angle =
                Vector3.Angle(
                    flatForward,
                    direction
                );


            if (angle > forwardAngle)
                continue;


            if (distance < closestDistance)
            {
                closestDistance =
                    distance;


                closestEnemy =
                    enemy;
            }
        }


        return closestEnemy;
    }


    // =========================================================
    // CHAIN LIGHTNING - NASTĘPNY CEL
    // =========================================================

    private EnemyHealth FindNextChainLightningTarget(
        EnemyHealth currentTarget,
        List<EnemyHealth> hitTargets,
        SkillInstance skill)
    {
        if (currentTarget == null)
            return null;


        GameObject[] enemyObjects =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );


        EnemyHealth bestTarget =
            null;


        float bestDistance =
            Mathf.Infinity;


        Vector3 origin =
            currentTarget.transform.position;


        Vector3 forwardDirection;


        if (hitTargets.Count >= 2)
        {
            EnemyHealth previousTarget =
                hitTargets[
                    hitTargets.Count - 2
                ];


            if (previousTarget != null)
            {
                forwardDirection =
                    currentTarget.transform.position -
                    previousTarget.transform.position;
            }
            else
            {
                forwardDirection =
                    transform.forward;
            }
        }
        else
        {
            forwardDirection =
                currentTarget.transform.position -
                transform.position;
        }


        forwardDirection.y = 0f;


        if (forwardDirection.sqrMagnitude <= 0.0001f)
        {
            forwardDirection =
                transform.forward;


            forwardDirection.y = 0f;
        }


        forwardDirection.Normalize();


        foreach (GameObject enemyObject in enemyObjects)
        {
            if (enemyObject == null)
                continue;


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


            if (enemy == currentTarget)
                continue;


            if (hitTargets.Contains(enemy))
                continue;


            Vector3 direction =
                enemy.transform.position -
                origin;


            direction.y = 0f;


            float distance =
                direction.magnitude;


            if (distance > skill.chainRange)
                continue;


            if (distance <= 0.001f)
                continue;


            direction.Normalize();


            float angle =
                Vector3.Angle(
                    forwardDirection,
                    direction
                );


            if (angle > skill.chainForwardAngle)
                continue;


            if (distance < bestDistance)
            {
                bestDistance =
                    distance;


                bestTarget =
                    enemy;
            }
        }


        return bestTarget;
    }


    // =========================================================
    // CHAIN LIGHTNING - EFEKT WRÓG -> WRÓG
    // =========================================================

    private void CreateChainLightningEffect(
        Transform startTarget,
        Transform endTarget,
        SkillInstance skill)
    {
        if (skill == null ||
            skill.data == null)
        {
            return;
        }


        if (startTarget == null ||
            endTarget == null)
        {
            return;
        }


        if (skill.chainLightningEffectPrefab == null)
        {
            return;
        }


        GameObject effect =
            Instantiate(
                skill.chainLightningEffectPrefab
            );


        if (effect == null)
            return;


        LightningChain lightning =
            effect.GetComponent<LightningChain>();


        if (lightning == null)
        {
            lightning =
                effect.GetComponentInChildren<LightningChain>();
        }


        if (lightning == null)
        {
            Destroy(
                effect
            );


            return;
        }


        lightning.Initialize(
            startTarget,
            endTarget
        );
    }


    // =========================================================
    // CHAIN LIGHTNING - EFEKT WIEŻA -> WRÓG
    // =========================================================

    private void CreateChainLightningEffectFromPoint(
        Vector3 startPosition,
        Transform endTarget,
        SkillInstance skill)
    {
        if (skill == null ||
            skill.data == null)
        {
            return;
        }


        if (endTarget == null)
            return;


        if (skill.chainLightningEffectPrefab == null)
        {
            return;
        }


        GameObject effect =
            Instantiate(
                skill.chainLightningEffectPrefab
            );


        if (effect == null)
            return;


        LightningChain lightning =
            effect.GetComponent<LightningChain>();


        if (lightning == null)
        {
            lightning =
                effect.GetComponentInChildren<LightningChain>();
        }


        if (lightning == null)
        {
            Destroy(
                effect
            );


            return;
        }


        lightning.InitializeFromPoint(
            startPosition,
            endTarget
        );
    }


    // =========================================================
    // SHOOT
    // =========================================================

    private void Shoot(
        SkillInstance skill)
    {
        if (skill == null ||
            skill.data == null)
        {
            return;
        }


        // =====================================================
        // LIGHTNING STRIKE
        // =====================================================

        if (skill.data.skillID ==
            "LightningStrike")
        {
            return;
        }


        // =====================================================
        // ELECTRIC SHOCK
        // =====================================================

        if (skill.data.skillID ==
            "ElectricShock")
        {
            ShootElectricShock(
                skill
            );

            return;
        }


        // =====================================================
        // ENERGY ORB
        // =====================================================

        if (skill.data.skillID ==
            "EnergyOrb")
        {
            ShootEnergyOrb(
                skill
            );

            return;
        }


        // =====================================================
        // SPINNING LASER
        // =====================================================

        if (skill.data.skillID ==
            "SpinningLaser")
        {
            ShootSpinningLaser(
                skill
            );

            return;
        }


        // =====================================================
        // DEVOURING CLOUD
        // =====================================================

        if (skill.data.skillID ==
            "DevouringCloud")
        {
            DevouringCloud cloud =
                GetComponent<DevouringCloud>();


            if (cloud == null)
            {
                return;
            }


            cloud.CreateCloud(
                skill
            );


            return;
        }


        // =====================================================
        // WHIRL OF CHAOS
        // =====================================================

        if (skill.data.skillID ==
            "WhirlOfChaos")
        {
            WhirlOfChaos whirl =
                GetComponent<WhirlOfChaos>();


            if (whirl == null)
            {
                return;
            }


            whirl.CreateWhirl(
                skill
            );


            return;
        }


        // =====================================================
        // BLIZZARD
        // =====================================================

        if (skill.data.skillID == "Blizzard")
        {
            if (skill.data.blizzardEffectPrefab == null)
            {
                return;
            }


            GameObject[] enemies =
                GameObject.FindGameObjectsWithTag(
                    "Enemy"
                );


            Vector3 blizzardPosition =
                transform.position;


            int bestEnemyCount = 0;


            foreach (GameObject enemy in enemies)
            {
                if (enemy == null)
                    continue;


                EnemyHealth enemyHealth =
                    enemy.GetComponent<EnemyHealth>();


                if (enemyHealth == null)
                    continue;


                if (enemyHealth.IsDying)
                    continue;


                if (!IsTargetInRange(
                        enemy.transform.position,
                        skill.range))
                {
                    continue;
                }


                Vector3 candidatePosition =
                    enemy.transform.position;


                int enemyCount = 0;


                foreach (GameObject otherEnemy in enemies)
                {
                    if (otherEnemy == null)
                        continue;


                    EnemyHealth otherHealth =
                        otherEnemy.GetComponent<EnemyHealth>();


                    if (otherHealth == null)
                        continue;


                    if (otherHealth.IsDying)
                        continue;


                    if (!IsTargetInRange(
                            otherEnemy.transform.position,
                            skill.range))
                    {
                        continue;
                    }


                    float distance =
                        Vector3.Distance(
                            candidatePosition,
                            otherEnemy.transform.position
                        );


                    if (distance <= skill.blizzardRadius)
                    {
                        enemyCount++;
                    }
                }


                if (enemyCount > bestEnemyCount)
                {
                    bestEnemyCount =
                        enemyCount;


                    blizzardPosition =
                        candidatePosition;
                }
            }


            if (bestEnemyCount <= 0)
                return;


            GameObject blizzardObject =
                Instantiate(
                    skill.data.blizzardEffectPrefab,
                    blizzardPosition,
                    Quaternion.identity
                );


            if (blizzardObject == null)
                return;


            BlizzardEffect blizzard =
                blizzardObject.GetComponent<BlizzardEffect>();


            if (blizzard == null)
            {
                Destroy(
                    blizzardObject
                );


                return;
            }


            blizzard.Initialize(
                skill
            );


            return;
        }


        // =====================================================
        // RAIN OF FIRE
        // =====================================================

        if (skill.data.skillID == "RainOfFire")
        {
            if (activeRainOfFire != null)
            {
                return;
            }


            if (skill.data.rainOfFireEffectPrefab == null)
            {
                return;
            }


            GameObject[] enemies =
                GameObject.FindGameObjectsWithTag(
                    "Enemy"
                );


            Vector3 rainOfFirePosition =
                transform.position;


            int bestEnemyCount = 0;


            foreach (GameObject enemy in enemies)
            {
                if (enemy == null)
                    continue;


                EnemyHealth enemyHealth =
                    enemy.GetComponent<EnemyHealth>();


                if (enemyHealth == null)
                    continue;


                if (enemyHealth.IsDying)
                    continue;


                if (!IsTargetInRange(
                        enemy.transform.position,
                        skill.range))
                {
                    continue;
                }


                Vector3 candidatePosition =
                    enemy.transform.position;


                int enemyCount = 0;


                foreach (GameObject otherEnemy in enemies)
                {
                    if (otherEnemy == null)
                        continue;


                    EnemyHealth otherHealth =
                        otherEnemy.GetComponent<EnemyHealth>();


                    if (otherHealth == null)
                        continue;


                    if (otherHealth.IsDying)
                        continue;


                    if (!IsTargetInRange(
                            otherEnemy.transform.position,
                            skill.range))
                    {
                        continue;
                    }


                    float distance =
                        Vector3.Distance(
                            candidatePosition,
                            otherEnemy.transform.position
                        );


                    if (distance <= skill.rainOfFireRadius)
                    {
                        enemyCount++;
                    }
                }


                if (enemyCount > bestEnemyCount)
                {
                    bestEnemyCount =
                        enemyCount;


                    rainOfFirePosition =
                        candidatePosition;
                }
            }


            if (bestEnemyCount <= 0)
                return;


            GameObject rainOfFireObject =
                Instantiate(
                    skill.data.rainOfFireEffectPrefab,
                    rainOfFirePosition,
                    Quaternion.identity
                );


            if (rainOfFireObject == null)
                return;


            RainOfFireEffect rainOfFire =
                rainOfFireObject.GetComponent<RainOfFireEffect>();


            if (rainOfFire == null)
            {
                Destroy(
                    rainOfFireObject
                );


                return;
            }


            activeRainOfFire =
                rainOfFire;


            rainOfFire.CreateRainOfFire(
                skill,
                rainOfFirePosition
            );


            return;
        }


        // =====================================================
        // VOLCANIC SPHERE
        // =====================================================

        if (skill.data.skillID == "VolcanicSphere")
        {
            return;
        }


        // =====================================================
        // SLIME
        // =====================================================

        if (skill.data.skillID ==
            "Slime")
        {
            ShootSlime(
                skill
            );


            return;
        }


        // =====================================================
        // FLAMING CIRCLE
        // =====================================================

        if (skill.data.skillID ==
            "FlamingCircle")
        {
            ShootFlamingCircle(
                skill
            );


            return;
        }


        // =====================================================
        // SZUKAMY NAJBLIŻSZEGO WROGA
        // =====================================================

        GameObject[] enemyObjects =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );


        GameObject closestEnemy =
            null;


        float closestDistance =
            Mathf.Infinity;


        foreach (GameObject enemy in enemyObjects)
        {
            if (enemy == null)
                continue;


            EnemyHealth enemyHealth =
                enemy.GetComponent<EnemyHealth>();


            if (enemyHealth == null)
                continue;


            if (enemyHealth.IsDying)
                continue;


            float distance =
                Vector3.Distance(
                    transform.position,
                    enemy.transform.position
                );


            if (distance <= skill.range &&
                distance < closestDistance)
            {
                closestDistance =
                    distance;


                closestEnemy =
                    enemy;
            }
        }


        if (closestEnemy == null)
            return;


        // =====================================================
        // ICE SPIKES
        // =====================================================

        if (skill.data.skillID ==
            "IceSpikes")
        {
            ShootIceSpikes(
                skill,
                closestEnemy.transform
            );


            return;
        }


        // =====================================================
        // FREEZING WAVE
        // =====================================================

        if (skill.data.skillID ==
            "FreezingWave")
        {
            ShootFreezingWave(
                skill,
                closestEnemy.transform
            );


            return;
        }


        // =====================================================
        // ICE SHOT
        // =====================================================

        if (skill.data.skillID == "IceShot")
        {
            ShootIceShot(
                skill,
                closestEnemy.transform
            );


            return;
        }


        // =====================================================
        // SEARING SHOT
        // =====================================================

        if (skill.data.skillID == "SearingShot")
        {
            ShootSearingShot(
                skill,
                closestEnemy.transform
            );


            return;
        }


        // =====================================================
        // BRAK NORMALNEGO PREFABU
        // =====================================================

        if (skill.data.projectilePrefab == null)
            return;


        // =====================================================
        // SHOTGUN
        // =====================================================

        ShotgunProjectile shotgun =
            skill.data.projectilePrefab
                .GetComponent<ShotgunProjectile>();


        if (shotgun != null)
        {
            ShootShotgun(
                skill,
                closestEnemy
            );


            return;
        }


        // =====================================================
        // NORMALNY POCISK
        // =====================================================

        ShootNormalProjectile(
            skill,
            closestEnemy.transform
        );
    }


    // =========================================================
    // ELECTRIC SHOCK
    // =========================================================

    private void ShootElectricShock(
        SkillInstance skill)
    {
        if (skill == null ||
            skill.data == null)
        {
            return;
        }


        if (firePoint == null)
        {
            return;
        }


        if (skill.data.electricShockProjectilePrefab == null)
        {
            return;
        }


        int projectileCount =
            skill.GetProjectileCount();


        List<EnemyHealth> availableTargets =
            GetValidTargetsInRange(
                skill.range
            );


        if (availableTargets.Count == 0)
            return;


        int shotsToFire =
            Mathf.Min(
                projectileCount,
                availableTargets.Count
            );


        for (int i = 0;
             i < shotsToFire;
             i++)
        {
            int randomIndex =
                Random.Range(
                    0,
                    availableTargets.Count
                );


            EnemyHealth target =
                availableTargets[randomIndex];


            availableTargets.RemoveAt(
                randomIndex
            );


            if (target == null)
                continue;


            GameObject projectileObject =
                Instantiate(
                    skill.data.electricShockProjectilePrefab,
                    firePoint.position,
                    Quaternion.identity
                );


            if (projectileObject == null)
                continue;


            ElectricShockProjectile projectile =
                projectileObject.GetComponent<ElectricShockProjectile>();


            if (projectile == null)
            {
                Destroy(
                    projectileObject
                );


                continue;
            }


            projectile.InitializeElectricShock(
                skill,
                target
            );
        }
    }


    // =========================================================
    // ENERGY ORB
    // =========================================================

    private void ShootEnergyOrb(
        SkillInstance skill)
    {
        if (skill == null ||
            skill.data == null)
        {
            return;
        }


        if (firePoint == null)
        {
            return;
        }


        if (skill.energyOrbProjectilePrefab == null)
        {
            return;
        }


        int projectileCount =
            skill.GetProjectileCount();


        List<EnemyHealth> availableTargets =
            GetValidTargetsInRange(
                skill.range
            );


        if (availableTargets.Count == 0)
            return;


        int shotsToFire =
            Mathf.Min(
                projectileCount,
                availableTargets.Count
            );


        for (int i = 0;
             i < shotsToFire;
             i++)
        {
            int randomIndex =
                Random.Range(
                    0,
                    availableTargets.Count
                );


            EnemyHealth target =
                availableTargets[randomIndex];


            availableTargets.RemoveAt(
                randomIndex
            );


            if (target == null)
                continue;


            GameObject projectileObject =
                Instantiate(
                    skill.energyOrbProjectilePrefab,
                    firePoint.position,
                    Quaternion.identity
                );


            if (projectileObject == null)
                continue;


            EnergyOrbProjectile projectile =
                projectileObject.GetComponent<EnergyOrbProjectile>();


            if (projectile == null)
            {
                Destroy(
                    projectileObject
                );


                continue;
            }


            projectile.InitializeEnergyOrb(
                skill,
                target
            );
        }
    }


    // =========================================================
    // WSPÓLNE - POPRAWNE CELE W ZASIĘGU
    // =========================================================

    private List<EnemyHealth> GetValidTargetsInRange(
        float range)
    {
        List<EnemyHealth> targets =
            new List<EnemyHealth>();


        GameObject[] enemyObjects =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );


        foreach (GameObject enemyObject in enemyObjects)
        {
            if (enemyObject == null)
                continue;


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


            float distance =
                Vector3.Distance(
                    transform.position,
                    enemy.transform.position
                );


            if (distance > range)
                continue;


            if (!targets.Contains(enemy))
            {
                targets.Add(
                    enemy
                );
            }
        }


        return targets;
    }


    // =========================================================
    // WSPÓLNE - SPRAWDZENIE RANGE
    // =========================================================

    private bool IsTargetInRange(
        Vector3 targetPosition,
        float range)
    {
        if (range < 0f)
            return false;


        float distance =
            Vector3.Distance(
                transform.position,
                targetPosition
            );


        return distance <= range;
    }


    // =========================================================
    // FLAMING CIRCLE
    // =========================================================

    private void ShootFlamingCircle(
        SkillInstance skill)
    {
        if (skill == null ||
            skill.data == null)
        {
            return;
        }


        if (firePoint == null)
            return;


        if (skill.flamingCirclePrefab == null)
        {
            return;
        }


        int projectileCount =
            skill.GetProjectileCount();


        for (int i = 0;
             i < projectileCount;
             i++)
        {
            float angle =
                (360f / projectileCount) * i;


            Vector3 direction =
                Quaternion.Euler(
                    0f,
                    angle,
                    0f
                ) *
                Vector3.forward;


            GameObject flamingCircleObject =
                Instantiate(
                    skill.flamingCirclePrefab,
                    transform.position,
                    Quaternion.LookRotation(
                        direction
                    )
                );


            if (flamingCircleObject == null)
                continue;


            FlamingCircle flamingCircle =
                flamingCircleObject.GetComponent<FlamingCircle>();


            if (flamingCircle == null)
            {
                Destroy(
                    flamingCircleObject
                );


                continue;
            }


            flamingCircle.Initialize(
                skill
            );
        }
    }


    // =========================================================
    // FROST NOVA
    // =========================================================

    private void ShootFrostNova(
        SkillInstance skill)
    {
        if (skill == null ||
            skill.data == null)
        {
            return;
        }


        if (skill.frostNovaEffectPrefab == null)
        {
            return;
        }


        GameObject frostNovaObject =
            Instantiate(
                skill.frostNovaEffectPrefab,
                transform.position,
                Quaternion.identity
            );


        if (frostNovaObject == null)
            return;


        FrostNova frostNova =
            frostNovaObject.GetComponent<FrostNova>();


        if (frostNova == null)
        {
            Destroy(
                frostNovaObject
            );


            return;
        }


        frostNova.Initialize(
            skill
        );
    }


    // =========================================================
    // ICE SPIKES
    // =========================================================

    private void ShootIceSpikes(
        SkillInstance skill,
        Transform target)
    {
        if (skill == null ||
            skill.data == null)
        {
            return;
        }


        if (firePoint == null)
        {
            return;
        }


        if (target == null)
            return;


        if (skill.iceSpikesProjectilePrefab == null)
        {
            return;
        }


        int projectileCount =
            skill.GetProjectileCount();


        for (int i = 0;
             i < projectileCount;
             i++)
        {
            GameObject projectileObject =
                Instantiate(
                    skill.iceSpikesProjectilePrefab,
                    firePoint.position,
                    Quaternion.identity
                );


            if (projectileObject == null)
                continue;


            IceSpikesProjectile projectile =
                projectileObject.GetComponent<IceSpikesProjectile>();


            if (projectile == null)
            {
                Destroy(
                    projectileObject
                );


                continue;
            }


            projectile.Initialize(
                skill
            );


            ProjectileMovement movement =
                projectileObject.GetComponent<ProjectileMovement>();


            if (movement == null)
            {
                Destroy(
                    projectileObject
                );


                continue;
            }


            movement.Initialize(
                target
            );
        }
    }


    // =========================================================
    // SLIME
    // =========================================================

    private void ShootSlime(
        SkillInstance skill)
    {
        if (skill == null ||
            skill.data == null)
        {
            return;
        }


        if (firePoint == null)
            return;


        if (skill.slimeProjectilePrefab == null)
        {
            return;
        }


        int slimeCount =
            Mathf.Max(
                1,
                skill.slimeCount
            );


        float spread =
            Mathf.Max(
                0f,
                skill.slimeSpreadAngle
            );


        float angleStep =
            spread / slimeCount;


        float randomStartAngle =
            Random.Range(
                0f,
                360f
            );


        for (int i = 0;
             i < slimeCount;
             i++)
        {
            float angle =
                randomStartAngle +
                angleStep * i;


            Vector3 direction =
                Quaternion.Euler(
                    0f,
                    angle,
                    0f
                ) *
                Vector3.forward;


            direction.y = 0f;


            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction =
                    Vector3.forward;
            }


            direction.Normalize();


            GameObject slimeObject =
                Instantiate(
                    skill.slimeProjectilePrefab,
                    firePoint.position,
                    Quaternion.LookRotation(
                        direction
                    )
                );


            if (slimeObject == null)
                continue;


            SlimeProjectile slime =
                slimeObject.GetComponent<SlimeProjectile>();


            ProjectileBase projectile =
                slimeObject.GetComponent<ProjectileBase>();


            if (slime == null ||
                projectile == null)
            {
                Destroy(
                    slimeObject
                );


                continue;
            }


            projectile.Initialize(
                skill
            );


            slime.SetDirection(
                direction
            );
        }
    }


    // =========================================================
    // ICE SHOT
    // =========================================================

    private void ShootIceShot(
        SkillInstance skill,
        Transform target)
    {
        if (skill == null ||
            skill.data == null)
        {
            return;
        }


        if (firePoint == null)
        {
            return;
        }


        if (target == null)
            return;


        if (skill.data.iceShotProjectilePrefab == null)
        {
            return;
        }


        Vector3 direction =
            target.position -
            firePoint.position;


        direction.y = 0f;


        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction =
                transform.forward;


            direction.y = 0f;
        }


        direction.Normalize();


        int projectileCount =
            skill.GetProjectileCount();


        float spawnOffset =
            1.8f;


        for (int i = 0;
             i < projectileCount;
             i++)
        {
            Vector3 spawnPosition =
                firePoint.position +
                direction * spawnOffset;


            GameObject projectileObject =
                Instantiate(
                    skill.data.iceShotProjectilePrefab,
                    spawnPosition,
                    Quaternion.identity
                );


            if (projectileObject == null)
                continue;


            IceShotProjectile projectile =
                projectileObject.GetComponent<IceShotProjectile>();


            if (projectile == null)
            {
                Destroy(
                    projectileObject
                );


                continue;
            }


            projectile.Initialize(
                skill
            );


            ProjectileMovement movement =
                projectileObject.GetComponent<ProjectileMovement>();


            if (movement == null)
            {
                Destroy(
                    projectileObject
                );


                continue;
            }


            movement.InitializeGroundRoll(
                target
            );
        }
    }


    // =========================================================
    // NORMALNY POCISK
    // =========================================================

    private void ShootNormalProjectile(
        SkillInstance skill,
        Transform target)
    {
        if (firePoint == null)
            return;


        if (skill.projectilePrefab == null)
            return;


        int projectileCount =
            skill.GetProjectileCount();


        for (int i = 0;
             i < projectileCount;
             i++)
        {
            GameObject projectileObject =
                Instantiate(
                    skill.projectilePrefab,
                    firePoint.position,
                    Quaternion.identity
                );


            if (projectileObject == null)
                continue;


            ProjectileBase projectile =
                projectileObject.GetComponent<ProjectileBase>();


            if (projectile == null)
            {
                Destroy(
                    projectileObject
                );


                continue;
            }


            projectile.Initialize(
                skill
            );


            ShurikenProjectile shuriken =
                projectileObject.GetComponent<ShurikenProjectile>();


            if (shuriken != null)
            {
                shuriken.SetFirstTarget(
                    target
                );


                continue;
            }


            SpearProjectile spear =
                projectileObject.GetComponent<SpearProjectile>();


            if (spear != null)
            {
                Vector3 direction =
                    target.position -
                    firePoint.position;


                direction.y = 0f;


                if (direction.sqrMagnitude <=
                    0.0001f)
                {
                    direction =
                        transform.forward;
                }


                spear.SetDirection(
                    direction
                );


                continue;
            }


            ProjectileMovement movement =
                projectileObject.GetComponent<ProjectileMovement>();


            if (movement == null)
            {
                Destroy(
                    projectileObject
                );


                continue;
            }


            movement.Initialize(
                target
            );
        }
    }


    // =========================================================
    // SHOTGUN
    // =========================================================

    private void ShootShotgun(
        SkillInstance skill,
        GameObject target)
    {
        if (firePoint == null)
            return;


        if (skill.projectilePrefab == null)
            return;


        int projectileCount =
            Mathf.Max(
                1,
                skill.shotgunProjectileCount
            );


        float spreadAngle =
            skill.spreadAngle;


        Vector3 direction =
            target.transform.position -
            firePoint.position;


        direction.y = 0f;


        if (direction.sqrMagnitude <=
            0.0001f)
        {
            direction =
                transform.forward;
        }


        direction.Normalize();


        for (int i = 0;
             i < projectileCount;
             i++)
        {
            float angle;


            if (projectileCount == 1)
            {
                angle = 0f;
            }
            else
            {
                float step =
                    spreadAngle /
                    (projectileCount - 1);


                angle =
                    -spreadAngle / 2f +
                    step * i;
            }


            Vector3 shotDirection =
                Quaternion.AngleAxis(
                    angle,
                    Vector3.up
                ) *
                direction;


            shotDirection.Normalize();


            Vector3 endpoint =
                firePoint.position +
                shotDirection *
                skill.range;


            GameObject projectileObject =
                Instantiate(
                    skill.projectilePrefab,
                    firePoint.position,
                    Quaternion.identity
                );


            if (projectileObject == null)
                continue;


            ProjectileBase projectile =
                projectileObject.GetComponent<ProjectileBase>();


            ProjectileMovement movement =
                projectileObject.GetComponent<ProjectileMovement>();


            if (projectile == null ||
                movement == null)
            {
                Destroy(
                    projectileObject
                );


                continue;
            }


            projectile.Initialize(
                skill
            );


            movement.Initialize(
                endpoint
            );
        }
    }


    // =========================================================
    // FREEZING WAVE
    // =========================================================

    private void ShootFreezingWave(
        SkillInstance skill,
        Transform target)
    {
        if (skill == null ||
            skill.data == null)
        {
            return;
        }


        if (firePoint == null)
        {
            return;
        }


        if (target == null)
            return;


        if (skill.data.freezingWavePrefab == null)
        {
            return;
        }


        Vector3 direction =
            target.position -
            firePoint.position;


        direction.y = 0f;


        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction =
                transform.forward;
        }


        direction.Normalize();


        int projectileCount =
            skill.GetProjectileCount();


        for (int i = 0;
             i < projectileCount;
             i++)
        {
            GameObject waveObject =
                Instantiate(
                    skill.data.freezingWavePrefab,
                    firePoint.position,
                    Quaternion.LookRotation(
                        direction
                    )
                );


            if (waveObject == null)
                continue;


            FreezingWave wave =
                waveObject.GetComponent<FreezingWave>();


            if (wave == null)
            {
                Destroy(
                    waveObject
                );


                continue;
            }


            wave.Initialize(
                skill,
                direction
            );
        }
    }


    // =========================================================
    // SEARING SHOT
    // =========================================================

    private void ShootSearingShot(
        SkillInstance skill,
        Transform target)
    {
        if (skill == null ||
            skill.data == null)
        {
            return;
        }


        if (firePoint == null)
        {
            return;
        }


        if (target == null)
            return;


        if (skill.data.searingShotProjectilePrefab == null)
        {
            return;
        }


        int projectileCount =
            skill.GetProjectileCount();


        for (int i = 0;
             i < projectileCount;
             i++)
        {
            GameObject projectileObject =
                Instantiate(
                    skill.data.searingShotProjectilePrefab,
                    firePoint.position,
                    Quaternion.identity
                );


            if (projectileObject == null)
                continue;


            SearingShotProjectile projectile =
                projectileObject.GetComponent<SearingShotProjectile>();


            if (projectile == null)
            {
                Destroy(
                    projectileObject
                );


                continue;
            }


            projectile.Initialize(
                skill
            );


            ProjectileMovement movement =
                projectileObject.GetComponent<ProjectileMovement>();


            if (movement == null)
            {
                Destroy(
                    projectileObject
                );


                continue;
            }


            movement.Initialize(
                target
            );
        }
    }
}