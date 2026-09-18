using System.Collections.Generic;
using UnityEngine;

public class SkillUpgradeManager : MonoBehaviour
{
    // =========================================================
    // ULEPSZENIE + POZIOM
    // =========================================================

    [System.Serializable]
    public class UpgradeLevel
    {
        public SkillUpgradeData upgrade;
        public int level = 1;
    }


    // =========================================================
    // POSIADANE ULEPSZENIA
    // =========================================================

    [Header("Posiadane ulepszenia")]

    [SerializeField]
    private List<SkillUpgradeData> activeUpgrades =
        new List<SkillUpgradeData>();


    // =========================================================
    // POZIOMY ULEPSZEŃ
    // =========================================================

    [Header("Poziomy ulepszeń")]

    [SerializeField]
    private List<UpgradeLevel> upgradeLevels =
        new List<UpgradeLevel>();


    // =========================================================
    // PUBLICZNY DOSTĘP DO POZIOMÓW ULEPSZEŃ
    // =========================================================

    public IReadOnlyList<UpgradeLevel> UpgradeLevels
    {
        get
        {
            return upgradeLevels;
        }
    }


    // =========================================================
    // STATYSTYKI KRYTYCZNE
    // =========================================================

    [Header("Statystyki krytyczne")]

    [Tooltip("Bazowa szansa na trafienie krytyczne w procentach.")]
    [SerializeField]
    private float baseCriticalChance = 0f;


    [Tooltip("Bazowy mnożnik obrażeń krytycznych. 1.5 = 150% obrażeń.")]
    [SerializeField]
    private float baseCriticalDamage = 1.5f;


    // =========================================================
    // PUBLICZNY DOSTĘP
    // =========================================================

    public IReadOnlyList<SkillUpgradeData> ActiveUpgrades
    {
        get
        {
            return activeUpgrades;
        }
    }


    // =========================================================
    // DODANIE ULEPSZENIA
    // =========================================================

    public void AddUpgrade(
        SkillUpgradeData upgrade)
    {
        if (upgrade == null)
        {
            return;
        }


        UpgradeLevel existing =
            GetUpgradeLevelEntry(upgrade);


        if (existing != null)
        {
            existing.level++;
        }
        else
        {
            UpgradeLevel newEntry =
                new UpgradeLevel();


            newEntry.upgrade =
                upgrade;


            newEntry.level =
                1;


            upgradeLevels.Add(
                newEntry
            );


            if (!activeUpgrades.Contains(upgrade))
            {
                activeUpgrades.Add(
                    upgrade
                );
            }
        }


        RefreshAllSkills();
    }


    // =========================================================
    // KUPNO ULEPSZENIA
    // =========================================================

    public bool BuyUpgrade(
        SkillUpgradeData upgrade)
    {
        if (upgrade == null)
        {
            return false;
        }


        AddUpgrade(upgrade);


        UpgradeLevel current =
            GetUpgradeLevelEntry(
                upgrade
            );


        int currentLevel =
            current != null
                ? current.level
                : 0;

        return true;
    }


    // =========================================================
    // USUNIĘCIE ULEPSZENIA
    // =========================================================

    public void RemoveUpgrade(
        SkillUpgradeData upgrade)
    {
        if (upgrade == null)
            return;


        activeUpgrades.Remove(
            upgrade
        );


        for (
            int i = upgradeLevels.Count - 1;
            i >= 0;
            i--)
        {
            if (upgradeLevels[i] == null)
                continue;


            if (
                upgradeLevels[i].upgrade ==
                upgrade)
            {
                upgradeLevels.RemoveAt(i);
            }
        }


        RefreshAllSkills();
    }


    // =========================================================
    // ZNALEZIENIE POZIOMU ULEPSZENIA
    // =========================================================

    private UpgradeLevel GetUpgradeLevelEntry(
        SkillUpgradeData upgrade)
    {
        if (upgrade == null)
            return null;


        for (
            int i = 0;
            i < upgradeLevels.Count;
            i++)
        {
            UpgradeLevel entry =
                upgradeLevels[i];


            if (entry == null)
                continue;


            if (entry.upgrade == upgrade)
                return entry;
        }


        return null;
    }


