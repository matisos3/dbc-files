using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GameStatsManager : MonoBehaviour
{
    // =========================================================
    // SINGLETON
    // =========================================================

    public static GameStatsManager Instance { get; private set; }


    // =========================================================
    // STATYSTYKI GRY
    // =========================================================

    private int enemiesKilled = 0;

    private int damageDealt = 0;

    private int goldEarned = 0;

    private int skillsPurchased = 0;

    private int upgradesPurchased = 0;


    // =========================================================
    // CZAS GRY
    // =========================================================

    private float gameStartTime = 0f;

    private bool gameStarted = false;

    private bool gameFinished = false;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }


    private void Start()
    {
        StartGameTimer();
    }


    private void Update()
    {
        if (!gameStarted)
            return;

        if (gameFinished)
            return;
    }


    // =========================================================
    // START GRY
    // =========================================================

    public void StartGameTimer()
    {
        gameStartTime =
            Time.realtimeSinceStartup;

        gameStarted = true;

        gameFinished = false;
    }


    // =========================================================
    // KONIEC GRY
    // =========================================================

    public void FinishGame()
    {
        if (!gameStarted)
            return;

        gameFinished = true;
    }


    // =========================================================
    // PRZECIWNICY
    // =========================================================

    public void AddEnemyKilled()
    {
        if (enemiesKilled < int.MaxValue)
        {
            enemiesKilled++;
        }

        if (enemiesKilled < 0)
        {
            enemiesKilled = 0;
        }
    }


    public int GetEnemiesKilled()
    {
        return enemiesKilled;
    }


    // =========================================================
    // OBRAŻENIA
    // =========================================================

    public void AddDamageDealt(float damage)
    {
        if (damage <= 0f)
            return;

        if (float.IsNaN(damage) ||
            float.IsInfinity(damage))
        {
            return;
        }

        int roundedDamage =
            Mathf.RoundToInt(damage);

        if (roundedDamage <= 0)
            return;


        if (
            damageDealt >
            int.MaxValue - roundedDamage)
        {
            damageDealt =
                int.MaxValue;

            return;
        }


        damageDealt +=
            roundedDamage;


        if (damageDealt < 0)
        {
            damageDealt = 0;
        }
    }


    public int GetDamageDealt()
    {
        return damageDealt;
    }


    // =========================================================
    // ZŁOTO
    // =========================================================

    public void AddGoldEarned(int amount)
    {
        if (amount <= 0)
            return;


        if (
            goldEarned >
            int.MaxValue - amount)
        {
            goldEarned =
                int.MaxValue;

            return;
        }


        goldEarned +=
            amount;


        if (goldEarned < 0)
        {
            goldEarned = 0;
        }
    }


    public int GetGoldEarned()
    {
        return goldEarned;
    }


    // =========================================================
    // ZAKUPIONE SKILLE
    // =========================================================

    public void AddSkillPurchased()
    {
        if (skillsPurchased < int.MaxValue)
        {
            skillsPurchased++;
        }

        if (skillsPurchased < 0)
        {
            skillsPurchased = 0;
        }
    }


    public int GetSkillsPurchased()
    {
        return skillsPurchased;
    }


    // =========================================================
    // ZAKUPIONE ULEPSZENIA
    // =========================================================

    public void AddUpgradePurchased()
    {
        if (upgradesPurchased < int.MaxValue)
        {
            upgradesPurchased++;
        }

        if (upgradesPurchased < 0)
        {
            upgradesPurchased = 0;
        }
    }


    public int GetUpgradesPurchased()
    {
        return upgradesPurchased;
    }


    // =========================================================
    // CZAS GRY
    // =========================================================

    public int GetGameDuration()
    {
        if (!gameStarted)
            return 0;


        float elapsed =
            Time.realtimeSinceStartup -
            gameStartTime;


        if (elapsed <= 0f)
            return 0;


        if (
            float.IsNaN(elapsed) ||
            float.IsInfinity(elapsed))
        {
            return 0;
        }


        if (elapsed >= int.MaxValue)
        {
            return int.MaxValue;
        }


        return Mathf.RoundToInt(
            elapsed
        );
    }


    // =========================================================
    // AKTUALNA FALA
    // =========================================================

    public int GetCurrentWave()
    {
        WaveManager waveManager =
            FindAnyObjectByType<WaveManager>();


        if (waveManager == null)
            return 0;


        return waveManager.CurrentWave;
    }


    // =========================================================
    // BUILD — SKILLE
    // =========================================================

    /// <summary>
    /// Zwraca wszystkie aktualnie posiadane skille
    /// w formacie:
    ///
    /// skillID:level,skillID:level,skillID:level
    ///
    /// Skill jest identyfikowany wyłącznie przez
    /// SkillData.skillID.
    /// </summary>
    public string GetPurchasedSkillsSummary()
    {
        Dictionary<string, int> skillLevels =
            new Dictionary<string, int>(
                StringComparer.Ordinal
            );


        Tower[] towers =
            FindObjectsByType<Tower>();


        foreach (Tower tower in towers)
        {
            if (tower == null)
                continue;


            if (tower.activeSkills == null)
                continue;


            foreach (
                SkillInstance skill
                in tower.activeSkills)
            {
                if (skill == null)
                    continue;


                if (skill.data == null)
                    continue;


                string skillID =
                    skill.data.skillID;


                if (string.IsNullOrEmpty(skillID))
                    continue;


                int level =
                    Mathf.Max(
                        1,
                        skill.level
                    );


                if (
                    !skillLevels.TryGetValue(
                        skillID,
                        out int existingLevel))
                {
                    skillLevels.Add(
                        skillID,
                        level
                    );
                }
                else
                {
                    // Jeżeli ten sam skill znajduje się
                    // na więcej niż jednej wieży,
                    // zapisujemy najwyższy poziom.
                    if (level > existingLevel)
                    {
                        skillLevels[skillID] =
                            level;
                    }
                }
            }
        }


        if (skillLevels.Count == 0)
            return string.Empty;


        List<string> skillIDs =
            new List<string>(
                skillLevels.Keys
            );


        skillIDs.Sort(
            StringComparer.Ordinal
        );


        StringBuilder result =
            new StringBuilder();


        for (
            int i = 0;
            i < skillIDs.Count;
            i++)
        {
            if (i > 0)
            {
                result.Append(",");
            }


            string skillID =
                skillIDs[i];


            result.Append(skillID);

            result.Append(":");

            result.Append(
                skillLevels[skillID]
            );
        }


        return result.ToString();
    }


    // =========================================================
    // BUILD — ULEPSZENIA
    // =========================================================

    /// <summary>
    /// Zwraca wszystkie aktualnie posiadane ulepszenia
    /// w formacie:
    ///
    /// upgradeID:level,upgradeID:level,upgradeID:level
    ///
    /// Ulepszenie jest identyfikowane wyłącznie przez
    /// SkillUpgradeData.upgradeID.
    /// </summary>
    public string GetPurchasedUpgradesSummary()
    {
        SkillUpgradeManager upgradeManager =
            FindAnyObjectByType<SkillUpgradeManager>();


        if (upgradeManager == null)
            return string.Empty;


        IReadOnlyList<
            SkillUpgradeManager.UpgradeLevel
        > upgradeLevels =
            upgradeManager.UpgradeLevels;


        if (upgradeLevels == null)
            return string.Empty;


        Dictionary<string, int> upgradeLevelMap =
            new Dictionary<string, int>(
                StringComparer.Ordinal
            );


        foreach (
            SkillUpgradeManager.UpgradeLevel entry
            in upgradeLevels)
        {
            if (entry == null)
                continue;


            if (entry.upgrade == null)
                continue;


            string upgradeID =
                entry.upgrade.upgradeID;


            if (string.IsNullOrEmpty(upgradeID))
                continue;


            int level =
                Mathf.Max(
                    1,
                    entry.level
                );


            if (
                !upgradeLevelMap.TryGetValue(
                    upgradeID,
                    out int existingLevel))
            {
                upgradeLevelMap.Add(
                    upgradeID,
                    level
                );
            }
            else
            {
                // Zabezpieczenie na wypadek,
                // gdyby to samo ulepszenie wystąpiło
                // więcej niż raz na liście.
                if (level > existingLevel)
                {
                    upgradeLevelMap[upgradeID] =
                        level;
                }
            }
        }


        if (upgradeLevelMap.Count == 0)
            return string.Empty;


        List<string> upgradeIDs =
            new List<string>(
                upgradeLevelMap.Keys
            );


        upgradeIDs.Sort(
            StringComparer.Ordinal
        );


        StringBuilder result =
            new StringBuilder();


        for (
            int i = 0;
            i < upgradeIDs.Count;
            i++)
        {
            if (i > 0)
            {
                result.Append(",");
            }


            string upgradeID =
                upgradeIDs[i];


            result.Append(upgradeID);

            result.Append(":");

            result.Append(
                upgradeLevelMap[upgradeID]
            );
        }


        return result.ToString();
    }


    // =========================================================
    // PEŁNY BUILD
    // =========================================================

    /// <summary>
    /// Zwraca cały build gracza w jednym tekście.
    /// </summary>
    public string GetBuildSummary()
    {
        string skills =
            GetPurchasedSkillsSummary();


        string upgrades =
            GetPurchasedUpgradesSummary();


        StringBuilder result =
            new StringBuilder();


        result.Append("Skills=");

        result.Append(
            string.IsNullOrEmpty(skills)
                ? "-"
                : skills
        );


        result.Append(";");


        result.Append("Upgrades=");

        result.Append(
            string.IsNullOrEmpty(upgrades)
                ? "-"
                : upgrades
        );


        return result.ToString();
    }


    // =========================================================
    // RESET STATYSTYK
    // =========================================================

    public void ResetStats()
    {
        enemiesKilled = 0;

        damageDealt = 0;

        goldEarned = 0;

        skillsPurchased = 0;

        upgradesPurchased = 0;


        gameStartTime =
            Time.realtimeSinceStartup;


        gameStarted = true;

        gameFinished = false;
    }
}