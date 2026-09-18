using UnityEngine;

public class EnemyAffixController : MonoBehaviour
{
    [Header("Wartości affiksów")]
    [SerializeField] private float extraHealthPercent = 50f;
    [SerializeField] private float extraDamagePercent = 50f;
    [SerializeField] private float extraMoveSpeedPercent = 50f;
    [SerializeField] private float extraAttackSpeedPercent = 30f;

    [SerializeField] private float damageReductionPercent = 25f;
    [SerializeField] private float dodgeChancePercent = 25f;
    [SerializeField] private float regenerationPercentPerSecond = 5f;

    [Header("Enrage")]
    [SerializeField] private float enrageHealthThresholdPercent = 30f;
    [SerializeField] private float enrageBonusPercent = 30f;

    private bool hasEnrage;
    private bool isEnraged;

    [Header("Damage Aura")]
    [SerializeField] private GameObject damageAuraVisualPrefab;

    [Header("Projectile Slow Aura")]
    [SerializeField] private GameObject projectileSlowAuraVisualPrefab;

    private EnemyAffixes affixes;
    private EnemyHealth health;
    private EnemyMovement movement;
    private EnemyType enemyType;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        affixes =
            GetComponent<EnemyAffixes>();

        health =
            GetComponent<EnemyHealth>();

        movement =
            GetComponent<EnemyMovement>();

        enemyType =
            GetComponent<EnemyType>();
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (health != null)
        {
            health.OnHealthChanged +=
                CheckEnrage;
        }

        // =====================================================
        // ETAP 1
        //
        // Najpierw nakładamy affixy zmieniające BAZOWE
        // statystyki przeciwnika.
        //
        // Muszą być wykonane PRZED InitializeHealth(),
        // ponieważ InitializeHealth() ustawia currentHealth
        // na aktualne maxHealth.
        // =====================================================

        ApplyPreInitializationAffixes();

        // =====================================================
        // ETAP 2
        //
        // Teraz inicjalizujemy HP.
        //
        // W tym momencie:
        //
        // ExtraHealth jest już uwzględnione,
        // więc currentHealth otrzyma prawidłowe maxHealth.
        //
        // InitializeHealth() może bezpiecznie wyzerować
        // wartości runtime:
        //
        // damage reduction
        // dodge
        // regeneration
        // itd.
        // =====================================================

        if (health != null)
        {
            health.InitializeHealth();
        }

        // =====================================================
        // ETAP 3
        //
        // Dopiero PO InitializeHealth() nakładamy affixy,
        // których wartości są resetowane podczas inicjalizacji.
        // =====================================================

        ApplyPostInitializationAffixes();

        // =====================================================
        // SHARED HEALTH
        //
        // Musi być wykonane po InitializeHealth().
        // =====================================================

