using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

public class ActiveSkillsUI : MonoBehaviour
{
    // =========================================================
    // GŁÓWNE REFERENCJE
    // =========================================================

    [Header("Wieża")]
    [SerializeField]
    private Tower tower;

    [Header("Kontener ikon")]
    [SerializeField]
    private RectTransform iconContainer;

    [SerializeField]
    private GameObject iconPrefab;

    [Header("Ustawienie ikon")]
    [SerializeField]
    private float iconSpacing = 10f;

    [SerializeField]
    private float iconWidth = 70f;

    [SerializeField]
    private float iconHeight = 70f;

    [Header("Okno opisu")]
    [SerializeField]
    private GameObject descriptionWindow;

    [SerializeField]
    private Image descriptionIcon;

    [SerializeField]
    private TMP_Text descriptionText;

    [SerializeField]
    private RectTransform descriptionRect;

    // =========================================================
    // WYMIARY OKNA
    // =========================================================

    [Header("Wymiary okna opisu")]
    [SerializeField]
    private float descriptionPadding = 30f;

    [SerializeField]
    private float descriptionHorizontalPadding = 40f;

    [SerializeField]
    private float minDescriptionWidth = 350f;

    [SerializeField]
    private float maxDescriptionWidth = 400f;

    // =========================================================
    // TAGI
    // =========================================================

    [Header("Tagi")]
    [SerializeField]
    private Transform tagIconContainer;

    [SerializeField]
    private GameObject tagIconPrefab;

    [SerializeField]
    private SkillTagIconDatabase tagIconDatabase;

    // =========================================================
    // LOKALIZACJA
    // =========================================================

    [Header("Lokalizacja statystyk")]
    [SerializeField]
    private LocalizedStringTable statLocalizationTable;

    // =========================================================
    // LONG PRESS
    // =========================================================

    [Header("Czas przytrzymania")]
    [SerializeField]
    private float holdDuration = 0.5f;

    // =========================================================
    // STAN
    // =========================================================

    private readonly List<GameObject> spawnedIcons =
        new List<GameObject>();

    private SkillInstance selectedSkill;

    private string lastSignature = "";

    // =========================================================
    // ROZMIAR OKNA
    // =========================================================

    private Vector2 initialDescriptionSize;

    private bool initialDescriptionSizeCaptured;

    private Coroutine resizeDescriptionCoroutine;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (tower == null)
        {
            tower = FindAnyObjectByType<Tower>();
        }

        DisableIconContainerLayouts();

        if (descriptionRect != null)
        {
            initialDescriptionSize =
                descriptionRect.sizeDelta;

            initialDescriptionSizeCaptured = true;
        }

