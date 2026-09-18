using UnityEngine;

[System.Serializable]
public class SkillInstance
{
    // =========================================================
    // PODSTAWOWE
    // =========================================================

    public SkillData data;

    public int level = 1;

    public float cooldownTimer;


    // =========================================================
    // AKTUALNE PARAMETRY SKILLA
    // =========================================================

    public float damage;
    public float fireRate;
    public float range;
    public float explosionRadius;


    // =========================================================
    // PROJECTILE COUNT
    // =========================================================

    // Bazowa liczba pocisków.
    // Aktualną liczbę należy pobierać przez GetProjectileCount().
    public int projectileCount;

    // Sposób zwiększania liczby pocisków.
    //
    // 0    = brak wzrostu
    // 1.1  = +1 pocisk co 10 poziomów
    // 1.2  = +1 pocisk co 5 poziomów
    // 1.25 = +1 pocisk co 4 poziomy
    // 1.5  = +1 pocisk co 2 poziomy
    // 2.0  = +1 pocisk co 1 poziom
    public float projectileCountGrowth;

// =========================================================
// PIERCING COUNT
// =========================================================

// Liczba przeciwników, przez których może przejść atak.
public int piercingCount;


    // =========================================================
    // CC
    // =========================================================

    public float rootDuration;
    public float slowPercent;
    public float slowDuration;


    // =========================================================
    // CHAIN
    // =========================================================

    public int chainCount;
    public float chainRange;
    public float chainDamageMultiplier;
    public float chainForwardAngle;
    public GameObject chainLightningEffectPrefab;
    public float chainLightningDelay;


    // =========================================================
    // LIGHTNING ARROW
    // =========================================================

    public float lightningStunChance;
    public float lightningStunDuration;
    public float lightningBonusDamage;


    // =========================================================
    // LIGHTNING STRIKE
    // =========================================================

    public GameObject lightningStrikeEffectPrefab;

    public float lightningStrikeChance;
    public int lightningStrikeMaxHits;
    public float lightningStrikeDamageReduction;
    public float lightningStrikeDelay;


    // =========================================================
    // ELECTRIC SHOCK
    // =========================================================

    public GameObject electricShockProjectilePrefab;

    public float electricShockDamage;
    public float electricShockStunDuration;
    public float electricShockProjectileSpeed;
    public float electricShockRadius;


    // =========================================================
    // ENERGY ORB
    // =========================================================

    public GameObject energyOrbProjectilePrefab;

    public float energyOrbProjectileSpeed;
    public float energyOrbHitRadius;
    public float energyOrbKnockback;


    // =========================================================
    // SPINNING LASER
    // =========================================================

    public GameObject spinningLaserPrefab;

    public int spinningLaserCount;
    public float spinningLaserRadius;
    public float spinningLaserRotationSpeed;
    public float spinningLaserDamage;
    public float spinningLaserDamageInterval;
    public float spinningLaserWidth;


    // =========================================================
    // DOT
    // =========================================================

    public float dotDamage;
    public float dotDuration;
    public float dotTickInterval;


    // =========================================================
    // SHOTGUN
    // =========================================================

    // Osobna liczba pocisków dla Shotguna.
    // Nie używa już projectileCount.
    public int shotgunProjectileCount;

    public float spreadAngle;


    // =========================================================
    // CORRUPTED
    // =========================================================

    public float corruptedSlowPercent;
    public float corruptedAttackSpeedReduction;
    public float corruptedDamageTakenIncrease;
    public float corruptedDuration;
    public int corruptedMaxStacks;
    public int corruptedMaxTargets;
    public float corruptedChainRange;


    // =========================================================
    // DEVOURING CLOUD
    // =========================================================

    public float cloudRadius;
    public float cloudDuration;
    public float cloudTickInterval;


    // =========================================================
    // WHIRL OF CHAOS
    // =========================================================

    public float whirlRadius;
    public float whirlDuration;
    public float whirlTickInterval;
    public float whirlPullSpeed;
    public float whirlDamage;


    // =========================================================
    // SLIME
    // =========================================================

    public int slimeCount;
    public float slimeRadius;
    public float slimeSlowPercent;
    public float slimeDotDamage;
    public float slimeDotDuration;
    public float slimeDotTickInterval;
    public float slimeSpreadAngle;


