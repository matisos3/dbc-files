using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SkillUpgradeSelectionManager : MonoBehaviour
{
    // =========================================================
    // ULEPSZENIA
    // =========================================================

    [Header("Ulepszenia")]
    public List<SkillUpgradeData> allUpgrades =
        new List<SkillUpgradeData>();

    public SkillUpgradeButton[] buttons;


    // =========================================================
    // OPIS
    // =========================================================

    [Header("Opis")]
    public TMP_Text descriptionText;

    public GameObject descriptionWindow;

    public RectTransform descriptionRect;


    // =========================================================
    // PADDING
    // =========================================================

    [Header("Padding opisu")]
    public float descriptionPadding = 20f;

    public float descriptionHorizontalPadding = 40f;

    public float minDescriptionWidth = 250f;

    public float maxDescriptionWidth = 700f;


    // =========================================================
    // CENA
    // =========================================================

    [Header("Cena")]
    public TMP_Text costText;


    // =========================================================
    // MANAGERY
    // =========================================================

    [Header("Manager")]
    public SkillUpgradeManager upgradeManager;

    public EnemySpawner enemySpawner;


    // =========================================================
    // LOCALIZATION
    // =========================================================

    [Header("Localization")]
    public LocalizedStringTable statLocalizationTable;


    // =========================================================
    // KLUCZE STATYSTYK
    // =========================================================

    [Header("Klucze lokalizacyjne statystyk")]

    public string damageKey = "damage";

    public string attackSpeedKey = "attack_speed";

    public string cooldownKey = "cooldown";

    public string rangeKey = "range";

    public string aoeKey = "aoe";

    public string explosionRadiusKey = "explosion_radius";

    public string projectileCountKey = "projectile_count";

    public string piercingCountKey = "piercing_count";

    public string chainCountKey = "chain_count";

    public string chainRangeKey = "chain_range";

    public string chainDamageKey = "chain_damage";

    public string dotDamageKey = "dot_damage";

    public string dotDurationKey = "dot_duration";

    public string slowPercentKey = "slow_percent";

    public string slowDurationKey = "slow_duration";

    public string rootDurationKey = "root_duration";

    public string freezeDurationKey = "freeze_duration";


    // =========================================================
    // KRYTYCZNE
    // =========================================================

    [Header("Klucze lokalizacyjne statystyk krytycznych")]

    public string criticalChanceKey = "critical_chance";

    public string criticalDamageKey = "critical_damage";


    // =========================================================
    // NOWE STATYSTYKI OBRONY
    // =========================================================

    [Header("Klucze lokalizacyjne statystyk obrony")]

    public string healthRegenerationKey = "health_regeneration";

    public string maxHealthKey = "max_health";

    public string damageReductionFlatKey = "damage_reduction_flat";

    public string damageReductionPercentKey = "damage_reduction_percent";


    // =========================================================
    // TAGI
    // =========================================================

    [Header("Tagi ulepszeń")]
    public SkillUpgradeTagIconDatabase tagIconDatabase;


    // =========================================================
    // UI TAGÓW
    // =========================================================

    [Header("UI tagów")]
    public Transform tagIconContainer;

    public Image tagIconPrefab;


    // =========================================================
    // WEWNĘTRZNE
    // =========================================================

    private SkillUpgradeButton selectedButton;

    private SkillUpgradeData selectedUpgrade;

    private Vector2 initialDescriptionSize;

    private bool initialDescriptionSizeCaptured;


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

        if (upgradeManager == null)
        {
            upgradeManager =
                FindAnyObjectByType<SkillUpgradeManager>();
        }

        if (enemySpawner == null)
        {
            enemySpawner =
                FindAnyObjectByType<EnemySpawner>();
        }

        if (tagIconContainer != null)
        {
            tagIconContainer.gameObject.SetActive(true);
        }

        ClearTagIcons();

        ShowUpgrades();

        if (descriptionWindow != null)
        {
            descriptionWindow.SetActive(false);
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 pointerPosition =
                Mouse.current.position.ReadValue();

            CheckClickOutsideUpgrade(
                pointerPosition
            );
        }

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press
                .wasPressedThisFrame)
        {
            Vector2 pointerPosition =
                Touchscreen.current.primaryTouch
                    .position.ReadValue();

            CheckClickOutsideUpgrade(
                pointerPosition
            );
        }
    }


    // =========================================================
    // CHECK CLICK OUTSIDE
    // =========================================================

    private void CheckClickOutsideUpgrade(
        Vector2 pointerPosition)
    {
        if (EventSystem.current == null)
            return;

        PointerEventData pointerData =
            new PointerEventData(
                EventSystem.current
            );

        pointerData.position =
            pointerPosition;

        List<RaycastResult> results =
            new List<RaycastResult>();

        EventSystem.current.RaycastAll(
            pointerData,
            results
        );

        foreach (RaycastResult result in results)
        {
            SkillUpgradeButton upgradeButton =
                result.gameObject.GetComponent<SkillUpgradeButton>();

            if (upgradeButton != null)
            {
                return;
            }

            upgradeButton =
                result.gameObject.GetComponentInParent<SkillUpgradeButton>();

            if (upgradeButton != null)
            {
                return;
            }
        }

        if (selectedButton != null)
        {
            CloseDescription();
        }
    }


    // =========================================================
    // SHOW UPGRADES
    // =========================================================

    public void ShowUpgrades()
    {
        if (allUpgrades == null ||
            allUpgrades.Count == 0)
        {
            return;
        }

        List<SkillUpgradeData> pool =
            new List<SkillUpgradeData>(
                allUpgrades
            );

        Shuffle(pool);

        int count =
            Mathf.Min(
                buttons.Length,
                pool.Count
            );

        for (int i = 0;
             i < buttons.Length;
             i++)
        {
            if (buttons[i] == null)
                continue;

            if (i < count)
            {
                buttons[i].gameObject.SetActive(true);

                buttons[i].Setup(
                    pool[i],
                    this
                );
            }
            else
            {
                buttons[i].gameObject.SetActive(false);
            }
        }

        ClearSelection();
    }


    // =========================================================
    // BUTTON CLICKED
    // =========================================================

    public void ButtonClicked(
        SkillUpgradeButton button,
        SkillUpgradeData upgrade)
    {
        if (button == null ||
            upgrade == null)
        {
            return;
        }

        if (selectedButton == button)
        {
            BuyUpgrade(upgrade);
            return;
        }

        SelectButton(
            button,
            upgrade
        );
    }


    // =========================================================
    // SELECT
    // =========================================================

    private void SelectButton(
        SkillUpgradeButton button,
        SkillUpgradeData upgrade)
    {
        ClearSelection();

        selectedButton =
            button;

        selectedUpgrade =
            upgrade;

        button.Highlight();

        ShowDescription(upgrade);
    }


    // =========================================================
    // SHOW DESCRIPTION
    // =========================================================

    private void ShowDescription(
        SkillUpgradeData upgrade)
    {
        if (upgrade == null)
            return;

        if (descriptionWindow != null)
        {
            descriptionWindow.SetActive(true);
        }

        if (descriptionText != null)
        {
            descriptionText.text =
                "\n\n" +
                BuildDescription(upgrade);
        }

        if (costText != null)
        {
            costText.text =
                upgrade.cost.ToString();
        }

        BuildTagIcons(upgrade);

        ResizeDescriptionWindow();
    }


    // =========================================================
    // HIDE TAG CONTAINER
    // =========================================================

    private void HideTagIconContainer()
    {
        if (tagIconContainer == null)
            return;

        tagIconContainer.gameObject.SetActive(false);
    }


    // =========================================================
    // RESIZE DESCRIPTION WINDOW
    // =========================================================

    private void ResizeDescriptionWindow()
    {
        if (descriptionText == null ||
            descriptionRect == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();

        float textWidth =
            descriptionText.preferredWidth;

        float textHeight =
            descriptionText.preferredHeight;

        float targetWidth =
            textWidth +
            descriptionHorizontalPadding;

        targetWidth =
            Mathf.Clamp(
                targetWidth,
                minDescriptionWidth,
                maxDescriptionWidth
            );

        float tagHeight = 0f;

        if (tagIconContainer != null)
        {
            RectTransform tagRect =
                tagIconContainer as RectTransform;

            if (tagRect != null)
            {
                Canvas.ForceUpdateCanvases();

                tagHeight =
                    tagRect.rect.height;

                if (tagHeight <= 0f)
                {
                    tagHeight =
                        LayoutUtility.GetPreferredHeight(
                            tagRect
                        );
                }
            }
        }

        float tagSpacing = 0f;

        if (tagHeight > 0f)
        {
            tagSpacing = 10f;
        }

        float totalHeight =
            textHeight +
            tagHeight +
            tagSpacing +
            descriptionPadding;

        Vector2 size =
            descriptionRect.sizeDelta;

        size.x =
            targetWidth;

        size.y =
            totalHeight;

        descriptionRect.sizeDelta =
            size;

        Canvas.ForceUpdateCanvases();
    }


    // =========================================================
    // BUILD DESCRIPTION
    // =========================================================

    private string BuildDescription(
        SkillUpgradeData upgrade)
    {
        if (upgrade == null)
            return string.Empty;

        string result =
            GetLocalizedString(
                upgrade.localizedName
            );

        string description =
            GetLocalizedString(
                upgrade.localizedDescription
            );

        if (!string.IsNullOrEmpty(description))
        {
            if (!string.IsNullOrEmpty(result))
            {
                result += "\n\n";
            }

            result +=
                description;
        }

        if (upgrade.modifiers != null &&
            upgrade.modifiers.Count > 0)
        {
            string modifiersText =
                BuildModifiersDescription(
                    upgrade
                );

            if (!string.IsNullOrEmpty(modifiersText))
            {
                if (!string.IsNullOrEmpty(result))
                {
                    result += "\n\n";
                }

                result +=
                    modifiersText;
            }
        }

        return result;
    }


    // =========================================================
    // BUILD MULTIPLE MODIFIERS
    // =========================================================

    private string BuildModifiersDescription(
        SkillUpgradeData upgrade)
    {
        if (upgrade == null ||
            upgrade.modifiers == null ||
            upgrade.modifiers.Count == 0)
        {
            return string.Empty;
        }

        int currentLevel = 0;

        if (upgradeManager != null)
        {
            currentLevel =
                upgradeManager.GetUpgradeLevel(
                    upgrade
                );
        }

        List<string> lines =
            new List<string>();

        for (int i = 0;
             i < upgrade.modifiers.Count;
             i++)
        {
            SkillUpgradeData.UpgradeModifier modifier =
                upgrade.modifiers[i];

            if (modifier == null)
                continue;

            string statName =
                GetLocalizedStatName(
                    modifier.stat
                );

            if (string.IsNullOrEmpty(statName))
            {
                statName =
                    GetFallbackStatName(
                        modifier.stat
                    );
            }

            float currentValue =
                CalculateModifierValue(
                    modifier,
                    currentLevel
                );

            float nextValue =
                CalculateModifierValue(
                    modifier,
                    currentLevel + 1
                );

            string currentText =
                FormatModifierValue(
                    modifier,
                    currentValue
                );

            string nextText =
                FormatModifierValue(
                    modifier,
                    nextValue
                );

            if (!string.IsNullOrEmpty(statName))
            {
                lines.Add(
                    statName +
                    "\n" +
                    currentText +
                    "  <size=150%>→</size>  " +
                    nextText
                );
            }
            else
            {
                lines.Add(
                    currentText +
                    "  <size=150%>→</size>  " +
                    nextText
                );
            }
        }

        return
            string.Join(
                "\n",
                lines
            );
    }


    // =========================================================
    // CALCULATE MODIFIER VALUE
    // =========================================================
    //
    // Dla zwykłych statystyk:
    //
    // level 1 = value
    // level 2 = value * 2
    // level 3 = value * 3
    //
    // Dla MaxHealth i HealthRegeneration:
    //
    // level 1 = value
    // level 2 = value + value * multiplier
    // level 3 = value + value * multiplier
    //           + value * multiplier^2
    //
    // Dzięki temu opis używa dokładnie tej samej
    // logiki kumulacyjnej co SkillUpgradeManager.
    // =========================================================

    private float CalculateModifierValue(
        SkillUpgradeData.UpgradeModifier modifier,
        int level)
    {
        if (modifier == null)
            return 0f;

        if (level <= 0)
            return 0f;

        // =====================================================
        // NIELINIOWE STATYSTYKI
        // =====================================================

        if (IsNonlinearStat(modifier.stat))
        {
            float multiplier =
                Mathf.Max(
                    1f,
                    modifier.nonlinearMultiplier
                );

            float totalValue = 0f;

            for (int i = 0;
                 i < level;
                 i++)
            {
                float levelValue =
                    modifier.value *
                    Mathf.Pow(
                        multiplier,
                        i
                    );

                totalValue +=
                    levelValue;
            }

            return totalValue;
        }

        // =====================================================
        // ZWYKŁE STATYSTYKI
        // =====================================================

        return
            modifier.value *
            level;
    }


    // =========================================================
    // CZY STATYSTYKA JEST NIELINIOWA?
    // =========================================================

    private bool IsNonlinearStat(
        SkillUpgradeData.UpgradeStat stat)
    {
        return
            stat ==
                SkillUpgradeData.UpgradeStat.MaxHealth ||
            stat ==
                SkillUpgradeData.UpgradeStat.HealthRegeneration;
    }


    // =========================================================
    // LOCALIZED STAT NAME
    // =========================================================

    private string GetLocalizedStatName(
        SkillUpgradeData.UpgradeStat stat)
    {
        if (statLocalizationTable == null)
        {
            return GetFallbackStatName(stat);
        }

        string key =
            GetStatLocalizationKey(stat);

        if (string.IsNullOrEmpty(key))
        {
            return GetFallbackStatName(stat);
        }

        var table =
            statLocalizationTable.GetTable();

        if (table == null)
        {
            return GetFallbackStatName(stat);
        }

        var entry =
            table.GetEntry(key);

        if (entry == null)
        {
            return GetFallbackStatName(stat);
        }

        return entry.GetLocalizedString();
    }


    // =========================================================
    // STAT LOCALIZATION KEY
    // =========================================================

    private string GetStatLocalizationKey(
        SkillUpgradeData.UpgradeStat stat)
    {
        switch (stat)
        {
            case SkillUpgradeData.UpgradeStat.Damage:
                return damageKey;

            case SkillUpgradeData.UpgradeStat.AttackSpeed:
                return attackSpeedKey;

            case SkillUpgradeData.UpgradeStat.Cooldown:
                return cooldownKey;

            case SkillUpgradeData.UpgradeStat.Range:
                return rangeKey;

            case SkillUpgradeData.UpgradeStat.AoE:
                return aoeKey;

            case SkillUpgradeData.UpgradeStat.ExplosionRadius:
                return explosionRadiusKey;

            case SkillUpgradeData.UpgradeStat.ProjectileCount:
                return projectileCountKey;

            case SkillUpgradeData.UpgradeStat.PiercingCount:
                return piercingCountKey;

            case SkillUpgradeData.UpgradeStat.ChainCount:
                return chainCountKey;

            case SkillUpgradeData.UpgradeStat.ChainRange:
                return chainRangeKey;

            case SkillUpgradeData.UpgradeStat.ChainDamage:
                return chainDamageKey;

            case SkillUpgradeData.UpgradeStat.DotDamage:
                return dotDamageKey;

            case SkillUpgradeData.UpgradeStat.DotDuration:
                return dotDurationKey;

            case SkillUpgradeData.UpgradeStat.SlowPercent:
                return slowPercentKey;

            case SkillUpgradeData.UpgradeStat.SlowDuration:
                return slowDurationKey;

            case SkillUpgradeData.UpgradeStat.RootDuration:
                return rootDurationKey;

            case SkillUpgradeData.UpgradeStat.FreezeDuration:
                return freezeDurationKey;

            case SkillUpgradeData.UpgradeStat.CriticalChance:
                return criticalChanceKey;

            case SkillUpgradeData.UpgradeStat.CriticalDamage:
                return criticalDamageKey;

            case SkillUpgradeData.UpgradeStat.HealthRegeneration:
                return healthRegenerationKey;

            case SkillUpgradeData.UpgradeStat.MaxHealth:
                return maxHealthKey;

            case SkillUpgradeData.UpgradeStat.DamageReductionFlat:
                return damageReductionFlatKey;

            case SkillUpgradeData.UpgradeStat.DamageReductionPercent:
                return damageReductionPercentKey;

            default:
                return string.Empty;
        }
    }


    // =========================================================
    // FALLBACK STAT NAME
    // =========================================================

    private string GetFallbackStatName(
        SkillUpgradeData.UpgradeStat stat)
    {
        switch (stat)
        {
            case SkillUpgradeData.UpgradeStat.Damage:
                return "Damage";

            case SkillUpgradeData.UpgradeStat.AttackSpeed:
                return "Attack Speed";

            case SkillUpgradeData.UpgradeStat.Cooldown:
                return "Cooldown";

            case SkillUpgradeData.UpgradeStat.Range:
                return "Range";

            case SkillUpgradeData.UpgradeStat.AoE:
                return "Area of Effect";

            case SkillUpgradeData.UpgradeStat.ExplosionRadius:
                return "Explosion Radius";

            case SkillUpgradeData.UpgradeStat.ProjectileCount:
                return "Projectile Count";

            case SkillUpgradeData.UpgradeStat.PiercingCount:
                return "piercing_count";

            case SkillUpgradeData.UpgradeStat.ChainCount:
                return "Chain Count";

            case SkillUpgradeData.UpgradeStat.ChainRange:
                return "Chain Range";

            case SkillUpgradeData.UpgradeStat.ChainDamage:
                return "Chain Damage";

            case SkillUpgradeData.UpgradeStat.DotDamage:
                return "Damage over Time";

            case SkillUpgradeData.UpgradeStat.DotDuration:
                return "Duration";

            case SkillUpgradeData.UpgradeStat.SlowPercent:
                return "Slow";

            case SkillUpgradeData.UpgradeStat.SlowDuration:
                return "Slow Duration";

            case SkillUpgradeData.UpgradeStat.RootDuration:
                return "Root Duration";

            case SkillUpgradeData.UpgradeStat.FreezeDuration:
                return "Freeze Duration";

            case SkillUpgradeData.UpgradeStat.CriticalChance:
                return "Critical Chance";

            case SkillUpgradeData.UpgradeStat.CriticalDamage:
                return "Critical Damage";

            case SkillUpgradeData.UpgradeStat.HealthRegeneration:
                return "Health Regeneration";

            case SkillUpgradeData.UpgradeStat.MaxHealth:
                return "Max Health";

            case SkillUpgradeData.UpgradeStat.DamageReductionFlat:
                return "Damage Reduction";

            case SkillUpgradeData.UpgradeStat.DamageReductionPercent:
                return "Damage Reduction %";

            default:
                return string.Empty;
        }
    }


    // =========================================================
    // FORMAT MODIFIER VALUE
    // =========================================================

    private string FormatModifierValue(
        SkillUpgradeData.UpgradeModifier modifier,
        float value)
    {
        if (modifier == null)
            return string.Empty;

        if (modifier.modifierType ==
            SkillUpgradeData.ModifierType.Percent)
        {
            string number =
                FormatNumber(
                    Mathf.Abs(value)
                );

            return "+" +
                   number +
                   "%";
        }

        string flatNumber =
            FormatNumber(value);

        if (value > 0f)
        {
            return "+" +
                   flatNumber;
        }

        return flatNumber;
    }


    // =========================================================
    // FORMAT NUMBER
    // =========================================================

    private string FormatNumber(
        float value)
    {
        if (Mathf.Approximately(
                value,
                Mathf.Round(value)))
        {
            return Mathf.RoundToInt(value)
                .ToString();
        }

        return value.ToString("0.##");
    }


    // =========================================================
    // LOCALIZED STRING
    // =========================================================

    private string GetLocalizedString(
        LocalizedString localizedString)
    {
        if (localizedString == null)
            return string.Empty;

        if (!localizedString.IsEmpty)
        {
            return localizedString
                .GetLocalizedString();
        }

        return string.Empty;
    }


    // =========================================================
    // BUILD TAG ICONS
    // =========================================================

    private void BuildTagIcons(
        SkillUpgradeData upgrade)
    {
        if (tagIconContainer == null)
            return;

        ClearTagIcons();

        if (upgrade == null)
            return;

        if (tagIconDatabase == null)
        {
            return;
        }

        if (tagIconPrefab == null)
        {
            return;
        }

        if (upgrade.target ==
            SkillUpgradeData.UpgradeTarget.Global)
        {
            CreateTagIcon(
                SkillUpgradeTagIconDatabase.TagType.All
            );

            return;
        }

        if (upgrade.target ==
                SkillUpgradeData.UpgradeTarget.Element ||
            upgrade.target ==
                SkillUpgradeData.UpgradeTarget.ElementAndCategory)
        {
            CreateTagIcon(
                ConvertElementToTag(
                    upgrade.element
                )
            );
        }

        if (upgrade.target ==
                SkillUpgradeData.UpgradeTarget.Category ||
            upgrade.target ==
                SkillUpgradeData.UpgradeTarget.ElementAndCategory)
        {
            CreateTagIcon(
                ConvertCategoryToTag(
                    upgrade.category
                )
            );
        }
    }


    // =========================================================
    // CREATE TAG ICON
    // =========================================================

    private void CreateTagIcon(
        SkillUpgradeTagIconDatabase.TagType tag)
    {
        Sprite sprite =
            tagIconDatabase.GetIcon(tag);

        if (sprite == null)
        {
            return;
        }

        Image newIcon =
            Instantiate(
                tagIconPrefab,
                tagIconContainer
            );

        newIcon.sprite =
            sprite;

        newIcon.gameObject.SetActive(true);
    }


    // =========================================================
    // CLEAR TAG ICONS
    // =========================================================

    private void ClearTagIcons()
    {
        if (tagIconContainer == null)
            return;

        for (int i =
                 tagIconContainer.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(
                tagIconContainer
                    .GetChild(i)
                    .gameObject
            );
        }
    }


    // =========================================================
    // ELEMENT → TAG
    // =========================================================

    private SkillUpgradeTagIconDatabase.TagType
        ConvertElementToTag(
            SkillData.DamageType element)
    {
        switch (element)
        {
            case SkillData.DamageType.Fire:
                return
                    SkillUpgradeTagIconDatabase.TagType.Fire;

            case SkillData.DamageType.Cold:
                return
                    SkillUpgradeTagIconDatabase.TagType.Cold;

            case SkillData.DamageType.Chaos:
                return
                    SkillUpgradeTagIconDatabase.TagType.Chaos;

            case SkillData.DamageType.Lightning:
                return
                    SkillUpgradeTagIconDatabase.TagType.Lightning;

            case SkillData.DamageType.Physical:
                return
                    SkillUpgradeTagIconDatabase.TagType.Physical;

            default:
                return
                    SkillUpgradeTagIconDatabase.TagType.All;
        }
    }


    // =========================================================
    // CATEGORY → TAG
    // =========================================================

    private SkillUpgradeTagIconDatabase.TagType
        ConvertCategoryToTag(
            SkillCategory category)
    {
        if ((category &
            SkillCategory.Projectile) !=
            SkillCategory.None)
        {
            return
                SkillUpgradeTagIconDatabase.TagType.Projectile;
        }

        if ((category &
            SkillCategory.Area) !=
            SkillCategory.None)
        {
            return
                SkillUpgradeTagIconDatabase.TagType.Area;
        }

        if ((category &
            SkillCategory.Piercing) !=
            SkillCategory.None)
        {
            return
                SkillUpgradeTagIconDatabase.TagType.Piercing;
        }

        if ((category &
            SkillCategory.Chain) !=
            SkillCategory.None)
        {
            return
                SkillUpgradeTagIconDatabase.TagType.Chain;
        }

        if ((category &
            SkillCategory.DoT) !=
            SkillCategory.None)
        {
            return
                SkillUpgradeTagIconDatabase.TagType.DoT;
        }

        if ((category &
            SkillCategory.Melee) !=
            SkillCategory.None)
        {
            return
                SkillUpgradeTagIconDatabase.TagType.Melee;
        }

        return
            SkillUpgradeTagIconDatabase.TagType.All;
    }


    // =========================================================
    // BUY UPGRADE
    // =========================================================

    private void BuyUpgrade(
        SkillUpgradeData upgrade)
    {
        if (upgrade == null)
            return;

        if (upgradeManager == null)
        {
            upgradeManager =
                FindAnyObjectByType<SkillUpgradeManager>();
        }

        if (enemySpawner == null)
        {
            enemySpawner =
                FindAnyObjectByType<EnemySpawner>();
        }

        if (enemySpawner == null)
        {
            return;
        }

        // -----------------------------------------------------
        // GOLD
        // -----------------------------------------------------

        if (upgrade.cost > 0)
        {
            if (!enemySpawner.TrySpendGold(
                    upgrade.cost))
            {
                return;
            }
        }

        // -----------------------------------------------------
        // MANAGER
        // -----------------------------------------------------

        if (upgradeManager == null)
        {
            return;
        }

        // -----------------------------------------------------
        // ADD / LEVEL UP
        // -----------------------------------------------------

        upgradeManager.BuyUpgrade(
            upgrade
        );

        // =====================================================
        // STATYSTYKA
        // ULEPSZENIE ZOSTAŁO KUPIONE
        // =====================================================

        if (GameStatsManager.Instance != null)
        {
            GameStatsManager.Instance.AddUpgradePurchased();
        }

        // =====================================================
        // ULEPSZENIE KUPIONE
        // USUWAMY JE Z UI
        // =====================================================

        if (selectedButton != null)
        {
            selectedButton.gameObject.SetActive(false);
        }

        // -----------------------------------------------------
        // ZAMYKAMY OPIS I CZYŚCIMY WYBÓR
        // -----------------------------------------------------

        ClearSelection();
    }


    // =========================================================
    // CLEAR SELECTION
    // =========================================================

    private void ClearSelection()
    {
        if (selectedButton != null)
        {
            selectedButton.RemoveHighlight();
        }

        selectedButton = null;

        selectedUpgrade = null;

        ClearTagIcons();

        if (descriptionWindow != null)
        {
            descriptionWindow.SetActive(false);
        }

        if (descriptionText != null)
        {
            descriptionText.text = "";
        }

        if (costText != null)
        {
            costText.text = "";
        }

        // =====================================================
        // PRZYWRÓĆ POCZĄTKOWY ROZMIAR PANELU
        // =====================================================

        if (descriptionRect != null &&
            initialDescriptionSizeCaptured)
        {
            descriptionRect.sizeDelta =
                initialDescriptionSize;
        }

        Canvas.ForceUpdateCanvases();
    }


    // =========================================================
    // REFRESH
    // =========================================================

    public void RefreshUpgrades()
    {
        ClearSelection();

        ShowUpgrades();
    }


    // =========================================================
    // SHUFFLE
    // =========================================================

    private void Shuffle(
        List<SkillUpgradeData> list)
    {
        for (int i = 0;
             i < list.Count;
             i++)
        {
            int randomIndex =
                Random.Range(
                    i,
                    list.Count
                );

            SkillUpgradeData temp =
                list[i];

            list[i] =
                list[randomIndex];

            list[randomIndex] =
                temp;
        }
    }


    // =========================================================
    // CLOSE DESCRIPTION
    // =========================================================

    public void CloseDescription()
    {
        if (descriptionText != null)
            descriptionText.text = "";

        if (costText != null)
            costText.text = "";

        if (descriptionWindow != null)
            descriptionWindow.SetActive(false);

        if (selectedButton != null)
        {
            selectedButton.RemoveHighlight();
            selectedButton = null;
        }

        selectedUpgrade = null;

        ClearTagIcons();

        // =====================================================
        // RESET ROZMIARU
        // =====================================================

        if (descriptionRect != null &&
            initialDescriptionSizeCaptured)
        {
            descriptionRect.sizeDelta =
                initialDescriptionSize;
        }

        // =====================================================
        // WYMUSZENIE PRZELICZENIA UI
        // =====================================================

        Canvas.ForceUpdateCanvases();
    }
}