using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class WaveManager : MonoBehaviour
{
    [Header("Lokalizacja")]
    [SerializeField] private LocalizedString waveTextLocalization;

    [Header("UI")]
    public TMP_Text waveText;
    public Image waveFill;

    [Header("Progress odświeżenia skilli")]
    public SkillRefreshProgressBar skillRefreshProgressBar;


    [Header("Wave")]
    private EnemySpawner enemySpawner;

    public float waveDuration = 20f;

    private float timer;

    public int currentWave = 0;

// =========================================================
// INFORMACJE DLA UI FALI
// =========================================================

public int CurrentWave
{
    get
    {
        return currentWave;
    }
}


public float CurrentSpawnTime
{
    get
    {
        if (enemySpawner == null)
            return 0f;

        return enemySpawner.CurrentSpawnTime;
    }
}


public int CurrentEnemyHealth
{
    get
    {
        if (enemySpawner == null)
            return 0;

        return enemySpawner.GetCurrentEnemyHealth();
    }
}


public int CurrentEnemyDamage
{
    get
    {
        if (enemySpawner == null)
            return 0;

        return enemySpawner.GetCurrentEnemyDamage();
    }
}


    // =========================================================
    // TRUDNOŚĆ FAL
    // =========================================================

    [Header("Trudność fal")]

    [Tooltip("Mnożnik maksymalnego HP na każdą kolejną falę. 1.01 = +1% na falę.")]
    [SerializeField]
    private float healthMultiplierPerWave = 1.01f;

    [Tooltip("Mnożnik obrażeń na każdą kolejną falę. 1.01 = +1% na falę.")]
    [SerializeField]
    private float damageMultiplierPerWave = 1.01f;


    // =========================================================
    // SKILL SELECTION
    // =========================================================

    private SkillSelectionManager skillSelectionManager;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        timer = waveDuration;


        // =====================================================
        // ENEMY SPAWNER
        // =====================================================

        enemySpawner =
            FindAnyObjectByType<EnemySpawner>();


        // =====================================================
        // SKILL SELECTION
        // =====================================================

        skillSelectionManager =
            FindAnyObjectByType<SkillSelectionManager>();


        // =====================================================
        // USTAWIENIE FALI
        // =====================================================

        if (enemySpawner != null)
        {
            enemySpawner.SetCurrentWave(
                currentWave
            );

            ApplyDifficultyToSpawner();
        }


        // =====================================================
        // PROGRESS BAR
        // =====================================================

        if (skillRefreshProgressBar != null)
        {
            skillRefreshProgressBar.ResetProgress();
        }


        UpdateUI();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        timer -=
            Time.deltaTime;


        UpdateWaveFill();
        UpdateSkillRefreshProgress();


        if (timer <= 0f)
        {
            NextWave();
        }
    }


    // =========================================================
    // PROGRESS ODŚWIEŻENIA SKILLI
    // =========================================================

    private void UpdateSkillRefreshProgress()
    {
        if (skillRefreshProgressBar == null)
            return;


        float waveProgress = 0f;


        if (waveDuration > 0f)
        {
            waveProgress =
                1f -
                Mathf.Clamp01(
                    timer / waveDuration
                );
        }


        int waveInCycle =
            currentWave % 5;


        float totalProgress =
            (waveInCycle + waveProgress) / 5f;


        skillRefreshProgressBar.SetProgress(
            totalProgress
        );
    }


    // =========================================================
    // NASTĘPNA FALA
    // =========================================================

    private void NextWave()
    {
        currentWave++;

        timer = waveDuration;

        EnemySpawner spawner =
        FindAnyObjectByType<EnemySpawner>();


        // =====================================================
        // USTAW AKTUALNĄ FALĘ
        // =====================================================

        if (enemySpawner != null)
        {
            enemySpawner.SetCurrentWave(
                currentWave
            );

            ApplyDifficultyToSpawner();
        }


        // =====================================================
        // PROGRESS BAR ODŚWIEŻENIA SKILLI
        // =====================================================

        if (skillRefreshProgressBar != null)
        {
            int wavesSinceRefresh =
                currentWave % 5;


            skillRefreshProgressBar.SetProgress(
                wavesSinceRefresh
            );
        }


        // =====================================================
        // CO 5 FAL DODAJEMY REFRESH POINT
        // =====================================================

        if (currentWave % 5 == 0)
        {
            spawner.AddRefreshPoints(1);


            // =================================================
            // RESET PASKA
            // =================================================

            if (skillRefreshProgressBar != null)
            {
                skillRefreshProgressBar.ResetProgress();
            }
        }


        UpdateUI();


        // =====================================================
        // CO 10 FAL BOSS
        // =====================================================

        if (currentWave % 10 == 0)
        {
            if (enemySpawner != null)
            {
                enemySpawner.SpawnBoss();
            }
        }
    }


    // =========================================================
    // OBLICZENIE TRUDNOŚCI
    // =========================================================

    private void ApplyDifficultyToSpawner()
    {
        if (enemySpawner == null)
            return;


        // =====================================================
        // ZABEZPIECZENIE
        // =====================================================

        float safeHealthMultiplier =
            Mathf.Max(
                1f,
                healthMultiplierPerWave
            );


        float safeDamageMultiplier =
            Mathf.Max(
                1f,
                damageMultiplierPerWave
            );


        // =====================================================
        // FALA 0/1 = 1.0x
        //
        // FALA 2 = 1.01x
        // FALA 3 = 1.01²
        // itd.
        // =====================================================

        int multiplierWave =
            Mathf.Max(
                0,
                currentWave - 1
            );


        float healthMultiplier =
            Mathf.Pow(
                safeHealthMultiplier,
                multiplierWave
            );


        float damageMultiplier =
            Mathf.Pow(
                safeDamageMultiplier,
                multiplierWave
            );


        enemySpawner.SetDifficultyMultipliers(
            healthMultiplier,
            damageMultiplier
        );
    }


    // =========================================================
    // WAVE FILL
    // =========================================================

    private void UpdateWaveFill()
    {
        if (waveFill == null)
            return;


        if (waveDuration <= 0f)
        {
            waveFill.fillAmount = 0f;
            return;
        }


        waveFill.fillAmount =
            Mathf.Clamp01(
                timer /
                waveDuration
            );
    }


    // =========================================================
    // UI
    // =========================================================

private void OnEnable()
{
    waveTextLocalization.StringChanged += OnWaveTextChanged;
}

private void OnDisable()
{
    waveTextLocalization.StringChanged -= OnWaveTextChanged;
}

private void UpdateUI()
{
    if (waveText == null)
        return;

    waveTextLocalization.RefreshString();
}

private void OnWaveTextChanged(string localizedText)
{
    if (waveText == null)
        return;

    waveText.text = localizedText + " " + currentWave;
}
}