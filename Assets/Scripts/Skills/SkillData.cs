using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(
    fileName = "New Skill",
    menuName = "TowerDefense/Skill"
)]
public class SkillData : ScriptableObject
{
    // =========================================================
    // ID
    // =========================================================

    [Header("Identity")]

    [Tooltip("Stały identyfikator skilla. NIE tłumaczyć.")]
    public string skillID;


    // =========================================================
    // LOCALIZATION
    // =========================================================

    [Header("Localization")]

    public LocalizedString localizedName;

    public LocalizedString localizedDescription;


    // =========================================================
    // VISUAL
    // =========================================================

    [Header("Visual")]

    public Sprite icon;


    // =========================================================
    // COST
    // =========================================================

    [Header("Cena")]

    public int cost = 500;

    public int costIncreasePerLevel = 500;


    // =========================================================
    // BASIC ATTACK
    // =========================================================

    [Header("Atak skilla")]

    public GameObject projectilePrefab;

    public float damage = 10f;
    public float damageUpgrade = 2f;

    public float fireRate = 1f;
    public float fireRateUpgrade = 0.1f;

    public float range = 10f;
    public float rangeUpgrade = 0f;

    public float explosionRadius = 3f;
    public float explosionRadiusUpgrade = 0f;

    [Header("Projectile")]
    public int projectileCount = 1;
    public float projectileCountGrowth = 0f;

    [Header("Piercing")]
    public int piercingCount = 1;
    public int piercingCountUpgrade = 0;

    public DamageType damageType;

    [Header("Kategorie skilla")]
    public SkillCategory categories = SkillCategory.None;


    // =========================================================
    // CC
    // =========================================================

    [Header("CC")]

    public float rootDuration = 1f;
    public float rootDurationUpgrade = 0f;

    [Range(0f, 1f)]
    public float slowPercent = 0.2f;

    [Range(0f, 1f)]
    public float slowPercentUpgrade = 0.05f;

    public float slowDuration = 2f;
    public float slowDurationUpgrade = 0f;


    // =========================================================
    // CHAIN
    // =========================================================

    [Header("Chain")]

    public int chainCount = 2;
    public int chainCountUpgrade = 0;

    public float chainRange = 5f;
    public float chainRangeUpgrade = 0f;

    public float chainDamageMultiplier = 1f;
    public float chainDamageMultiplierUpgrade = 0f;

    public float chainForwardAngle = 90f;
    public float chainForwardAngleUpgrade = 0f;

    [Header("Chain Lightning")]

    public GameObject chainLightningEffectPrefab;

    public float chainLightningDelay = 0f;


    // =========================================================
    // LIGHTNING ARROW
    // =========================================================

    [Header("Lightning Arrow")]

    [Range(0f, 1f)]
    [Tooltip("Szansa na porażenie przeciwnika. 0.3 = 30%.")]
    public float lightningStunChance = 0f;

    [Tooltip("Czas ogłuszenia przeciwnika.")]
    public float lightningStunDuration = 0f;

    [Tooltip("Dodatkowe obrażenia zadawane po udanym porażeniu.")]
    public float lightningBonusDamage = 0f;


    // =========================================================
    // LIGHTNING ARROW UPGRADES
    // =========================================================

    [Header("Lightning Arrow Upgrade")]

    [Range(0f, 1f)]
    [Tooltip("Zwiększenie szansy na porażenie na poziom.")]
    public float lightningStunChanceUpgrade = 0f;

    [Tooltip("Zwiększenie czasu ogłuszenia na poziom.")]
    public float lightningStunDurationUpgrade = 0f;

    [Tooltip("Zwiększenie dodatkowych obrażeń na poziom.")]
    public float lightningBonusDamageUpgrade = 0f;


    // =========================================================
    // LIGHTNING STRIKE
    // =========================================================

    [Header("Lightning Strike")]

    [Tooltip("Prefab efektu pioruna pojawiającego się na przeciwniku.")]
    public GameObject lightningStrikeEffectPrefab;

    [Range(0f, 1f)]
    [Tooltip("Szansa na kolejne uderzenie pioruna. 0.5 = 50%.")]
    public float lightningStrikeChance = 0f;