    // =========================================================
    // BLIZZARD
    // =========================================================

    public float blizzardRadius;
    public float blizzardDuration;
    public float blizzardTickInterval;
    public float blizzardDamage;
    public float blizzardSlowPercent;


    // =========================================================
    // ICE SPIKES
    // =========================================================

    public int iceSpikeCount;
    public float iceSpikeSpeed;
    public float iceSpikeDamageMultiplier;
    public float iceSpikeLifetime;


    // =========================================================
    // FREEZING WAVE
    // =========================================================

    public float freezingWaveRadius;
    public float freezingWaveSpeed;
    public float freezingWaveDistance;
    public float freezingWaveFreezeDuration;
    public float freezingWaveDamage;


    // =========================================================
    // FROST NOVA
    // =========================================================

    public float frostNovaRadius;
    public float frostNovaDamage;
    public float frostNovaFreezeDuration;
    public float frostNovaCooldown;


    // =========================================================
    // ICE SHOT
    // =========================================================

    public float iceShotDamage;
    public float iceShotSlowPercent;
    public float iceShotSlowDuration;
    public float iceShotRadius;


    // =========================================================
    // BREATH OF FIRE
    // =========================================================

    public float breathOfFireRange;
    public float breathOfFireAngle;
    public float breathOfFireDamage;
    public float breathOfFireTickInterval;
    public float breathOfFireBurnDamage;
    public float breathOfFireBurnDuration;
    public float breathOfFireBurnTickInterval;


    // =========================================================
    // FLAMING CIRCLE
    // =========================================================

    public float flamingCircleDamage;
    public float flamingCircleRadius;
    public float flamingCircleDuration;
    public float flamingCircleTickInterval;


    // =========================================================
    // SEARING SHOT
    // =========================================================

    public float searingShotDamage;
    public float searingShotBurnDamage;
    public float searingShotBurnDuration;
    public float searingShotBurnTickInterval;
    public float searingShotExplosionDamage;
    public float searingShotExplosionRadius;


    // =========================================================
    // RAIN OF FIRE
    // =========================================================

    public float rainOfFireRadius;
    public float rainOfFireDuration;
    public float rainOfFireTickInterval;
    public float rainOfFireDamage;
    public float rainOfFireBurnDamage;
    public float rainOfFireBurnDuration;
    public float rainOfFireBurnTickInterval;


    // =========================================================
    // VOLCANIC SPHERE
    // =========================================================

    public int volcanicSphereCount;
    public float volcanicSphereDamage;
    public float volcanicSphereSpeed;


    // =========================================================
    // KONSTRUKTOR
    // =========================================================

    public SkillInstance(
        SkillData skill)
    {
        data = skill;

        level = 1;

        cooldownTimer = 0f;

        CopyValuesFromData();
    }


    // =========================================================
    // COPY DATA
    // =========================================================

