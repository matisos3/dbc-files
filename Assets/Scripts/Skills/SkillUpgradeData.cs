using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(
    fileName = "New Skill Upgrade",
    menuName = "TowerDefense/Skill Upgrade"
)]
public class SkillUpgradeData : ScriptableObject
{
    // =========================================================
    // ID
    // =========================================================

    [Header("Identity")]
    [Tooltip("Stały identyfikator ulepszenia. NIE tłumaczyć.")]
    public string upgradeID;


    // =========================================================
    // LOKALIZACJA
    // =========================================================

    [Header("Localization")]
    [Tooltip("Lokalizowana nazwa ulepszenia.")]
    public LocalizedString localizedName;

    [Tooltip("Lokalizowany opis ulepszenia.")]
    public LocalizedString localizedDescription;


    // =========================================================
    // WIZUALNE
    // =========================================================

    [Header("Visual")]
    public Sprite icon;


    // =========================================================
    // CENA
    // =========================================================

    [Header("Cena")]
    public int cost = 500;


    // =========================================================
    // CEL ULEPSZENIA
    // =========================================================

    public enum UpgradeTarget
    {
        Global,
        Element,
        Category,
        ElementAndCategory
    }

    [Header("Cel ulepszenia")]
    public UpgradeTarget target;


    // =========================================================
    // ELEMENT
    // =========================================================

    [Header("Element")]
    public SkillData.DamageType element;


    // =========================================================
    // KATEGORIA
    // =========================================================

    [Header("Kategoria")]
    public SkillCategory category;


    // =========================================================
    // STATYSTYKI
    // =========================================================

    public enum UpgradeStat
    {
        Damage,
        AttackSpeed,
        Cooldown,
        Range,
        AoE,
        CriticalChance,
        CriticalDamage,
        ExplosionRadius,
        ProjectileCount,
        PiercingCount,
        IceShotPiercingCount,

        ChainCount,
        ChainRange,
        ChainDamage,

        DotDamage,
        DotDuration,

        SlowPercent,
        SlowDuration,
        RootDuration,
        FreezeDuration,

        // =====================================================
        // OBRONA WIEŻY
        // =====================================================

        HealthRegeneration,
        MaxHealth,
        DamageReductionFlat,
        DamageReductionPercent
    }


    // =========================================================
    // TYP MODYFIKATORA
    // =========================================================

    public enum ModifierType
    {
        Percent,
        Flat
    }


    // =========================================================
    // MODYFIKATOR
    // =========================================================

    [System.Serializable]
    public class UpgradeModifier
    {
        [Header("Statystyka")]
        public UpgradeStat stat;


        [Header("Typ")]
        public ModifierType modifierType =
            ModifierType.Percent;


        [Header("Wartość")]
        [Tooltip(
            "Dla Percent: np. 5 = +5%.\n" +
            "Dla Flat: np. 2 = +2.\n" +
            "Dla Cooldown Percent: np. 3 = -3% czasu odnowienia."
        )]
        public float value = 5f;


        // =====================================================
        // NIELINIOWA PROGRESJA
        // =====================================================

        [Header("Nieliniowa progresja")]

        [Tooltip(
            "Mnożnik wzrostu wartości na kolejnych poziomach.\n\n" +
            "1.0 = brak dodatkowego wzrostu.\n" +
            "1.1 = każdy kolejny poziom zwiększa wartość o 10%.\n" +
            "1.2 = każdy kolejny poziom zwiększa wartość o 20%.\n" +
            "1.5 = każdy kolejny poziom zwiększa wartość o 50%.\n\n" +
            "Ta wartość jest używana dla MaxHealth " +
            "oraz HealthRegeneration."
        )]
        [Min(1f)]
        public float nonlinearMultiplier = 1.2f;
    }


    // =========================================================
    // MODYFIKATORY
    // =========================================================

    [Header("Modyfikatory")]

    [Tooltip(
        "Jedno ulepszenie może modyfikować wiele statystyk jednocześnie."
    )]
    public List<UpgradeModifier> modifiers =
        new List<UpgradeModifier>();
}