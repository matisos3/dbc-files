using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Localization;

public class GameOverUI : MonoBehaviour
{
    [Header("Panel Game Over")]
    [SerializeField] private GameObject gameOverPanel;


    [Header("Statystyki")]
    [SerializeField] private TMP_Text enemiesKilledText;
    [SerializeField] private TMP_Text damageDealtText;
    [SerializeField] private TMP_Text waveText;


    [Header("Localization - nazwy statystyk")]
    [SerializeField] private LocalizedString enemiesKilledLabel;
    [SerializeField] private LocalizedString damageDealtLabel;
    [SerializeField] private LocalizedString waveLabel;


    // =========================================================
    // XP
    // =========================================================

    [Header("XP")]
    [SerializeField] private TMP_Text playerLevelText;
    [SerializeField] private TMP_Text xpGainedText;
    [SerializeField] private TMP_Text xpProgressText;

    [SerializeField] private Image xpBarFill;

    [SerializeField] private TMP_Text levelUpText;


    [Header("XP - Localization")]
    [SerializeField] private LocalizedString playerLevelLabel;
    [SerializeField] private LocalizedString xpGainedLabel;


    [Header("XP - Animacja")]
    [SerializeField] private float xpAnimationDuration = 1.0f;
    [SerializeField] private float levelUpPauseDuration = 0.7f;


    [Header("Przyciski")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;


    [Header("Scena Menu")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";


    private bool gameOverShown = false;

    private Coroutine xpAnimationCoroutine;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);


        if (restartButton != null)
        {
            restartButton.onClick.AddListener(
                RestartGame
            );
        }


        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(
                GoToMainMenu
            );
        }


        // -----------------------------------------------------
        // LEVEL UP TEXT
        // -----------------------------------------------------

        if (levelUpText != null)
        {
            levelUpText.gameObject.SetActive(false);
        }


        // -----------------------------------------------------
        // XP BAR
        // -----------------------------------------------------