    // =========================================================
    // PUBLICZNY POZIOM ULEPSZENIA
    // =========================================================

    public int GetUpgradeLevel(
        SkillUpgradeData upgrade)
    {
        UpgradeLevel entry =
            GetUpgradeLevelEntry(
                upgrade
            );


        if (entry == null)
            return 0;


        return Mathf.Max(
            0,
            entry.level
        );
    }


    // =========================================================
    // CZY POSIADA ULEPSZENIE
    // =========================================================

    public bool HasUpgrade(
        SkillUpgradeData upgrade)
    {
        if (upgrade == null)
            return false;


        return GetUpgradeLevel(
            upgrade
        ) > 0;
    }


    // =========================================================
    // ODŚWIEŻENIE WSZYSTKICH SKILLI
    // =========================================================

    public void RefreshAllSkills()
    {
        Tower[] towers =
            FindObjectsByType<Tower>();


        foreach (
            Tower tower
            in towers)
        {
            if (tower == null)
                continue;


            RefreshTowerSkills(
                tower
            );
        }


        // =====================================================
        // ODŚWIEŻENIE HP WIEŻ
        // =====================================================

        TowerHealth[] towerHealths =
            FindObjectsByType<TowerHealth>();


        foreach (
            TowerHealth towerHealth
            in towerHealths)
        {
            if (towerHealth == null)
                continue;


            towerHealth.RefreshHealthStats();
        }
    }


    // =========================================================
    // ODŚWIEŻENIE SKILLI JEDNEJ WIEŻY
    // =========================================================

    public void RefreshTowerSkills(
        Tower tower)
    {
        if (tower == null)
            return;


        if (tower.activeSkills == null)
            return;


        foreach (
            SkillInstance skill
            in tower.activeSkills)
        {
            if (skill == null)
                continue;


            skill.RebuildStats(
                tower
            );
        }
    }


    // =========================================================
    // CZY ULEPSZENIE ZAWIERA DANĄ STATYSTYKĘ
    // =========================================================

    private bool UpgradeContainsStat(
        SkillUpgradeData upgrade,
        SkillUpgradeData.UpgradeStat stat)
    {
        if (
            upgrade == null ||
            upgrade.modifiers == null)
        {
            return false;
        }


        for (
            int i = 0;
            i < upgrade.modifiers.Count;
            i++)
        {
            SkillUpgradeData.UpgradeModifier modifier =
                upgrade.modifiers[i];


            if (modifier == null)
                continue;


            if (modifier.stat == stat)
                return true;
        }


        return false;
    }


    // =========================================================
    // CZY ULEPSZENIE PASUJE DO SKILLA
    // =========================================================

    public bool UpgradeMatchesSkill(
        SkillUpgradeData upgrade,
        SkillInstance skill)
    {
        if (
            upgrade == null ||
            skill == null ||
            skill.data == null)
        {
            return false;
        }


        if (
            upgrade.modifiers == null ||
            upgrade.modifiers.Count == 0)
        {
            return false;
        }


        bool isProjectileSkill =
            (
                skill.data.categories &
                SkillCategory.Projectile
            ) != SkillCategory.None;


        // =====================================================
        // ATTACK SPEED
        // =====================================================

        if (
            UpgradeContainsStat(
                upgrade,
                SkillUpgradeData.UpgradeStat.AttackSpeed))
        {
            if (!isProjectileSkill)
                return false;
        }


        // =====================================================
        // COOLDOWN
        // =====================================================

        if (
            UpgradeContainsStat(
                upgrade,
                SkillUpgradeData.UpgradeStat.Cooldown))
        {
            if (isProjectileSkill)
                return false;
        }


        // =====================================================
        // CEL
        // =====================================================

        switch (upgrade.target)
        {
            case SkillUpgradeData.UpgradeTarget.Global:

                return true;


            case SkillUpgradeData.UpgradeTarget.Element:

                return
                    skill.data.damageType ==
                    upgrade.element;


            case SkillUpgradeData.UpgradeTarget.Category:

                return
                    (
                        skill.data.categories &
                        upgrade.category
                    ) != SkillCategory.None;


            case SkillUpgradeData.UpgradeTarget.ElementAndCategory:

                return
                    skill.data.damageType ==
                    upgrade.element
                    &&
                    (
                        skill.data.categories &
                        upgrade.category
                    ) != SkillCategory.None;


            default:

                return false;
        }
    }