        ApplySharedHealthRegistration();
    }

    // =========================================================
    // PRE-INITIALIZATION AFFIXES
    //
    // Affixy zmieniające bazowe statystyki.
    // =========================================================

    private void ApplyPreInitializationAffixes()
    {
        if (affixes == null)
            return;

        foreach (
            EnemyAffixes.AffixType affix
            in affixes.Affixes)
        {
            switch (affix)
            {
                case EnemyAffixes.AffixType.ExtraHealth:
                    ApplyExtraHealth();
                    break;

                case EnemyAffixes.AffixType.ExtraDamage:
                    ApplyExtraDamage();
                    break;

                case EnemyAffixes.AffixType.ExtraMoveSpeed:
                    ApplyExtraMoveSpeed();
                    break;

                case EnemyAffixes.AffixType.ExtraAttackSpeed:
                    ApplyExtraAttackSpeed();
                    break;
            }
        }
    }

    // =========================================================
    // POST-INITIALIZATION AFFIXES
    //
    // Affixy ustawiające wartości runtime.
    // =========================================================

    private void ApplyPostInitializationAffixes()
    {
        if (affixes == null)
            return;

        foreach (
            EnemyAffixes.AffixType affix
            in affixes.Affixes)
        {
            switch (affix)
            {
                case EnemyAffixes.AffixType.DamageAura:
                    ApplyDamageAura();
                    break;

                case EnemyAffixes.AffixType.Regeneration:
                    ApplyRegeneration();
                    break;

                case EnemyAffixes.AffixType.DamageReduction:
                    ApplyDamageReduction();
                    break;

                case EnemyAffixes.AffixType.Dodge:
                    ApplyDodge();
                    break;

                case EnemyAffixes.AffixType.SlowProjectileAura:
                    ApplySlowProjectileAura();
                    break;

                case EnemyAffixes.AffixType.TimedShield:
                    ApplyTimedShield();
                    break;

                case EnemyAffixes.AffixType.Reincarnation:
                    break;

                case EnemyAffixes.AffixType.Illusion:
                    break;

                case EnemyAffixes.AffixType.Enrage:
                    ApplyEnrage();
                    break;

                case EnemyAffixes.AffixType.SharedHealth:
                    break;
            }
        }
    }

    // =========================================================
    // EXTRA ATTACK SPEED
    // =========================================================

    private void ApplyExtraAttackSpeed()
    {
        if (movement == null)
            return;

        float multiplier =
            1f +
            extraAttackSpeedPercent / 100f;

        movement.SetExtraAttackSpeedMultiplier(
            multiplier
        );
    }

    // =========================================================
    // DAMAGE AURA
    // =========================================================

    private void ApplyDamageAura()
    {
        EnemyDamageAura existingAura =
            GetComponent<EnemyDamageAura>();

        if (existingAura == null)
        {
            existingAura =
                gameObject.AddComponent<
                    EnemyDamageAura>();
        }

        existingAura.SetVisualPrefab(
            damageAuraVisualPrefab
        );
    }

    // =========================================================
    // REGENERATION
    // =========================================================

    private void ApplyRegeneration()
    {
        if (health == null)
            return;

        health.SetRegeneration(
            regenerationPercentPerSecond
        );
    }

    // =========================================================
    // DAMAGE REDUCTION
    // =========================================================

    private void ApplyDamageReduction()
    {
        if (health == null)
            return;

        float multiplier =
            1f -
            damageReductionPercent / 100f;

        health.SetDamageTakenMultiplier(
            multiplier
        );
    }

    // =========================================================
    // SHARED HEALTH REGISTRATION
    // =========================================================

    private void ApplySharedHealthRegistration()
    {
        if (affixes == null ||
            health == null ||
            enemyType == null)
        {
            return;
        }

        // SharedHealth tylko dla Championów.
        if (enemyType.Rank !=
            EnemyType.EnemyRank.Champion)
        {
            return;
        }

        if (!affixes.HasAffix(
                EnemyAffixes.AffixType.SharedHealth))
        {
            return;
        }

        // Szukamy grupy utworzonej przez Spawner.
        SharedHealthGroup group =
            GetComponentInParent<
                SharedHealthGroup>();

        if (group == null)
        {
            return;
        }

        health.SetSharedHealthGroup(
            group
        );

        group.Register(
            health
        );
    }

    // =========================================================
    // DODGE
    // =========================================================

    private void ApplyDodge()
    {
        if (health == null)
            return;

        health.SetDodgeChance(
            dodgeChancePercent / 100f
        );
    }

    // =========================================================
    // PROJECTILE SLOW AURA
    // =========================================================

    private void ApplySlowProjectileAura()
    {
        EnemyProjectileSlowAura existingAura =
            GetComponent<
                EnemyProjectileSlowAura>();

        if (existingAura == null)
        {
            existingAura =
                gameObject.AddComponent<
                    EnemyProjectileSlowAura>();
        }

        existingAura.SetVisualPrefab(
            projectileSlowAuraVisualPrefab
        );
    }

    // =========================================================
    // TIMED SHIELD
    // =========================================================

    private void ApplyTimedShield()
    {
        if (health == null)
            return;

        health.EnableTimedShield();
    }

    // =========================================================
    // EXTRA HEALTH
    // =========================================================

    private void ApplyExtraHealth()
    {
        if (health == null)
            return;

        float multiplier =
            1f +
            extraHealthPercent / 100f;

        health.maxHealth *=
            multiplier;
    }

    // =========================================================
    // EXTRA DAMAGE
    // =========================================================

    private void ApplyExtraDamage()
    {
        if (movement == null)
            return;

        float multiplier =
            1f +
            extraDamagePercent / 100f;

        movement.damage =
            Mathf.RoundToInt(
                movement.damage *
                multiplier
            );
    }

    // =========================================================
    // EXTRA MOVE SPEED
    // =========================================================

    private void ApplyExtraMoveSpeed()
    {
        if (movement == null)
            return;

        float multiplier =
            1f +
            extraMoveSpeedPercent / 100f;

        movement.SetExtraMoveSpeedMultiplier(
            multiplier
        );
    }


    // =========================================================
    // ENRAGE
    // =========================================================

    private void ApplyEnrage()
    {
        hasEnrage = true;
    }

    // =========================================================
    // CHECK ENRAGE
    // =========================================================

    private void CheckEnrage(
        float currentHealth,
        float maxHealth)
    {
        if (!hasEnrage ||
            isEnraged ||
            maxHealth <= 0f)
        {
            return;
        }

        float healthPercent =
            currentHealth /
            maxHealth *
            100f;

        if (healthPercent <=
            enrageHealthThresholdPercent)
        {
            ActivateEnrage();
        }
    }

    // =========================================================
    // ACTIVATE ENRAGE
    // =========================================================

    private void ActivateEnrage()
    {
        if (isEnraged)
            return;

        isEnraged = true;

        float multiplier =
            1f +
            enrageBonusPercent / 100f;

        if (movement != null)
        {
            movement.SetDamageMultiplier(
                multiplier
            );

            movement.SetAttackSpeedMultiplier(
                multiplier
            );

            movement.SetMoveSpeedMultiplier(
                multiplier
            );
        }
    }
}