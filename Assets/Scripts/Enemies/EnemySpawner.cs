using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabry przeciwników")]
    public GameObject enemyPrefab;
    public GameObject championPrefab;
    public GameObject elitePrefab;
    public GameObject bossPrefab;
    public GameObject enemyHealthBarCanvasPrefab;

    [Header("Health Bar")]
    [SerializeField] private float healthBarHeight = 2f;
    [SerializeField] private float healthBarSideOffset = 0.1f;

    [Header("Gold Popup")]
    [SerializeField] private GameObject goldRewardPopupPrefab;
    [SerializeField] private Canvas worldCanvas;


    // =========================================================
    // SPAWN
    // =========================================================

    [Header("Spawn")]
    public float spawnRadius = 15f;

    private float timeBetweenSpawns = 3f;

    [Tooltip("Czas spawnu na początku gry / Wave 1.")]
    [SerializeField] private float spawnTimeAtWave1 = 3f;

    [Tooltip("Minimalny czas pomiędzy spawnami. Nigdy nie zostanie przekroczony w dół.")]
    [SerializeField] private float minimumSpawnTime = 2.5f;

    [Tooltip("Fala, na której zostanie osiągnięty minimalny czas spawnu.")]
    [SerializeField] private int spawnTimeMinimumWave = 50;

    private int currentWave = 0;

    // =========================================================
    // NOWY TIMER SPAWNU
    // =========================================================

    // Absolutny moment następnego dozwolonego spawnu.
    // Dzięki temu SetCurrentWave() nie może przypadkowo
    // wywołać dwóch spawnów jeden po drugim.
    private float nextSpawnTime = 0f;

    // Czy wykonano już pierwszy spawn.
    private bool hasSpawnedAtLeastOnce = false;


    // =========================================================
    // CHAMPION
    // =========================================================

    [Header("Champion")]
    [SerializeField] private int championMinCount = 2;
    [SerializeField] private int championMaxCount = 4;
    [SerializeField] private float championSpawnSpacing = 1.5f;


    // =========================================================
    // CEL PRZECIWNIKÓW
    // =========================================================

    [Header("Cel przeciwników")]
    public Transform targetTower;


    // =========================================================
    // EKONOMIA
    // =========================================================

    [Header("Ekonomia Gry")]
    public TextMeshProUGUI goldText;
    public int startingGold = 100;

    public TextMeshProUGUI refreshPointsText;
    public int startingRefreshPoints = 1;

    [SerializeField]
    private GameObject refreshSkillsButton;


    // =========================================================
    // TRUDNOŚĆ FAL
    // =========================================================

    private float currentHealthMultiplier = 1f;
    private float currentDamageMultiplier = 1f;


    // =========================================================
    // BAZOWE STATYSTYKI ZWYKŁEGO PRZECIWNIKA
    // =========================================================

    private float baseEnemyHealth = 1f;
    private float baseEnemyDamage = 0f;

    private bool baseStatsCached = false;


    // =========================================================
    // AFFIXY
    // =========================================================

    [Header("Affix System")]

    [Tooltip("Minimalna liczba affiksów dla Elite/Bossa.")]
    [SerializeField] private int eliteMinAffixes = 2;

    [Tooltip("Maksymalna liczba affiksów dla Elite.")]
    [SerializeField] private int eliteMaxAffixes = 2;

    [Tooltip("Minimalna liczba affiksów dla Bossa.")]
    [SerializeField] private int bossMinAffixes = 3;

    [Tooltip("Maksymalna liczba affiksów dla Bossa.")]
    [SerializeField] private int bossMaxAffixes = 3;

    [Tooltip("Minimalna liczba affiksów dla Championa.")]
    [SerializeField] private int championMinAffixes = 1;

    [Tooltip("Maksymalna liczba affiksów dla Championa.")]
    [SerializeField] private int championMaxAffixes = 1;

    [Header("Dostępne Affixy")]

    [SerializeField]
    private List<EnemyAffixes.AffixType> availableAffixes =
        new List<EnemyAffixes.AffixType>
        {
            EnemyAffixes.AffixType.ExtraAttackSpeed,
            EnemyAffixes.AffixType.DamageAura,
            EnemyAffixes.AffixType.DamageReduction,
            EnemyAffixes.AffixType.Dodge,
            EnemyAffixes.AffixType.Enrage,
            EnemyAffixes.AffixType.ExtraDamage,
            EnemyAffixes.AffixType.ExtraHealth,
            EnemyAffixes.AffixType.ExtraMoveSpeed,
            EnemyAffixes.AffixType.Illusion,
            EnemyAffixes.AffixType.Reincarnation,
            EnemyAffixes.AffixType.SharedHealth,
            EnemyAffixes.AffixType.SlowProjectileAura,
            EnemyAffixes.AffixType.TimedShield,
            EnemyAffixes.AffixType.Regeneration
        };


    // =========================================================
    // POZOSTAŁE
    // =========================================================

    private int currentGold;
    private int currentRefreshPoints;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        currentGold = startingGold;

        UpdateGoldUI();

        currentRefreshPoints = startingRefreshPoints;

        UpdateRefreshUI();


        spawnTimeMinimumWave =
            Mathf.Max(
                1,
                spawnTimeMinimumWave
            );

        minimumSpawnTime =
            Mathf.Max(
                0.01f,
                minimumSpawnTime
            );

        spawnTimeAtWave1 =
            Mathf.Max(
                minimumSpawnTime,
                spawnTimeAtWave1
            );


        CacheBaseStatsFromPrefab();

        UpdateSpawnTime();


        // Na początku nie ustawiamy od razu czasu następnego
        // spawnu. Pierwszy spawn może nastąpić natychmiast
        // po ustawieniu pierwszej aktywnej fali.
        nextSpawnTime = 0f;
        hasSpawnedAtLeastOnce = false;


        if (targetTower == null)
        {
            GameObject tower =
                GameObject.FindWithTag("Tower");

            if (tower != null)
            {
                targetTower =
                    tower.transform;
            }
        }
    }


    // =========================================================
    // POBRANIE BAZOWYCH STATYSTYK Z PREFABU
    // =========================================================

    private void CacheBaseStatsFromPrefab()
    {
        baseStatsCached = false;


        if (enemyPrefab == null)
        {
            baseEnemyHealth = 1f;
            baseEnemyDamage = 0f;

            return;
        }


        EnemyHealth enemyHealth =
            enemyPrefab.GetComponent<EnemyHealth>();


        if (enemyHealth == null)
        {
            baseEnemyHealth = 1f;
        }
        else
        {
            baseEnemyHealth =
                enemyHealth.maxHealth;


            if (float.IsNaN(baseEnemyHealth) ||
                float.IsInfinity(baseEnemyHealth) ||
                baseEnemyHealth <= 0f)
            {
                baseEnemyHealth = 1f;
            }
        }


        EnemyMovement enemyMovement =
            enemyPrefab.GetComponent<EnemyMovement>();


        if (enemyMovement == null)
        {
            baseEnemyDamage = 0f;
        }
        else
        {
            baseEnemyDamage =
                enemyMovement.damage;


            if (float.IsNaN(baseEnemyDamage) ||
                float.IsInfinity(baseEnemyDamage) ||
                baseEnemyDamage < 0f)
            {
                baseEnemyDamage = 0f;
            }
        }


        baseStatsCached = true;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (currentWave <= 0)
            return;


        // Co 10 fala jest falą specjalną.
        // Normalne spawnowanie jest wtedy wyłączone.
        if (currentWave % 10 == 0)
            return;


        // -----------------------------------------------------
        // PIERWSZY SPAWN
        // -----------------------------------------------------

        if (!hasSpawnedAtLeastOnce)
        {
            SpawnEnemy();

            hasSpawnedAtLeastOnce = true;

            nextSpawnTime =
                Time.time +
                timeBetweenSpawns;

            return;
        }


        // -----------------------------------------------------
        // KOLEJNY SPAWN
        // -----------------------------------------------------

        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();

            // Liczymy kolejny spawn od momentu wykonania
            // aktualnego spawnu.
            //
            // Dzięki temu zawsze musi minąć pełny
            // timeBetweenSpawns.
            nextSpawnTime =
                Time.time +
                timeBetweenSpawns;
        }
    }


    // =========================================================
    // USTAWIENIE AKTUALNEJ FALI
    // =========================================================

    public void SetCurrentWave(int wave)
    {
        int previousWave =
            currentWave;


        currentWave =
            Mathf.Max(
                0,
                wave
            );


        UpdateSpawnTime();


        // -----------------------------------------------------
        // NIE RESETUJEMY TIMERA PRZY KAŻDYM SetCurrentWave()
        // -----------------------------------------------------
        //
        // To było źródłem problemu:
        //
        // countdown = 0f;
        //
        // Następny Update() natychmiast wykonywał SpawnEnemy().
        //
        // Teraz timer jest zachowywany.
        // -----------------------------------------------------


        // Jeśli właśnie uruchomiliśmy pierwszą aktywną falę,
        // pozwalamy na pierwszy spawn.
        if (currentWave > 0 &&
            currentWave % 10 != 0 &&
            !hasSpawnedAtLeastOnce)
        {
            nextSpawnTime = Time.time;
        }


        // Jeśli przechodzimy na nową normalną falę,
        // a poprzednia fala była falą specjalną,
        // upewniamy się, że nie nastąpi natychmiastowy
        // drugi spawn.
        if (previousWave != currentWave &&
            previousWave > 0 &&
            previousWave % 10 == 0 &&
            currentWave % 10 != 0)
        {
            nextSpawnTime =
                Mathf.Max(
                    nextSpawnTime,
                    Time.time + timeBetweenSpawns
                );
        }
    }


    // =========================================================
    // AKTUALIZACJA CZASU SPAWNU
    // =========================================================

    private void UpdateSpawnTime()
    {
        if (currentWave <= 1)
        {
            timeBetweenSpawns =
                spawnTimeAtWave1;
        }
        else if (currentWave <= spawnTimeMinimumWave)
        {
            float progress =
                (currentWave - 1f) /
                Mathf.Max(
                    1f,
                    spawnTimeMinimumWave - 1f
                );


            timeBetweenSpawns =
                Mathf.Lerp(
                    spawnTimeAtWave1,
                    minimumSpawnTime,
                    progress
                );
        }
        else
        {
            timeBetweenSpawns =
                minimumSpawnTime;
        }


        // Twarde zabezpieczenie.
        timeBetweenSpawns =
            Mathf.Max(
                minimumSpawnTime,
                timeBetweenSpawns
            );
    }


    // =========================================================
    // USTAWIENIE MNOŻNIKÓW TRUDNOŚCI
    // =========================================================

    public void SetDifficultyMultipliers(
        float healthMultiplier,
        float damageMultiplier)
    {
        currentHealthMultiplier =
            Mathf.Max(
                1f,
                healthMultiplier
            );

        currentDamageMultiplier =
            Mathf.Max(
                1f,
                damageMultiplier
            );
    }


    // =========================================================
    // LOSOWANIE TYPU PRZECIWNIKA
    // =========================================================

    private GameObject GetRandomEnemyPrefab()
    {
        if (currentWave < 10)
        {
            return enemyPrefab;
        }


        if (currentWave % 10 == 0)
        {
            return null;
        }


        float randomValue =
            Random.Range(
                0f,
                100f
            );


        if (randomValue < 96f)
        {
            return enemyPrefab;
        }


        if (randomValue < 99f)
        {
            return championPrefab;
        }


        return elitePrefab;
    }


    // =========================================================
    // GŁÓWNY SPAWN
    // =========================================================

    private void SpawnEnemy()
    {
        if (targetTower == null)
            return;


        GameObject prefabToSpawn =
            GetRandomEnemyPrefab();


        if (prefabToSpawn == null)
            return;


        if (prefabToSpawn == championPrefab)
        {
            SpawnChampionGroup();
            return;
        }


        SpawnSingleEnemy(
            prefabToSpawn,
            GetRandomSpawnPosition()
        );
    }


    // =========================================================
    // GRUPA CHAMPIONÓW
    // =========================================================

    private void SpawnChampionGroup()
    {
        if (championPrefab == null)
            return;


        int count =
            Random.Range(
                championMinCount,
                championMaxCount + 1
            );


        Vector3 centerPosition =
            GetRandomSpawnPosition();


        List<EnemyAffixes.AffixType> groupAffixes =
            GenerateAffixes(
                EnemyType.EnemyRank.Champion
            );


        SharedHealthGroup sharedGroup =
            new GameObject(
                "ChampionSharedHealthGroup"
            ).AddComponent<SharedHealthGroup>();


        for (int i = 0;
             i < count;
             i++)
        {
            Vector2 randomOffset =
                Random.insideUnitCircle *
                championSpawnSpacing;


            Vector3 spawnPosition =
                centerPosition +
                new Vector3(
                    randomOffset.x,
                    0f,
                    randomOffset.y
                );


            spawnPosition.y =
                centerPosition.y;


            GameObject champion =
                SpawnSingleEnemy(
                    championPrefab,
                    spawnPosition,
                    groupAffixes
                );


            if (champion == null)
                continue;


            EnemyHealth championHealth =
                champion.GetComponent<EnemyHealth>();


            if (championHealth != null)
            {
                championHealth.SetSharedHealthGroup(
                    sharedGroup
                );
            }
        }
    }


    // =========================================================
    // POJEDYNCZY PRZECIWNIK
    // =========================================================

    private GameObject SpawnSingleEnemy(
        GameObject prefab,
        Vector3 spawnPosition)
    {
        return SpawnSingleEnemy(
            prefab,
            spawnPosition,
            null
        );
    }


    // =========================================================
    // POJEDYNCZY PRZECIWNIK + AFFIXY
    // =========================================================

    private GameObject SpawnSingleEnemy(
        GameObject prefab,
        Vector3 spawnPosition,
        List<EnemyAffixes.AffixType> predefinedAffixes)
    {
        if (prefab == null)
            return null;


        if (targetTower == null)
            return null;


        Vector3 direction =
            targetTower.position -
            spawnPosition;


        direction.y = 0f;


        Quaternion rotation =
            Quaternion.identity;


        if (direction != Vector3.zero)
        {
            rotation =
                Quaternion.LookRotation(
                    direction
                );
        }


        GameObject enemy =
            Instantiate(
                prefab,
                spawnPosition,
                rotation
            );


        ApplyEnemyScale(enemy);


        EnemyType enemyType =
            enemy.GetComponent<EnemyType>();


        EnemyType.EnemyRank rank =
            EnemyType.EnemyRank.Normal;


        if (enemyType != null)
        {
            rank =
                enemyType.Rank;
        }


        if (rank == EnemyType.EnemyRank.Normal)
        {
            ClearEnemyAffixes(enemy);
        }
        else if (predefinedAffixes != null)
        {
            SetEnemyAffixes(
                enemy,
                predefinedAffixes
            );
        }
        else
        {
            List<EnemyAffixes.AffixType> randomAffixes =
                GenerateAffixes(rank);


            SetEnemyAffixes(
                enemy,
                randomAffixes
            );
        }


        SetupDodgeVisual(enemy);
        ApplyExtraHealthScale(enemy);

        ApplyWaveDifficulty(enemy);

        SetupHealthBar(enemy);


        return enemy;
    }


    // =========================================================
    // SKALOWANIE HP I OBRAŻEŃ PRZEZ FALĘ
    // =========================================================

    private void ApplyWaveDifficulty(GameObject enemy)
    {
        if (enemy == null)
            return;


        // =====================================================
        // HP
        // =====================================================

        EnemyHealth enemyHealth =
            enemy.GetComponent<EnemyHealth>();


        if (enemyHealth != null)
        {
            double calculatedHealth =
                (double)enemyHealth.maxHealth *
                (double)currentHealthMultiplier;


            if (double.IsNaN(calculatedHealth) ||
                double.IsInfinity(calculatedHealth))
            {
                calculatedHealth =
                    float.MaxValue;
            }


            calculatedHealth =
                System.Math.Max(
                    1.0,
                    calculatedHealth
                );


            calculatedHealth =
                System.Math.Min(
                    calculatedHealth,
                    float.MaxValue
                );


            enemyHealth.maxHealth =
                (float)calculatedHealth;
        }


        // =====================================================
        // DAMAGE
        // =====================================================

        EnemyMovement enemyMovement =
            enemy.GetComponent<EnemyMovement>();


        if (enemyMovement != null)
        {
            double calculatedDamage =
                (double)enemyMovement.damage *
                (double)currentDamageMultiplier;


            if (double.IsNaN(calculatedDamage) ||
                double.IsInfinity(calculatedDamage))
            {
                calculatedDamage =
                    int.MaxValue;
            }


            calculatedDamage =
                System.Math.Max(
                    0.0,
                    calculatedDamage
                );


            calculatedDamage =
                System.Math.Min(
                    calculatedDamage,
                    int.MaxValue
                );


            enemyMovement.damage =
                (int)System.Math.Round(
                    calculatedDamage
                );
        }
    }


    // =========================================================
    // GENEROWANIE AFFIXÓW
    // =========================================================

    private List<EnemyAffixes.AffixType> GenerateAffixes(
        EnemyType.EnemyRank rank)
    {
        List<EnemyAffixes.AffixType> result =
            new List<EnemyAffixes.AffixType>();


        if (availableAffixes == null ||
            availableAffixes.Count == 0)
        {
            return result;
        }


        int minCount = 0;
        int maxCount = 0;


        switch (rank)
        {
            case EnemyType.EnemyRank.Normal:

                minCount = 0;
                maxCount = 0;

                break;


            case EnemyType.EnemyRank.Champion:

                minCount =
                    championMinAffixes;

                maxCount =
                    championMaxAffixes;

                break;


            case EnemyType.EnemyRank.Elite:

                minCount =
                    eliteMinAffixes;

                maxCount =
                    eliteMaxAffixes;

                break;


            case EnemyType.EnemyRank.Boss:

                minCount =
                    bossMinAffixes;

                maxCount =
                    bossMaxAffixes;

                break;
        }


        minCount =
            Mathf.Clamp(
                minCount,
                0,
                availableAffixes.Count
            );


        maxCount =
            Mathf.Clamp(
                maxCount,
                minCount,
                availableAffixes.Count
            );


        if (maxCount <= 0)
            return result;


        int amount =
            Random.Range(
                minCount,
                maxCount + 1
            );


        List<EnemyAffixes.AffixType> pool =
            new List<EnemyAffixes.AffixType>(
                availableAffixes
            );


        if (rank != EnemyType.EnemyRank.Champion)
        {
            pool.Remove(
                EnemyAffixes.AffixType.SharedHealth
            );
        }


        while (
            result.Count < amount &&
            pool.Count > 0
        )
        {
            int index =
                Random.Range(
                    0,
                    pool.Count
                );


            EnemyAffixes.AffixType selected =
                pool[index];


            pool.RemoveAt(index);


            result.Add(
                selected
            );
        }


        return result;
    }


    // =========================================================
    // USTAWIANIE AFFIXÓW
    // =========================================================

    private void SetEnemyAffixes(
        GameObject enemy,
        List<EnemyAffixes.AffixType> affixesToSet)
    {
        if (enemy == null)
            return;


        EnemyAffixes enemyAffixes =
            enemy.GetComponent<EnemyAffixes>();


        if (enemyAffixes == null)
        {
            enemyAffixes =
                enemy.AddComponent<EnemyAffixes>();
        }


        enemyAffixes.ClearAffixes();


        if (affixesToSet == null)
            return;


        foreach (
            EnemyAffixes.AffixType affix
            in affixesToSet)
        {
            if (!enemyAffixes.Affixes.Contains(affix))
            {
                enemyAffixes.Affixes.Add(
                    affix
                );
            }
        }
    }


    // =========================================================
    // CZYSZCZENIE AFFIXÓW
    // =========================================================

    private void ClearEnemyAffixes(
        GameObject enemy)
    {
        if (enemy == null)
            return;


        EnemyAffixes enemyAffixes =
            enemy.GetComponent<EnemyAffixes>();


        if (enemyAffixes != null)
        {
            enemyAffixes.ClearAffixes();
        }
    }


    // =========================================================
    // DODGE VISUAL
    // =========================================================

    private void SetupDodgeVisual(
        GameObject enemy)
    {
        if (enemy == null)
            return;


        EnemyAffixes affixes =
            enemy.GetComponent<EnemyAffixes>();


        if (affixes == null)
            return;


        if (!affixes.HasAffix(
            EnemyAffixes.AffixType.Dodge))
        {
            return;
        }


        if (enemy.GetComponent<EnemyDodgeVisual>() == null)
        {
            enemy.AddComponent<EnemyDodgeVisual>();
        }
    }


    // =========================================================
    // SKALA PRZECIWNIKA
    // =========================================================

    private void ApplyEnemyScale(
        GameObject enemy)
    {
        if (enemy == null)
            return;


        EnemyType enemyType =
            enemy.GetComponent<EnemyType>();


        float scale = 1f;


        if (enemyType != null)
        {
            switch (enemyType.Rank)
            {
                case EnemyType.EnemyRank.Normal:

                    scale = 1f;

                    break;


                case EnemyType.EnemyRank.Champion:

                    scale = 1.15f;

                    break;


                case EnemyType.EnemyRank.Elite:

                    scale = 1.25f;

                    break;


                case EnemyType.EnemyRank.Boss:

                    scale = 1.5f;

                    break;
            }
        }


        enemy.transform.localScale =
            Vector3.one * scale;
    }


    // =========================================================
    // EXTRA HEALTH SCALE
    // =========================================================

    private void ApplyExtraHealthScale(
        GameObject enemy)
    {
        if (enemy == null)
            return;


        EnemyAffixes affixes =
            enemy.GetComponent<EnemyAffixes>();


        if (affixes == null)
            return;


        if (!affixes.HasAffix(
            EnemyAffixes.AffixType.ExtraHealth))
        {
            return;
        }


        enemy.transform.localScale *=
            1.2f;
    }


    // =========================================================
    // SPAWN ILUSJI
    // =========================================================

    public GameObject SpawnIllusion(
        GameObject originalEnemy,
        float illusionHealth)
    {
        if (originalEnemy == null)
            return null;


        if (targetTower == null)
        {
            GameObject tower =
                GameObject.FindWithTag("Tower");


            if (tower != null)
            {
                targetTower =
                    tower.transform;
            }
        }


        if (targetTower == null)
            return null;


        EnemyType originalEnemyType =
            originalEnemy.GetComponent<EnemyType>();


        GameObject illusionPrefab =
            enemyPrefab;


        if (originalEnemyType != null)
        {
            switch (originalEnemyType.Rank)
            {
                case EnemyType.EnemyRank.Normal:

                    break;


                case EnemyType.EnemyRank.Champion:

                    illusionPrefab =
                        championPrefab;

                    break;


                case EnemyType.EnemyRank.Elite:

                    illusionPrefab =
                        elitePrefab;

                    break;


                case EnemyType.EnemyRank.Boss:

                    illusionPrefab =
                        bossPrefab;

                    break;
            }
        }


        if (illusionPrefab == null)
            return null;


        Vector3 spawnPosition =
            originalEnemy.transform.position;


        Vector2 offset =
            Random.insideUnitCircle * 2f;


        spawnPosition +=
            new Vector3(
                offset.x,
                0f,
                offset.y
            );


        spawnPosition.y =
            originalEnemy.transform.position.y;


        Vector3 direction =
            targetTower.position -
            spawnPosition;


        direction.y = 0f;


        Quaternion rotation =
            Quaternion.identity;


        if (direction != Vector3.zero)
        {
            rotation =
                Quaternion.LookRotation(
                    direction
                );
        }


        GameObject illusion =
            Instantiate(
                illusionPrefab,
                spawnPosition,
                rotation
            );


        ApplyEnemyScale(
            illusion
        );


        ClearEnemyAffixes(
            illusion
        );


        EnemyHealth illusionHealthComponent =
            illusion.GetComponent<EnemyHealth>();


        if (illusionHealthComponent != null)
        {
            illusionHealthComponent.InitializeIllusion(
                illusionHealth
            );
        }

        SetupHealthBar(
            illusion
        );


        return illusion;
    }


    // =========================================================
    // LOSOWA POZYCJA SPAWNU
    // =========================================================

    private Vector3 GetRandomSpawnPosition()
    {
        if (targetTower == null)
            return Vector3.zero;


        float angle =
            Random.Range(
                0f,
                Mathf.PI * 2f
            );


        Vector3 spawnPosition =
            targetTower.position +
            new Vector3(
                Mathf.Cos(angle) *
                spawnRadius,

                0.0f,

                Mathf.Sin(angle) *
                spawnRadius
            );


        return spawnPosition;
    }


    // =========================================================
    // HEALTH BAR
    // =========================================================

    private void SetupHealthBar(
        GameObject enemy)
    {
        if (enemy == null)
            return;


        if (enemyHealthBarCanvasPrefab == null)
        {
            return;
        }


        EnemyHealth enemyHealth =
            enemy.GetComponent<EnemyHealth>();


        if (enemyHealth == null)
        {
            return;
        }


        EnemyType enemyType =
            enemy.GetComponent<EnemyType>();


        EnemyType.EnemyRank rank =
            EnemyType.EnemyRank.Normal;


        if (enemyType != null)
        {
            rank =
                enemyType.Rank;
        }


        GameObject healthBar =
            Instantiate(
                enemyHealthBarCanvasPrefab,
                enemy.transform
            );


        PositionHealthBarAboveEnemy(
            enemy,
            healthBar
        );


        HealthBarLookAtCamera lookAtCamera =
            healthBar.GetComponent<
                HealthBarLookAtCamera
            >();


        if (lookAtCamera == null)
        {
            lookAtCamera =
                healthBar.AddComponent<
                    HealthBarLookAtCamera
                >();
        }


        EnemyHealthBarUI healthBarUI =
            healthBar.GetComponent<
                EnemyHealthBarUI
            >();


        if (healthBarUI == null)
        {
            Destroy(
                healthBar
            );


            return;
        }


        healthBarUI.Setup(
            rank
        );


        Image activeFill =
            healthBarUI.GetActiveFill();


        if (activeFill == null)
        {
            Destroy(
                healthBar
            );


            return;
        }


        activeFill.fillAmount =
            1f;


        enemyHealth.healthBarFill =
            activeFill;


        Image shieldFillImage =
            null;


        Transform background =
            activeFill.transform.parent;


        if (background != null)
        {
            Transform shieldFill =
                background.Find(
                    "ShieldFill"
                );


            if (shieldFill != null)
            {
                shieldFillImage =
                    shieldFill.GetComponent<
                        Image
                    >();
            }
        }


        if (shieldFillImage != null)
        {
            shieldFillImage.fillAmount =
                0f;


            enemyHealth.SetShieldBar(
                shieldFillImage
            );
        }


        enemyHealth.SetHealthBar(
            healthBar
        );


        StartCoroutine(
            SetupAffixUIAfterInitialization(
                enemy,
                healthBar
            )
        );
    }


    // =========================================================
    // AFFIX UI
    // =========================================================

    private IEnumerator SetupAffixUIAfterInitialization(
        GameObject enemy,
        GameObject healthBar)
    {
        if (enemy == null ||
            healthBar == null)
        {
            yield break;
        }


        yield return null;


        if (enemy == null ||
            healthBar == null)
        {
            yield break;
        }


        EnemyAffixes affixes =
            enemy.GetComponent<EnemyAffixes>();


        if (affixes == null)
        {
            yield break;
        }


        EnemyAffixUI affixUI =
            healthBar.GetComponent<
                EnemyAffixUI
            >();


        if (affixUI == null)
            yield break;


        affixUI.Setup(
            affixes
        );
    }


    // =========================================================
    // POZYCJONOWANIE HEALTH BAR
    // =========================================================

    private void PositionHealthBarAboveEnemy(
        GameObject enemy,
        GameObject healthBar)
    {
        if (enemy == null ||
            healthBar == null)
        {
            return;
        }


        Renderer[] allRenderers =
            enemy.GetComponentsInChildren<Renderer>(
                true
            );


        List<Renderer> modelRenderers =
            new List<Renderer>();


        foreach (Renderer renderer in allRenderers)
        {
            if (renderer == null)
                continue;


            if (renderer.GetComponent<ParticleSystemRenderer>() != null)
                continue;


            if (renderer.GetComponentInParent<EnemyProjectileSlowAura>() != null)
                continue;


            modelRenderers.Add(
                renderer
            );
        }


        if (modelRenderers.Count == 0)
        {
            healthBar.transform.position =
                enemy.transform.position +
                Vector3.up *
                healthBarHeight;


            healthBar.transform.rotation =
                Quaternion.identity;


            return;
        }


        Bounds enemyBounds =
            modelRenderers[0].bounds;


        for (int i = 1;
             i < modelRenderers.Count;
             i++)
        {
            Renderer renderer =
                modelRenderers[i];


            if (renderer == null)
                continue;


            enemyBounds.Encapsulate(
                renderer.bounds
            );
        }


        Vector3 worldCenter =
            enemyBounds.center;


        Vector3 worldTop =
            new Vector3(
                enemyBounds.center.x,
                enemyBounds.max.y,
                enemyBounds.center.z
            );


        Vector3 finalPosition =
            worldTop +
            Vector3.up *
            healthBarHeight;


        Camera mainCamera =
            Camera.main;


        if (mainCamera != null)
        {
            Vector3 directionToCamera =
                mainCamera.transform.position -
                worldCenter;


            directionToCamera.y = 0f;


            if (directionToCamera.sqrMagnitude >
                0.0001f)
            {
                directionToCamera.Normalize();


                finalPosition +=
                    directionToCamera *
                    healthBarSideOffset;
            }
        }


        healthBar.transform.position =
            finalPosition;


        healthBar.transform.rotation =
            Quaternion.identity;
    }


    // =========================================================
    // BOSS
    // =========================================================

    public void SpawnBoss()
    {
        if (bossPrefab == null ||
            targetTower == null)
        {
            return;
        }


        Vector3 spawnPosition =
            GetRandomSpawnPosition();


        Vector3 direction =
            targetTower.position -
            spawnPosition;


        direction.y = 0f;


        Quaternion rotation =
            Quaternion.identity;


        if (direction != Vector3.zero)
        {
            rotation =
                Quaternion.LookRotation(
                    direction
                );
        }


        GameObject boss =
            Instantiate(
                bossPrefab,
                spawnPosition,
                rotation
            );


        ApplyEnemyScale(
            boss
        );


        List<EnemyAffixes.AffixType> bossAffixes =
            GenerateAffixes(
                EnemyType.EnemyRank.Boss
            );


        SetEnemyAffixes(
            boss,
            bossAffixes
        );


        SetupDodgeVisual(boss);
        ApplyExtraHealthScale(boss);


        ApplyWaveDifficulty(
            boss
        );


        SetupHealthBar(
            boss
        );
    }


    // =========================================================
    // GOLD
    // =========================================================

    public void AddGold(
        int amount)
    {
        currentGold +=
            amount;


        // =====================================================
        // TELEMETRIA
        //
        // Liczymy tylko faktycznie zdobyte złoto.
        //
        // Nie liczymy wydawania złota.
        // Nie liczymy startingGold.
        // Nie liczymy popupu.
        // =====================================================

        if (amount > 0 &&
            GameStatsManager.Instance != null)
        {
            GameStatsManager.Instance.AddGoldEarned(
                amount
            );
        }


        UpdateGoldUI();
    }


    public bool TrySpendGold(
        int amount)
    {
        if (amount <= 0)
            return true;


        if (currentGold < amount)
            return false;


        currentGold -=
            amount;


        UpdateGoldUI();


        return true;
    }


    private void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text =
                currentGold.ToString();
        }
    }


    // =========================================================
    // REFRESH POINTS
    // =========================================================

    public void AddRefreshPoints(
        int amount)
    {
        currentRefreshPoints +=
            amount;

        UpdateRefreshUI();
    }


    public bool TrySpendRefreshPoints(
        int amount)
    {
        if (amount <= 0)
            return true;


        if (currentRefreshPoints < amount)
            return false;

        currentRefreshPoints -=
            amount;

        UpdateRefreshUI();


        return true;
    }


    private void UpdateRefreshUI()
    {
        if (refreshPointsText != null)
        {
            refreshPointsText.text =
                currentRefreshPoints.ToString();
        }


        if (currentRefreshPoints <= 0)
        {
            if (refreshSkillsButton != null)
                refreshSkillsButton.SetActive(false);
        }
        else
        {
            if (refreshSkillsButton != null)
                refreshSkillsButton.SetActive(true);
        }
    }


    public void ShowGoldReward(
        Vector3 worldPosition,
        int amount)
    {
        if (goldRewardPopupPrefab == null)
            return;


        if (worldCanvas == null)
            return;


        Camera mainCamera =
            Camera.main;


        if (mainCamera == null)
            return;


        Vector3 popupWorldPosition =
            worldPosition +
            Vector3.up *
            2f;


        Vector3 screenPosition =
            mainCamera.WorldToScreenPoint(
                popupWorldPosition
            );


        if (screenPosition.z < 0f)
            return;


        GameObject popup =
            Instantiate(
                goldRewardPopupPrefab,
                worldCanvas.transform
            );


        RectTransform popupRect =
            popup.GetComponent<RectTransform>();


        if (popupRect != null)
        {
            popupRect.position =
                screenPosition;
        }


        GoldRewardPopup popupScript =
            popup.GetComponent<GoldRewardPopup>();


        if (popupScript != null)
        {
            popupScript.Setup(
                amount
            );
        }
    }


    // =========================================================
    // INFORMACJE O AKTUALNEJ FALI
    // =========================================================

    public float CurrentSpawnTime
    {
        get
        {
            return timeBetweenSpawns;
        }
    }


    public float CurrentHealthMultiplier
    {
        get
        {
            return currentHealthMultiplier;
        }
    }


    public float CurrentDamageMultiplier
    {
        get
        {
            return currentDamageMultiplier;
        }
    }


    // =========================================================
    // AKTUALNE HP ZWYKŁEGO PRZECIWNIKA
    // =========================================================

    public int GetCurrentEnemyHealth()
    {
        if (!baseStatsCached)
            return 0;


        float finalHealth =
            baseEnemyHealth *
            currentHealthMultiplier;


        if (float.IsNaN(finalHealth) ||
            float.IsInfinity(finalHealth))
        {
            return 0;
        }


        return Mathf.Max(
            1,
            Mathf.RoundToInt(
                finalHealth
            )
        );
    }


    // =========================================================
    // AKTUALNE OBRAŻENIA ZWYKŁEGO PRZECIWNIKA
    // =========================================================

    public int GetCurrentEnemyDamage()
    {
        if (!baseStatsCached)
            return 0;


        float finalDamage =
            baseEnemyDamage *
            currentDamageMultiplier;


        if (float.IsNaN(finalDamage) ||
            float.IsInfinity(finalDamage))
        {
            return 0;
        }


        return Mathf.Max(
            0,
            Mathf.RoundToInt(
                finalDamage
            )
        );
    }
}