    // =========================================================
    // MNOŻNIK PROCENTOWY
    // =========================================================

    private float GetPercentMultiplier(
        SkillInstance skill,
        SkillUpgradeData.UpgradeStat stat)
    {
        float multiplier = 1f;


        if (skill == null)
            return multiplier;


        foreach (
            UpgradeLevel entry
            in upgradeLevels)
        {
            if (entry == null)
                continue;


            SkillUpgradeData upgrade =
                entry.upgrade;


            if (upgrade == null)
                continue;


            if (entry.level <= 0)
                continue;


            if (
                !UpgradeMatchesSkill(
                    upgrade,
                    skill))
            {
                continue;
            }


            if (upgrade.modifiers == null)
                continue;


            foreach (
                SkillUpgradeData.UpgradeModifier modifier
                in upgrade.modifiers)
            {
                if (modifier == null)
                    continue;


                if (modifier.stat != stat)
                    continue;


                if (
                    modifier.modifierType !=
                    SkillUpgradeData.ModifierType.Percent)
                {
                    continue;
                }


                float singleLevelMultiplier =
                    1f +
                    modifier.value / 100f;


                for (
                    int i = 0;
                    i < entry.level;
                    i++)
                {
                    multiplier *=
                        singleLevelMultiplier;
                }
            }
        }


        return multiplier;
    }


    // =========================================================
    // BONUS PŁASKI
    // =========================================================

    private float GetFlatBonus(
        SkillInstance skill,
        SkillUpgradeData.UpgradeStat stat)
    {
        float bonus = 0f;


        if (skill == null)
            return bonus;


        foreach (
            UpgradeLevel entry
            in upgradeLevels)
        {
            if (entry == null)
                continue;


            SkillUpgradeData upgrade =
                entry.upgrade;


            if (upgrade == null)
                continue;


            if (entry.level <= 0)
                continue;


            if (
                !UpgradeMatchesSkill(
                    upgrade,
                    skill))
            {
                continue;
            }


            if (upgrade.modifiers == null)
                continue;


            foreach (
                SkillUpgradeData.UpgradeModifier modifier
                in upgrade.modifiers)
            {
                if (modifier == null)
                    continue;


                if (modifier.stat != stat)
                    continue;


                if (
                    modifier.modifierType !=
                    SkillUpgradeData.ModifierType.Flat)
                {
                    continue;
                }


                bonus +=
                    modifier.value *
                    entry.level;
            }
        }


        return bonus;
    }


    // =========================================================
    // GETTERY — SKILLE
    // =========================================================

    public float GetDamage(
        SkillInstance skill)
    {
        return skill == null
            ? 0f
            : skill.damage;
    }


    public float GetAttackSpeed(
        SkillInstance skill)
    {
        return skill == null
            ? 0f
            : skill.fireRate;
    }


    public float GetCooldown(
        SkillInstance skill)
    {
        return skill == null
            ? 0f
            : skill.fireRate;
    }


    public float GetRange(
        SkillInstance skill)
    {
        return skill == null
            ? 0f
            : skill.range;
    }


    public float GetExplosionRadius(
        SkillInstance skill)
    {
        return skill == null
            ? 0f
            : skill.explosionRadius;
    }


    public int GetProjectileCount(
        SkillInstance skill)
    {
        return skill == null
            ? 0
            : skill.projectileCount;
    }


    public float GetChainRange(
        SkillInstance skill)
    {
        return skill == null
            ? 0f
            : skill.chainRange;
    }


    public float GetChainDamage(
        SkillInstance skill)
    {
        return skill == null
            ? 0f
            : skill.chainDamageMultiplier;
    }


    public float GetDotDamage(
        SkillInstance skill)
    {
        return skill == null
            ? 0f
            : skill.dotDamage;
    }


    public float GetDotDuration(
        SkillInstance skill)
    {
        return skill == null
            ? 0f
            : skill.dotDuration;
    }


    public float GetSlowPercent(
        SkillInstance skill)
    {
        return skill == null
            ? 0f
            : skill.slowPercent;
    }