    private void CopyValuesFromData()
    {
        if (data == null)
            return;


        // =====================================================
        // ATAK
        // =====================================================

        damage =
            data.damage;

        fireRate =
            data.fireRate;

        range =
            data.range;

        explosionRadius =
            data.explosionRadius;


        // =====================================================
        // PROJECTILE COUNT
        // =====================================================

        projectileCount =
            data.projectileCount;

        projectileCountGrowth =
            data.projectileCountGrowth;


// =====================================================
// PIERCING
// =====================================================

piercingCount = data.piercingCount;

piercingCount = Mathf.Max(1, piercingCount);


        // =====================================================
        // CC
        // =====================================================

        rootDuration =
            data.rootDuration;

        slowPercent =
            data.slowPercent;

        slowDuration =
            data.slowDuration;


        // =====================================================
        // CHAIN
        // =====================================================

        chainCount =
            data.chainCount;

        chainRange =
            data.chainRange;

        chainDamageMultiplier =
            data.chainDamageMultiplier;

        chainForwardAngle =
            data.chainForwardAngle;

        chainLightningEffectPrefab =
            data.chainLightningEffectPrefab;

        chainLightningDelay =
            data.chainLightningDelay;


        // =====================================================
        // LIGHTNING ARROW
        // =====================================================

        lightningStunChance =
            data.lightningStunChance;

        lightningStunDuration =
            data.lightningStunDuration;

        lightningBonusDamage =
            data.lightningBonusDamage;


        // =====================================================
        // LIGHTNING STRIKE
        // =====================================================

        lightningStrikeEffectPrefab =
            data.lightningStrikeEffectPrefab;

        lightningStrikeChance =
            data.lightningStrikeChance;

        lightningStrikeMaxHits =
            data.lightningStrikeMaxHits;

        lightningStrikeDamageReduction =
            data.lightningStrikeDamageReduction;

        lightningStrikeDelay =
            data.lightningStrikeDelay;


        // =====================================================
        // ELECTRIC SHOCK
        // =====================================================

        electricShockProjectilePrefab =
            data.electricShockProjectilePrefab;

        electricShockDamage =
            data.electricShockDamage;

        electricShockStunDuration =
            data.electricShockStunDuration;

        electricShockProjectileSpeed =
            data.electricShockProjectileSpeed;
        electricShockRadius = data.electricShockRadius;


        // =====================================================
        // ENERGY ORB
        // =====================================================

        energyOrbProjectilePrefab =
            data.energyOrbProjectilePrefab;

        energyOrbProjectileSpeed =
            data.energyOrbProjectileSpeed;

        energyOrbHitRadius =
            data.energyOrbHitRadius;

        energyOrbKnockback =
            data.energyOrbKnockback;


        // =====================================================
        // SPINNING LASER
        // =====================================================

        spinningLaserPrefab =
            data.spinningLaserPrefab;

        spinningLaserCount =
            data.spinningLaserCount;

        spinningLaserRadius =
            data.spinningLaserRadius;

        spinningLaserRotationSpeed =
            data.spinningLaserRotationSpeed;

        spinningLaserDamage =
            data.spinningLaserDamage;

        spinningLaserDamageInterval =
            data.spinningLaserDamageInterval;

        spinningLaserWidth = data.spinningLaserWidth;


        // =====================================================
        // DOT
        // =====================================================

        dotDamage =
            data.dotDamage;

        dotDuration =
            data.dotDuration;

        dotTickInterval =
            data.dotTickInterval;


        // =====================================================
        // SHOTGUN
        // =====================================================

        shotgunProjectileCount =
            data.shotgunProjectileCount;

        spreadAngle =
            data.spreadAngle;


        // =====================================================
        // CORRUPTED
        // =====================================================

        corruptedSlowPercent =
            data.corruptedSlowPercent;

        corruptedAttackSpeedReduction =
            data.corruptedAttackSpeedReduction;

        corruptedDamageTakenIncrease =
            data.corruptedDamageTakenIncrease;

        corruptedDuration =
            data.corruptedDuration;

        corruptedMaxStacks =
            data.corruptedMaxStacks;

        corruptedMaxTargets =
            data.corruptedMaxTargets;

        corruptedChainRange =
            data.corruptedChainRange;


        // =====================================================
        // DEVOURING CLOUD
        // =====================================================

        cloudRadius =
            data.cloudRadius;

        cloudDuration =
            data.cloudDuration;

        cloudTickInterval =
            data.cloudTickInterval;


        // =====================================================
        // WHIRL OF CHAOS
        // =====================================================

        whirlRadius =
            data.whirlRadius;

        whirlDuration =
            data.whirlDuration;

        whirlTickInterval =
            data.whirlTickInterval;

        whirlPullSpeed =
            data.whirlPullSpeed;

        whirlDamage =
            data.whirlDamage;


        // =====================================================
        // SLIME
        // =====================================================

        slimeCount =
            data.slimeCount;

        slimeRadius =
            data.slimeRadius;

        slimeSlowPercent =
            data.slimeSlowPercent;

        slimeDotDamage =
            data.slimeDotDamage;

        slimeDotDuration =
            data.slimeDotDuration;

        slimeDotTickInterval =
            data.slimeDotTickInterval;

        slimeSpreadAngle =
            data.slimeSpreadAngle;


        // =====================================================
        // BLIZZARD
        // =====================================================

        blizzardRadius =
            data.blizzardRadius;

        blizzardDuration =
            data.blizzardDuration;

        blizzardTickInterval =
            data.blizzardTickInterval;

        blizzardDamage =
            data.blizzardDamage;

        blizzardSlowPercent =
            data.blizzardSlowPercent;


        // =====================================================
        // ICE SPIKES
        // =====================================================

        iceSpikeCount =
            data.iceSpikeCount;

        iceSpikeSpeed =
            data.iceSpikeSpeed;

        iceSpikeDamageMultiplier =
            data.iceSpikeDamageMultiplier;

        iceSpikeLifetime =
            data.iceSpikeLifetime;


        // =====================================================
        // FREEZING WAVE
        // =====================================================

        freezingWaveRadius =
            data.freezingWaveRadius;

        freezingWaveSpeed =
            data.freezingWaveSpeed;

        freezingWaveDistance =
            data.freezingWaveDistance;

        freezingWaveFreezeDuration =
            data.freezingWaveFreezeDuration;

        freezingWaveDamage =
            data.freezingWaveDamage;


        // =====================================================
        // FROST NOVA
        // =====================================================

        frostNovaRadius =
            data.frostNovaRadius;

        frostNovaDamage =
            data.frostNovaDamage;

        frostNovaFreezeDuration =
            data.frostNovaFreezeDuration;

        frostNovaCooldown =
            data.frostNovaCooldown;


        // =====================================================
        // ICE SHOT
        // =====================================================

        iceShotDamage =
            data.iceShotDamage;

        iceShotSlowPercent =
            data.iceShotSlowPercent;

        iceShotSlowDuration =
            data.iceShotSlowDuration;

        iceShotRadius = data.iceShotRadius;


        // =====================================================
        // BREATH OF FIRE
        // =====================================================

        breathOfFireRange =
            data.breathOfFireRange;

        breathOfFireAngle =
            data.breathOfFireAngle;

        breathOfFireDamage =
            data.breathOfFireDamage;

        breathOfFireTickInterval =
            data.breathOfFireTickInterval;

        breathOfFireBurnDamage =
            data.breathOfFireBurnDamage;

        breathOfFireBurnDuration =
            data.breathOfFireBurnDuration;

        breathOfFireBurnTickInterval =
            data.breathOfFireBurnTickInterval;


        // =====================================================
        // FLAMING CIRCLE
        // =====================================================

        flamingCircleDamage =
            data.flamingCircleDamage;

        flamingCircleRadius =
            data.flamingCircleRadius;

        flamingCircleDuration =
            data.flamingCircleDuration;

        flamingCircleTickInterval =
            data.flamingCircleTickInterval;


        // =====================================================
        // SEARING SHOT
        // =====================================================

        searingShotDamage =
            data.searingShotDamage;

        searingShotBurnDamage =
            data.searingShotBurnDamage;

        searingShotBurnDuration =
            data.searingShotBurnDuration;

        searingShotBurnTickInterval =
            data.searingShotBurnTickInterval;

        searingShotExplosionDamage =
            data.searingShotExplosionDamage;

        searingShotExplosionRadius =
            data.searingShotExplosionRadius;


        // =====================================================
        // RAIN OF FIRE
        // =====================================================

        rainOfFireRadius =
            data.rainOfFireRadius;

        rainOfFireDuration =
            data.rainOfFireDuration;

        rainOfFireTickInterval =
            data.rainOfFireTickInterval;

        rainOfFireDamage =
            data.rainOfFireDamage;

        rainOfFireBurnDamage =
            data.rainOfFireBurnDamage;

        rainOfFireBurnDuration =
            data.rainOfFireBurnDuration;

        rainOfFireBurnTickInterval =
            data.rainOfFireBurnTickInterval;


        // =====================================================
        // VOLCANIC SPHERE
        // =====================================================

        volcanicSphereCount =
            data.volcanicSphereCount;

        volcanicSphereDamage =
            data.volcanicSphereDamage;

        volcanicSphereSpeed =
            data.volcanicSphereSpeed;
    }