    [Tooltip("Maksymalna liczba uderzeń w jednej serii.")]
    public int lightningStrikeMaxHits = 0;

    [Range(0f, 1f)]
    [Tooltip("Zmniejszenie obrażeń każdego kolejnego uderzenia. 0.3 = -30%.")]
    public float lightningStrikeDamageReduction = 0f;

    [Tooltip("Opóźnienie pomiędzy kolejnymi uderzeniami.")]
    public float lightningStrikeDelay = 0f;


    // =========================================================
    // LIGHTNING STRIKE UPGRADES
    // =========================================================

    [Header("Lightning Strike Upgrade")]

    [Range(0f, 1f)]
    [Tooltip("Zwiększenie szansy na kolejne uderzenie na poziom.")]
    public float lightningStrikeChanceUpgrade = 0f;

    [Tooltip("Zwiększenie maksymalnej liczby uderzeń na poziom.")]
    public int lightningStrikeMaxHitsUpgrade = 0;

    [Range(0f, 1f)]
    [Tooltip("Zmniejszenie redukcji obrażeń na poziom. Np. 0.05 = 5 punktów procentowych mniej redukcji.")]
    public float lightningStrikeDamageReductionUpgrade = 0f;

    [Tooltip("Zmniejszenie opóźnienia pomiędzy uderzeniami na poziom.")]
    public float lightningStrikeDelayUpgrade = 0f;


    // =========================================================
    // DOT
    // =========================================================

    [Header("DoT")]

    public float dotDamage = 15f;
    public float dotDamageUpgrade = 2f;

    public float dotDuration = 3f;
    public float dotDurationUpgrade = 0f;

    public float dotTickInterval = 0.5f;
    public float dotTickIntervalUpgrade = 0f;


    // =========================================================
    // SHOTGUN
    // =========================================================

    [Header("Shotgun")]

    public int shotgunProjectileCount = 5;
    public int projectileCountUpgrade = 0;

    public float spreadAngle = 30f;
    public float spreadAngleUpgrade = 0f;


    // =========================================================
    // CORRUPTED
    // =========================================================

    [Header("Corrupted Projectile")]

    public float corruptedSlowPercent = 0.1f;
    public float corruptedSlowPercentUpgrade = 0.02f;

    public float corruptedAttackSpeedReduction = 0.1f;
    public float corruptedAttackSpeedReductionUpgrade = 0.02f;

    public float corruptedDamageTakenIncrease = 0.1f;
    public float corruptedDamageTakenIncreaseUpgrade = 0.02f;

    public float corruptedDuration = 5f;
    public float corruptedDurationUpgrade = 0.5f;

    public int corruptedMaxStacks = 3;
    public int corruptedMaxStacksUpgrade = 0;

    public int corruptedMaxTargets = 3;
    public int corruptedMaxTargetsUpgrade = 0;

    public float corruptedChainRange = 5f;
    public float corruptedChainRangeUpgrade = 0.5f;


    // =========================================================
    // DEVOURING CLOUD
    // =========================================================

    [Header("Devouring Cloud")]

    public GameObject cloudEffectPrefab;

    public float cloudRadius = 3f;
    public float cloudRadiusUpgrade = 0.5f;

    public float cloudDuration = 5f;
    public float cloudDurationUpgrade = 0f;

    public float cloudTickInterval = 1f;
    public float cloudTickIntervalUpgrade = 0f;


    // =========================================================
    // WHIRL OF CHAOS
    // =========================================================

    [Header("Whirl of Chaos")]

    public GameObject whirlEffectPrefab;

    public float whirlRadius = 3f;
    public float whirlRadiusUpgrade = 0.5f;

    public float whirlDuration = 5f;
    public float whirlDurationUpgrade = 0f;

    public float whirlTickInterval = 1f;
    public float whirlTickIntervalUpgrade = 0f;

    public float whirlPullSpeed = 5f;
    public float whirlPullSpeedUpgrade = 0f;

    public float whirlDamage = 10f;
    public float whirlDamageUpgrade = 2f;