    // =========================================================
    // GETTERY — KRYTYCZNE
    // =========================================================

    /// <summary>
    /// Zwraca aktualną szansę na trafienie krytyczne
    /// w procentach.
    /// </summary>
    public float GetCriticalChance()
    {
        float bonus =
            GetGlobalFlatBonus(
                SkillUpgradeData.UpgradeStat.CriticalChance
            );


        return Mathf.Clamp(
            baseCriticalChance + bonus,
            0f,
            100f
        );
    }


    /// <summary>
    /// Zwraca aktualny mnożnik obrażeń krytycznych.
    /// </summary>
    public float GetCriticalDamage()
    {
        float bonus =
            GetGlobalFlatBonus(
                SkillUpgradeData.UpgradeStat.CriticalDamage
            );


        return Mathf.Max(
            1f,
            baseCriticalDamage + bonus
        );
    }


    // =========================================================
    // OBLICZANIE OBRAŻEŃ KRYTYCZNYCH
    // =========================================================

    public float CalculateDamageWithCritical(
        float damage,
        out bool isCritical)
    {
        isCritical = false;


        if (damage <= 0f)
            return 0f;


        float criticalChance =
            GetCriticalChance();


        if (criticalChance <= 0f)
            return damage;


        if (criticalChance >= 100f)
        {
            isCritical = true;


            return
                damage *
                GetCriticalDamage();
        }


        if (
            Random.Range(
                0f,
                100f
            ) < criticalChance)
        {
            isCritical = true;


            return
                damage *
                GetCriticalDamage();
        }


        return damage;
    }


    // =========================================================
    // GETTERY — OBRONA WIEŻY
    // =========================================================

    public float GetHealthRegeneration()
    {
        return GetGlobalNonlinearFlatBonus(
            SkillUpgradeData.UpgradeStat.HealthRegeneration
        );
    }


    public float GetMaxHealthBonus()
    {
        return GetGlobalNonlinearFlatBonus(
            SkillUpgradeData.UpgradeStat.MaxHealth
        );
    }


    public float GetDamageReductionFlat()
    {
        return GetGlobalFlatBonus(
            SkillUpgradeData.UpgradeStat.DamageReductionFlat
        );
    }


    public float GetDamageReductionPercent()
    {
        float totalPercent = 0f;


        if (upgradeLevels == null)
            return 0f;


        foreach (
            UpgradeLevel entry
            in upgradeLevels)
        {
            if (entry == null)
                continue;


            if (entry.upgrade == null)
                continue;


            if (entry.level <= 0)
                continue;


            SkillUpgradeData upgrade =
                entry.upgrade;


            if (
                upgrade.target !=
                SkillUpgradeData.UpgradeTarget.Global)
            {
                continue;
            }


            if (upgrade.modifiers == null)
                continue;


            foreach (
                SkillUpgradeData.UpgradeModifier modifier
                in upgrade.modifiers)
            {
                if (modifier == null)
                    continue;


                if (
                    modifier.stat !=
                    SkillUpgradeData.UpgradeStat.DamageReductionPercent)
                {
                    continue;
                }


                if (
                    modifier.modifierType !=
                    SkillUpgradeData.ModifierType.Percent)
                {
                    continue;
                }


                totalPercent +=
                    modifier.value *
                    entry.level;
            }
        }


        return Mathf.Clamp(
            totalPercent,
            0f,
            100f
        );
    }


    // =========================================================
    // GLOBALNY BONUS PŁASKI
    // =========================================================