    // =========================================================
    // GET PROJECTILE COUNT
    // =========================================================

    public int GetProjectileCount()
    {
        if (data == null)
            return 1;


        int baseCount =
            Mathf.Max(
                1,
                data.projectileCount
            );


        float growth =
            data.projectileCountGrowth;


        // Brak wzrostu.
        if (growth <= 1f)
            return baseCount;


        // Określamy co ile poziomów dochodzi
        // jeden dodatkowy pocisk.
        int levelsPerProjectile =
            Mathf.Max(
                1,
                Mathf.RoundToInt(
                    1f /
                    (growth - 1f)
                )
            );


        int additionalProjectiles =
            Mathf.Max(
                0,
                (level - 1) /
                levelsPerProjectile
            );


        return
            baseCount +
            additionalProjectiles;
    }


    // =========================================================
    // UPGRADE SKILLA
    // =========================================================

    public void Upgrade()
    {
        if (data == null)
            return;


        level++;


        // Odbudowujemy wartości bazowe + zwykły level.
        RebuildStats(null);
    }


    // =========================================================
    // REBUILD STATS
    // =========================================================

    public void RebuildStats(
        Tower tower)
    {
        if (data == null)
            return;


        // =====================================================
        // 1. RESET DO WARTOŚCI Z SkillData
        // =====================================================

        CopyValuesFromData();


        // =====================================================
        // 2. ZWYKŁE LEVEL-UPY
        // =====================================================

        int upgradeCount =
            Mathf.Max(
                0,
                level - 1
            );


        for (
            int i = 0;
            i < upgradeCount;
            i++)
        {
            ApplySingleLevelUpgrade();
        }


        // =====================================================
        // 3. GLOBALNE / ELEMENTARNE / KATEGORYJNE ULEPSZENIA
        // =====================================================

        if (
            tower != null &&
            tower.skillUpgradeManager != null
        )
        {
            tower.skillUpgradeManager.ApplyUpgrades(
                this
            );
        }
    }