    // =========================================================
    // SLIME
    // =========================================================

    [Header("Slime")]

    public GameObject slimeProjectilePrefab;

    public int slimeCount = 3;

    public float slimeRadius = 2f;
    public float slimeRadiusUpgrade = 0.25f;

    [Range(0f, 1f)]
    public float slimeSlowPercent = 0.3f;

    [Range(0f, 1f)]
    public float slimeSlowPercentUpgrade = 0.05f;

    public float slimeDotDamage = 5f;
    public float slimeDotDamageUpgrade = 1f;

    public float slimeDotDuration = 5f;
    public float slimeDotDurationUpgrade = 0f;

    public float slimeDotTickInterval = 0.5f;
    public float slimeDotTickIntervalUpgrade = 0f;

    public float slimeSpreadAngle = 45f;
    public float slimeSpreadAngleUpgrade = 0f;


    // =========================================================
    // BLIZZARD
    // =========================================================

    [Header("Blizzard")]

    public GameObject blizzardEffectPrefab;

    public float blizzardDamage = 10f;
    public float blizzardDamageUpgrade = 2f;

    public float blizzardRadius = 4f;
    public float blizzardRadiusUpgrade = 0.5f;

    public float blizzardDuration = 6f;
    public float blizzardDurationUpgrade = 0f;

    public float blizzardTickInterval = 1f;
    public float blizzardTickIntervalUpgrade = 0f;

    [Range(0f, 1f)]
    public float blizzardSlowPercent = 0.2f;

    [Range(0f, 1f)]
    public float blizzardSlowPercentUpgrade = 0.05f;


    // =========================================================
    // ICE SPIKES
    // =========================================================

    [Header("Ice Spikes")]

    [Tooltip("Prefab głównego pocisku Ice Spikes.")]
    public GameObject iceSpikesProjectilePrefab;

    [Tooltip("Prefab pojedynczego odłamka Ice Spikes.")]
    public GameObject iceSpikeProjectilePrefab;

    [Tooltip("Liczba odłamków tworzonych po trafieniu.")]
    public int iceSpikeCount = 8;

    [Tooltip("Mnożnik obrażeń pojedynczego odłamka.")]
    public float iceSpikeDamageMultiplier = 0.5f;

    [Tooltip("Prędkość odłamków.")]
    public float iceSpikeSpeed = 10f;

    [Tooltip("Czas życia odłamków.")]
    public float iceSpikeLifetime = 3f;


    // =========================================================
    // ICE SPIKES UPGRADES
    // =========================================================

    [Header("Ice Spikes Upgrade")]

    public int iceSpikeCountUpgrade = 0;

    public float iceSpikeDamageMultiplierUpgrade = 0f;

    public float iceSpikeSpeedUpgrade = 0f;

    public float iceSpikeLifetimeUpgrade = 0f;


    // =========================================================
    // FREEZING WAVE
    // =========================================================

    [Header("Freezing Wave")]

    [Tooltip("Prefab fali Freezing Wave.")]
    public GameObject freezingWavePrefab;

    [Tooltip("Promień fali podczas sprawdzania trafień.")]
    public float freezingWaveRadius = 1.5f;

    [Tooltip("Prędkość poruszania się fali.")]
    public float freezingWaveSpeed = 12f;

    [Tooltip("Maksymalny dystans fali.")]
    public float freezingWaveDistance = 10f;

    [Tooltip("Czas zamrożenia przeciwnika.")]
    public float freezingWaveFreezeDuration = 3f;

    [Tooltip("Obrażenia zadawane przez falę.")]
    public float freezingWaveDamage = 20f;


    // =========================================================
    // FREEZING WAVE UPGRADES
    // =========================================================

    [Header("Freezing Wave Upgrade")]

    public float freezingWaveRadiusUpgrade = 0f;

    public float freezingWaveSpeedUpgrade = 0f;

    public float freezingWaveDistanceUpgrade = 0f;

    public float freezingWaveFreezeDurationUpgrade = 0.5f;

    public float freezingWaveDamageUpgrade = 2f;


    // =========================================================
    // FROST NOVA
    // =========================================================