    private float GetGlobalFlatBonus(
        SkillUpgradeData.UpgradeStat stat)
    {
        float bonus = 0f;


        if (upgradeLevels == null)
            return bonus;


        foreach (
            UpgradeLevel entry
            in upgradeLevels)
        {
            if (entry == null)
                continue;


            if (entry.upgrade == null)
                continue;


            if (entry.level <= 0)
                continue;


            SkillUpgradeData upgrade =
                entry.upgrade;


            if (
                upgrade.target !=
                SkillUpgradeData.UpgradeTarget.Global)
            {
                continue;
            }


            if (upgrade.modifiers == null)
                continue;


            foreach (
                SkillUpgradeData.UpgradeModifier modifier
                in upgrade.modifiers)
            {
                if (modifier == null)
                    continue;


                if (modifier.stat != stat)
                    continue;


                if (
                    modifier.modifierType !=
                    SkillUpgradeData.ModifierType.Flat)
                {
                    continue;
                }


                bonus +=
                    modifier.value *
                    entry.level;
            }
        }


        return bonus;
    }

// =========================================================
// GLOBALNY BONUS PŁASKI — NIELINIOWY
// =========================================================

private float GetGlobalNonlinearFlatBonus(
    SkillUpgradeData.UpgradeStat stat)
{
    float bonus = 0f;


    if (
        stat !=
        SkillUpgradeData.UpgradeStat.MaxHealth &&
        stat !=
        SkillUpgradeData.UpgradeStat.HealthRegeneration)
    {
        return GetGlobalFlatBonus(stat);
    }


    if (upgradeLevels == null)
        return bonus;


    foreach (
        UpgradeLevel entry
        in upgradeLevels)
    {
        if (entry == null)
            continue;


        if (entry.upgrade == null)
            continue;


        if (entry.level <= 0)
            continue;


        SkillUpgradeData upgrade =
            entry.upgrade;


        if (
            upgrade.target !=
            SkillUpgradeData.UpgradeTarget.Global)
        {
            continue;
        }


        if (upgrade.modifiers == null)
            continue;


        foreach (
            SkillUpgradeData.UpgradeModifier modifier
            in upgrade.modifiers)
        {
            if (modifier == null)
                continue;


            if (modifier.stat != stat)
                continue;


            if (
                modifier.modifierType !=
                SkillUpgradeData.ModifierType.Flat)
            {
                continue;
            }


            // =================================================
            // NIELINIOWY PRZYROST
            //
            // Przykład:
            //
            // value = 5
            // multiplier = 1.05
            //
            // poziom 1:
            // 5
            //
            // poziom 2:
            // 5 + 5.25
            //
            // poziom 3:
            // 5 + 5.25 + 5.5125
            // =================================================

            float nonlinearMultiplier =
                Mathf.Max(
                    1f,
                    modifier.nonlinearMultiplier
                );


            // =================================================
            // SUMUJEMY WSZYSTKIE POPRZEDNIE POZIOMY
            // =================================================

            for (
                int level = 0;
                level < entry.level;
                level++)
            {
                float levelValue =
                    modifier.value *
                    Mathf.Pow(
                        nonlinearMultiplier,
                        level
                    );


                bonus +=
                    levelValue;
            }
        }
    }


    return Mathf.Max(
        0f,
        bonus
    );
}


    // =========================================================
    // APPLY UPGRADES DO SKILLA
    // =========================================================