    // =========================================================
    // POJEDYNCZY ZWYKŁY LEVEL-UP
    // =========================================================

    private void ApplySingleLevelUpgrade()
    {
        if (data == null)
            return;


        // =====================================================
        // ATAK
        // =====================================================

        damage +=
            data.damageUpgrade;

        fireRate +=
            data.fireRateUpgrade;

        range +=
            data.rangeUpgrade;

        explosionRadius +=
            data.explosionRadiusUpgrade;


// =====================================================
// PIERCING
// =====================================================

piercingCount +=
    data.piercingCountUpgrade;


        // =====================================================
        // CC
        // =====================================================

        rootDuration +=
            data.rootDurationUpgrade;

        slowPercent +=
            data.slowPercentUpgrade;

        slowPercent =
            Mathf.Clamp01(
                slowPercent
            );

        slowDuration +=
            data.slowDurationUpgrade;


        // =====================================================
        // CHAIN
        // =====================================================

        chainCount +=
            data.chainCountUpgrade;

        chainRange +=
            data.chainRangeUpgrade;

        chainDamageMultiplier +=
            data.chainDamageMultiplierUpgrade;

        chainForwardAngle +=
            data.chainForwardAngleUpgrade;


        // =====================================================
        // DOT
        // =====================================================

        dotDamage +=
            data.dotDamageUpgrade;

        dotDuration +=
            data.dotDurationUpgrade;

        dotTickInterval +=
            data.dotTickIntervalUpgrade;


        // =====================================================
        // SHOTGUN
        // =====================================================

        shotgunProjectileCount +=
            data.projectileCountUpgrade;

        spreadAngle +=
            data.spreadAngleUpgrade;


        // =====================================================
        // CORRUPTED
        // =====================================================

        corruptedSlowPercent +=
            data.corruptedSlowPercentUpgrade;

        corruptedAttackSpeedReduction +=
            data.corruptedAttackSpeedReductionUpgrade;

        corruptedDamageTakenIncrease +=
            data.corruptedDamageTakenIncreaseUpgrade;

        corruptedDuration +=
            data.corruptedDurationUpgrade;

        corruptedMaxStacks +=
            data.corruptedMaxStacksUpgrade;

        corruptedMaxTargets +=
            data.corruptedMaxTargetsUpgrade;

        corruptedChainRange +=
            data.corruptedChainRangeUpgrade;


        // =====================================================
        // DEVOURING CLOUD
        // =====================================================

        cloudRadius +=
            data.cloudRadiusUpgrade;

        cloudDuration +=
            data.cloudDurationUpgrade;

        cloudTickInterval +=
            data.cloudTickIntervalUpgrade;


        // =====================================================
        // WHIRL OF CHAOS
        // =====================================================

        whirlRadius +=
            data.whirlRadiusUpgrade;

        whirlDuration +=
            data.whirlDurationUpgrade;

        whirlTickInterval +=
            data.whirlTickIntervalUpgrade;

        whirlPullSpeed +=
            data.whirlPullSpeedUpgrade;

        whirlDamage +=
            data.whirlDamageUpgrade;


        // =====================================================
        // SLIME
        // =====================================================

        slimeRadius +=
            data.slimeRadiusUpgrade;

        slimeSlowPercent +=
            data.slimeSlowPercentUpgrade;

        slimeSlowPercent =
            Mathf.Clamp01(
                slimeSlowPercent
            );

        slimeDotDamage +=
            data.slimeDotDamageUpgrade;

        slimeDotDuration +=
            data.slimeDotDurationUpgrade;

        slimeDotTickInterval +=
            data.slimeDotTickIntervalUpgrade;

        slimeSpreadAngle +=
            data.slimeSpreadAngleUpgrade;


        // =====================================================
        // BLIZZARD
        // =====================================================

        blizzardRadius +=
            data.blizzardRadiusUpgrade;

        blizzardDuration +=
            data.blizzardDurationUpgrade;

        blizzardTickInterval +=
            data.blizzardTickIntervalUpgrade;

        blizzardDamage +=
            data.blizzardDamageUpgrade;

        blizzardSlowPercent +=
            data.blizzardSlowPercentUpgrade;

        blizzardSlowPercent =
            Mathf.Clamp01(
                blizzardSlowPercent
            );


        // =====================================================
        // ICE SPIKES
        // =====================================================

        iceSpikeCount +=
            data.iceSpikeCountUpgrade;

        iceSpikeSpeed +=
            data.iceSpikeSpeedUpgrade;

        iceSpikeLifetime +=
            data.iceSpikeLifetimeUpgrade;

        iceSpikeDamageMultiplier +=
            data.iceSpikeDamageMultiplierUpgrade;


        // =====================================================
        // FREEZING WAVE
        // =====================================================

        freezingWaveRadius +=
            data.freezingWaveRadiusUpgrade;

        freezingWaveSpeed +=
            data.freezingWaveSpeedUpgrade;

        freezingWaveDistance +=
            data.freezingWaveDistanceUpgrade;

        freezingWaveFreezeDuration +=
            data.freezingWaveFreezeDurationUpgrade;

        freezingWaveDamage +=
            data.freezingWaveDamageUpgrade;


        // =====================================================
        // FROST NOVA
        // =====================================================

        frostNovaRadius +=
            data.frostNovaRadiusUpgrade;

        frostNovaDamage +=
            data.frostNovaDamageUpgrade;

        frostNovaFreezeDuration +=
            data.frostNovaFreezeDurationUpgrade;

        frostNovaCooldown =
            Mathf.Max(
                0.1f,
                frostNovaCooldown -
                data.frostNovaCooldownUpgrade
            );


        // =====================================================
        // ICE SHOT
        // =====================================================

        iceShotDamage +=
            data.iceShotDamageUpgrade;

        iceShotSlowPercent +=
            data.iceShotSlowPercentUpgrade;

        iceShotSlowPercent =
            Mathf.Clamp01(
                iceShotSlowPercent
            );

        iceShotSlowDuration +=
            data.iceShotSlowDurationUpgrade;

        iceShotRadius += data.iceShotRadiusUpgrade;


        // =====================================================
        // BREATH OF FIRE
        // =====================================================

        breathOfFireRange +=
            data.breathOfFireRangeUpgrade;

        breathOfFireAngle +=
            data.breathOfFireAngleUpgrade;

        breathOfFireDamage +=
            data.breathOfFireDamageUpgrade;

        breathOfFireBurnDamage +=
            data.breathOfFireBurnDamageUpgrade;

        breathOfFireBurnDuration +=
            data.breathOfFireBurnDurationUpgrade;


        // =====================================================
        // FLAMING CIRCLE
        // =====================================================

        flamingCircleDamage +=
            data.flamingCircleDamageUpgrade;

        flamingCircleRadius +=
            data.flamingCircleRadiusUpgrade;

        flamingCircleDuration +=
            data.flamingCircleDurationUpgrade;


        // =====================================================
        // SEARING SHOT
        // =====================================================

        searingShotDamage +=
            data.searingShotDamageUpgrade;

        searingShotBurnDamage +=
            data.searingShotBurnDamageUpgrade;

        searingShotExplosionDamage +=
            data.searingShotExplosionDamageUpgrade;

        searingShotExplosionRadius +=
            data.searingShotExplosionRadiusUpgrade;


        // =====================================================
        // RAIN OF FIRE
        // =====================================================

        rainOfFireRadius +=
            data.rainOfFireRadiusUpgrade;

        rainOfFireDuration +=
            data.rainOfFireDurationUpgrade;

        rainOfFireTickInterval +=
            data.rainOfFireTickIntervalUpgrade;

        rainOfFireDamage +=
            data.rainOfFireDamageUpgrade;

        rainOfFireBurnDamage +=
            data.rainOfFireBurnDamageUpgrade;

        rainOfFireBurnDuration +=
            data.rainOfFireBurnDurationUpgrade;

        rainOfFireBurnTickInterval +=
            data.rainOfFireBurnTickIntervalUpgrade;


        // =====================================================
        // VOLCANIC SPHERE
        // =====================================================

        volcanicSphereCount +=
            data.volcanicSphereCountUpgrade;

        volcanicSphereDamage +=
            data.volcanicSphereDamageUpgrade;

        volcanicSphereSpeed +=
            data.volcanicSphereSpeedUpgrade;


        // =====================================================
        // LIGHTNING ARROW
        // =====================================================

        lightningStunChance +=
            data.lightningStunChanceUpgrade;

        lightningStunChance =
            Mathf.Clamp01(
                lightningStunChance
            );

        lightningStunDuration +=
            data.lightningStunDurationUpgrade;

        lightningBonusDamage +=
            data.lightningBonusDamageUpgrade;


        // =====================================================
        // LIGHTNING STRIKE
        // =====================================================

        lightningStrikeChance +=
            data.lightningStrikeChanceUpgrade;

        lightningStrikeChance =
            Mathf.Clamp01(
                lightningStrikeChance
            );

        lightningStrikeMaxHits +=
            data.lightningStrikeMaxHitsUpgrade;

        lightningStrikeDamageReduction +=
            data.lightningStrikeDamageReductionUpgrade;

        lightningStrikeDamageReduction =
            Mathf.Clamp01(
                lightningStrikeDamageReduction
            );

        lightningStrikeDelay +=
            data.lightningStrikeDelayUpgrade;

        lightningStrikeDelay =
            Mathf.Max(
                0f,
                lightningStrikeDelay
            );


        // =====================================================
        // ELECTRIC SHOCK
        // =====================================================

        electricShockDamage +=
            data.electricShockDamageUpgrade;

        electricShockStunDuration +=
            data.electricShockStunDurationUpgrade;

        electricShockProjectileSpeed +=
            data.electricShockProjectileSpeedUpgrade;

        electricShockRadius += data.electricShockRadiusUpgrade;


        // =====================================================
        // ENERGY ORB
        // =====================================================

        energyOrbProjectileSpeed +=
            data.energyOrbProjectileSpeedUpgrade;

        energyOrbHitRadius +=
            data.energyOrbHitRadiusUpgrade;

        energyOrbKnockback +=
            data.energyOrbKnockbackUpgrade;


        // =====================================================
        // SPINNING LASER
        // =====================================================

        spinningLaserCount +=
            data.spinningLaserCountUpgrade;

        spinningLaserRadius +=
            data.spinningLaserRadiusUpgrade;

        spinningLaserRotationSpeed +=
            data.spinningLaserRotationSpeedUpgrade;

        spinningLaserDamage +=
            data.spinningLaserDamageUpgrade;

        spinningLaserDamageInterval -=
            data.spinningLaserDamageIntervalUpgrade;

        spinningLaserWidth += data.spinningLaserWidthUpgrade;


        // =====================================================
        // ZABEZPIECZENIA
        // =====================================================

        spinningLaserCount =
            Mathf.Max(
                1,
                spinningLaserCount
            );

        spinningLaserRadius =
            Mathf.Max(
                0f,
                spinningLaserRadius
            );

        spinningLaserRotationSpeed =
            Mathf.Max(
                0f,
                spinningLaserRotationSpeed
            );

        spinningLaserDamage =
            Mathf.Max(
                0f,
                spinningLaserDamage
            );

        spinningLaserDamageInterval =
            Mathf.Max(
                0.05f,
                spinningLaserDamageInterval
            );


        // =====================================================
        // DODATKOWE ZABEZPIECZENIA
        // =====================================================

        fireRate =
            Mathf.Max(
                0.01f,
                fireRate
            );

        range =
            Mathf.Max(
                0f,
                range
            );

        explosionRadius =
            Mathf.Max(
                0f,
                explosionRadius
            );

        projectileCount =
            Mathf.Max(
                1,
                projectileCount
            );

        shotgunProjectileCount =
            Mathf.Max(
                1,
                shotgunProjectileCount
            );

        chainCount =
            Mathf.Max(
                0,
                chainCount
            );

        chainRange =
            Mathf.Max(
                0f,
                chainRange
            );

        dotTickInterval =
            Mathf.Max(
                0.01f,
                dotTickInterval
            );

        cloudTickInterval =
            Mathf.Max(
                0.01f,
                cloudTickInterval
            );

        whirlTickInterval =
            Mathf.Max(
                0.01f,
                whirlTickInterval
            );

        slimeDotTickInterval =
            Mathf.Max(
                0.01f,
                slimeDotTickInterval
            );

        blizzardTickInterval =
            Mathf.Max(
                0.01f,
                blizzardTickInterval
            );
    }