    [Header("Frost Nova")]

    [Tooltip("Prefab efektu Frost Nova.")]
    public GameObject frostNovaEffectPrefab;

    [Tooltip("Promień działania Frost Nova wokół wieży.")]
    public float frostNovaRadius = 4f;

    [Tooltip("Obrażenia zadawane wszystkim przeciwnikom w promieniu.")]
    public float frostNovaDamage = 25f;

    [Tooltip("Czas zamrożenia przeciwników.")]
    public float frostNovaFreezeDuration = 3f;

    [Tooltip("Czas odnowienia Frost Nova.")]
    public float frostNovaCooldown = 5f;


    // =========================================================
    // FROST NOVA UPGRADES
    // =========================================================

    [Header("Frost Nova Upgrade")]

    [Tooltip("Zwiększenie promienia Frost Nova na poziom.")]
    public float frostNovaRadiusUpgrade = 0.5f;

    [Tooltip("Zwiększenie obrażeń Frost Nova na poziom.")]
    public float frostNovaDamageUpgrade = 3f;

    [Tooltip("Zwiększenie czasu zamrożenia na poziom.")]
    public float frostNovaFreezeDurationUpgrade = 0.5f;

    [Tooltip("Zmniejszenie cooldownu Frost Nova na poziom.")]
    public float frostNovaCooldownUpgrade = 0.25f;


    // =========================================================
    // ICE SHOT
    // =========================================================

    [Header("Ice Shot")]

    [Tooltip("Prefab pocisku Ice Shot.")]
    public GameObject iceShotProjectilePrefab;

    [Tooltip("Obrażenia Ice Shot.")]
    public float iceShotDamage = 30f;

    [Tooltip("Promień / rozmiar pocisku Ice Shot. Jest również używany jako AoE.")]
    public float iceShotRadius = 0f;

    [Tooltip("Procent spowolnienia przeciwników.")]
    public float iceShotSlowPercent = 20f;

    [Tooltip("Czas trwania spowolnienia.")]
    public float iceShotSlowDuration = 2f;


    // =========================================================
    // ICE SHOT UPGRADES
    // =========================================================

    [Header("Ice Shot Upgrade")]

    [Tooltip("Zwiększenie obrażeń Ice Shot na poziom.")]
    public float iceShotDamageUpgrade = 4f;

    [Tooltip("Zwiększenie promienia / rozmiaru Ice Shot na poziom.")]
    public float iceShotRadiusUpgrade = 0f;

    [Tooltip("Zwiększenie spowolnienia Ice Shot na poziom.")]
    public float iceShotSlowPercentUpgrade = 2f;

    [Tooltip("Zwiększenie czasu spowolnienia na poziom.")]
    public float iceShotSlowDurationUpgrade = 0.25f;


    // =========================================================
    // BREATH OF FIRE
    // =========================================================

    [Header("Breath of Fire")]

    [Tooltip("Prefab efektu ognia Breath of Fire.")]
    public GameObject breathOfFireEffectPrefab;

    [Tooltip("Maksymalny zasięg ognia od wieży.")]
    public float breathOfFireRange = 5f;

    [Tooltip("Szerokość stożka ognia w stopniach.")]
    public float breathOfFireAngle = 60f;

    [Tooltip("Obrażenia zadawane przeciwnikom znajdującym się w ogniu.")]
    public float breathOfFireDamage = 10f;

    [Tooltip("Co ile sekund przeciwnicy w ogniu otrzymują bezpośrednie obrażenia.")]
    public float breathOfFireTickInterval = 0.5f;

    [Tooltip("Obrażenia zadawane przez podpalenie.")]
    public float breathOfFireBurnDamage = 5f;

    [Tooltip("Jak długo utrzymuje się podpalenie po opuszczeniu ognia.")]
    public float breathOfFireBurnDuration = 3f;

    [Tooltip("Co ile sekund podpalenie zadaje obrażenia.")]
    public float breathOfFireBurnTickInterval = 0.5f;


    // =========================================================
    // BREATH OF FIRE UPGRADES
    // =========================================================

    [Header("Breath of Fire Upgrade")]

