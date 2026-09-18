using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class SkillSelectionManager : MonoBehaviour
{
// =========================================================
// REFERENCES
// =========================================================

[Header("Skills")]
public List<SkillData> allSkills;
public SkillButton[] buttons;

[Header("Description")]
public TMP_Text descriptionText;

[Header("Localization statystyk")]
[SerializeField]
private LocalizedStringTable statLocalizationTable;

[Header("Tagi skilla")]
public SkillTagIconDatabase tagIconDatabase;
public Transform tagIconContainer;
public UnityEngine.UI.Image tagIconPrefab;

[Header("Wymiary Okna")]
[SerializeField]
private GameObject descriptionWindow;

[SerializeField]
private RectTransform descriptionRect;

[SerializeField]
private float descriptionPadding = 30f;

[SerializeField]
private float descriptionHorizontalPadding = 40f;

[SerializeField]
private float minDescriptionWidth = 350f;

[SerializeField]
private float maxDescriptionWidth = 400f;

[Header("Resources")]
[SerializeField]
private TMP_Text costText;

[Header("Tower")]
public Tower tower;


// =========================================================
// CENA SKILLI
// =========================================================

[Header("Cena skilla")]
[SerializeField]
private int maxActiveSkills = 6;


// =========================================================
// LICZNIK ZAKUPÓW KAŻDEGO SKILLA
// =========================================================
//
// Każdy skill ma osobny licznik.
//
// Klucz = SkillData.skillID
// Wartość = liczba zakupów tego konkretnego skilla
//
// Przykład:
//
// FireArrow    = 2
// ColdArrow    = 1
// Lightning   = 0
//
// FireArrow:
// 500 -> 1500 -> 4500
//
// ColdArrow:
// 500 -> 1500
//
// Lightning:
// 500
//
// =========================================================

private readonly Dictionary<string, int> skillPurchaseCounts =
    new Dictionary<string, int>(StringComparer.Ordinal);


// =========================================================
// INTERNAL
// =========================================================

private SkillButton selectedButton;
private SkillData selectedSkill;

private Vector2 initialDescriptionSize;
private bool initialDescriptionSizeCaptured;

private Coroutine resizeDescriptionCoroutine;


// =========================================================
// COLORS
// =========================================================

private const string CurrentValueColor = "#ffffff";
private const string ArrowColor = "#888888";
private const string AttackSpeedColor = "#ffee00";


// =========================================================
// STATISTIC ENTRY
// =========================================================

private class StatisticEntry
{
    public string text;
    public int priority;
    public int order;

    public StatisticEntry(
        string text,
        int priority,
        int order)
    {
        this.text = text;
        this.priority = priority;
        this.order = order;
    }
}


// =========================================================
// START
// =========================================================

private void Start()
{
    if (descriptionRect != null)
    {
        initialDescriptionSize =
            descriptionRect.sizeDelta;

        initialDescriptionSizeCaptured = true;
    }

    LocalizationSettings.SelectedLocaleChanged +=
        OnLanguageChanged;

    ShowSkills();

    if (descriptionWindow != null)
        descriptionWindow.SetActive(false);

    ClearTagIcons();
}


// =========================================================
// LANGUAGE CHANGED
// =========================================================

private void OnLanguageChanged(Locale locale)
{
    if (selectedSkill == null)
        return;

    if (descriptionWindow == null ||
        !descriptionWindow.activeSelf)
    {
        return;
    }

    RefreshSelectedSkillDescription();
}


// =========================================================
// REFRESH DESCRIPTION
// =========================================================

private void RefreshSelectedSkillDescription()
{
    if (selectedSkill == null)
        return;

    ShowDescription(selectedSkill);
}


// =========================================================
// UPDATE
// =========================================================

private void Update()
{
    bool clicked = false;
    Vector2 pointerPosition = Vector2.zero;

    if (Mouse.current != null &&
        Mouse.current.leftButton.wasPressedThisFrame)
    {
        clicked = true;
        pointerPosition =
            Mouse.current.position.ReadValue();
    }

    if (Touchscreen.current != null &&
        Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
    {
        clicked = true;
        pointerPosition =
            Touchscreen.current.primaryTouch.position.ReadValue();
    }

    if (clicked)
    {
        CheckClickOutsideSkill(pointerPosition);
    }
}


// =========================================================
// CLICK OUTSIDE
// =========================================================

private void CheckClickOutsideSkill(
    Vector2 screenPosition)
{
    if (EventSystem.current == null)
        return;

    PointerEventData eventData =
        new PointerEventData(EventSystem.current);

    eventData.position = screenPosition;

    List<RaycastResult> results =
        new List<RaycastResult>();

    EventSystem.current.RaycastAll(
        eventData,
        results
    );

    foreach (RaycastResult result in results)
    {
        if (result.gameObject.GetComponent<SkillButton>() != null ||
            result.gameObject.GetComponentInParent<SkillButton>() != null)
        {
            return;
        }
    }

    CloseDescription();
}


// =========================================================
// SHOW SKILLS
// =========================================================

public void ShowSkills()
{
    if (buttons == null ||
        buttons.Length == 0)
    {
        return;
    }

    if (allSkills == null ||
        allSkills.Count == 0)
    {
        return;
    }

    // =========================================================
    // TWORZYMY PULĘ DOSTĘPNYCH SKILLI
    // =========================================================
    //
    // Jeżeli wieża ma mniej niż 6 różnych skilli:
    //
    // - pokazujemy posiadane skille
    // - pokazujemy nowe skille
    //
    // Jeżeli wieża ma już 6 różnych skilli:
    //
    // - pokazujemy tylko skille już posiadane
    //
    // Dzięki temu nadal można je ulepszać.
    // =========================================================

    List<SkillData> pool =
        new List<SkillData>();

    int activeSkillCount =
        GetActiveSkillCount();

    foreach (SkillData skill in allSkills)
    {
        if (skill == null)
            continue;

        if (string.IsNullOrEmpty(skill.skillID))
            continue;

        bool alreadyOwned =
            HasSkill(skill);

        if (alreadyOwned)
        {
            pool.Add(skill);
            continue;
        }

        if (activeSkillCount < maxActiveSkills)
        {
            pool.Add(skill);
        }
    }

    // =========================================================
    // PRZYWRACAMY PRZYCISKI
    // =========================================================

    foreach (SkillButton button in buttons)
    {
        if (button == null)
            continue;

        button.gameObject.SetActive(false);
    }

    // =========================================================
    // LOSOWANIE SKILLI
    // =========================================================

    foreach (SkillButton button in buttons)
    {
        if (button == null)
            continue;

        if (pool.Count == 0)
            break;

        int randomIndex =
            UnityEngine.Random.Range(
                0,
                pool.Count
            );

        SkillData selectedSkillFromPool =
            pool[randomIndex];

        button.Setup(
            selectedSkillFromPool,
            this
        );

        button.gameObject.SetActive(true);

        pool.RemoveAt(randomIndex);
    }

    CloseDescription();
}


// =========================================================
// BUTTON CLICKED
// =========================================================

public void ButtonClicked(
    SkillButton button,
    SkillData skill)
{
    if (button == null ||
        skill == null)
    {
        return;
    }

    SkillUpgradeSelectionManager upgradeManager =
        FindAnyObjectByType<SkillUpgradeSelectionManager>();

    if (upgradeManager != null)
    {
        upgradeManager.CloseDescription();
        Canvas.ForceUpdateCanvases();
    }

    if (selectedButton == button)
    {
        SelectSkill(
            skill,
            button
        );

        return;
    }

    if (selectedButton != null)
    {
        selectedButton.RemoveHighlight();
    }

    selectedButton = button;
    selectedSkill = skill;

    selectedButton.Highlight();

    ShowDescription(skill);
}


// =========================================================
// SHOW DESCRIPTION
// =========================================================

private void ShowDescription(
    SkillData skill)
{
    if (skill == null)
        return;

    selectedSkill = skill;

    if (descriptionWindow != null)
    {
        descriptionWindow.SetActive(true);
    }

    // =========================================================
    // AKTUALNA CENA TEGO KONKRETNEGO SKILLA
    // =========================================================

    if (costText != null)
    {
        costText.text =
            GetCurrentSkillCost(skill).ToString();
    }

    string skillName = "";

    if (skill.localizedName != null)
    {
        skillName =
            skill.localizedName.GetLocalizedString();
    }

    string localizedDescription = "";

    if (skill.localizedDescription != null)
    {
        localizedDescription =
            skill.localizedDescription.GetLocalizedString();
    }

    SkillInstance existingSkill =
        GetExistingSkill(skill);

    string statistics;

    if (existingSkill == null)
    {
        statistics =
            BuildInitialStatistics(skill);
    }
    else
    {
        statistics =
            BuildUpgradeStatistics(existingSkill);
    }

    string finalText =
        "\n\n<size=120%><b>" +
        skillName +
        "</b></size>\n\n" +

        "<i>" +
        localizedDescription +
        "</i>\n\n<smallcaps>" +

        statistics +

        "</smallcaps>\n\n";

    if (descriptionText != null)
    {
        descriptionText.text =
            finalText;
    }

    BuildTagIcons(skill);

    if (resizeDescriptionCoroutine != null)
    {
        StopCoroutine(
            resizeDescriptionCoroutine
        );
    }

    resizeDescriptionCoroutine =
        StartCoroutine(
            ResizeDescriptionNextFrame()
        );
}


// =========================================================
// LOCALIZE STAT LABEL
// =========================================================

private string LocalizeStatLabel(
    string fallback)
{
    if (string.IsNullOrEmpty(fallback))
        return "";

    string key =
        GetLocalizationKey(fallback);

    if (string.IsNullOrEmpty(key))
        return fallback;

    if (statLocalizationTable == null)
        return fallback;

    try
    {
        string localizedValue =
            LocalizationSettings.StringDatabase
                .GetLocalizedString(
                    statLocalizationTable.TableReference,
                    key
                );

        if (!string.IsNullOrEmpty(localizedValue))
            return localizedValue;
    }
    catch
    {
        // Fallback poniżej.
    }

    return fallback;
}


// =========================================================
// LOCALIZATION KEY MAP
// =========================================================

private string GetLocalizationKey(
    string fallback)
{
    switch (fallback)
    {
        case "Obrażenia":
            return "damage";

        case "Szybkość ataku":
            return "attack_speed";

        case "Cooldown":
        case "Czas odnowienia":
            return "cooldown";

        case "Zasięg":
            return "range";

        case "Promień":
            return "radius";

        case "Promień eksplozji":
            return "explosion_radius";

        case "Liczba pocisków":
            return "projectile_count";

        case "Liczba przebić":
            return "piercing_count";

        case "Spowolnienie":
            return "slow_percent";

        case "Czas spowolnienia":
            return "slow_duration";

        case "Czas unieruchomienia":
            return "root_duration";

        case "Liczba przeskoków":
            return "chain_count";

        case "Zasięg łańcucha":
            return "chain_range";

        case "ChainMulti":
            return "chain_damage";

        case "Szansa na przeskok":
            return "chain_chance";

        case "Szansa na porażenie":
            return "stun_chance";

        case "Dodatkowe obrażenia":
            return "bonus_damage";

        case "Obrażenia DoT":
            return "dot_damage";

        case "Czas DoT":
            return "dot_duration";

        case "Interwał DoT":
            return "dot_tick_interval";

        case "Redukcja szybkości ataku":
            return "corrupted_attack_speed";

        case "Otrzymywane obrażenia":
            return "corrupted_damage_taken";

        case "Czas działania":
            return "duration";

        case "Czas trwania":
            return "duration";

        case "Maks. kumulacji":
            return "max_stacks";

        case "Maks. celów":
            return "max_targets";

        case "Promień chmury":
            return "cloud_radius";

        case "Interwał obrażeń chmury":
            return "cloud_tick_interval";

        case "Promień wiru":
            return "whirl_radius";

        case "Interwał obrażeń":
            return "damage_interval";

        case "Siła przyciągania":
            return "whirl_pull_speed";

        case "Ilość Slime":
            return "slime_count";

        case "Promień Slime":
            return "slime_radius";

        case "Interwał Slime DoT":
            return "slime_dot_tick_interval";

        case "Obrażenia fali":
            return "wave_damage";

        case "Czas zamrożenia":
            return "freeze_duration";

        case "Promień Blizzard":
            return "blizzard_radius";

        case "Interwał":
            return "blizzard_tick_interval";

        case "Liczba odłamków":
            return "ice_spike_count";

        case "Mnożnik obrażeń odłamków":
            return "ice_spike_damage";

        case "Prędkość odłamków":
            return "ice_spike_speed";

        case "Czas życia odłamków":
            return "ice_spike_lifetime";

        case "Promień fali":
            return "freezing_wave_radius";

        case "Prędkość fali":
            return "freezing_wave_speed";

        case "Dystans fali":
            return "freezing_wave_distance";

        case "Promień Frost Nova":
            return "frost_nova_radius";

        case "Obrażenia ognia":
            return "fire_damage";

        case "Obrażenia podpalenia":
            return "burn_damage";

        case "Czas podpalenia":
            return "burn_duration";

        case "Obrażenia eksplozji":
            return "explosion_damage";

        case "Zasięg ognia":
            return "fire_range";

        case "Promień Flaming Circle":
            return "flaming_circle_radius";

        case "Promień Rain Of Fire":
            return "rain_of_fire_radius";

        case "Interwał podpalenia":
            return "burn_tick_interval";

        case "Liczba kul":
            return "sphere_count";

        case "Obrażenia kuli":
            return "sphere_damage";

        case "Prędkość kuli":
            return "sphere_speed";

        case "Czas ogłuszenia":
            return "stun_duration";

        case "Prędkość pocisku":
            return "projectile_speed";

        case "Promień trafienia":
            return "hit_radius";

        case "Siła odrzutu":
            return "knockback";

        case "Liczba ramion":
            return "laser_count";

        case "Prędkość obrotu":
            return "rotation_speed";

        default:
            return null;
    }
}


// =========================================================
// INITIAL STATISTICS
// =========================================================

private string BuildInitialStatistics(
    SkillData skill)
{
    List<StatisticEntry> lines =
        new List<StatisticEntry>();

    int order = 0;

    AddInitialStat(lines, skill, "damage",
        "Obrażenia", "damage", ref order);

    bool isProjectile =
        (skill.categories & SkillCategory.Projectile) != 0;

    string fireRateLabel =
        isProjectile
            ? "Szybkość ataku"
            : "Czas odnowienia";

    string fireRateFormat =
        isProjectile
            ? "attackSpeed"
            : "cooldown";

    AddInitialStat(lines, skill, "fireRate",
        fireRateLabel, fireRateFormat, ref order);

    if ((skill.categories & SkillCategory.Piercing) != 0)
    {
        AddInitialStat(lines, skill, "piercingCount",
            "Liczba przebić", "integer", ref order);
    }

    AddInitialStat(lines, skill, "slowPercent",
        "Spowolnienie", "percentFraction", ref order);

    AddInitialStat(lines, skill, "slowDuration",
        "Czas spowolnienia", "seconds", ref order);

    AddInitialStat(lines, skill, "rootDuration",
        "Czas unieruchomienia", "seconds", ref order);

    AddInitialStat(lines, skill, "chainCount",
        "Liczba przeskoków", "integer", ref order);

    AddInitialStat(lines, skill, "chainDamageMultiplier",
        "ChainMulti", "multiplier", ref order);

    AddInitialStat(lines, skill, "lightningStrikeChance",
        "Szansa na przeskok", "percentFraction", ref order);

    AddInitialStat(lines, skill, "dotDamage",
        "Obrażenia DoT", "damage", ref order);

    AddInitialStat(lines, skill, "dotDuration",
        "Czas DoT", "seconds", ref order);

    if (skill.projectileCount > 1)
    {
        AddInitialStat(lines, skill, "projectileCount",
            "Liczba pocisków", "integer", ref order);
    }

    AddInitialStat(lines, skill, "shotgunProjectileCount",
        "Liczba pocisków", "integer", ref order);

    AddInitialStat(lines, skill, "corruptedSlowPercent",
        "Spowolnienie", "percentFraction", ref order);

    AddInitialStat(lines, skill, "corruptedAttackSpeedReduction",
        "Redukcja szybkości ataku",
        "percentFraction", ref order);

    AddInitialStat(lines, skill, "corruptedDamageTakenIncrease",
        "Otrzymywane obrażenia",
        "percentIncrease", ref order);

    AddInitialStat(lines, skill, "corruptedDuration",
        "Czas działania", "seconds", ref order);

    AddInitialStat(lines, skill, "corruptedMaxStacks",
        "Maks. kumulacji", "integer", ref order);

    AddInitialStat(lines, skill, "corruptedMaxTargets",
        "Maks. celów", "integer", ref order);

    AddInitialStat(lines, skill, "cloudDuration",
        "Czas działania", "seconds", ref order);

    AddInitialStat(lines, skill, "whirlDuration",
        "Czas działania", "seconds", ref order);

    AddInitialStat(lines, skill, "whirlDamage",
        "Obrażenia", "damage", ref order);
    
    AddInitialStat(lines, skill, "whirlPullSpeed",
        "Siła przyciągania", "number", ref order);

    AddInitialStat(lines, skill, "slimeCount",
        "Ilość Slime", "integer", ref order);

    AddInitialStat(lines, skill, "slimeSlowPercent",
        "Spowolnienie", "percentFraction", ref order);

    AddInitialStat(lines, skill, "slimeDotDamage",
        "Obrażenia DoT", "damage", ref order);

    AddInitialStat(lines, skill, "slimeDotDuration",
        "Czas DoT", "seconds", ref order);

    AddInitialStat(lines, skill, "blizzardDamage",
        "Obrażenia", "damage", ref order);

    AddInitialStat(lines, skill, "blizzardDuration",
        "Czas działania", "seconds", ref order);

    AddInitialStat(lines, skill, "blizzardSlowPercent",
        "Spowolnienie", "percentFraction", ref order);

    AddInitialStat(lines, skill, "iceSpikeCount",
        "Liczba odłamków", "integer", ref order);

    AddInitialStat(lines, skill, "iceSpikeDamageMultiplier",
        "Mnożnik obrażeń odłamków", "multiplier", ref order);

    AddInitialStat(lines, skill, "freezingWaveFreezeDuration",
        "Czas zamrożenia", "seconds", ref order);

    AddInitialStat(lines, skill, "freezingWaveDamage",
        "Obrażenia fali", "damage", ref order);

    AddInitialStat(lines, skill, "frostNovaDamage",
        "Obrażenia", "damage", ref order);

    AddInitialStat(lines, skill, "frostNovaFreezeDuration",
        "Czas zamrożenia", "seconds", ref order);

    AddInitialStat(lines, skill, "frostNovaCooldown",
        "Cooldown", "seconds", ref order);

    AddInitialStat(lines, skill, "iceShotDamage",
        "Obrażenia", "damage", ref order);

    AddInitialStat(lines, skill, "iceShotSlowPercent",
        "Spowolnienie", "percentDirect", ref order);

    AddInitialStat(lines, skill, "iceShotSlowDuration",
        "Czas spowolnienia", "seconds", ref order);

    AddInitialStat(lines, skill, "breathOfFireDamage",
        "Obrażenia ognia", "damage", ref order);

    AddInitialStat(lines, skill, "breathOfFireBurnDamage",
        "Obrażenia podpalenia", "damage", ref order);

    AddInitialStat(lines, skill, "breathOfFireBurnDuration",
        "Czas podpalenia", "seconds", ref order);

    AddInitialStat(lines, skill, "flamingCircleDamage",
        "Obrażenia podpalenia", "damage", ref order);

    AddInitialStat(lines, skill, "flamingCircleDuration",
        "Czas podpalenia", "seconds", ref order);

    AddInitialStat(lines, skill, "searingShotDamage",
        "Obrażenia", "damage", ref order);

    AddInitialStat(lines, skill, "searingShotBurnDamage",
        "Obrażenia podpalenia", "damage", ref order);

    AddInitialStat(lines, skill, "searingShotBurnDuration",
        "Czas podpalenia", "seconds", ref order);

    AddInitialStat(lines, skill, "searingShotExplosionDamage",
        "Obrażenia eksplozji", "damage", ref order);

    AddInitialStat(lines, skill, "rainOfFireDuration",
        "Czas działania", "seconds", ref order);

    AddInitialStat(lines, skill, "rainOfFireDamage",
        "Obrażenia", "damage", ref order);

    AddInitialStat(lines, skill, "rainOfFireBurnDamage",
        "Obrażenia podpalenia", "damage", ref order);

    AddInitialStat(lines, skill, "rainOfFireBurnDuration",
        "Czas podpalenia", "seconds", ref order);

    AddInitialStat(lines, skill, "volcanicSphereCount",
        "Liczba kul", "integer", ref order);

    AddInitialStat(lines, skill, "volcanicSphereDamage",
        "Obrażenia", "damage", ref order);

    AddInitialStat(lines, skill, "electricShockDamage",
        "Obrażenia", "damage", ref order);

    AddInitialStat(lines, skill, "electricShockStunDuration",
        "Czas ogłuszenia", "seconds", ref order);

    AddInitialStat(lines, skill, "energyOrbKnockback",
        "Siła odrzutu", "number", ref order);

    AddInitialStat(lines, skill, "spinningLaserCount",
        "Liczba ramion", "integer", ref order);

    AddInitialStat(lines, skill, "spinningLaserDamage",
        "Obrażenia", "damage", ref order);

    AddInitialStat(lines, skill, "lightningStunChance",
        "Szansa na porażenie", "percentFraction", ref order);

    AddInitialStat(lines, skill, "lightningBonusDamage",
        "Dodatkowe obrażenia", "damage", ref order);

    return BuildStatisticsText(lines);
}


// =========================================================
// UPGRADE STATISTICS
// =========================================================

private string BuildUpgradeStatistics(
    SkillInstance skill)
{
    if (skill == null ||
        skill.data == null)
    {
        return "";
    }

    List<StatisticEntry> lines =
        new List<StatisticEntry>();

    int order = 0;

    SkillData data = skill.data;

    AddUpgradeStat(lines, skill, data,
        "damage", "damageUpgrade",
        "Obrażenia", "damage", ref order);

    bool isProjectile =
        (data.categories & SkillCategory.Projectile) != 0;

    string fireRateLabel =
        isProjectile
            ? "Szybkość ataku"
            : "Cooldown";

    string fireRateFormat =
        isProjectile
            ? "attackSpeed"
            : "cooldown";

    AddUpgradeStat(lines, skill, data,
        "fireRate", "fireRateUpgrade",
        fireRateLabel, fireRateFormat, ref order);

    AddUpgradeStat(lines, skill, data,
        "range", "rangeUpgrade",
        "Zasięg", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "explosionRadius", "explosionRadiusUpgrade",
        "Promień eksplozji", "number", ref order);

    AddCurrentPiercingStat(lines, skill, ref order);

    AddUpgradeStat(lines, skill, data,
        "slowPercent", "slowPercentUpgrade",
        "Spowolnienie", "percentFraction", ref order);

    AddUpgradeStat(lines, skill, data,
        "slowDuration", "slowDurationUpgrade",
        "Czas spowolnienia", "seconds", ref order);

    AddUpgradeStat(lines, skill, data,
        "rootDuration", "rootDurationUpgrade",
        "Czas unieruchomienia", "seconds", ref order);

    AddUpgradeStat(lines, skill, data,
        "chainCount", "chainCountUpgrade",
        "Liczba przeskoków", "integer", ref order);

    AddUpgradeStat(lines, skill, data,
        "chainRange", "chainRangeUpgrade",
        "Zasięg łańcucha", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "chainDamageMultiplier",
        "chainDamageMultiplierUpgrade",
        "ChainMulti", "multiplier", ref order);

    AddUpgradeStat(lines, skill, data,
        "lightningStrikeChance",
        "lightningStrikeChanceUpgrade",
        "Szansa na przeskok",
        "percentFraction", ref order);

    AddUpgradeStat(lines, skill, data,
        "dotDamage", "dotDamageUpgrade",
        "Obrażenia DoT", "damage", ref order);

    AddUpgradeStat(lines, skill, data,
        "dotDuration", "dotDurationUpgrade",
        "Czas DoT", "seconds", ref order);

    AddUpgradeStat(lines, skill, data,
        "dotTickInterval", "dotTickIntervalUpgrade",
        "Interwał DoT", "seconds", ref order);

    // =========================================================
    // LICZBA POCISKÓW
    // =========================================================

    if (data.shotgunProjectileCount <= 0)
    {
        AddUpgradeStat(
            lines,
            skill,
            data,
            "projectileCount",
            "projectileCountUpgrade",
            "Liczba pocisków",
            "integer",
            ref order
        );
    }

    AddUpgradeStat(
        lines,
        skill,
        data,
        "shotgunProjectileCount",
        "projectileCountUpgrade",
        "Liczba pocisków",
        "integer",
        ref order
    );

    AddUpgradeStat(lines, skill, data,
        "corruptedSlowPercent",
        "corruptedSlowPercentUpgrade",
        "Spowolnienie", "percentFraction", ref order);

    AddUpgradeStat(lines, skill, data,
        "corruptedAttackSpeedReduction",
        "corruptedAttackSpeedReductionUpgrade",
        "Redukcja szybkości ataku",
        "percentFraction", ref order);

    AddUpgradeStat(lines, skill, data,
        "corruptedDamageTakenIncrease",
        "corruptedDamageTakenIncreaseUpgrade",
        "Otrzymywane obrażenia",
        "percentIncrease", ref order);

    AddUpgradeStat(lines, skill, data,
        "corruptedDuration",
        "corruptedDurationUpgrade",
        "Czas działania", "seconds", ref order);

    AddUpgradeStat(lines, skill, data,
        "corruptedMaxStacks",
        "corruptedMaxStacksUpgrade",
        "Maks. kumulacji", "integer", ref order);

    AddUpgradeStat(lines, skill, data,
        "corruptedMaxTargets",
        "corruptedMaxTargetsUpgrade",
        "Maks. celów", "integer", ref order);

    AddUpgradeStat(lines, skill, data,
        "corruptedChainRange",
        "corruptedChainRangeUpgrade",
        "Zasięg łańcucha", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "cloudRadius", "cloudRadiusUpgrade",
        "Promień chmury", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "cloudDuration", "cloudDurationUpgrade",
        "Czas działania", "seconds", ref order);

    AddUpgradeStat(lines, skill, data,
        "cloudTickInterval", "cloudTickIntervalUpgrade",
        "Interwał obrażeń chmury", "seconds", ref order);

    AddUpgradeStat(lines, skill, data,
        "whirlRadius", "whirlRadiusUpgrade",
        "Promień wiru", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "whirlDuration", "whirlDurationUpgrade",
        "Czas działania", "seconds", ref order);

    AddUpgradeStat(lines, skill, data,
        "whirlTickInterval", "whirlTickIntervalUpgrade",
        "Interwał obrażeń", "seconds", ref order);

    AddUpgradeStat(lines, skill, data,
        "whirlPullSpeed", "whirlPullSpeedUpgrade",
        "Siła przyciągania", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "whirlDamage", "whirlDamageUpgrade",
        "Obrażenia", "damage", ref order);

    AddUpgradeStat(lines, skill, data,
        "slimeRadius", "slimeRadiusUpgrade",
        "Promień Slime", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "slimeSlowPercent", "slimeSlowPercentUpgrade",
        "Spowolnienie", "percentFraction", ref order);

    AddUpgradeStat(lines, skill, data,
        "slimeDotDamage", "slimeDotDamageUpgrade",
        "Obrażenia DoT", "damage", ref order);

    AddUpgradeStat(lines, skill, data,
        "slimeDotDuration", "slimeDotDurationUpgrade",
        "Czas DoT", "seconds", ref order);

    AddUpgradeStat(lines, skill, data,
        "slimeDotTickInterval", "slimeDotTickIntervalUpgrade",
        "Interwał Slime DoT", "seconds", ref order);

    AddUpgradeStat(lines, skill, data,
        "blizzardDamage", "blizzardDamageUpgrade",
        "Obrażenia", "damage", ref order);

    AddUpgradeStat(lines, skill, data,
        "blizzardRadius", "blizzardRadiusUpgrade",
        "Promień Blizzard", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "blizzardDuration", "blizzardDurationUpgrade",
        "Czas trwania", "seconds", ref order);

    AddUpgradeStat(lines, skill, data,
        "blizzardTickInterval", "blizzardTickIntervalUpgrade",
        "Interwał", "seconds", ref order);

    AddUpgradeStat(lines, skill, data,
        "blizzardSlowPercent", "blizzardSlowPercentUpgrade",
        "Spowolnienie", "percentFraction", ref order);

    AddUpgradeStat(lines, skill, data,
        "iceSpikeCount", "iceSpikeCountUpgrade",
        "Liczba odłamków", "integer", ref order);

    AddUpgradeStat(lines, skill, data,
        "iceSpikeDamageMultiplier",
        "iceSpikeDamageMultiplierUpgrade",
        "Mnożnik obrażeń odłamków",
        "multiplier", ref order);

    AddUpgradeStat(lines, skill, data,
        "iceSpikeSpeed", "iceSpikeSpeedUpgrade",
        "Prędkość odłamków", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "iceSpikeLifetime", "iceSpikeLifetimeUpgrade",
        "Czas życia odłamków", "seconds", ref order);

    AddUpgradeStat(lines, skill, data,
        "freezingWaveRadius",
        "freezingWaveRadiusUpgrade",
        "Promień fali", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "freezingWaveSpeed",
        "freezingWaveSpeedUpgrade",
        "Prędkość fali", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "freezingWaveDistance",
        "freezingWaveDistanceUpgrade",
        "Dystans fali", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "freezingWaveFreezeDuration",
        "freezingWaveFreezeDurationUpgrade",
        "Czas zamrożenia", "seconds", ref order);

    AddUpgradeStat(lines, skill, data,
        "freezingWaveDamage",
        "freezingWaveDamageUpgrade",
        "Obrażenia fali", "damage", ref order);

    AddUpgradeStat(lines, skill, data,
        "frostNovaRadius", "frostNovaRadiusUpgrade",
        "Promień Frost Nova", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "frostNovaDamage", "frostNovaDamageUpgrade",
        "Obrażenia", "damage", ref order);

    AddUpgradeStat(lines, skill, data,
        "frostNovaFreezeDuration",
        "frostNovaFreezeDurationUpgrade",
        "Czas zamrożenia", "seconds", ref order);

    AddCooldownUpgradeStat(
        lines,
        skill,
        data,
        "frostNovaCooldown",
        "frostNovaCooldownUpgrade",
        "Cooldown",
        ref order
    );

    AddUpgradeStat(lines, skill, data,
        "iceShotDamage", "iceShotDamageUpgrade",
        "Obrażenia", "damage", ref order);

    AddUpgradeStat(lines, skill, data,
        "iceShotExplosionRadius",
        "iceShotExplosionRadiusUpgrade",
        "Promień eksplozji", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "iceShotSlowPercent",
        "iceShotSlowPercentUpgrade",
        "Spowolnienie", "percentDirect", ref order);

    AddUpgradeStat(lines, skill, data,
        "iceShotSlowDuration",
        "iceShotSlowDurationUpgrade",
        "Czas spowolnienia", "seconds", ref order);

    AddUpgradeStat(lines, skill, data,
        "breathOfFireRange",
        "breathOfFireRangeUpgrade",
        "Zasięg ognia", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "breathOfFireDamage",
        "breathOfFireDamageUpgrade",
        "Obrażenia", "damage", ref order);

    AddUpgradeStat(lines, skill, data,
        "breathOfFireBurnDamage",
        "breathOfFireBurnDamageUpgrade",
        "Obrażenia podpalenia", "damage", ref order);

    AddUpgradeStat(lines, skill, data,
        "breathOfFireBurnDuration",
        "breathOfFireBurnDurationUpgrade",
        "Czas podpalenia", "seconds", ref order);

    AddUpgradeStat(lines, skill, data,
        "flamingCircleDamage",
        "flamingCircleDamageUpgrade",
        "Obrażenia podpalenia", "damage", ref order);

    AddUpgradeStat(lines, skill, data,
        "flamingCircleRadius",
        "flamingCircleRadiusUpgrade",
        "Promień Flaming Circle", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "flamingCircleDuration",
        "flamingCircleDurationUpgrade",
        "Czas podpalenia", "seconds", ref order);

    AddUpgradeStat(lines, skill, data,
        "searingShotDamage",
        "searingShotDamageUpgrade",
        "Obrażenia", "damage", ref order);

    AddUpgradeStat(lines, skill, data,
        "searingShotBurnDamage",
        "searingShotBurnDamageUpgrade",
        "Obrażenia podpalenia", "damage", ref order);

    AddUpgradeStat(lines, skill, data,
        "searingShotExplosionDamage",
        "searingShotExplosionDamageUpgrade",
        "Obrażenia eksplozji", "damage", ref order);

    AddUpgradeStat(lines, skill, data,
        "searingShotExplosionRadius",
        "searingShotExplosionRadiusUpgrade",
        "Promień eksplozji", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "rainOfFireRadius",
        "rainOfFireRadiusUpgrade",
        "Promień Rain Of Fire", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "rainOfFireDuration",
        "rainOfFireDurationUpgrade",
        "Czas działania", "seconds", ref order);

    AddUpgradeStat(lines, skill, data,
        "rainOfFireTickInterval",
        "rainOfFireTickIntervalUpgrade",
        "Interwał obrażeń", "seconds", ref order);

    AddUpgradeStat(lines, skill, data,
        "rainOfFireDamage",
        "rainOfFireDamageUpgrade",
        "Obrażenia", "damage", ref order);

    AddUpgradeStat(lines, skill, data,
        "rainOfFireBurnDamage",
        "rainOfFireBurnDamageUpgrade",
        "Obrażenia podpalenia", "damage", ref order);

    AddUpgradeStat(lines, skill, data,
        "rainOfFireBurnDuration",
        "rainOfFireBurnDurationUpgrade",
        "Czas podpalenia", "seconds", ref order);

    AddUpgradeStat(lines, skill, data,
        "rainOfFireBurnTickInterval",
        "rainOfFireBurnTickIntervalUpgrade",
        "Interwał podpalenia", "seconds", ref order);

    AddUpgradeStat(lines, skill, data,
        "volcanicSphereCount",
        "volcanicSphereCountUpgrade",
        "Liczba kul", "integer", ref order);

    AddUpgradeStat(lines, skill, data,
        "volcanicSphereDamage",
        "volcanicSphereDamageUpgrade",
        "Obrażenia kuli", "damage", ref order);

    AddUpgradeStat(lines, skill, data,
        "volcanicSphereSpeed",
        "volcanicSphereSpeedUpgrade",
        "Prędkość kuli", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "electricShockDamage",
        "electricShockDamageUpgrade",
        "Obrażenia", "damage", ref order);

    AddUpgradeStat(lines, skill, data,
        "electricShockStunDuration",
        "electricShockStunDurationUpgrade",
        "Czas ogłuszenia", "seconds", ref order);

    AddUpgradeStat(lines, skill, data,
        "electricShockProjectileSpeed",
        "electricShockProjectileSpeedUpgrade",
        "Prędkość pocisku", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "energyOrbProjectileSpeed",
        "energyOrbProjectileSpeedUpgrade",
        "Prędkość pocisku", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "energyOrbHitRadius",
        "energyOrbHitRadiusUpgrade",
        "Promień trafienia", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "energyOrbKnockback",
        "energyOrbKnockbackUpgrade",
        "Siła odrzutu", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "spinningLaserCount",
        "spinningLaserCountUpgrade",
        "Liczba ramion", "integer", ref order);

    AddUpgradeStat(lines, skill, data,
        "spinningLaserRadius",
        "spinningLaserRadiusUpgrade",
        "Promień", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "spinningLaserRotationSpeed",
        "spinningLaserRotationSpeedUpgrade",
        "Prędkość obrotu", "number", ref order);

    AddUpgradeStat(lines, skill, data,
        "spinningLaserDamage",
        "spinningLaserDamageUpgrade",
        "Obrażenia", "damage", ref order);

    AddUpgradeStat(lines, skill, data,
        "spinningLaserDamageInterval",
        "spinningLaserDamageIntervalUpgrade",
        "Interwał obrażeń", "seconds", ref order);

    AddUpgradeStat(
        lines,
        skill,
        data,
        "lightningStunChance",
        "lightningStunChanceUpgrade",
        "Szansa na porażenie",
        "percentFraction",
        ref order
    );

    AddUpgradeStat(
        lines,
        skill,
        data,
        "lightningBonusDamage",
        "lightningBonusDamageUpgrade",
        "Dodatkowe obrażenia",
        "damage",
        ref order
    );

    return BuildStatisticsText(lines);
}


// =========================================================
// CURRENT PIERCING
// =========================================================

private void AddCurrentPiercingStat(
    List<StatisticEntry> lines,
    SkillInstance skill,
    ref int order)
{
    if (lines == null ||
        skill == null ||
        skill.data == null)
    {
        return;
    }

    if ((skill.data.categories &
        SkillCategory.Piercing) == 0)
    {
        return;
    }

    int piercing =
        Mathf.Max(
            1,
            skill.piercingCount
        );

    string label =
        LocalizeStatLabel(
            "Liczba przebić"
        );

    lines.Add(
        new StatisticEntry(
            label +
            ": " +
            Value(piercing.ToString()),
            1,
            order++
        )
    );
}


// =========================================================
// INITIAL STAT HELPER
// =========================================================

private void AddInitialStat(
    List<StatisticEntry> lines,
    SkillData skill,
    string fieldName,
    string label,
    string format,
    ref int order,
    float minimumValue = 0f)
{
    if (skill == null)
        return;

    FieldInfo field =
        typeof(SkillData).GetField(
            fieldName,
            BindingFlags.Public |
            BindingFlags.Instance
        );

    if (field == null)
        return;

    object rawValue =
        field.GetValue(skill);

    if (rawValue == null)
        return;

    float value;

    try
    {
        value =
            Convert.ToSingle(rawValue);
    }
    catch
    {
        return;
    }

    if (value == 0f ||
        value < minimumValue)
    {
        return;
    }

    string formatted =
        FormatValue(
            value,
            format,
            skill
        );

    int priority = 1;

    if (format == "damage")
    {
        formatted =
            DamageValue(
                formatted,
                skill
            );

        priority = 0;
    }
    else if (format == "attackSpeed")
    {
        formatted =
            AttackSpeedValue(
                formatted
            );

        priority = 2;
    }
    else if (format == "cooldown")
    {
        formatted =
            AttackSpeedValue(
                formatted
            );

        priority = 3;
    }
    else
    {
        formatted =
            Value(formatted);
    }

    string localizedLabel =
        LocalizeStatLabel(label);

    lines.Add(
        new StatisticEntry(
            localizedLabel +
            ": " +
            formatted,
            priority,
            order++
        )
    );
}


// =========================================================
// UPGRADE STAT HELPER
// =========================================================

private void AddUpgradeStat(
    List<StatisticEntry> lines,
    SkillInstance skill,
    SkillData data,
    string currentFieldName,
    string upgradeFieldName,
    string label,
    string format,
    ref int order)
{
    if (skill == null ||
        data == null)
    {
        return;
    }

    float currentValue;

    if (!TryGetFloatField(
            skill,
            currentFieldName,
            out currentValue))
    {
        return;
    }

    float upgradeValue;

    if (!TryGetFloatField(
            data,
            upgradeFieldName,
            out upgradeValue))
    {
        return;
    }

    if (upgradeValue == 0f)
        return;

    float newValue =
        currentValue +
        upgradeValue;

    if (format == "percentFraction")
    {
        currentValue =
            Mathf.Clamp01(currentValue);

        newValue =
            Mathf.Clamp01(newValue);
    }

    if (format == "percentDirect")
    {
        currentValue =
            Mathf.Max(0f, currentValue);

        newValue =
            Mathf.Max(0f, newValue);
    }

    string oldFormatted =
        FormatValue(
            currentValue,
            format,
            data
        );

    string newFormatted =
        FormatValue(
            newValue,
            format,
            data
        );

    int priority = 1;

    if (format == "damage")
    {
        oldFormatted =
            DamageValue(
                oldFormatted,
                data
            );

        newFormatted =
            DamageValue(
                newFormatted,
                data
            );

        priority = 0;
    }
    else if (format == "attackSpeed")
    {
        oldFormatted =
            AttackSpeedValue(
                oldFormatted
            );

        newFormatted =
            AttackSpeedValue(
                newFormatted
            );

        priority = 2;
    }
    else if (format == "cooldown")
    {
        oldFormatted =
            AttackSpeedValue(
                oldFormatted
            );

        newFormatted =
            AttackSpeedValue(
                newFormatted
            );

        priority = 3;
    }
    else
    {
        oldFormatted =
            Value(oldFormatted);

        newFormatted =
            Value(newFormatted);
    }

    lines.Add(
        new StatisticEntry(
            LocalizeStatLabel(label) +
            ": " +
            oldFormatted +
            " " +
            Arrow() +
            " " +
            newFormatted,
            priority,
            order++
        )
    );
}


// =========================================================
// COOLDOWN UPGRADE
// =========================================================

private void AddCooldownUpgradeStat(
    List<StatisticEntry> lines,
    SkillInstance skill,
    SkillData data,
    string currentFieldName,
    string upgradeFieldName,
    string label,
    ref int order)
{
    if (skill == null ||
        data == null)
    {
        return;
    }

    float currentValue;

    if (!TryGetFloatField(
            skill,
            currentFieldName,
            out currentValue))
    {
        return;
    }

    float upgradeValue;

    if (!TryGetFloatField(
            data,
            upgradeFieldName,
            out upgradeValue))
    {
        return;
    }

    if (upgradeValue == 0f)
        return;

    float newValue =
        Mathf.Max(
            0.1f,
            currentValue - upgradeValue
        );

    string oldFormatted =
        FormatValue(
            currentValue,
            "seconds",
            data
        );

    string newFormatted =
        FormatValue(
            newValue,
            "seconds",
            data
        );

    lines.Add(
        new StatisticEntry(
            LocalizeStatLabel(label) +
            ": " +
            Value(oldFormatted) +
            " " +
            Arrow() +
            " " +
            Value(newFormatted),
            3,
            order++
        )
    );
}


// =========================================================
// BUILD SORTED STATISTICS TEXT
// =========================================================

private string BuildStatisticsText(
    List<StatisticEntry> entries)
{
    if (entries == null ||
        entries.Count == 0)
    {
        return "";
    }

    entries.Sort(
        (a, b) =>
        {
            int priorityCompare =
                a.priority.CompareTo(
                    b.priority
                );

            if (priorityCompare != 0)
                return priorityCompare;

            return a.order.CompareTo(
                b.order
            );
        }
    );

    List<string> lines =
        new List<string>();

    foreach (StatisticEntry entry in entries)
    {
        if (entry == null ||
            string.IsNullOrEmpty(entry.text))
        {
            continue;
        }

        lines.Add(entry.text);
    }

    return string.Join(
        "\n",
        lines
    );
}


// =========================================================
// REFLECTION VALUE
// =========================================================

private bool TryGetFloatField(
    object target,
    string fieldName,
    out float value)
{
    value = 0f;

    if (target == null)
        return false;

    FieldInfo field =
        target.GetType().GetField(
            fieldName,
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance
        );

    if (field == null)
        return false;

    object rawValue =
        field.GetValue(target);

    if (rawValue == null)
        return false;

    try
    {
        value =
            Convert.ToSingle(rawValue);

        return true;
    }
    catch
    {
        return false;
    }
}


// =========================================================
// FORMAT VALUE
// =========================================================

private string FormatValue(
    float value,
    string format,
    SkillData skill)
{
    switch (format)
    {
        case "damage":
            return FormatNumber(value);

        case "attackSpeed":
            return FormatAttackSpeed(value);

        case "cooldown":
            return FormatNumber(value) + " s";

        case "seconds":
            return FormatNumber(value) + " s";

        case "integer":
            return Mathf.RoundToInt(value).ToString();

        case "angle":
            return FormatNumber(value) + "°";

        case "multiplier":
            return FormatNumber(100 * value) + "%";

        case "percentFraction":
            return FormatPercent(value);

        case "percentIncrease":
            return "+" + FormatPercent(value);

        case "percentDirect":
            return FormatNumber(value) + "%";

        case "number":
        default:
            return FormatNumber(value);
    }
}


// =========================================================
// DAMAGE COLOR
// =========================================================

private string DamageValue(
    string value,
    SkillData skill)
{
    return
        "<color=" +
        GetDamageColor(skill) +
        ">" +
        value +
        "</color>";
}


private string GetDamageColor(
    SkillData skill)
{
    if (skill == null)
        return "#FFFFFF";

    switch (skill.damageType)
    {
        case SkillData.DamageType.Fire:
            return "#f55a00";

        case SkillData.DamageType.Cold:
            return "#167bff";

        case SkillData.DamageType.Chaos:
            return "#009113";

        case SkillData.DamageType.Lightning:
            return "#00f7ff";

        case SkillData.DamageType.Physical:
            return "#7d7e7e";

        default:
            return "#FFFFFF";
    }
}


// =========================================================
// ATTACK SPEED COLOR
// =========================================================

private string AttackSpeedValue(
    string value)
{
    return
        "<color=" +
        AttackSpeedColor +
        ">" +
        value +
        "</color>";
}


// =========================================================
// NORMAL VALUE COLOR
// =========================================================

private string Value(
    string value)
{
    return
        "<color=" +
        CurrentValueColor +
        ">" +
        value +
        "</color>";
}


// =========================================================
// ARROW
// =========================================================

private string Arrow()
{
    return
        "<color=" +
        ArrowColor +
        ">→</color>";
}


// =========================================================
// NUMBER FORMAT
// =========================================================

private string FormatNumber(
    float value)
{
    if (Mathf.Approximately(
            value,
            Mathf.Round(value)))
    {
        return Mathf.RoundToInt(value).ToString();
    }

    return value.ToString("0.##");
}


// =========================================================
// PERCENT FORMAT
// =========================================================

private string FormatPercent(
    float value)
{
    return
        Mathf.RoundToInt(
            value * 100f
        ) + "%";
}


// =========================================================
// ATTACK SPEED FORMAT
// =========================================================

private string FormatAttackSpeed(
    float value)
{
    return FormatNumber(value) + " s";
}


// =========================================================
// RESIZE DESCRIPTION
// =========================================================

private void ResizeDescriptionWindow()
{
    if (descriptionText == null ||
        descriptionRect == null)
    {
        return;
    }

    Canvas.ForceUpdateCanvases();

    UnityEngine.UI.LayoutRebuilder
        .ForceRebuildLayoutImmediate(
            descriptionText.rectTransform
        );

    Canvas.ForceUpdateCanvases();

    float preferredWidth =
        descriptionText.preferredWidth;

    float preferredHeight =
        descriptionText.preferredHeight;

    float targetWidth =
        preferredWidth +
        descriptionHorizontalPadding;

    targetWidth =
        Mathf.Clamp(
            targetWidth,
            minDescriptionWidth,
            maxDescriptionWidth
        );

    float targetHeight =
        preferredHeight +
        descriptionPadding;

    Vector2 size =
        descriptionRect.sizeDelta;

    size.x = targetWidth;
    size.y = targetHeight;

    descriptionRect.sizeDelta = size;

    Canvas.ForceUpdateCanvases();

    UnityEngine.UI.LayoutRebuilder
        .ForceRebuildLayoutImmediate(
            descriptionRect
        );
}


// =========================================================
// RESIZE NEXT FRAME
// =========================================================

private IEnumerator ResizeDescriptionNextFrame()
{
    yield return null;

    if (descriptionWindow == null ||
        !descriptionWindow.activeSelf)
    {
        resizeDescriptionCoroutine = null;
        yield break;
    }

    Canvas.ForceUpdateCanvases();

    ResizeDescriptionWindow();

    Canvas.ForceUpdateCanvases();

    resizeDescriptionCoroutine = null;
}


// =========================================================
// CLEAR TAG ICONS
// =========================================================

private void ClearTagIcons()
{
    if (tagIconContainer == null)
        return;

    for (
        int i = tagIconContainer.childCount - 1;
        i >= 0;
        i--)
    {
        Transform child =
            tagIconContainer.GetChild(i);

        if (child != null)
        {
            Destroy(child.gameObject);
        }
    }

    Canvas.ForceUpdateCanvases();
}


// =========================================================
// BUILD TAG ICONS
// =========================================================

private void BuildTagIcons(
    SkillData skill)
{
    if (skill == null)
    {
        return;
    }

    if (tagIconContainer == null)
    {
        return;
    }

    if (tagIconPrefab == null)
    {
        return;
    }

    if (tagIconDatabase == null)
    {
        return;
    }

    if (!tagIconContainer.gameObject.activeSelf)
    {
        tagIconContainer.gameObject.SetActive(true);
    }

    ClearTagIcons();

    SkillTagIconDatabase.TagType elementTag;

    switch (skill.damageType)
    {
        case SkillData.DamageType.Fire:
            elementTag =
                SkillTagIconDatabase.TagType.Fire;
            break;

        case SkillData.DamageType.Cold:
            elementTag =
                SkillTagIconDatabase.TagType.Cold;
            break;

        case SkillData.DamageType.Chaos:
            elementTag =
                SkillTagIconDatabase.TagType.Chaos;
            break;

        case SkillData.DamageType.Lightning:
            elementTag =
                SkillTagIconDatabase.TagType.Lightning;
            break;

        case SkillData.DamageType.Physical:
        default:
            elementTag =
                SkillTagIconDatabase.TagType.Physical;
            break;
    }

    CreateTagIcon(elementTag);

    SkillCategory categories =
        skill.categories;

    if ((categories &
        SkillCategory.Projectile) != 0)
    {
        CreateTagIcon(
            SkillTagIconDatabase.TagType.Projectile
        );
    }

    if ((categories &
        SkillCategory.Area) != 0)
    {
        CreateTagIcon(
            SkillTagIconDatabase.TagType.Area
        );
    }

    if ((categories &
        SkillCategory.Piercing) != 0)
    {
        CreateTagIcon(
            SkillTagIconDatabase.TagType.Piercing
        );
    }

    if ((categories &
        SkillCategory.Chain) != 0)
    {
        CreateTagIcon(
            SkillTagIconDatabase.TagType.Chain
        );
    }

    if ((categories &
        SkillCategory.DoT) != 0)
    {
        CreateTagIcon(
            SkillTagIconDatabase.TagType.DoT
        );
    }

    if ((categories &
        SkillCategory.Melee) != 0)
    {
        CreateTagIcon(
            SkillTagIconDatabase.TagType.Melee
        );
    }

    Canvas.ForceUpdateCanvases();

    RectTransform containerRect =
        tagIconContainer as RectTransform;

    if (containerRect != null)
    {
        UnityEngine.UI.LayoutRebuilder
            .ForceRebuildLayoutImmediate(
                containerRect
            );
    }

    Canvas.ForceUpdateCanvases();
}


// =========================================================
// CREATE TAG ICON
// =========================================================

private void CreateTagIcon(
    SkillTagIconDatabase.TagType tag)
{
    if (tagIconContainer == null)
    {
        return;
    }

    if (tagIconPrefab == null)
    {
        return;
    }

    if (tagIconDatabase == null)
    {
        return;
    }

    Sprite icon =
        tagIconDatabase.GetIcon(tag);

    if (icon == null)
    {
        return;
    }

    UnityEngine.UI.Image newIcon =
        Instantiate(
            tagIconPrefab,
            tagIconContainer,
            false
        );

    if (newIcon == null)
    {
        return;
    }

    newIcon.sprite = icon;
    newIcon.enabled = true;

    newIcon.gameObject.SetActive(true);

    RectTransform iconRect =
        newIcon.rectTransform;

    if (iconRect != null)
    {
        iconRect.localScale =
            Vector3.one;

        iconRect.localRotation =
            Quaternion.identity;
    }
}


// =========================================================
// CLOSE DESCRIPTION
// =========================================================

public void CloseDescription()
{
    if (resizeDescriptionCoroutine != null)
    {
        StopCoroutine(
            resizeDescriptionCoroutine
        );

        resizeDescriptionCoroutine = null;
    }

    if (descriptionText != null)
        descriptionText.text = "";

    if (costText != null)
        costText.text = "";

    ClearTagIcons();

    if (descriptionWindow != null)
    {
        descriptionWindow.SetActive(false);
    }

    if (selectedButton != null)
    {
        selectedButton.RemoveHighlight();
        selectedButton = null;
    }

    selectedSkill = null;

    if (descriptionRect != null &&
        initialDescriptionSizeCaptured)
    {
        descriptionRect.sizeDelta =
            initialDescriptionSize;
    }

    Canvas.ForceUpdateCanvases();
}


// =========================================================
// SELECT / BUY SKILL
// =========================================================

private void SelectSkill(
    SkillData skill,
    SkillButton clickedButton)
{
    if (skill == null)
        return;

    // =========================================================
    // SPRAWDZENIE TOWER
    // =========================================================

    if (tower == null)
    {
        return;
    }

    // =========================================================
    // SPRAWDZENIE CZY SKILL JEST JUŻ POSIADANY
    // =========================================================

    bool alreadyOwned =
        GetExistingSkill(skill) != null;

    // =========================================================
    // LIMIT 6 RÓŻNYCH SKILLI
    // =========================================================
    //
    // Jeżeli jest to NOWY skill i wieża ma już 6 różnych
    // skilli, zakup zostaje zablokowany.
    //
    // Jeżeli skill jest już posiadany, można go nadal ulepszać.
    // =========================================================

    if (!alreadyOwned &&
        GetActiveSkillCount() >= maxActiveSkills)
    {
        return;
    }

    // =========================================================
    // AKTUALNA CENA TEGO KONKRETNEGO SKILLA
    // =========================================================

    int currentCost =
        GetCurrentSkillCost(skill);

    EnemySpawner spawner =
        FindAnyObjectByType<EnemySpawner>();

    if (spawner == null)
    {
        return;
    }

    // =========================================================
    // POBRANIE ZŁOTA
    // =========================================================

    if (!spawner.TrySpendGold(currentCost))
    {
        return;
    }

    // =========================================================
    // DODANIE / ULEPSZENIE SKILLA
    // =========================================================

    tower.AddSkill(skill);

    // =========================================================
    // ZWIĘKSZAMY LICZNIK TYLKO TEGO SKILLA
    // =========================================================

    IncrementSkillPurchaseCount(skill);

    // =========================================================
    // TELEMETRIA - ZAKUPIONY SKILL
    // =========================================================
    // Liczymy tylko po udanym wydaniu złota
    // i faktycznym dodaniu / ulepszeniu skilla.
    // =========================================================

    if (GameStatsManager.Instance != null)
    {
        GameStatsManager.Instance.AddSkillPurchased();
    }

    // =========================================================
    // PO ZAKUPIE NIE UKRYWAMY CAŁEJ LOGIKI
    // =========================================================

    if (clickedButton != null)
    {
        clickedButton.gameObject.SetActive(false);
    }

    CloseDescription();

}


// =========================================================
// GET EXISTING SKILL
// =========================================================

private SkillInstance GetExistingSkill(
    SkillData skillData)
{
    if (tower == null ||
        tower.activeSkills == null ||
        skillData == null)
    {
        return null;
    }

    foreach (
        SkillInstance instance
        in tower.activeSkills)
    {
        if (instance == null ||
            instance.data == null)
        {
            continue;
        }

        // Stały identyfikator skilla.
        // Nie używamy nazwy lokalizowanej.
        if (instance.data.skillID ==
            skillData.skillID)
        {
            return instance;
        }
    }

    return null;
}


// =========================================================
// HAS SKILL
// =========================================================

private bool HasSkill(
    SkillData skill)
{
    return GetExistingSkill(skill) != null;
}


// =========================================================
// GET ACTIVE SKILL COUNT
// =========================================================
//
// Liczymy RÓŻNE skillID, a nie liczbę zakupów.
//
// Przykład:
//
// FireArrow lvl 4
// ColdArrow lvl 2
// Blizzard lvl 1
//
// activeSkills.Count = 3
//
// Liczba różnych skilli = 3.
//
// =========================================================

private int GetActiveSkillCount()
{
    if (tower == null ||
        tower.activeSkills == null)
    {
        return 0;
    }

    HashSet<string> uniqueSkillIDs =
        new HashSet<string>(
            StringComparer.Ordinal
        );

    foreach (
        SkillInstance instance
        in tower.activeSkills)
    {
        if (instance == null ||
            instance.data == null)
        {
            continue;
        }

        if (string.IsNullOrEmpty(
                instance.data.skillID))
        {
            continue;
        }

        uniqueSkillIDs.Add(
            instance.data.skillID
        );
    }

    return uniqueSkillIDs.Count;
}


// =========================================================
// GET SKILL PURCHASE COUNT
// =========================================================
//
// Pobiera liczbę zakupów tylko dla konkretnego skillID.
//
// =========================================================

private int GetSkillPurchaseCount(
    SkillData skill)
{
    if (skill == null)
        return 0;

    if (string.IsNullOrEmpty(
            skill.skillID))
    {
        return 0;
    }

    if (skillPurchaseCounts.TryGetValue(
            skill.skillID,
            out int count))
    {
        return Mathf.Max(
            0,
            count
        );
    }

    return 0;
}


// =========================================================
// INCREMENT SKILL PURCHASE COUNT
// =========================================================
//
// Zwiększa licznik tylko tego jednego skilla.
//
// =========================================================

private void IncrementSkillPurchaseCount(
    SkillData skill)
{
    if (skill == null)
        return;

    if (string.IsNullOrEmpty(
            skill.skillID))
    {
        return;
    }

    int currentCount =
        GetSkillPurchaseCount(skill);

    skillPurchaseCounts[
        skill.skillID
    ] = currentCount + 1;
}


// =========================================================
// GET CURRENT SKILL COST
// =========================================================
//
// Cena zależy tylko od liczby zakupów TEGO skilla.
//
// Wzór:
//
// cost = baseCost + purchaseCount * costIncreasePerLevel
//
// Przykład:
//
// baseCost = 500
// costIncreasePerLevel = 500
//
// 0 zakupów = 500
// 1 zakup   = 1000
// 2 zakupy  = 1500
// 3 zakupy  = 2000
// 4 zakupy = 2500
//
// =========================================================

public int GetCurrentSkillCost(
    SkillData skill)
{
    if (skill == null)
        return 0;

    int baseCost =
        Mathf.Max(
            0,
            skill.cost
        );

    int costIncrease =
        Mathf.Max(
            0,
            skill.costIncreasePerLevel
        );

    int purchaseCount =
        GetSkillPurchaseCount(skill);

    // =========================================================
    // PIERWSZY ZAKUP
    // =========================================================

    if (purchaseCount <= 0)
    {
        return baseCost;
    }

    // =========================================================
    // ZABEZPIECZENIE PRZED OVERFLOW
    // =========================================================
    //
    // Liczymy na long, żeby nie dopuścić do przepełnienia int.
    // =========================================================

    long calculatedCost =
        (long)baseCost +
        (long)purchaseCount *
        (long)costIncrease;

    if (calculatedCost <= 0)
        return 0;

    if (calculatedCost >= int.MaxValue)
        return int.MaxValue;

    return (int)calculatedCost;
}


// =========================================================
// PUBLICZNY PODGLĄD LICZBY ZAKUPÓW
// =========================================================
//
// Przydatne np. później dla innych systemów.
//
// =========================================================

public int GetSkillPurchaseCountByID(
    string skillID)
{
    if (string.IsNullOrEmpty(skillID))
        return 0;

    if (skillPurchaseCounts.TryGetValue(
            skillID,
            out int count))
    {
        return Mathf.Max(
            0,
            count
        );
    }

    return 0;
}


// =========================================================
// REFRESH
// =========================================================

public void RefreshSkills()
{
    EnemySpawner spawner =
        FindAnyObjectByType<EnemySpawner>();

    if (selectedButton != null)
    {
        selectedButton.RemoveHighlight();
        selectedButton = null;
    }

    selectedSkill = null;

    if (descriptionText != null)
        descriptionText.text = "";

    if (costText != null)
        costText.text = "";

    if (spawner == null)
    {
        return;
    }

    if (!spawner.TrySpendRefreshPoints(1))
    {
        return;
    }

    ClearTagIcons();

    ShowSkills();

    SkillUpgradeSelectionManager upgradeManager =
        FindAnyObjectByType<SkillUpgradeSelectionManager>();

    if (upgradeManager != null)
    {
        upgradeManager.RefreshUpgrades();
    }
}


// =========================================================
// DESTROY
// =========================================================

private void OnDestroy()
{
    LocalizationSettings.SelectedLocaleChanged -=
        OnLanguageChanged;
}
}