    // =========================================================
    // PREFAB - GŁÓWNY POCISK ICE SPIKES
    // =========================================================

    public GameObject iceSpikesProjectilePrefab
    {
        get
        {
            return data != null
                ? data.iceSpikesProjectilePrefab
                : null;
        }
    }


    // =========================================================
    // PREFAB - ODŁAMEK ICE SPIKES
    // =========================================================

    public GameObject iceSpikeProjectilePrefab
    {
        get
        {
            return data != null
                ? data.iceSpikeProjectilePrefab
                : null;
        }
    }


    // =========================================================
    // PREFAB - NORMALNY POCISK
    // =========================================================

    public GameObject projectilePrefab
    {
        get
        {
            return data != null
                ? data.projectilePrefab
                : null;
        }
    }


    // =========================================================
    // DEVOURING CLOUD
    // =========================================================

    public GameObject cloudEffectPrefab
    {
        get
        {
            return data != null
                ? data.cloudEffectPrefab
                : null;
        }
    }


    // =========================================================
    // WHIRL OF CHAOS
    // =========================================================

    public GameObject whirlEffectPrefab
    {
        get
        {
            return data != null
                ? data.whirlEffectPrefab
                : null;
        }
    }


    // =========================================================
    // SLIME
    // =========================================================

    public GameObject slimeProjectilePrefab
    {
        get
        {
            return data != null
                ? data.slimeProjectilePrefab
                : null;
        }
    }


    // =========================================================
    // FROST NOVA
    // =========================================================

    public GameObject frostNovaEffectPrefab
    {
        get
        {
            return data != null
                ? data.frostNovaEffectPrefab
                : null;
        }
    }


    // =========================================================
    // BREATH OF FIRE
    // =========================================================

    public GameObject breathOfFireEffectPrefab
    {
        get
        {
            return data != null
                ? data.breathOfFireEffectPrefab
                : null;
        }
    }


    // =========================================================
    // FLAMING CIRCLE
    // =========================================================

    public GameObject flamingCirclePrefab
    {
        get
        {
            return data != null
                ? data.flamingCirclePrefab
                : null;
        }
    }


    // =========================================================
    // VFX
    // =========================================================

    public GameObject hitEffectPrefab
    {
        get
        {
            return data != null
                ? data.hitEffectPrefab
                : null;
        }
    }


    public GameObject explosionEffectPrefab
    {
        get
        {
            return data != null
                ? data.explosionEffectPrefab
                : null;
        }
    }


    // =========================================================
    // RAIN OF FIRE
    // =========================================================

    public GameObject rainOfFireEffectPrefab
    {
        get
        {
            return data != null
                ? data.rainOfFireEffectPrefab
                : null;
        }
    }
}