    public float breathOfFireRangeUpgrade = 0.5f;

    public float breathOfFireAngleUpgrade = 5f;

    public float breathOfFireDamageUpgrade = 2f;

    public float breathOfFireBurnDamageUpgrade = 1f;

    public float breathOfFireBurnDurationUpgrade = 0.25f;


    // =========================================================
    // FLAMING CIRCLE
    // =========================================================

    [Header("Flaming Circle")]

    public GameObject flamingCirclePrefab;

    public float flamingCircleDamage = 0f;
    public float flamingCircleRadius = 0f;
    public float flamingCircleDuration = 0f;
    public float flamingCircleTickInterval = 0f;


    // =========================================================
    // FLAMING CIRCLE UPGRADE
    // =========================================================

    [Header("Flaming Circle Upgrade")]

    public float flamingCircleDamageUpgrade = 0f;
    public float flamingCircleRadiusUpgrade = 0f;
    public float flamingCircleDurationUpgrade = 0f;


    // =========================================================
    // SEARING SHOT
    // =========================================================

    [Header("Searing Shot")]

    public GameObject searingShotProjectilePrefab;

    public float searingShotDamage = 20f;

    public float searingShotBurnDamage = 10f;

    public float searingShotBurnDuration = 5f;

    public float searingShotBurnTickInterval = 0.5f;

    public float searingShotExplosionDamage = 30f;

    public float searingShotExplosionRadius = 3f;


    // =========================================================
    // SEARING SHOT UPGRADE
    // =========================================================

    [Header("Searing Shot Upgrade")]

    public float searingShotDamageUpgrade = 3f;

    public float searingShotBurnDamageUpgrade = 2f;

    public float searingShotExplosionDamageUpgrade = 5f;

    public float searingShotExplosionRadiusUpgrade = 0.2f;


    // =========================================================
    // RAIN OF FIRE
    // =========================================================

    [Header("Rain Of Fire")]

    public GameObject rainOfFireEffectPrefab;

    public float rainOfFireRadius = 0f;
    public float rainOfFireRadiusUpgrade = 0f;

    public float rainOfFireDuration = 0f;
    public float rainOfFireDurationUpgrade = 0f;

    public float rainOfFireTickInterval = 0f;
    public float rainOfFireTickIntervalUpgrade = 0f;

    public float rainOfFireDamage = 0f;
    public float rainOfFireDamageUpgrade = 0f;

    public float rainOfFireBurnDamage = 0f;
    public float rainOfFireBurnDamageUpgrade = 0f;

    public float rainOfFireBurnDuration = 0f;
    public float rainOfFireBurnDurationUpgrade = 0f;

    public float rainOfFireBurnTickInterval = 0f;
    public float rainOfFireBurnTickIntervalUpgrade = 0f;


    // =========================================================
    // VOLCANIC SPHERE
    // =========================================================

    [Header("Volcanic Sphere")]

    public GameObject volcanicSphereProjectilePrefab;

    public int volcanicSphereCount = 0;

    public float volcanicSphereDamage = 0f;

    public float volcanicSphereSpeed = 0f;


    // =========================================================
    // VOLCANIC SPHERE UPGRADE
    // =========================================================

    [Header("Volcanic Sphere Upgrade")]

    public int volcanicSphereCountUpgrade = 0;

    public float volcanicSphereDamageUpgrade = 0f;