    public void ApplyUpgrades(
        SkillInstance skill)
    {
        if (skill == null)
            return;


        // =====================================================
        // DAMAGE
        // =====================================================

        skill.damage =
            ApplyValue(
                skill,
                skill.damage,
                SkillUpgradeData.UpgradeStat.Damage
            );


        // =====================================================
        // BLIZZARD DAMAGE
        // =====================================================

        skill.blizzardDamage =
            ApplyValue(
                skill,
                skill.blizzardDamage,
                SkillUpgradeData.UpgradeStat.Damage
            );


        // =====================================================
        // RAIN OF FIRE DAMAGE
        // =====================================================

        skill.rainOfFireDamage =
            ApplyValue(
                skill,
                skill.rainOfFireDamage,
                SkillUpgradeData.UpgradeStat.Damage
            );


        // =====================================================
        // ATTACK SPEED / COOLDOWN
        // =====================================================

        skill.fireRate =
            ApplyAttackSpeedOrCooldown(skill);


        // =====================================================
        // RANGE
        // =====================================================

        skill.range =
            ApplyValue(
                skill,
                skill.range,
                SkillUpgradeData.UpgradeStat.Range
            );


        // =====================================================
        // BREATH OF FIRE RANGE
        // =====================================================

        if (skill.breathOfFireRange > 0f)
        {
            skill.breathOfFireRange =
                ApplyValue(
                    skill,
                    skill.breathOfFireRange,
                    SkillUpgradeData.UpgradeStat.Range
                );
        }


        // =====================================================
        // AOE
        // =====================================================

        ApplyAoEUpgrade(skill);


        // =====================================================
        // EXPLOSION RADIUS
        // =====================================================

        skill.explosionRadius =
            ApplyValue(
                skill,
                skill.explosionRadius,
                SkillUpgradeData.UpgradeStat.ExplosionRadius
            );


        // =====================================================
        // PROJECTILE COUNT
        // =====================================================

        skill.projectileCount =
            Mathf.Max(
                1,
                Mathf.RoundToInt(
                    ApplyValue(
                        skill,
                        skill.projectileCount,
                        SkillUpgradeData.UpgradeStat.ProjectileCount
                    )
                )
            );


        skill.piercingCount =
            Mathf.Max(
                1,
                Mathf.RoundToInt(
                    ApplyValue(
                        skill,
                        skill.piercingCount,
                        SkillUpgradeData.UpgradeStat.PiercingCount
                    )
                )
            );


        // =====================================================
        // CHAIN COUNT
        // =====================================================

        skill.chainCount =
            Mathf.Max(
                0,
                Mathf.RoundToInt(
                    ApplyValue(
                        skill,
                        skill.chainCount,
                        SkillUpgradeData.UpgradeStat.ChainCount
                    )
                )
            );


        // =====================================================
        // CHAIN RANGE
        // =====================================================

        skill.chainRange =
            ApplyValue(
                skill,
                skill.chainRange,
                SkillUpgradeData.UpgradeStat.ChainRange
            );


        // =====================================================
        // CHAIN DAMAGE
        // =====================================================

        skill.chainDamageMultiplier =
            ApplyValue(
                skill,
                skill.chainDamageMultiplier,
                SkillUpgradeData.UpgradeStat.ChainDamage
            );


        // =====================================================
        // DOT DAMAGE
        // =====================================================

        skill.dotDamage =
            ApplyValue(
                skill,
                skill.dotDamage,
                SkillUpgradeData.UpgradeStat.DotDamage
            );


        // =====================================================
        // DOT DURATION
        // =====================================================

        skill.dotDuration =
            ApplyValue(
                skill,
                skill.dotDuration,
                SkillUpgradeData.UpgradeStat.DotDuration
            );


        // =====================================================
        // SLOW PERCENT
        // =====================================================

        skill.slowPercent =
            Mathf.Clamp01(
                ApplyValue(
                    skill,
                    skill.slowPercent,
                    SkillUpgradeData.UpgradeStat.SlowPercent
                )
            );
    }


    // =========================================================
    // AOE UPGRADE
    // =========================================================