        if (xpBarFill != null)
        {
            xpBarFill.fillAmount = 0f;
        }
    }


    // =========================================================
    // SHOW GAME OVER
    // =========================================================

    public void ShowGameOver()
    {
        if (gameOverShown)
            return;


        gameOverShown = true;


        UpdateStatistics();


        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);


        Time.timeScale = 0f;
    }


    // =========================================================
    // AKTUALIZACJA STATYSTYK
    // =========================================================

    private void UpdateStatistics()
    {
        if (GameStatsManager.Instance == null)
        {
            return;
        }


        // =====================================================
        // POBRANIE STATYSTYK AKTUALNEJ GRY
        // =====================================================

        int enemiesKilled =
            GameStatsManager.Instance.GetEnemiesKilled();


        int damageDealt =
            GameStatsManager.Instance.GetDamageDealt();


        int currentWave =
            Mathf.Max(
                0,
                GameStatsManager.Instance.GetCurrentWave() - 1
            );


        int goldEarned =
            GameStatsManager.Instance.GetGoldEarned();


        int gameDuration =
            GameStatsManager.Instance.GetGameDuration();


        int skillsPurchased =
            GameStatsManager.Instance.GetSkillsPurchased();


        int upgradesPurchased =
            GameStatsManager.Instance.GetUpgradesPurchased();


        // =====================================================
        // PRZECIWNICY POKONANI
        // =====================================================

        if (enemiesKilledText != null)
        {
            string label =
                GetLocalizedText(
                    enemiesKilledLabel,
                    "Przeciwnicy pokonani"
                );


            enemiesKilledText.text =
                label + ": " +
                enemiesKilled;
        }


        // =====================================================
        // OBRAŻENIA ZADANE
        // =====================================================

        if (damageDealtText != null)
        {
            string label =
                GetLocalizedText(
                    damageDealtLabel,
                    "Obrażenia zadane"
                );


            damageDealtText.text =
                label + ": " +
                FormatDamage(damageDealt);
        }


        // =====================================================
        // FALA
        // =====================================================

        if (waveText != null)
        {
            string label =
                GetLocalizedText(
                    waveLabel,
                    "Fala"
                );


            waveText.text =
                label + ": " +
                currentWave;
        }


        // =====================================================
        // PLAYFAB
        // =====================================================

        if (PlayFabManager.Instance != null)
        {
            PlayFabManager.Instance.SaveGameOverStats(
                enemiesKilled,
                damageDealt,
                currentWave,
                goldEarned,
                gameDuration,
                skillsPurchased,
                upgradesPurchased
            );


            // -------------------------------------------------
            // XP
            //
            // 1 obrażenie = 1 XP
            // -------------------------------------------------

            PlayFabManager.Instance.AddGameOverXP(
                damageDealt,
                OnXPProcessed
            );
        }
    }


    // =========================================================
    // XP — PRZETWORZENIE
    // =========================================================

    private void OnXPProcessed(
        PlayFabManager.XPResult result)
    {
        if (result == null)
            return;


        if (!result.success)
            return;


        // -----------------------------------------------------
        // ZDOBYTE XP
        // -----------------------------------------------------

        if (xpGainedText != null)
        {
            string label =
                GetLocalizedText(
                    xpGainedLabel,
                    "Zdobyte XP"
                );


            xpGainedText.text =
                label + ": +" +
                FormatXP(result.xpGained);
        }


        // -----------------------------------------------------
        // ZATRZYMANIE POPRZEDNIEJ ANIMACJI
        // -----------------------------------------------------

        if (xpAnimationCoroutine != null)
        {
            StopCoroutine(xpAnimationCoroutine);
        }


        // -----------------------------------------------------
        // START ANIMACJI
        // -----------------------------------------------------

        xpAnimationCoroutine =
            StartCoroutine(
                AnimateXP(result)
            );
    }


    // =========================================================
    // ANIMACJA XP
    // =========================================================

    private IEnumerator AnimateXP(
        PlayFabManager.XPResult result)
    {
        if (PlayFabManager.Instance == null)
            yield break;


        int currentLevel =
            result.oldLevel;


        int maximumLevel =
            PlayFabManager.Instance.GetMaximumLevel();


        double startXP =
            result.oldXP;


        double targetXP =
            result.newXP;


        // -----------------------------------------------------
        // ZABEZPIECZENIE
        // -----------------------------------------------------

        if (targetXP < startXP)
        {
            targetXP = startXP;
        }


        // -----------------------------------------------------
        // POCZĄTKOWY POZIOM
        // -----------------------------------------------------

        UpdateLevelText(currentLevel);


        // -----------------------------------------------------
        // MAX LEVEL
        // -----------------------------------------------------

        if (currentLevel >= maximumLevel)
        {
            if (xpBarFill != null)
                xpBarFill.fillAmount = 1f;


            if (xpProgressText != null)
                xpProgressText.text = "MAX";


            yield break;
        }


        // -----------------------------------------------------
        // POCZĄTKOWY POSTĘP
        // -----------------------------------------------------

        UpdateXPBar(
            startXP,
            currentLevel
        );


        // -----------------------------------------------------
        // BRAK ZDOBYTEGO XP
        // -----------------------------------------------------

        if (Math.Abs(targetXP - startXP) < 0.001)
        {
            UpdateXPBar(
                targetXP,
                currentLevel
            );

            yield break;
        }


        // =====================================================
        // ANIMACJA POZIOM PO POZIOMIE
        // =====================================================

        double animatedXP =
            startXP;


        while (
            animatedXP < targetXP &&
            currentLevel < maximumLevel)
        {
            // -------------------------------------------------
            // GRANICA NASTĘPNEGO POZIOMU
            // -------------------------------------------------

            double nextLevelXP =
                GetTotalXPRequiredForLevel(
                    currentLevel + 1
                );


            // -------------------------------------------------
            // XP DO GRANICY
            // -------------------------------------------------

            double xpAtStart =
                animatedXP;


            double xpAtEnd =
                Math.Min(
                    targetXP,
                    nextLevelXP
                );


            // -------------------------------------------------
            // ANIMOWANIE TEGO FRAGMENTU
            // -------------------------------------------------

            float elapsed = 0f;


            while (
                elapsed < xpAnimationDuration &&
                animatedXP < xpAtEnd)
            {
                elapsed +=
                    Time.unscaledDeltaTime;


                float t =
                    Mathf.Clamp01(
                        elapsed /
                        Mathf.Max(
                            0.01f,
                            xpAnimationDuration
                        )
                    );


                // SmoothStep
                float smoothT =
                    t * t *
                    (3f - 2f * t);


                animatedXP =
                    Mathf.Lerp(
                        (float)xpAtStart,
                        (float)xpAtEnd,
                        smoothT
                    );


                // -------------------------------------------------
                // POPRAWKA DLA DOUBLE
                // -------------------------------------------------

                if (animatedXP > xpAtEnd)
                    animatedXP = xpAtEnd;


                UpdateXPBar(
                    animatedXP,
                    currentLevel
                );


                yield return null;
            }


            // -------------------------------------------------
            // DOKŁADNE USTAWIENIE KOŃCA
            // -------------------------------------------------

            animatedXP =
                xpAtEnd;


            UpdateXPBar(
                animatedXP,
                currentLevel
            );


            // -------------------------------------------------
            // CZY OSIĄGNIĘTO NOWY POZIOM?
            // -------------------------------------------------

            if (
                animatedXP >= nextLevelXP &&
                currentLevel < maximumLevel)
            {
                currentLevel++;


                // ---------------------------------------------
                // LEVEL UP
                // ---------------------------------------------

                UpdateLevelText(
                    currentLevel
                );


                if (xpBarFill != null)
                {
                    xpBarFill.fillAmount = 1f;
                }


                if (xpProgressText != null)
                {
                    xpProgressText.text =
                        FormatXP(
                            PlayFabManager.Instance
                                .GetXPRequiredForNextLevel()
                        ) +
                        " / " +
                        FormatXP(
                            PlayFabManager.Instance
                                .GetXPRequiredForNextLevel()
                        );
                }


                // ---------------------------------------------
                // EFEKT LEVEL UP
                // ---------------------------------------------

                if (levelUpText != null)
                {
                    levelUpText.gameObject.SetActive(true);
                }


                yield return new WaitForSecondsRealtime(
                    levelUpPauseDuration
                );


                if (levelUpText != null)
                {
                    levelUpText.gameObject.SetActive(false);
                }


                // ---------------------------------------------
                // NOWY POZIOM — PASEK OD ZERA
                // ---------------------------------------------

                if (xpBarFill != null)
                {
                    xpBarFill.fillAmount = 0f;
                }


                if (xpProgressText != null)
                {
                    xpProgressText.text =
                        "0 / " +
                        FormatXP(
                            PlayFabManager.Instance
                                .GetXPRequiredForLevel(
                                    currentLevel
                                )
                        );
                }


                // -------------------------------------------------
                // WAŻNE:
                // animatedXP pozostaje na aktualnej granicy.
                // Kolejna iteracja zaczyna od tego XP.
                // -------------------------------------------------
            }
        }


        // =====================================================
        // KOŃCOWY STAN
        // =====================================================

        UpdateLevelText(
            result.newLevel
        );


        if (
            result.newLevel >=
            maximumLevel)
        {
            if (xpBarFill != null)
                xpBarFill.fillAmount = 1f;


            if (xpProgressText != null)
                xpProgressText.text = "MAX";
        }
        else
        {
            UpdateXPBar(
                targetXP,
                result.newLevel
            );
        }


        xpAnimationCoroutine = null;
    }


    // =========================================================
    // AKTUALIZACJA POZIOMU
    // =========================================================

    private void UpdateLevelText(
        int level)
    {
        if (playerLevelText == null)
            return;


        string label =
            GetLocalizedText(
                playerLevelLabel,
                "Poziom"
            );


        playerLevelText.text =
            label + ": " +
            level;
    }


    // =========================================================
    // AKTUALIZACJA PASKA XP
    // =========================================================

    private void UpdateXPBar(
        double totalXP,
        int level)
    {
        if (PlayFabManager.Instance == null)
            return;


        int maximumLevel =
            PlayFabManager.Instance.GetMaximumLevel();


        // -----------------------------------------------------
        // MAX LEVEL
        // -----------------------------------------------------

        if (level >= maximumLevel)
        {
            if (xpBarFill != null)
                xpBarFill.fillAmount = 1f;


            if (xpProgressText != null)
                xpProgressText.text = "MAX";


            return;
        }


        // -----------------------------------------------------
        // XP NA POCZĄTKU AKTUALNEGO POZIOMU
        // -----------------------------------------------------

        double levelStartXP =
            GetTotalXPRequiredForLevel(level);


        double levelEndXP =
            GetTotalXPRequiredForLevel(level + 1);


        double xpIntoLevel =
            totalXP - levelStartXP;


        double xpRequired =
            levelEndXP - levelStartXP;


        xpIntoLevel =
            Math.Max(
                0.0,
                Math.Min(
                    xpIntoLevel,
                    xpRequired
                )
            );


        // -----------------------------------------------------
        // FILL
        // -----------------------------------------------------

        float fill =
            xpRequired > 0.0
                ? (float)(
                    xpIntoLevel /
                    xpRequired
                )
                : 0f;


        if (xpBarFill != null)
        {
            xpBarFill.fillAmount =
                Mathf.Clamp01(fill);
        }


        // -----------------------------------------------------
        // TEKST
        // -----------------------------------------------------

        if (xpProgressText != null)
        {
            xpProgressText.text =
                FormatXP(xpIntoLevel) +
                " / " +
                FormatXP(xpRequired);
        }
    }


    // =========================================================
    // CAŁKOWITE XP POTRZEBNE DO OSIĄGNIĘCIA POZIOMU
    // =========================================================
    //
    // Poziom 1 = 0 XP
    //
    // Poziom 2 = 1000 XP
    // Poziom 3 = 2100 XP
    // Poziom 4 = 3310 XP
    // itd.
    //
    // Zgodne z systemem PlayFabManager:
    //
    // baseXPToLevel2 = 1000
    // xpMultiplier   = 1.1
    //
    // =========================================================

    private double GetTotalXPRequiredForLevel(
        int level)
    {
        if (PlayFabManager.Instance == null)
            return 0.0;


        if (level <= 1)
            return 0.0;


        double baseXP =
            PlayFabManager.Instance
                .GetBaseXPToLevel2();


        double multiplier =
            PlayFabManager.Instance
                .GetXPMultiplier();


        double totalXP = 0.0;


        for (int currentLevel = 1;
             currentLevel < level;
             currentLevel++)
        {
            double xpForThisLevel =
                baseXP *
                Math.Pow(
                    multiplier,
                    currentLevel - 1
                );


            totalXP +=
                xpForThisLevel;
        }


        return totalXP;
    }


    // =========================================================
    // LOKALIZACJA
    // =========================================================

    private string GetLocalizedText(
        LocalizedString localizedString,
        string fallback)
    {
        if (
            localizedString == null ||
            localizedString.IsEmpty)
        {
            return fallback;
        }


        string result =
            localizedString.GetLocalizedString();


        if (string.IsNullOrEmpty(result))
            return fallback;


        return result;
    }


    // =========================================================
    // FORMATOWANIE OBRAŻEŃ
    // =========================================================

    private string FormatDamage(
        int damage)
    {
        if (damage < 1000)
        {
            return damage.ToString();
        }


        if (damage < 1000000)
        {
            return (damage / 1000f)
                .ToString("0.##") +
                "K";
        }


        if (damage < 1000000000)
        {
            return (damage / 1000000f)
                .ToString("0.##") +
                "M";
        }


        return (damage / 1000000000f)
            .ToString("0.##") +
            "B";
    }


    // =========================================================
    // FORMATOWANIE XP
    // =========================================================

    private string FormatXP(
        double xp)
    {
        if (
            double.IsNaN(xp) ||
            double.IsInfinity(xp))
        {
            return "0";
        }


        if (xp < 1000.0)
        {
            return xp.ToString("0");
        }


        if (xp < 1000000.0)
        {
            return (xp / 1000.0)
                .ToString("0.##") +
                "K";
        }


        if (xp < 1000000000.0)
        {
            return (xp / 1000000.0)
                .ToString("0.##") +
                "M";
        }


        return (xp / 1000000000.0)
            .ToString("0.##") +
            "B";
    }


    // =========================================================
    // RESTART
    // =========================================================

    public void RestartGame()
    {
        Time.timeScale = 1f;


        if (xpAnimationCoroutine != null)
        {
            StopCoroutine(
                xpAnimationCoroutine
            );

            xpAnimationCoroutine = null;
        }


        if (GameStatsManager.Instance != null)
        {
            GameStatsManager.Instance.ResetStats();
        }


        string currentScene =
            SceneManager.GetActiveScene().name;


        SceneManager.LoadScene(
            currentScene
        );
    }


    // =========================================================
    // MAIN MENU
    // =========================================================

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;


        if (xpAnimationCoroutine != null)
        {
            StopCoroutine(
                xpAnimationCoroutine
            );

            xpAnimationCoroutine = null;
        }


        if (GameStatsManager.Instance != null)
        {
            GameStatsManager.Instance.ResetStats();
        }


        if (
            string.IsNullOrEmpty(
                mainMenuSceneName))
        {
            return;
        }


        SceneManager.LoadScene(
            mainMenuSceneName
        );
    }


    // =========================================================
    // ON DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (xpAnimationCoroutine != null)
        {
            StopCoroutine(
                xpAnimationCoroutine
            );

            xpAnimationCoroutine = null;
        }


        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(
                RestartGame
            );
        }


        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(
                GoToMainMenu
            );
        }
    }
}