    public float volcanicSphereSpeedUpgrade = 0f;


// =========================================================
// ELECTRIC SHOCK
// =========================================================

[Header("Electric Shock")]

[Tooltip("Prefab pocisku Electric Shock.")]
public GameObject electricShockProjectilePrefab;

[Tooltip("Obrażenia zadawane każdemu trafionemu przeciwnikowi.")]
public float electricShockDamage = 0f;

[Tooltip("Czas ogłuszenia każdego trafionego przeciwnika.")]
public float electricShockStunDuration = 0f;

[Tooltip("Prędkość pocisku Electric Shock.")]
public float electricShockProjectileSpeed = 0f;

[Tooltip("Promień / rozmiar pocisku Electric Shock.")]
public float electricShockRadius = 0f;

// =========================================================
// ELECTRIC SHOCK UPGRADE
// =========================================================

[Header("Electric Shock Upgrade")]

[Tooltip("Zwiększenie obrażeń Electric Shock na poziom.")]
public float electricShockDamageUpgrade = 0f;

[Tooltip("Zwiększenie czasu ogłuszenia na poziom.")]
public float electricShockStunDurationUpgrade = 0f;

[Tooltip("Zwiększenie prędkości pocisku na poziom.")]
public float electricShockProjectileSpeedUpgrade = 0f;

[Tooltip("Zwiększenie promienia / rozmiaru Electric Shock na poziom.")]
public float electricShockRadiusUpgrade = 0f;


// =========================================================
// ENERGY ORB
// =========================================================

[Header("Energy Orb")]

[Tooltip("Prefab pocisku Energy Orb.")]
public GameObject energyOrbProjectilePrefab;

[Tooltip("Prędkość pocisku Energy Orb.")]
public float energyOrbProjectileSpeed = 0f;

[Tooltip("Promień działania po trafieniu pierwszego przeciwnika.")]
public float energyOrbHitRadius = 0f;

[Tooltip("Siła odrzutu przeciwników.")]
public float energyOrbKnockback = 0f;


// =========================================================
// ENERGY ORB UPGRADE
// =========================================================

[Header("Energy Orb Upgrade")]

[Tooltip("Zwiększenie prędkości pocisku Energy Orb na poziom.")]
public float energyOrbProjectileSpeedUpgrade = 0f;

[Tooltip("Zwiększenie promienia trafienia Energy Orb na poziom.")]
public float energyOrbHitRadiusUpgrade = 0f;

[Tooltip("Zwiększenie siły odrzutu Energy Orb na poziom.")]
public float energyOrbKnockbackUpgrade = 0f;


// =========================================================
// SPINNING LASER
// =========================================================

[Header("Spinning Laser")]

[Tooltip("Prefab pojedynczego ramienia Spinning Laser.")]
public GameObject spinningLaserPrefab;

[Tooltip("Liczba obracających się ramion.")]
public int spinningLaserCount = 0;

[Tooltip("Odległość ramion od środka wieży.")]
public float spinningLaserRadius = 0f;

[Tooltip("Prędkość obrotu ramion w stopniach na sekundę.")]
public float spinningLaserRotationSpeed = 0f;

[Tooltip("Obrażenia zadawane przeciwnikom przez laser.")]
public float spinningLaserDamage = 0f;

[Tooltip("Minimalny czas pomiędzy kolejnymi obrażeniami tego samego przeciwnika.")]
public float spinningLaserDamageInterval = 0f;

[Tooltip("Szerokość / grubość lasera. To jest parametr AoE.")]
public float spinningLaserWidth = 0f;


// =========================================================
// SPINNING LASER UPGRADE
// =========================================================

[Header("Spinning Laser Upgrade")]

[Tooltip("Zwiększenie liczby ramion na poziom.")]
public int spinningLaserCountUpgrade = 0;

[Tooltip("Zwiększenie odległości ramion od wieży na poziom.")]
public float spinningLaserRadiusUpgrade = 0f;

[Tooltip("Zwiększenie prędkości obrotu na poziom.")]
public float spinningLaserRotationSpeedUpgrade = 0f;

[Tooltip("Zwiększenie obrażeń na poziom.")]
public float spinningLaserDamageUpgrade = 0f;

[Tooltip("Zmniejszenie interwału obrażeń na poziom.")]
public float spinningLaserDamageIntervalUpgrade = 0f;

[Tooltip("Zwiększenie szerokości / grubości lasera na poziom.")]
public float spinningLaserWidthUpgrade = 0f;

    // =========================================================
    // VFX
    // =========================================================

    [Header("VFX")]

    public GameObject hitEffectPrefab;

    public GameObject explosionEffectPrefab;


    // =========================================================
    // DAMAGE TYPE
    // =========================================================

    public enum DamageType
    {
        Fire,
        Cold,
        Chaos,
        Lightning,
        Physical
    }
}