        if (descriptionWindow != null)
        {
            descriptionWindow.SetActive(false);
        }
    }

    private void Start()
    {
        DisableIconContainerLayouts();

        RefreshIcons();
    }

    private void Update()
    {
        RefreshIfNeeded();
    }

    // =========================================================
    // WYŁĄCZENIE AUTOMATYCZNEGO LAYOUTU
    // =========================================================

    private void DisableIconContainerLayouts()
    {
        if (iconContainer == null)
        {
            return;
        }

        LayoutGroup[] layoutGroups =
            iconContainer.GetComponents<LayoutGroup>();

        for (int i = 0; i < layoutGroups.Length; i++)
        {
            if (layoutGroups[i] != null)
            {
                layoutGroups[i].enabled = false;
            }
        }

        ContentSizeFitter contentSizeFitter =
            iconContainer.GetComponent<ContentSizeFitter>();

        if (contentSizeFitter != null)
        {
            contentSizeFitter.enabled = false;
        }

        AspectRatioFitter aspectRatioFitter =
            iconContainer.GetComponent<AspectRatioFitter>();

        if (aspectRatioFitter != null)
        {
            aspectRatioFitter.enabled = false;
        }
    }

    // =========================================================
    // WYŁĄCZENIE LAYOUTU NA PREFABIE IKONY
    // =========================================================

    private void DisableIconLayouts(
        GameObject iconObject)
    {
        if (iconObject == null)
        {
            return;
        }

        LayoutGroup[] layoutGroups =
            iconObject.GetComponentsInChildren<LayoutGroup>(
                true
            );

        for (int i = 0; i < layoutGroups.Length; i++)
        {
            if (layoutGroups[i] != null)
            {
                layoutGroups[i].enabled = false;
            }
        }

        ContentSizeFitter[] fitters =
            iconObject.GetComponentsInChildren<ContentSizeFitter>(
                true
            );

        for (int i = 0; i < fitters.Length; i++)
        {
            if (fitters[i] != null)
            {
                fitters[i].enabled = false;
            }
        }

        AspectRatioFitter[] aspectFitters =
            iconObject.GetComponentsInChildren<AspectRatioFitter>(
                true
            );

        for (int i = 0; i < aspectFitters.Length; i++)
        {
            if (aspectFitters[i] != null)
            {
                aspectFitters[i].enabled = false;
            }
        }
    }

    // =========================================================
    // RECTTRANSFORM IKONY
    // =========================================================

    private RectTransform GetIconRectTransform(
        GameObject iconObject)
    {
        if (iconObject == null)
        {
            return null;
        }

        RectTransform rootRect =
            iconObject.GetComponent<RectTransform>();

        if (rootRect != null)
        {
            return rootRect;
        }

        RectTransform childRect =
            iconObject.GetComponentInChildren<RectTransform>(
                true
            );

        return childRect;
    }

    // =========================================================
    // ODŚWIEŻANIE
    // =========================================================

    private void RefreshIfNeeded()
    {
        if (tower == null)
        {
            tower = FindAnyObjectByType<Tower>();

            if (tower == null)
            {
                return;
            }
        }

        string newSignature =
            BuildSkillsSignature();

        if (newSignature != lastSignature)
        {
            RefreshIcons();
        }
    }

    private string BuildSkillsSignature()
    {
        if (tower == null ||
            tower.activeSkills == null)
        {
            return "";
        }

        StringBuilder builder =
            new StringBuilder();

        foreach (
            SkillInstance skill
            in tower.activeSkills)
        {
            if (skill == null ||
                skill.data == null)
            {
                continue;
            }

            builder.Append(
                skill.data.skillID
            );

            builder.Append("|");

            builder.Append(
                skill.level
            );

            builder.Append(";");

            // Aktualne wartości SkillInstance.
            builder.Append(skill.damage);
            builder.Append("|");
            builder.Append(skill.fireRate);
            builder.Append("|");
            builder.Append(skill.range);
            builder.Append("|");
            builder.Append(skill.explosionRadius);
            builder.Append("|");
            builder.Append(skill.projectileCount);
            builder.Append("|");
            builder.Append(skill.chainCount);
            builder.Append("|");
            builder.Append(skill.chainRange);
            builder.Append("|");
            builder.Append(skill.chainDamageMultiplier);
            builder.Append("|");
            builder.Append(skill.dotDamage);
            builder.Append("|");
            builder.Append(skill.dotDuration);
            builder.Append("|");
            builder.Append(skill.slowPercent);
            builder.Append("|");
            builder.Append(skill.slowDuration);
            builder.Append("|");
            builder.Append(skill.rootDuration);
            builder.Append(";");
        }

        return builder.ToString();
    }

    // =========================================================
    // TWORZENIE IKON
    // =========================================================

    public void RefreshIcons()
    {
        if (iconContainer == null)
        {
            return;
        }

        if (iconPrefab == null)
        {
            return;
        }

        if (tower == null)
        {
            tower = FindAnyObjectByType<Tower>();

            if (tower == null)
            {
                return;
            }
        }

        DisableIconContainerLayouts();

        ClearIcons();

        List<SkillInstance> uniqueSkills =
            GetUniqueSkills();

        for (
            int i = 0;
            i < uniqueSkills.Count && i < 6;
            i++)
        {
            SkillInstance skill =
                uniqueSkills[i];

            if (skill == null ||
                skill.data == null)
            {
                continue;
            }

            GameObject iconObject =
                Instantiate(
                    iconPrefab,
                    iconContainer,
                    false
                );

            if (iconObject == null)
            {
                continue;
            }

            iconObject.name =
                "ActiveSkillIcon_" +
                skill.data.skillID;

            DisableIconLayouts(
                iconObject
            );

            RectTransform iconRect =
                GetIconRectTransform(
                    iconObject
                );

            if (iconRect != null)
            {
                iconRect.anchorMin =
                    new Vector2(
                        0.5f,
                        0.5f
                    );

                iconRect.anchorMax =
                    new Vector2(
                        0.5f,
                        0.5f
                    );

                iconRect.pivot =
                    new Vector2(
                        0.5f,
                        0.5f
                    );

                iconRect.sizeDelta =
                    new Vector2(
                        iconWidth,
                        iconHeight
                    );

                iconRect.localScale =
                    Vector3.one;

                iconRect.localRotation =
                    Quaternion.identity;

                iconRect.anchoredPosition =
                    Vector2.zero;
            }

            ActiveSkillIcon activeSkillIcon =
                iconObject.GetComponent<ActiveSkillIcon>();

            if (activeSkillIcon == null)
            {
                activeSkillIcon =
                    iconObject.GetComponentInChildren<ActiveSkillIcon>(
                        true
                    );
            }

            if (activeSkillIcon != null)
            {
                activeSkillIcon.Setup(
                    skill,
                    this,
                    holdDuration
                );
            }

            spawnedIcons.Add(
                iconObject
            );
        }

        PositionIcons();

        lastSignature =
            BuildSkillsSignature();
    }

    // =========================================================
    // UNIKALNE SKILLE
    // =========================================================

    private List<SkillInstance> GetUniqueSkills()
    {
        List<SkillInstance> result =
            new List<SkillInstance>();

        if (tower == null ||
            tower.activeSkills == null)
        {
            return result;
        }

        HashSet<string> usedSkillIDs =
            new HashSet<string>();

        foreach (
            SkillInstance skill
            in tower.activeSkills)
        {
            if (skill == null ||
                skill.data == null)
            {
                continue;
            }

            string skillID =
                skill.data.skillID;

            if (string.IsNullOrEmpty(skillID))
            {
                continue;
            }

            if (usedSkillIDs.Contains(skillID))
            {
                continue;
            }

            usedSkillIDs.Add(
                skillID
            );

            result.Add(
                skill
            );
        }

        return result;
    }

    // =========================================================
    // POZYCJA IKON
    // =========================================================

    private void PositionIcons()
    {
        if (spawnedIcons == null ||
            spawnedIcons.Count == 0)
        {
            return;
        }

        int count =
            spawnedIcons.Count;

        float totalHeight =
            count * iconHeight +
            (count - 1) * iconSpacing;

        float startY =
            (totalHeight / 2f) -
            (iconHeight / 2f);

        for (int i = 0; i < count; i++)
        {
            if (spawnedIcons[i] == null)
            {
                continue;
            }

            RectTransform rect =
                GetIconRectTransform(
                    spawnedIcons[i]
                );

            if (rect == null)
            {
                continue;
            }

            float y =
                startY -
                i * (iconHeight + iconSpacing);

            rect.anchoredPosition =
                new Vector2(
                    0f,
                    y
                );
        }
    }

    // =========================================================
    // CZYSZCZENIE IKON
    // =========================================================

    private void ClearIcons()
    {
        for (
            int i =
                spawnedIcons.Count - 1;
            i >= 0;
            i--)
        {
            if (spawnedIcons[i] != null)
            {
                Destroy(
                    spawnedIcons[i]
                );
            }
        }

        spawnedIcons.Clear();
    }

    // =========================================================
    // LONG PRESS
    // =========================================================

    public void OnSkillIconHeld(
        SkillInstance skill)
    {
        if (skill == null ||
            skill.data == null)
        {
            return;
        }

        selectedSkill =
            skill;

        ShowSkillDetails(
            skill
        );
    }

    // =========================================================
    // POKAZANIE OPISU
    // =========================================================

    private void ShowSkillDetails(
        SkillInstance skill)
    {
        if (skill == null ||
            skill.data == null)
        {
            return;
        }

        if (descriptionWindow != null)
        {
            descriptionWindow.SetActive(true);
        }

        if (descriptionIcon != null)
        {
            descriptionIcon.sprite =
                skill.data.icon;

            descriptionIcon.enabled =
                skill.data.icon != null;
        }

        string localizedName =
            GetLocalizedString(
                skill.data.localizedName
            );

        string localizedDescription =
            GetLocalizedString(
                skill.data.localizedDescription
            );

        StringBuilder text =
            new StringBuilder();

        // =====================================================
        // NAZWA
        // =====================================================

        if (!string.IsNullOrEmpty(
                localizedName))
        {
            text.AppendLine(
                "<size=120%><b>" +
                localizedName +
                "</b></size>"
            );

            text.AppendLine();
        }

        // =====================================================
        // POZIOM
        // =====================================================

        text.AppendLine(
            "<b>" +
            GetStatLabel(
                "level",
                "Poziom"
            ) +
            ":</b> " +
            skill.level
        );

        text.AppendLine();

        // =====================================================
        // OPIS
        // =====================================================

        if (!string.IsNullOrEmpty(
                localizedDescription))
        {
            text.AppendLine(
                "<i>" +
                localizedDescription +
                "</i>"
            );

            text.AppendLine();
        }

        // =====================================================
        // STATYSTYKI
        // =====================================================

        BuildStatistics(
            text,
            skill
        );

        if (descriptionText != null)
        {
            descriptionText.text =
                text.ToString();
        }

        // =====================================================
        // TAGI
        // =====================================================

        BuildTagIcons(
            skill
        );

        // =====================================================
        // RESIZE
        // =====================================================

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
    // STATYSTYKI
    //
    // TA SAMA KOLEJNOŚĆ CO NORMALNY OPIS SKILLA
    // =========================================================

    private void BuildStatistics(
        StringBuilder text,
        SkillInstance skill)
    {
        if (skill == null ||
            skill.data == null)
        {
            return;
        }

        // -----------------------------------------------------
        // OBRAŻENIA
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "damage",
            "damage",
            "Obrażenia",
            "0.##",
            ValueType.Damage
        );

        // -----------------------------------------------------
        // SZYBKOŚĆ ATAKU / CZAS ODNOWIENIA
        // -----------------------------------------------------

        bool isProjectile =
            HasCategory(
                skill.data.categories,
                "Projectile"
            );

        AddSkillFloatStat(
            text,
            skill,
            "fireRate",
            isProjectile
                ? "attack_speed"
                : "cooldown",
            isProjectile
                ? "Szybkość ataku"
                : "Czas odnowienia",
            "0.##",
            isProjectile
                ? ValueType.AttackSpeed
                : ValueType.Cooldown
        );

        // -----------------------------------------------------
        // ZASIĘG
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "range",
            "range",
            "Zasięg",
            "0.##",
            ValueType.Number
        );

        // -----------------------------------------------------
        // PROMIEŃ EKSPLOZJI
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "explosionRadius",
            "explosion_radius",
            "Promień eksplozji",
            "0.##",
            ValueType.Number
        );

        // -----------------------------------------------------
        // PRZEBICIE
        //
        // WYŚWIETLANE TYLKO DLA SKILLI
        // POSIADAJĄCYCH TAG / KATEGORIĘ PIERCING
        // -----------------------------------------------------

        if (HasTag(
                skill,
                "Piercing"))
        {
            AddSkillFloatStat(
                text,
                skill,
                "piercingCount",
                "piercing_count",
                "Pociski przebijające",
                "0",
                ValueType.Integer
            );
        }

        // -----------------------------------------------------
        // SPOWOLNIENIE
        // -----------------------------------------------------

        AddSkillPercentStat(
            text,
            skill,
            "slowPercent",
            "slow_percent",
            "Spowolnienie"
        );

        // -----------------------------------------------------
        // CZAS SPOWOLNIENIA
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "slowDuration",
            "slow_duration",
            "Czas spowolnienia",
            "0.##",
            ValueType.Seconds
        );

        // -----------------------------------------------------
        // ROOT
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "rootDuration",
            "root_duration",
            "Czas unieruchomienia",
            "0.##",
            ValueType.Seconds
        );

        // -----------------------------------------------------
        // CHAIN
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "chainCount",
            "chain_count",
            "Liczba odbić",
            "0",
            ValueType.Integer
        );

        AddSkillFloatStat(
            text,
            skill,
            "chainDamageMultiplier",
            "chain_damage",
            "Mnożnik obrażeń łańcucha",
            "0.##",
            ValueType.Multiplier
        );

        AddSkillFloatStat(
            text,
            skill,
            "chainRange",
            "chain_range",
            "Zasięg łańcucha",
            "0.##",
            ValueType.Number
        );

        // -----------------------------------------------------
        // LIGHTNING
        // -----------------------------------------------------

        AddSkillPercentStat(
            text,
            skill,
            "lightningStrikeChance",
            "lightning_strike_chance",
            "Szansa na uderzenie pioruna"
        );

        // -----------------------------------------------------
        // DOT
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "dotDamage",
            "dot_damage",
            "Obrażenia DoT",
            "0.##",
            ValueType.Damage
        );

        AddSkillFloatStat(
            text,
            skill,
            "dotDuration",
            "dot_duration",
            "Czas DoT",
            "0.##",
            ValueType.Seconds
        );

        // -----------------------------------------------------
        // LICZBA POCISKÓW
        // -----------------------------------------------------

        // Celowo wyłączone.

        // -----------------------------------------------------
        // SHOTGUN
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "shotgunProjectileCount",
            "projectile_count",
            "Liczba pocisków",
            "0",
            ValueType.Integer
        );

        // -----------------------------------------------------
        // CORRUPTED
        // -----------------------------------------------------

        AddSkillPercentStat(
            text,
            skill,
            "corruptedSlowPercent",
            "slow_percent",
            "Spowolnienie"
        );

        AddSkillPercentStat(
            text,
            skill,
            "corruptedAttackSpeedReduction",
            "attack_speed",
            "Redukcja szybkości ataku"
        );

        AddSkillPercentStat(
            text,
            skill,
            "corruptedDamageTakenIncrease",
            "damage",
            "Zwiększenie otrzymywanych obrażeń"
        );

        AddSkillFloatStat(
            text,
            skill,
            "corruptedDuration",
            "dot_duration",
            "Czas działania",
            "0.##",
            ValueType.Seconds
        );

        AddSkillFloatStat(
            text,
            skill,
            "corruptedMaxStacks",
            "stack_count",
            "Maksymalna liczba kumulacji",
            "0",
            ValueType.Integer
        );

        AddSkillFloatStat(
            text,
            skill,
            "corruptedMaxTargets",
            "projectile_count",
            "Maksymalna liczba celów",
            "0",
            ValueType.Integer
        );

        // -----------------------------------------------------
        // DEVOURING CLOUD
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "cloudDuration",
            "dot_duration",
            "Czas działania",
            "0.##",
            ValueType.Seconds
        );

        // -----------------------------------------------------
        // WHIRL OF CHAOS
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "whirlDuration",
            "dot_duration",
            "Czas działania",
            "0.##",
            ValueType.Seconds
        );

        AddSkillFloatStat(
            text,
            skill,
            "whirlDamage",
            "damage",
            "Obrażenia",
            "0.##",
            ValueType.Damage
        );

        AddSkillFloatStat(
            text,
            skill,
            "whirlPullSpeed",
            "pull_speed",
            "Prędkość przyciągania",
            "0.##",
            ValueType.Number
        );

        // -----------------------------------------------------
        // SLIME
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "slimeCount",
            "projectile_count",
            "Liczba śluzów",
            "0",
            ValueType.Integer
        );

        AddSkillPercentStat(
            text,
            skill,
            "slimeSlowPercent",
            "slow_percent",
            "Spowolnienie"
        );

        AddSkillFloatStat(
            text,
            skill,
            "slimeDotDamage",
            "dot_damage",
            "Obrażenia DoT",
            "0.##",
            ValueType.Damage
        );

        AddSkillFloatStat(
            text,
            skill,
            "slimeDotDuration",
            "dot_duration",
            "Czas DoT",
            "0.##",
            ValueType.Seconds
        );

        // -----------------------------------------------------
        // BLIZZARD
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "blizzardDamage",
            "damage",
            "Obrażenia",
            "0.##",
            ValueType.Damage
        );

        AddSkillFloatStat(
            text,
            skill,
            "blizzardDuration",
            "dot_duration",
            "Czas działania",
            "0.##",
            ValueType.Seconds
        );

        AddSkillPercentStat(
            text,
            skill,
            "blizzardSlowPercent",
            "slow_percent",
            "Spowolnienie"
        );

        // -----------------------------------------------------
        // ICE SPIKE
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "iceSpikeCount",
            "projectile_count",
            "Liczba kolców",
            "0",
            ValueType.Integer
        );

        AddSkillFloatStat(
            text,
            skill,
            "iceSpikeDamageMultiplier",
            "damage",
            "Mnożnik obrażeń",
            "0.##",
            ValueType.Multiplier
        );

        // -----------------------------------------------------
        // FREEZING WAVE
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "freezingWaveFreezeDuration",
            "freeze_duration",
            "Czas zamrożenia",
            "0.##",
            ValueType.Seconds
        );

        AddSkillFloatStat(
            text,
            skill,
            "freezingWaveDamage",
            "damage",
            "Obrażenia",
            "0.##",
            ValueType.Damage
        );

        // -----------------------------------------------------
        // FROST NOVA
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "frostNovaDamage",
            "damage",
            "Obrażenia",
            "0.##",
            ValueType.Damage
        );

        AddSkillFloatStat(
            text,
            skill,
            "frostNovaFreezeDuration",
            "freeze_duration",
            "Czas zamrożenia",
            "0.##",
            ValueType.Seconds
        );

        AddSkillFloatStat(
            text,
            skill,
            "frostNovaCooldown",
            "cooldown",
            "Czas odnowienia",
            "0.##",
            ValueType.Cooldown
        );

        // -----------------------------------------------------
        // ICE SHOT
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "iceShotDamage",
            "damage",
            "Obrażenia",
            "0.##",
            ValueType.Damage
        );

        AddSkillPercentStat(
            text,
            skill,
            "iceShotSlowPercent",
            "slow_percent",
            "Spowolnienie"
        );

        AddSkillFloatStat(
            text,
            skill,
            "iceShotSlowDuration",
            "slow_duration",
            "Czas spowolnienia",
            "0.##",
            ValueType.Seconds
        );

        // -----------------------------------------------------
        // BREATH OF FIRE
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "breathOfFireDamage",
            "damage",
            "Obrażenia",
            "0.##",
            ValueType.Damage
        );

        AddSkillFloatStat(
            text,
            skill,
            "breathOfFireBurnDamage",
            "dot_damage",
            "Obrażenia podpalenia",
            "0.##",
            ValueType.Damage
        );

        AddSkillFloatStat(
            text,
            skill,
            "breathOfFireBurnDuration",
            "dot_duration",
            "Czas podpalenia",
            "0.##",
            ValueType.Seconds
        );

        // -----------------------------------------------------
        // FLAMING CIRCLE
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "flamingCircleDamage",
            "damage",
            "Obrażenia",
            "0.##",
            ValueType.Damage
        );

        AddSkillFloatStat(
            text,
            skill,
            "flamingCircleDuration",
            "dot_duration",
            "Czas działania",
            "0.##",
            ValueType.Seconds
        );

        // -----------------------------------------------------
        // SEARING SHOT
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "searingShotDamage",
            "damage",
            "Obrażenia",
            "0.##",
            ValueType.Damage
        );

        AddSkillFloatStat(
            text,
            skill,
            "searingShotBurnDamage",
            "dot_damage",
            "Obrażenia podpalenia",
            "0.##",
            ValueType.Damage
        );

        AddSkillFloatStat(
            text,
            skill,
            "searingShotBurnDuration",
            "dot_duration",
            "Czas podpalenia",
            "0.##",
            ValueType.Seconds
        );

        AddSkillFloatStat(
            text,
            skill,
            "searingShotExplosionDamage",
            "damage",
            "Obrażenia eksplozji",
            "0.##",
            ValueType.Damage
        );

        // -----------------------------------------------------
        // RAIN OF FIRE
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "rainOfFireDuration",
            "dot_duration",
            "Czas działania",
            "0.##",
            ValueType.Seconds
        );

        AddSkillFloatStat(
            text,
            skill,
            "rainOfFireDamage",
            "damage",
            "Obrażenia",
            "0.##",
            ValueType.Damage
        );

        AddSkillFloatStat(
            text,
            skill,
            "rainOfFireBurnDamage",
            "dot_damage",
            "Obrażenia podpalenia",
            "0.##",
            ValueType.Damage
        );

        AddSkillFloatStat(
            text,
            skill,
            "rainOfFireBurnDuration",
            "dot_duration",
            "Czas podpalenia",
            "0.##",
            ValueType.Seconds
        );

        // -----------------------------------------------------
        // VOLCANIC SPHERE
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "volcanicSphereCount",
            "projectile_count",
            "Liczba kul",
            "0",
            ValueType.Integer
        );

        AddSkillFloatStat(
            text,
            skill,
            "volcanicSphereDamage",
            "damage",
            "Obrażenia",
            "0.##",
            ValueType.Damage
        );

        // -----------------------------------------------------
        // ELECTRIC SHOCK
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "electricShockDamage",
            "damage",
            "Obrażenia",
            "0.##",
            ValueType.Damage
        );

        AddSkillFloatStat(
            text,
            skill,
            "electricShockStunDuration",
            "stun_duration",
            "Czas ogłuszenia",
            "0.##",
            ValueType.Seconds
        );

        // -----------------------------------------------------
        // ENERGY ORB
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "energyOrbKnockback",
            "knockback",
            "Odrzut",
            "0.##",
            ValueType.Number
        );

        // -----------------------------------------------------
        // SPINNING LASER
        // -----------------------------------------------------

        AddSkillFloatStat(
            text,
            skill,
            "spinningLaserCount",
            "projectile_count",
            "Liczba laserów",
            "0",
            ValueType.Integer
        );

        AddSkillFloatStat(
            text,
            skill,
            "spinningLaserDamage",
            "damage",
            "Obrażenia",
            "0.##",
            ValueType.Damage
        );

        // -----------------------------------------------------
        // LIGHTNING
        // -----------------------------------------------------

        AddSkillPercentStat(
            text,
            skill,
            "lightningStunChance",
            "stun_chance",
            "Szansa na ogłuszenie"
        );

        AddSkillFloatStat(
            text,
            skill,
            "lightningBonusDamage",
            "damage",
            "Dodatkowe obrażenia",
            "0.##",
            ValueType.Damage
        );
    }

    // =========================================================
    // TYP WARTOŚCI
    // =========================================================

    private enum ValueType
    {
        Number,
        Integer,
        Damage,
        AttackSpeed,
        Cooldown,
        Seconds,
        Multiplier
    }

    // =========================================================
    // DODANIE STATYSTYKI
    // =========================================================

    private void AddSkillFloatStat(
        StringBuilder text,
        SkillInstance skill,
        string fieldName,
        string localizationKey,
        string fallbackLabel,
        string format,
        ValueType valueType)
    {
        object value =
            GetFieldOrPropertyValue(
                skill,
                fieldName
            );

        if (value == null)
        {
            return;
        }

        float floatValue;

        try
        {
            floatValue =
                Convert.ToSingle(value);
        }
        catch
        {
            return;
        }

        if (Mathf.Approximately(
                floatValue,
                0f))
        {
            return;
        }

        string label =
            GetStatLabel(
                localizationKey,
                fallbackLabel
            );

        string formatted =
            FormatValue(
                floatValue,
                format,
                valueType
            );

        text.AppendLine(
            "<b>" +
            label +
            ":</b> " +
            formatted
        );
    }

    // =========================================================
    // DODANIE PROCENTU
    // =========================================================

    private void AddSkillPercentStat(
        StringBuilder text,
        SkillInstance skill,
        string fieldName,
        string localizationKey,
        string fallbackLabel)
    {
        object value =
            GetFieldOrPropertyValue(
                skill,
                fieldName
            );

        if (value == null)
        {
            return;
        }

        float floatValue;

        try
        {
            floatValue =
                Convert.ToSingle(value);
        }
        catch
        {
            return;
        }

        if (Mathf.Approximately(
                floatValue,
                0f))
        {
            return;
        }

        string label =
            GetStatLabel(
                localizationKey,
                fallbackLabel
            );

        string formatted =
            (floatValue * 100f)
                .ToString("0.#") +
            "%";

        text.AppendLine(
            "<b>" +
            label +
            ":</b> " +
            formatted
        );
    }

    // =========================================================
    // FORMATOWANIE
    // =========================================================

    private string FormatValue(
        float value,
        string format,
        ValueType valueType)
    {
        switch (valueType)
        {
            case ValueType.Integer:
                return value.ToString("0");

            case ValueType.Damage:
                return ColorizeDamage(
                    value.ToString(format)
                );

            case ValueType.AttackSpeed:
                return Colorize(
                    value.ToString(format),
                    "#E6C15A"
                );

            case ValueType.Cooldown:
                return Colorize(
                    value.ToString(format),
                    "#E6C15A"
                );

            case ValueType.Seconds:
                return value.ToString(format) + " s";

            case ValueType.Multiplier:
                return value.ToString(format) + "x";

            default:
                return value.ToString(format);
        }
    }

    // =========================================================
    // KOLOR OBRAŻEŃ
    // =========================================================

    private string ColorizeDamage(
        string value)
    {
        if (selectedSkill == null ||
            selectedSkill.data == null)
        {
            return value;
        }

        string color;

        switch (selectedSkill.data.damageType)
        {
            case SkillData.DamageType.Fire:
                color = "#f55a00";
                break;

            case SkillData.DamageType.Cold:
                color = "#167bff";
                break;

            case SkillData.DamageType.Chaos:
                color = "#009113";
                break;

            case SkillData.DamageType.Lightning:
                color = "#00f7ff";
                break;

            case SkillData.DamageType.Physical:
                color = "#7d7e7e";
                break;

            default:
                return value;
        }

        return Colorize(
            value,
            color
        );
    }

    // =========================================================
    // KOLOR
    // =========================================================

    private string Colorize(
        string value,
        string color)
    {
        return
            "<color=" +
            color +
            ">" +
            value +
            "</color>";
    }

    // =========================================================
    // ODCZYT POLA / WŁAŚCIWOŚCI
    // =========================================================

    private object GetFieldOrPropertyValue(
        SkillInstance skill,
        string fieldName)
    {
        if (skill == null)
        {
            return null;
        }

        Type type =
            skill.GetType();

        FieldInfo field =
            type.GetField(
                fieldName,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance
            );

        if (field != null)
        {
            return field.GetValue(
                skill
            );
        }

        PropertyInfo property =
            type.GetProperty(
                fieldName,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance
            );

        if (property != null)
        {
            return property.GetValue(
                skill
            );
        }

        return null;
    }

    // =========================================================
    // LOCALIZATION
    // =========================================================

    private string GetLocalizedString(
        LocalizedString localizedString)
    {
        if (localizedString == null)
        {
            return "";
        }

        try
        {
            return localizedString.GetLocalizedString();
        }
        catch
        {
            return "";
        }
    }

    private string GetStatLabel(
        string key,
        string fallback)
    {
        if (statLocalizationTable == null)
        {
            return fallback;
        }

        try
        {
            var table =
                statLocalizationTable.GetTable();

            if (table == null)
            {
                return fallback;
            }

            var entry =
                table.GetEntry(
                    key
                );

            if (entry == null)
            {
                return fallback;
            }

            return entry.LocalizedValue;
        }
        catch
        {
            return fallback;
        }
    }

    // =========================================================
    // TAGI
    // =========================================================

    private void BuildTagIcons(
        SkillInstance skill)
    {
        if (tagIconContainer == null)
        {
            return;
        }

        ClearTagIcons();

        if (skill == null ||
            skill.data == null)
        {
            return;
        }

        if (tagIconDatabase == null ||
            tagIconPrefab == null)
        {
            return;
        }

        AddDamageTypeTag(
            skill.data.damageType
        );

        SkillCategory categories =
            skill.data.categories;

        AddCategoryTag(
            categories,
            "Projectile",
            SkillTagIconDatabase.TagType.Projectile
        );

        AddCategoryTag(
            categories,
            "Area",
            SkillTagIconDatabase.TagType.Area
        );

        AddCategoryTag(
            categories,
            "Piercing",
            SkillTagIconDatabase.TagType.Piercing
        );

        AddCategoryTag(
            categories,
            "Chain",
            SkillTagIconDatabase.TagType.Chain
        );

        AddCategoryTag(
            categories,
            "DoT",
            SkillTagIconDatabase.TagType.DoT
        );

        AddCategoryTag(
            categories,
            "Melee",
            SkillTagIconDatabase.TagType.Melee
        );

        Canvas.ForceUpdateCanvases();

        RectTransform containerRect =
            tagIconContainer as RectTransform;

        if (containerRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                containerRect
            );
        }

        Canvas.ForceUpdateCanvases();
    }

    // =========================================================
    // DAMAGE TYPE -> TAG
    // =========================================================

    private void AddDamageTypeTag(
        SkillData.DamageType damageType)
    {
        SkillTagIconDatabase.TagType tagType;

        switch (damageType)
        {
            case SkillData.DamageType.Fire:

                tagType =
                    SkillTagIconDatabase.TagType.Fire;

                break;

            case SkillData.DamageType.Cold:

                tagType =
                    SkillTagIconDatabase.TagType.Cold;

                break;

            case SkillData.DamageType.Chaos:

                tagType =
                    SkillTagIconDatabase.TagType.Chaos;

                break;

            case SkillData.DamageType.Lightning:

                tagType =
                    SkillTagIconDatabase.TagType.Lightning;

                break;

            case SkillData.DamageType.Physical:

                tagType =
                    SkillTagIconDatabase.TagType.Physical;

                break;

            default:

                return;
        }

        CreateTagIcon(
            tagType
        );
    }

    // =========================================================
    // KATEGORIA -> TAG
    // =========================================================

    private void AddCategoryTag(
        SkillCategory categories,
        string categoryName,
        SkillTagIconDatabase.TagType tagType)
    {
        if (!HasCategory(
                categories,
                categoryName))
        {
            return;
        }

        CreateTagIcon(
            tagType
        );
    }

    // =========================================================
    // SPRAWDZENIE KATEGORII
    // =========================================================

    private bool HasCategory(
        SkillCategory categories,
        string categoryName)
    {
        Type type =
            typeof(SkillCategory);

        try
        {
            FieldInfo field =
                type.GetField(
                    categoryName,
                    BindingFlags.Public |
                    BindingFlags.Static
                );

            if (field == null)
            {
                return false;
            }

            object value =
                field.GetValue(null);

            if (value == null)
            {
                return false;
            }

            int categoryValue =
                Convert.ToInt32(value);

            int currentValue =
                Convert.ToInt32(categories);

            return
                (currentValue & categoryValue)
                != 0;
        }
        catch
        {
            return false;
        }
    }

    // =========================================================
    // SPRAWDZENIE TAGU SKILLA
    // =========================================================

    private bool HasTag(
        SkillInstance skill,
        string tagName)
    {
        if (skill == null ||
            skill.data == null)
        {
            return false;
        }

        return HasCategory(
            skill.data.categories,
            tagName
        );
    }

    // =========================================================
    // TWORZENIE IKONY TAGU
    // =========================================================

    private void CreateTagIcon(
        SkillTagIconDatabase.TagType tagType)
    {
        GameObject iconObject =
            Instantiate(
                tagIconPrefab,
                tagIconContainer
            );

        if (iconObject == null)
        {
            return;
        }

        iconObject.transform.localScale =
            Vector3.one;

        Image image =
            iconObject.GetComponent<Image>();

        if (image == null)
        {
            image =
                iconObject.GetComponentInChildren<Image>();
        }

        if (image == null)
        {
            return;
        }

        Sprite sprite =
            tagIconDatabase.GetIcon(
                tagType
            );

        if (sprite != null)
        {
            image.sprite =
                sprite;

            image.enabled =
                true;
        }
    }

    // =========================================================
    // CZYSZCZENIE TAGÓW
    // =========================================================

    private void ClearTagIcons()
    {
        if (tagIconContainer == null)
        {
            return;
        }

        for (
            int i =
                tagIconContainer.childCount - 1;
            i >= 0;
            i--)
        {
            Transform child =
                tagIconContainer.GetChild(i);

            if (child != null)
            {
                Destroy(
                    child.gameObject
                );
            }
        }
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

        LayoutRebuilder.ForceRebuildLayoutImmediate(
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

        size.x =
            targetWidth;

        size.y =
            targetHeight;

        descriptionRect.sizeDelta =
            size;

        Canvas.ForceUpdateCanvases();

        LayoutRebuilder.ForceRebuildLayoutImmediate(
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
            resizeDescriptionCoroutine =
                null;

            yield break;
        }

        Canvas.ForceUpdateCanvases();

        ResizeDescriptionWindow();

        Canvas.ForceUpdateCanvases();

        resizeDescriptionCoroutine =
            null;
    }

    // =========================================================
    // ZAMYKANIE OPISU
    // =========================================================

    public void CloseDescription()
    {
        if (resizeDescriptionCoroutine != null)
        {
            StopCoroutine(
                resizeDescriptionCoroutine
            );

            resizeDescriptionCoroutine =
                null;
        }

        if (descriptionText != null)
        {
            descriptionText.text = "";
        }

        ClearTagIcons();

        if (descriptionWindow != null)
        {
            descriptionWindow.SetActive(false);
        }

        selectedSkill =
            null;

        if (descriptionRect != null &&
            initialDescriptionSizeCaptured)
        {
            descriptionRect.sizeDelta =
                initialDescriptionSize;
        }

        Canvas.ForceUpdateCanvases();
    }
}