using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Ustawienia Życia")]
    public float maxHealth = 10f;

    private float currentHealth;

    private float damageTakenMultiplier = 1f;
    private float damageAuraMultiplier = 1f;
    private float corruptedDamageTakenMultiplier = 1f;
    private float dodgeChance = 0f;
    private float regenerationPerSecond = 0f;

    public Transform hitPoint;

    public Action<float, float> OnHealthChanged;

    private SkillUpgradeManager skillUpgradeManager;

    private bool lastHitWasCritical = false;
    public bool LastHitWasCritical => lastHitWasCritical;

    public Action<float> OnCriticalHit;

    [Header("Critical Damage Popup")]
    [SerializeField] private CriticalDamagePopup criticalDamagePopupPrefab;

    [Header("Nagroda")]
    public int goldReward = 15;

    private EnemySpawner spawnerManager;
    private GameObject myHealthBar;

    [Header("UI Paska Zdrowia")]
    public Image healthBarFill;

    [Header("Kolory Paska Zdrowia")]
    public Color greenColor = Color.green;
    public Color yellowColor = Color.yellow;
    public Color orangeColor = new Color(1f, 0.5f, 0f);
    public Color redColor = Color.red;

    [Header("Death")]
    [SerializeField] private Animator animator;
    [SerializeField] private float deathExtraDelay = 2f;

    private bool isDying = false;
    public bool IsDying => isDying;

    // =========================================================
    // STATYSTYKA ZABITYCH
    // =========================================================

    private bool killAlreadyRegistered = false;

    // =========================================================
    // GOLD
    // =========================================================

    private bool goldAlreadyGiven = false;

    // =========================================================
    // REINCARNATION
    // =========================================================

    [Header("Reincarnation")]
    [SerializeField] private float reincarnationHealthPercent = 30f;
    [SerializeField] private float reincarnationDelay = 3f;

    private bool hasReincarnation = false;
    private bool hasReincarnated = false;

    [Header("Reincarnation Particle")]
    [SerializeField] private GameObject reincarnationParticlePrefab;
    [SerializeField] private float reincarnationParticleScaleMultiplier = 1.05f;

    private GameObject activeReincarnationParticle;

    // =========================================================
    // ILLUSION
    // =========================================================

    [Header("Illusion")]
    [SerializeField] private float illusionHealthPercent = 50f;

    private readonly float[] illusionThresholds =
    {
        0.75f,
        0.50f,
        0.25f
    };

    private int illusionSpawnCount = 0;

    private bool isIllusion = false;

    private bool illusionStateLocked = false;

    public bool IsIllusion => isIllusion;

    // =========================================================
    // TIMED SHIELD
    // =========================================================

    [Header("Timed Shield")]
    [SerializeField] private float timedShieldPercent = 10f;
    [SerializeField] private float timedShieldInterval = 10f;

    private bool hasTimedShield = false;

    private float shieldMax = 0f;
    private float currentShield = 0f;
    private float shieldTimer = 0f;

    private Image shieldBarFill;

    [SerializeField] private GameObject timedShieldEffectPrefab;

    private GameObject activeTimedShieldEffect;

    // =========================================================
    // SHARED HEALTH
    // =========================================================

    private SharedHealthGroup sharedHealthGroup;
    private bool usesSharedHealth = false;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        spawnerManager =
            FindAnyObjectByType<EnemySpawner>();

        skillUpgradeManager =
            FindAnyObjectByType<SkillUpgradeManager>();

        if (animator == null)
        {
            animator =
                GetComponentInChildren<Animator>();
        }

        RefreshEnemyAffixes();
    }

    // =========================================================
    // REFRESH AFFIXES
    // =========================================================

    private void RefreshEnemyAffixes()
    {
        EnemyAffixes enemyAffixes =
            GetComponent<EnemyAffixes>();

        if (enemyAffixes == null)
        {
            hasReincarnation = false;
            hasTimedShield = false;
            return;
        }

        hasReincarnation =
            enemyAffixes.HasAffix(
                EnemyAffixes.AffixType.Reincarnation
            );

        hasTimedShield =
            enemyAffixes.HasAffix(
                EnemyAffixes.AffixType.TimedShield
            );
    }

    // =========================================================
    // INITIALIZE HEALTH
    // =========================================================

    public void InitializeHealth()
    {
        RefreshEnemyAffixes();

        if (spawnerManager == null)
        {
            spawnerManager =
                FindAnyObjectByType<EnemySpawner>();
        }

        if (skillUpgradeManager == null)
        {
            skillUpgradeManager =
                FindAnyObjectByType<SkillUpgradeManager>();
        }

        // =====================================================
        // ILLUSION
        // =====================================================

        bool preserveIllusion =
            illusionStateLocked &&
            isIllusion;

        currentHealth =
            maxHealth;

        isDying = false;

        hasReincarnated = false;

        if (!preserveIllusion)
        {
            isIllusion = false;
        }

        illusionSpawnCount = 0;

        lastHitWasCritical = false;

        killAlreadyRegistered = false;

        // =====================================================
        // GOLD
        // =====================================================

        if (!preserveIllusion)
        {
            goldAlreadyGiven = false;
        }

        // =====================================================
        // RESET MULTIPLIERÓW
        //
        // UWAGA:
        //
        // Te wartości są celowo resetowane tutaj.
        //
        // EnemyAffixController nakłada następnie affixy
        // runtime PO InitializeHealth().
        //
        // Dzięki temu nie ma konfliktu pomiędzy:
        //
        // InitializeHealth()
        // oraz
        // EnemyAffixController.
        // =====================================================

        damageTakenMultiplier = 1f;
        damageAuraMultiplier = 1f;
        corruptedDamageTakenMultiplier = 1f;
        dodgeChance = 0f;
        regenerationPerSecond = 0f;

        // =====================================================
        // RESET SHARED HEALTH
        // =====================================================

        sharedHealthGroup = null;
        usesSharedHealth = false;

        // =====================================================
        // RESET SHIELD
        // =====================================================

        if (hasTimedShield && !preserveIllusion)
        {
            InitializeTimedShield();
        }
        else
        {
            currentShield = 0f;
            shieldMax = 0f;
            shieldTimer = 0f;

            UpdateShieldBar();
        }

        // =====================================================
        // HEALTH BAR
        // =====================================================

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount =
                1f;

            healthBarFill.color =
                greenColor;
        }

        // =====================================================
        // ANIMATOR
        // =====================================================

        if (animator == null)
        {
            animator =
                GetComponentInChildren<Animator>();
        }

        if (animator != null &&
            animator.runtimeAnimatorController != null)
        {
            animator.ResetTrigger(
                "Death"
            );

            animator.SetBool(
                "IsAttacking",
                false
            );
        }

        OnHealthChanged?.Invoke(
            currentHealth,
            maxHealth
        );
    }

    // =========================================================
    // MARK AS ILLUSION
    // =========================================================

    public void MarkAsIllusion()
    {
        isIllusion = true;

        illusionStateLocked = true;

        hasReincarnation = false;
        hasReincarnated = true;

        hasTimedShield = false;

        usesSharedHealth = false;
        sharedHealthGroup = null;

        goldAlreadyGiven = true;
    }

    // =========================================================
    // INITIALIZE ILLUSION
    // =========================================================

    public void InitializeIllusion(
        float illusionHealth
    )
    {
        MarkAsIllusion();

        isDying = false;

        currentShield = 0f;
        shieldMax = 0f;
        shieldTimer = 0f;

        illusionSpawnCount =
            illusionThresholds.Length;

        maxHealth =
            Mathf.Max(
                1f,
                illusionHealth
            );

        currentHealth =
            maxHealth;

        lastHitWasCritical = false;

        killAlreadyRegistered = false;

        if (spawnerManager == null)
        {
            spawnerManager =
                FindAnyObjectByType<EnemySpawner>();
        }

        if (skillUpgradeManager == null)
        {
            skillUpgradeManager =
                FindAnyObjectByType<SkillUpgradeManager>();
        }

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount =
                1f;

            healthBarFill.color =
                greenColor;
        }

        UpdateShieldBar();

        OnHealthChanged?.Invoke(
            currentHealth,
            maxHealth
        );

        if (animator == null)
        {
            animator =
                GetComponentInChildren<Animator>();
        }

        if (animator != null &&
            animator.runtimeAnimatorController != null)
        {
            animator.ResetTrigger(
                "Death"
            );

            animator.SetBool(
                "IsAttacking",
                false
            );
        }
    }

    // =========================================================
    // HEALTH BAR
    // =========================================================

    public void SetHealthBar(
        GameObject bar
    )
    {
        myHealthBar =
            bar;
    }

    // =========================================================
    // SHIELD BAR
    // =========================================================

    public void SetShieldBar(
        Image shieldFill
    )
    {
        shieldBarFill =
            shieldFill;

        UpdateShieldBar();
    }

    // =========================================================
    // ENABLE TIMED SHIELD
    // =========================================================

    public void EnableTimedShield()
    {
        if (isIllusion)
            return;

        hasTimedShield = true;

        InitializeTimedShield();

        CreateTimedShieldEffect();
    }

    // =========================================================
    // CREATE TIMED SHIELD EFFECT
    // =========================================================

    private void CreateTimedShieldEffect()
    {
        if (isIllusion)
            return;

        if (activeTimedShieldEffect != null)
            return;

        if (timedShieldEffectPrefab == null)
            return;

        activeTimedShieldEffect =
            Instantiate(
                timedShieldEffectPrefab,
                transform
            );

        activeTimedShieldEffect.transform.localPosition =
            new Vector3(
                0f,
                0.75f,
                0f
            );

        activeTimedShieldEffect.transform.localRotation =
            Quaternion.identity;

        activeTimedShieldEffect.transform.localScale =
            new Vector3(
                0.5f,
                0.7f,
                0.5f
            );
    }

    // =========================================================
    // INITIALIZE TIMED SHIELD
    // =========================================================

    private void InitializeTimedShield()
    {
        if (!hasTimedShield)
            return;

        if (isIllusion)
            return;

        shieldMax =
            maxHealth *
            (timedShieldPercent / 100f);

        currentShield =
            shieldMax;

        shieldTimer =
            timedShieldInterval;

        UpdateShieldBar();

        if (activeTimedShieldEffect == null)
        {
            CreateTimedShieldEffect();
        }
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (isDying)
            return;

        UpdateRegeneration();

        UpdateTimedShield();
    }

    // =========================================================
    // CORRUPTED DAMAGE
    // =========================================================

    public void SetCorruptedDamageTakenMultiplier(
        float multiplier
    )
    {
        corruptedDamageTakenMultiplier =
            Mathf.Max(
                0f,
                multiplier
            );
    }

    // =========================================================
    // REGENERATION
    // =========================================================

    private void UpdateRegeneration()
    {
        if (regenerationPerSecond <= 0f)
            return;

        if (currentHealth <= 0f)
            return;

        if (maxHealth <= 0f)
            return;

        float regenerationAmount =
            maxHealth *
            (regenerationPerSecond / 100f) *
            Time.deltaTime;

        currentHealth =
            Mathf.Min(
                currentHealth +
                regenerationAmount,
                maxHealth
            );

        OnHealthChanged?.Invoke(
            currentHealth,
            maxHealth
        );

        UpdateHealthBar();
    }

    // =========================================================
    // TIMED SHIELD UPDATE
    // =========================================================

    private void UpdateTimedShield()
    {
        if (!hasTimedShield)
            return;

        if (shieldMax <= 0f)
            return;

        if (currentHealth <= 0f)
            return;

        shieldTimer -=
            Time.deltaTime;

        if (shieldTimer <= 0f)
        {
            RechargeTimedShield();
        }
    }

    // =========================================================
    // RECHARGE SHIELD
    // =========================================================

    private void RechargeTimedShield()
    {
        currentShield =
            shieldMax;

        shieldTimer =
            timedShieldInterval;

        UpdateShieldBar();

        CreateTimedShieldEffect();
    }

    // =========================================================
    // REGISTER DAMAGE FOR STATS
    // =========================================================

    private void RegisterDamageForStats(
        float damage
    )
    {
        if (damage <= 0f)
            return;

        if (float.IsNaN(damage) ||
            float.IsInfinity(damage))
        {
            return;
        }

        if (GameStatsManager.Instance != null)
        {
            GameStatsManager.Instance.AddDamageDealt(
                damage
            );
        }
    }

    // =========================================================
    // TAKE DAMAGE
    // =========================================================

    public void TakeDamage(
        float damage
    )
    {
        if (damage <= 0f)
            return;

        if (isDying)
            return;

        if (currentHealth <= 0f)
            return;

        lastHitWasCritical = false;

        // =====================================================
        // DODGE
        // =====================================================

        if (dodgeChance > 0f &&
            UnityEngine.Random.value < dodgeChance)
        {
            return;
        }

        // =====================================================
        // DAMAGE TAKEN MULTIPLIER
        // =====================================================

        damage *=
            damageTakenMultiplier;

        if (damage <= 0f)
            return;

        // =====================================================
        // TIMED SHIELD
        // =====================================================

        if (hasTimedShield &&
            currentShield > 0f)
        {
            float damageToShield =
                Mathf.Min(
                    damage,
                    currentShield
                );

            currentShield -=
                damageToShield;

            damage -=
                damageToShield;

            currentShield =
                Mathf.Max(
                    0f,
                    currentShield
                );

            RegisterDamageForStats(
                damageToShield
            );

            UpdateShieldBar();

            if (currentShield <= 0f)
            {
                if (activeTimedShieldEffect != null)
                {
                    Destroy(
                        activeTimedShieldEffect
                    );

                    activeTimedShieldEffect =
                        null;
                }
            }
        }

        // =====================================================
        // CAŁE OBRAŻENIE ZATRZYMANE PRZEZ SHIELD
        // =====================================================

        if (damage <= 0f)
            return;

        float healthBeforeDamage =
            currentHealth;

        // =====================================================
        // CORRUPTED DAMAGE
        // =====================================================

        damage *=
            corruptedDamageTakenMultiplier;

        if (damage <= 0f)
            return;

        // =====================================================
        // SKILL UPGRADES / CRITICAL
        // =====================================================

        if (skillUpgradeManager == null)
        {
            skillUpgradeManager =
                FindAnyObjectByType<SkillUpgradeManager>();
        }

        if (skillUpgradeManager != null)
        {
            damage =
                skillUpgradeManager.CalculateDamageWithCritical(
                    damage,
                    out lastHitWasCritical
                );
        }

        if (damage <= 0f)
            return;

        // =====================================================
        // RZECZYWISTE OBRAŻENIE HP
        // =====================================================

        float actualDamage =
            Mathf.Min(
                damage,
                currentHealth
            );

        actualDamage =
            Mathf.Max(
                0f,
                actualDamage
            );

        currentHealth -=
            actualDamage;

        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0f,
                maxHealth
            );

        RegisterDamageForStats(
            actualDamage
        );

        // =====================================================
        // CRITICAL
        // =====================================================

        if (lastHitWasCritical)
        {
            OnCriticalHit?.Invoke(
                actualDamage
            );

            ShowCriticalDamagePopup(
                actualDamage
            );
        }

        // =====================================================
        // HEALTH UPDATE
        // =====================================================

        OnHealthChanged?.Invoke(
            currentHealth,
            maxHealth
        );

        UpdateHealthBar();

        // =====================================================
        // ILLUSION
        // =====================================================

        if (!isIllusion &&
            currentHealth > 0f)
        {
            CheckIllusionThresholds(
                healthBeforeDamage,
                currentHealth
            );
        }

        // =====================================================
        // DEATH
        // =====================================================

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    // =========================================================
    // CRITICAL DAMAGE POPUP
    // =========================================================

    private void ShowCriticalDamagePopup(
        float damage
    )
    {
        bool showCriticalDamage =
            PlayerPrefs.GetInt(
                "ShowCriticalDamage",
                1
            ) == 1;

        if (!showCriticalDamage)
            return;

        if (criticalDamagePopupPrefab == null)
            return;

        Vector3 spawnPosition;

        if (hitPoint != null)
        {
            spawnPosition =
                hitPoint.position;
        }
        else
        {
            spawnPosition =
                transform.position +
                Vector3.up * 1.5f;
        }

        spawnPosition +=
            Vector3.up * 0.25f;

        CriticalDamagePopup popup =
            Instantiate(
                criticalDamagePopupPrefab,
                spawnPosition,
                Quaternion.identity
            );

        popup.Initialize(
            damage
        );
    }

    // =========================================================
    // ILLUSION THRESHOLDS
    // =========================================================

    private void CheckIllusionThresholds(
        float healthBeforeDamage,
        float healthAfterDamage
    )
    {
        if (isIllusion)
            return;

        EnemyAffixes enemyAffixes =
            GetComponent<EnemyAffixes>();

        if (enemyAffixes == null)
            return;

        if (!enemyAffixes.HasAffix(
            EnemyAffixes.AffixType.Illusion
        ))
        {
            return;
        }

        if (illusionSpawnCount >=
            illusionThresholds.Length)
        {
            return;
        }

        if (maxHealth <= 0f)
            return;

        float beforePercent =
            healthBeforeDamage /
            maxHealth;

        float afterPercent =
            healthAfterDamage /
            maxHealth;

        while (
            illusionSpawnCount <
            illusionThresholds.Length &&
            beforePercent >
            illusionThresholds[illusionSpawnCount] &&
            afterPercent <=
            illusionThresholds[illusionSpawnCount]
        )
        {
            SpawnIllusion();

            illusionSpawnCount++;

            beforePercent =
                illusionThresholds[
                    illusionSpawnCount - 1
                ];
        }
    }

    // =========================================================
    // SPAWN ILLUSION
    // =========================================================

    private void SpawnIllusion()
    {
        if (isIllusion)
            return;

        EnemyAffixes enemyAffixes =
            GetComponent<EnemyAffixes>();

        if (enemyAffixes == null)
            return;

        if (!enemyAffixes.HasAffix(
            EnemyAffixes.AffixType.Illusion
        ))
        {
            return;
        }

        if (illusionSpawnCount >=
            illusionThresholds.Length)
        {
            return;
        }

        if (spawnerManager == null)
        {
            spawnerManager =
                FindAnyObjectByType<EnemySpawner>();
        }

        if (spawnerManager == null)
        {
            return;
        }

        if (currentHealth <= 0f)
            return;

        float illusionHealth =
            currentHealth *
            (illusionHealthPercent / 100f);

        illusionHealth =
            Mathf.Max(
                1f,
                illusionHealth
            );

        spawnerManager.SpawnIllusion(
            gameObject,
            illusionHealth
        );
    }

    // =========================================================
    // HEALTH BAR
    // =========================================================

    private void UpdateHealthBar()
    {
        if (healthBarFill == null)
            return;

        if (maxHealth <= 0f)
            return;

        float healthPercent =
            Mathf.Clamp01(
                currentHealth /
                maxHealth
            );

        healthBarFill.fillAmount =
            healthPercent;

        UpdateHealthBarColor(
            healthPercent
        );
    }

    // =========================================================
    // HEALTH BAR COLOR
    // =========================================================

    private void UpdateHealthBarColor(
        float healthPercent
    )
    {
        if (healthBarFill == null)
            return;

        if (healthPercent <= 0.25f)
        {
            healthBarFill.color =
                redColor;
        }
        else if (healthPercent <= 0.5f)
        {
            healthBarFill.color =
                orangeColor;
        }
        else if (healthPercent <= 0.75f)
        {
            healthBarFill.color =
                yellowColor;
        }
        else
        {
            healthBarFill.color =
                greenColor;
        }
    }

    // =========================================================
    // SHIELD BAR
    // =========================================================

    private void UpdateShieldBar()
    {
        if (shieldBarFill == null)
            return;

        if (!hasTimedShield ||
            shieldMax <= 0f)
        {
            shieldBarFill.fillAmount =
                0f;

            return;
        }

        float shieldPercent =
            Mathf.Clamp01(
                currentShield /
                shieldMax
            );

        shieldBarFill.fillAmount =
            shieldPercent;
    }

    // =========================================================
    // DAMAGE TAKEN MULTIPLIER
    // =========================================================

    public void SetDamageTakenMultiplier(
        float multiplier
    )
    {
        damageTakenMultiplier =
            Mathf.Max(
                0f,
                multiplier
            );
    }

    // =========================================================
    // DAMAGE AURA
    // =========================================================

    public void SetDamageAuraMultiplier(
        float multiplier
    )
    {
        damageAuraMultiplier =
            Mathf.Max(
                0f,
                multiplier
            );
    }

    // =========================================================
    // DODGE
    // =========================================================

    public void SetDodgeChance(
        float chance
    )
    {
        dodgeChance =
            Mathf.Clamp01(
                chance
            );
    }

    // =========================================================
    // REGENERATION
    // =========================================================

    public void SetRegeneration(
        float percentPerSecond
    )
    {
        regenerationPerSecond =
            Mathf.Max(
                0f,
                percentPerSecond
            );
    }

    // =========================================================
    // SHARED HEALTH
    // =========================================================

    public void SetSharedHealthGroup(
        SharedHealthGroup group
    )
    {
        EnemyType enemyType =
            GetComponent<EnemyType>();

        if (enemyType == null ||
            enemyType.Rank !=
            EnemyType.EnemyRank.Champion)
        {
            return;
        }

        sharedHealthGroup =
            group;

        usesSharedHealth =
            group != null;

        if (usesSharedHealth)
        {
            group.Register(
                this
            );
        }
    }

    // =========================================================
    // SHARED HEALTH DISPLAY
    // =========================================================

    public void SetSharedHealthDisplay(
        float healthPercent
    )
    {
        healthPercent =
            Mathf.Clamp01(
                healthPercent
            );

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount =
                healthPercent;

            UpdateHealthBarColor(
                healthPercent
            );
        }
    }

    // =========================================================
    // DIE FROM SHARED HEALTH
    // =========================================================

    public void DieFromSharedHealth()
    {
        if (isDying)
            return;

        isDying = true;

        if (sharedHealthGroup != null)
        {
            sharedHealthGroup.Unregister(
                this
            );
        }

        if (!isIllusion)
        {
            RegisterKillForStats();
        }

        if (myHealthBar != null)
        {
            Destroy(
                myHealthBar
            );

            myHealthBar = null;
        }

        if (activeTimedShieldEffect != null)
        {
            Destroy(
                activeTimedShieldEffect
            );

            activeTimedShieldEffect =
                null;
        }

        GiveGoldReward();

        Destroy(
            gameObject
        );
    }

    // =========================================================
    // DIE
    // =========================================================

    private void Die()
    {
        if (isDying)
            return;

        isDying = true;

        if (usesSharedHealth &&
            sharedHealthGroup != null)
        {
            sharedHealthGroup.Unregister(
                this
            );
        }

        if (!isIllusion)
        {
            bool shouldGiveGold =
                !hasReincarnation ||
                hasReincarnated;

            if (shouldGiveGold)
            {
                GiveGoldReward();
            }
        }

        StartCoroutine(
            DeathRoutine()
        );
    }

    // =========================================================
    // GIVE GOLD
    // =========================================================

    private void GiveGoldReward()
    {
        if (isIllusion)
            return;

        if (goldAlreadyGiven)
            return;

        if (goldReward <= 0)
            return;

        goldAlreadyGiven = true;

        if (spawnerManager == null)
        {
            spawnerManager =
                FindAnyObjectByType<EnemySpawner>();
        }

        if (spawnerManager == null)
        {
            return;
        }

        spawnerManager.AddGold(
            goldReward
        );
    }

    // =========================================================
    // DEATH ROUTINE
    // =========================================================

    private IEnumerator DeathRoutine()
    {
        if (animator == null)
        {
            animator =
                GetComponentInChildren<Animator>();
        }

        if (animator != null &&
            animator.runtimeAnimatorController != null)
        {
            animator.ResetTrigger(
                "Death"
            );

            animator.SetBool(
                "IsAttacking",
                false
            );

            animator.SetTrigger(
                "Death"
            );
        }

        float animationLength =
            GetDeathAnimationLength();

        if (animationLength > 0f)
        {
            yield return new WaitForSeconds(
                animationLength
            );
        }
        else
        {
            yield return new WaitForSeconds(
                1.5f
            );
        }

        if (!isIllusion &&
            hasReincarnation &&
            !hasReincarnated)
        {
            CreateReincarnationParticle();

            yield return new WaitForSeconds(
                reincarnationDelay
            );

            DestroyReincarnationParticle();

            Reincarnate();

            yield break;
        }

        yield return new WaitForSeconds(
            deathExtraDelay
        );

        FinalDestroy();
    }

    // =========================================================
    // DEATH ANIMATION LENGTH
    // =========================================================

    private float GetDeathAnimationLength()
    {
        if (animator == null)
            return 0f;

        RuntimeAnimatorController controller =
            animator.runtimeAnimatorController;

        if (controller == null)
            return 0f;

        AnimationClip[] clips =
            controller.animationClips;

        if (clips == null)
            return 0f;

        foreach (AnimationClip clip in clips)
        {
            if (clip == null)
                continue;

            if (clip.name.Equals(
                "Death",
                StringComparison.OrdinalIgnoreCase
            ))
            {
                return clip.length;
            }
        }

        return 0f;
    }

    // =========================================================
    // REINCARNATE
    // =========================================================

    private void Reincarnate()
    {
        if (hasReincarnated)
            return;

        hasReincarnated = true;

        isDying = false;

        currentHealth =
            maxHealth *
            (reincarnationHealthPercent / 100f);

        currentHealth =
            Mathf.Clamp(
                currentHealth,
                1f,
                maxHealth
            );

        lastHitWasCritical = false;

        if (hasTimedShield)
        {
            InitializeTimedShield();
        }

        if (animator != null &&
            animator.runtimeAnimatorController != null)
        {
            animator.ResetTrigger(
                "Death"
            );

            animator.SetBool(
                "IsAttacking",
                false
            );
        }

        UpdateHealthBar();

        UpdateShieldBar();

        OnHealthChanged?.Invoke(
            currentHealth,
            maxHealth
        );

        EnemyMovement movement =
            GetComponent<EnemyMovement>();

        if (movement != null)
        {
            movement.ResumeAfterReincarnation();
        }
    }

    // =========================================================
    // REINCARNATION PARTICLE
    // =========================================================

    private void CreateReincarnationParticle()
    {
        if (reincarnationParticlePrefab == null)
            return;

        if (activeReincarnationParticle != null)
            return;

        Renderer[] allRenderers =
            GetComponentsInChildren<Renderer>(
                true
            );

        List<Renderer> modelRenderers =
            new List<Renderer>();

        foreach (Renderer renderer in allRenderers)
        {
            if (renderer == null)
                continue;

            if (renderer.GetComponentInParent<
                EnemyProjectileSlowAura
            >() != null)
            {
                continue;
            }

            if (renderer.GetComponentInParent<
                LightningChain
            >() != null)
            {
                continue;
            }

            if (renderer.transform.root !=
                transform.root &&
                renderer.gameObject.name.Contains(
                    "Shield"
                ))
            {
                continue;
            }

            modelRenderers.Add(
                renderer
            );
        }

        if (modelRenderers.Count == 0)
            return;

        Bounds enemyBounds =
            modelRenderers[0].bounds;

        for (
            int i = 1;
            i < modelRenderers.Count;
            i++
        )
        {
            enemyBounds.Encapsulate(
                modelRenderers[i].bounds
            );
        }

        activeReincarnationParticle =
            Instantiate(
                reincarnationParticlePrefab,
                transform
            );

        Vector3 localCenter =
            transform.InverseTransformPoint(
                enemyBounds.center
            );

        localCenter.y = 0f;

        activeReincarnationParticle
            .transform
            .localPosition =
            localCenter;

        activeReincarnationParticle
            .transform
            .localRotation =
            Quaternion.identity;

        Renderer[] particleRenderers =
            activeReincarnationParticle
                .GetComponentsInChildren<Renderer>();

        if (particleRenderers == null ||
            particleRenderers.Length == 0)
        {
            return;
        }

        Bounds particleBounds =
            particleRenderers[0].bounds;

        for (
            int i = 1;
            i < particleRenderers.Length;
            i++
        )
        {
            if (particleRenderers[i] != null)
            {
                particleBounds.Encapsulate(
                    particleRenderers[i].bounds
                );
            }
        }

        Vector3 enemySize =
            enemyBounds.size;

        Vector3 particleSize =
            particleBounds.size;

        if (particleSize.x <= 0.001f ||
            particleSize.y <= 0.001f ||
            particleSize.z <= 0.001f)
        {
            return;
        }

        float scaleX =
            enemySize.x /
            particleSize.x;

        float scaleY =
            enemySize.y /
            particleSize.y;

        float scaleZ =
            enemySize.z /
            particleSize.z;

        activeReincarnationParticle
            .transform
            .localScale =
            new Vector3(
                scaleX,
                scaleY,
                scaleZ
            ) *
            reincarnationParticleScaleMultiplier;
    }

    // =========================================================
    // DESTROY REINCARNATION PARTICLE
    // =========================================================

    private void DestroyReincarnationParticle()
    {
        if (activeReincarnationParticle != null)
        {
            Destroy(
                activeReincarnationParticle
            );

            activeReincarnationParticle = null;
        }
    }

    // =========================================================
    // REGISTER KILL
    // =========================================================

    private void RegisterKillForStats()
    {
        if (killAlreadyRegistered)
            return;

        if (isIllusion)
            return;

        killAlreadyRegistered = true;

        if (GameStatsManager.Instance != null)
        {
            GameStatsManager.Instance.AddEnemyKilled();
        }
    }

    // =========================================================
    // FINAL DESTROY
    // =========================================================

    private void FinalDestroy()
    {
        DestroyReincarnationParticle();

        if (!isIllusion)
        {
            RegisterKillForStats();
        }

        if (myHealthBar != null)
        {
            Destroy(
                myHealthBar
            );

            myHealthBar = null;
        }

        if (activeTimedShieldEffect != null)
        {
            Destroy(
                activeTimedShieldEffect
            );

            activeTimedShieldEffect = null;
        }

        Destroy(
            gameObject
        );
    }
}