    private void ApplyAoEUpgrade(
        SkillInstance skill)
    {
        if (
            skill == null ||
            skill.data == null)
        {
            return;
        }


        if (skill.explosionRadius > 0f)
        {
            skill.explosionRadius =
                ApplyValue(
                    skill,
                    skill.explosionRadius,
                    SkillUpgradeData.UpgradeStat.AoE
                );
        }


        if (skill.cloudRadius > 0f)
        {
            skill.cloudRadius =
                ApplyValue(
                    skill,
                    skill.cloudRadius,
                    SkillUpgradeData.UpgradeStat.AoE
                );
        }


        if (skill.whirlRadius > 0f)
        {
            skill.whirlRadius =
                ApplyValue(
                    skill,
                    skill.whirlRadius,
                    SkillUpgradeData.UpgradeStat.AoE
                );
        }


        if (skill.slimeRadius > 0f)
        {
            skill.slimeRadius =
                ApplyValue(
                    skill,
                    skill.slimeRadius,
                    SkillUpgradeData.UpgradeStat.AoE
                );
        }


        if (skill.blizzardRadius > 0f)
        {
            skill.blizzardRadius =
                ApplyValue(
                    skill,
                    skill.blizzardRadius,
                    SkillUpgradeData.UpgradeStat.AoE
                );
        }


        if (skill.freezingWaveRadius > 0f)
        {
            skill.freezingWaveRadius =
                ApplyValue(
                    skill,
                    skill.freezingWaveRadius,
                    SkillUpgradeData.UpgradeStat.AoE
                );
        }


        if (skill.frostNovaRadius > 0f)
        {
            skill.frostNovaRadius =
                ApplyValue(
                    skill,
                    skill.frostNovaRadius,
                    SkillUpgradeData.UpgradeStat.AoE
                );
        }


        if (skill.iceShotRadius > 0f)
        {
            skill.iceShotRadius =
                ApplyValue(
                    skill,
                    skill.iceShotRadius,
                    SkillUpgradeData.UpgradeStat.AoE
                );
        }


        if (skill.electricShockRadius > 0f)
        {
            skill.electricShockRadius =
                ApplyValue(
                    skill,
                    skill.electricShockRadius,
                    SkillUpgradeData.UpgradeStat.AoE
                );
        }


        if (skill.flamingCircleRadius > 0f)
        {
            skill.flamingCircleRadius =
                ApplyValue(
                    skill,
                    skill.flamingCircleRadius,
                    SkillUpgradeData.UpgradeStat.AoE
                );
        }


        if (skill.searingShotExplosionRadius > 0f)
        {
            skill.searingShotExplosionRadius =
                ApplyValue(
                    skill,
                    skill.searingShotExplosionRadius,
                    SkillUpgradeData.UpgradeStat.AoE
                );
        }


        if (skill.rainOfFireRadius > 0f)
        {
            skill.rainOfFireRadius =
                ApplyValue(
                    skill,
                    skill.rainOfFireRadius,
                    SkillUpgradeData.UpgradeStat.AoE
                );
        }


        if (skill.energyOrbHitRadius > 0f)
        {
            skill.energyOrbHitRadius =
                ApplyValue(
                    skill,
                    skill.energyOrbHitRadius,
                    SkillUpgradeData.UpgradeStat.AoE
                );
        }


        if (skill.breathOfFireAngle > 0f)
        {
            skill.breathOfFireAngle =
                ApplyValue(
                    skill,
                    skill.breathOfFireAngle,
                    SkillUpgradeData.UpgradeStat.AoE
                );
        }


        if (skill.spinningLaserWidth > 0f)
        {
            skill.spinningLaserWidth =
                ApplyValue(
                    skill,
                    skill.spinningLaserWidth,
                    SkillUpgradeData.UpgradeStat.AoE
                );
        }
    }


    // =========================================================
    // APPLY VALUE
    // =========================================================

    private float ApplyValue(
        SkillInstance skill,
        float baseValue,
        SkillUpgradeData.UpgradeStat stat)
    {
        float value = baseValue;


        value *=
            GetPercentMultiplier(
                skill,
                stat
            );


        value +=
            GetFlatBonus(
                skill,
                stat
            );


        return Mathf.Max(
            0f,
            value
        );
    }


    // =========================================================
    // ATTACK SPEED / COOLDOWN
    // =========================================================

    private float ApplyAttackSpeedOrCooldown(
        SkillInstance skill)
    {
        if (skill == null)
            return 0.01f;


        bool isProjectileSkill =
            (
                skill.data.categories &
                SkillCategory.Projectile
            ) != SkillCategory.None;


        // =====================================================
        // PROJECTILE → ATTACK SPEED
        // =====================================================

        if (isProjectileSkill)
        {
            float value =
                skill.fireRate;


            float multiplier =
                GetPercentMultiplier(
                    skill,
                    SkillUpgradeData.UpgradeStat.AttackSpeed
                );


            if (multiplier > 0f)
            {
                value /=
                    multiplier;
            }


            value +=
                GetFlatBonus(
                    skill,
                    SkillUpgradeData.UpgradeStat.AttackSpeed
                );


            return Mathf.Max(
                0.01f,
                value
            );
        }


        // =====================================================
        // NON-PROJECTILE → COOLDOWN
        // =====================================================

        float cooldownValue =
            skill.fireRate;


        float cooldownMultiplier =
            GetPercentMultiplier(
                skill,
                SkillUpgradeData.UpgradeStat.Cooldown
            );


        if (cooldownMultiplier > 0f)
        {
            cooldownValue *=
                cooldownMultiplier;
        }


        cooldownValue +=
            GetFlatBonus(
                skill,
                SkillUpgradeData.UpgradeStat.Cooldown
            );


        return Mathf.Max(
            0.01f,
            cooldownValue
        );
    }
}