using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerHealth : MonoBehaviour
{
    // =========================================================
    // PODSTAWOWE HP
    // =========================================================

    [Header("Ustawienia Wieży")]
    public float maxHealth = 500f;

    private float currentHealth;

    // Bazowe HP zapisane przed zastosowaniem ulepszeń.
    private float baseMaxHealth;

    // =========================================================
    // REGENERACJA
    // =========================================================

    [Header("Regeneracja")]
    [Tooltip("Bazowa regeneracja HP na sekundę.")]
    public float healthRegeneration = 0f;

    private float currentHealthRegeneration;

    // =========================================================
    // REDUKCJA OBRAŻEŃ
    // =========================================================

    [Header("Redukcja obrażeń")]
    [Tooltip("Stała redukcja każdego otrzymanego obrażenia.")]
    public float damageReductionFlat = 0f;

    [Tooltip("Procentowa redukcja otrzymywanych obrażeń.")]
    [Range(0f, 100f)]
    public float damageReductionPercent = 0f;

    private float currentDamageReductionFlat;
    private float currentDamageReductionPercent;

    // =========================================================
    // SKILL UPGRADE MANAGER
    // =========================================================

    [Header("Skill Upgrade Manager")]
    [Tooltip(
        "Manager ulepszeń może znajdować się na innym GameObject. " +
        "Jeżeli pole jest puste, zostanie znaleziony automatycznie."
    )]
    public SkillUpgradeManager skillUpgradeManager;

    // =========================================================
    // PASEK ZDROWIA
    // =========================================================

    [Header("Pasek zdrowia")]
    public GameObject healthBarPrefab;
    public Transform healthBarAnchor;

    private GameObject myHealthBar;
    private Image healthBarFill;
    private TMP_Text towerHealthText;

    // =========================================================
    // KOLORY PASKA
    // =========================================================

    [Header("Kolory paska")]
    public Color greenColor = Color.green;
    public Color yellowColor = Color.green;
    public Color orangeColor = new Color(1f, 0.5f, 0f);
    public Color redColor = Color.green;

    // =========================================================
    // ZNISZCZENIE WIEŻY
    // =========================================================

    [Header("Zniszczenie wieży")]
    [Tooltip("Prefab Particle System wybuchu wieży.")]
    [SerializeField] private GameObject towerExplosionPrefab;
    [SerializeField] private GameObject towerModel;

    [Tooltip(
        "Czas odtworzenia efektu wybuchu przed pokazaniem Game Over " +
        "i usunięciem wieży. Dopasuj do długości prefabrykatu TowerExplosion."
    )]
    [SerializeField] private float explosionDuration = 2f;

    // Czy wieża została zniszczona / rozpoczęła proces eksplozji.
    private bool isDestroyed = false;

    // Czy coroutine zniszczenia już została uruchomiona.
    private Coroutine destructionCoroutine;

    // =========================================================
    // GAME OVER UI
    // =========================================================

    [Header("Game Over")]
    [Tooltip(
        "GameOverUI odpowiedzialny za pokazanie ekranu wyników."
    )]
    [SerializeField] private GameOverUI gameOverUI;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Zapamiętujemy bazowe HP.
        baseMaxHealth = maxHealth;

        // Szukamy managera, jeżeli nie został przypisany ręcznie.
        if (skillUpgradeManager == null)
        {
            skillUpgradeManager =
                FindAnyObjectByType<SkillUpgradeManager>();
        }

        // Wartości bazowe.
        currentHealthRegeneration =
            healthRegeneration;

        currentDamageReductionFlat =
            damageReductionFlat;

        currentDamageReductionPercent =
            damageReductionPercent;

        // =====================================================
        // ULEPSZENIA
        // =====================================================

        RefreshHealthStats();

        // Wieża rozpoczyna z pełnym HP.
        currentHealth = maxHealth;

        // =====================================================
        // PASEK
        // =====================================================

        CreateHealthBar();

        // =====================================================
        // GAME OVER UI
        // =====================================================

        if (gameOverUI == null)
        {
            gameOverUI =
                FindAnyObjectByType<GameOverUI>();
        }
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // Po zniszczeniu wieży nie wykonujemy już regeneracji.
        if (isDestroyed)
            return;

        HandleRegeneration();
    }

    // =========================================================
    // PRZELICZANIE STATYSTYK
    // =========================================================

    public void RefreshHealthStats()
    {
        // Jeżeli bazowe HP nie zostało jeszcze zapisane,
        // zapamiętujemy aktualną wartość.
        if (baseMaxHealth <= 0f)
        {
            baseMaxHealth = maxHealth;
        }

        // Zapamiętujemy stare wartości.
        float oldMaxHealth = maxHealth;

        float healthPercent = 1f;

        if (oldMaxHealth > 0f)
        {
            healthPercent =
                currentHealth / oldMaxHealth;
        }

        healthPercent =
            Mathf.Clamp01(healthPercent);

        // =====================================================
        // RESET DO WARTOŚCI BAZOWYCH
        // =====================================================

        maxHealth = baseMaxHealth;

        currentHealthRegeneration =
            healthRegeneration;

        currentDamageReductionFlat =
            damageReductionFlat;

        currentDamageReductionPercent =
            damageReductionPercent;

        // =====================================================
        // ULEPSZENIA
        // =====================================================

        if (skillUpgradeManager != null)
        {
            // Max HP.
            maxHealth +=
                skillUpgradeManager.GetMaxHealthBonus();

            // Regeneracja.
            currentHealthRegeneration +=
                skillUpgradeManager.GetHealthRegeneration();

            // Stała redukcja.
            currentDamageReductionFlat +=
                skillUpgradeManager.GetDamageReductionFlat();

            // Procentowa redukcja.
            currentDamageReductionPercent +=
                skillUpgradeManager.GetDamageReductionPercent();
        }

        // =====================================================
        // ZABEZPIECZENIA
        // =====================================================

        maxHealth =
            Mathf.Max(
                1f,
                maxHealth
            );

        currentHealthRegeneration =
            Mathf.Max(
                0f,
                currentHealthRegeneration
            );

        currentDamageReductionFlat =
            Mathf.Max(
                0f,
                currentDamageReductionFlat
            );

        currentDamageReductionPercent =
            Mathf.Clamp(
                currentDamageReductionPercent,
                0f,
                100f
            );

        // =====================================================
        // AKTUALIZACJA AKTUALNEGO HP
        // =====================================================

        if (Application.isPlaying)
        {
            // Jeżeli wieża już miała HP,
            // zachowujemy procent jej zdrowia.

            if (oldMaxHealth > 0f &&
                currentHealth > 0f)
            {
                currentHealth =
                    maxHealth *
                    healthPercent;
            }

            currentHealth =
                Mathf.Clamp(
                    currentHealth,
                    0f,
                    maxHealth
                );

            UpdateHealthBar();
        }
    }

    // =========================================================
    // REGENERACJA
    // =========================================================

    private void HandleRegeneration()
    {
        if (isDestroyed)
            return;

        if (currentHealthRegeneration <= 0f)
            return;

        if (currentHealth <= 0f)
            return;

        if (currentHealth >= maxHealth)
            return;

        currentHealth +=
            currentHealthRegeneration *
            Time.deltaTime;

        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0f,
                maxHealth
            );

        UpdateHealthBar();
    }

    // =========================================================
    // OTRZYMYWANIE OBRAŻEŃ
    // =========================================================

    public void TakeDamage(float damage)
    {
        // Po rozpoczęciu zniszczenia wieża nie może już
        // otrzymać żadnych obrażeń.
        if (isDestroyed)
            return;

        if (damage <= 0f)
            return;

        if (currentHealth <= 0f)
            return;

        // =====================================================
        // 1. REDUKCJA STAŁA
        // =====================================================

        damage -=
            currentDamageReductionFlat;

        damage =
            Mathf.Max(
                0f,
                damage
            );

        // =====================================================
        // 2. REDUKCJA PROCENTOWA
        // =====================================================

        float reductionMultiplier =
            1f -
            (
                currentDamageReductionPercent /
                100f
            );

        reductionMultiplier =
            Mathf.Clamp01(
                reductionMultiplier
            );

        damage *=
            reductionMultiplier;

        // =====================================================
        // ZABIERAMY HP
        // =====================================================

        currentHealth -= damage;

        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0f,
                maxHealth
            );

        UpdateHealthBar();

        // =====================================================
        // ŚMIERĆ
        // =====================================================

        if (currentHealth <= 0f)
        {
            DestroyTower();
        }
    }

    // =========================================================
    // LECZENIE
    // =========================================================

    public void Heal(float amount)
    {
        if (isDestroyed)
            return;

        if (amount <= 0f)
            return;

        if (currentHealth <= 0f)
            return;

        currentHealth += amount;

        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0f,
                maxHealth
            );

        UpdateHealthBar();
    }

    // =========================================================
    // POBIERANIE HP
    // =========================================================

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetHealthPercent()
    {
        if (maxHealth <= 0f)
            return 0f;

        return currentHealth / maxHealth;
    }

    // =========================================================
    // CZY WIEŻA JEST ZNISZCZONA
    // =========================================================

    public bool IsDestroyed()
    {
        return isDestroyed;
    }

    // =========================================================
    // STATYSTYKI OBRONY
    // =========================================================

    public float GetHealthRegeneration()
    {
        return currentHealthRegeneration;
    }

    public float GetDamageReductionFlat()
    {
        return currentDamageReductionFlat;
    }

    public float GetDamageReductionPercent()
    {
        return currentDamageReductionPercent;
    }

    // =========================================================
    // PEŁNE HP
    // =========================================================

    public void RestoreFullHealth()
    {
        if (isDestroyed)
            return;

        if (currentHealth <= 0f)
            return;

        currentHealth =
            maxHealth;

        UpdateHealthBar();
    }

    // =========================================================
    // TWORZENIE PASKA ZDROWIA
    // =========================================================

    private void CreateHealthBar()
    {
        if (healthBarPrefab == null)
        {
            return;
        }

        if (healthBarAnchor == null)
        {
            return;
        }

        // =====================================================
        // UWAGA:
        // WRACAMY DO STAREGO, DZIAŁAJĄCEGO SPOSOBU
        // =====================================================

        myHealthBar =
            Instantiate(
                healthBarPrefab,
                healthBarAnchor.position,
                Quaternion.identity
            );

        // =====================================================
        // HEALTH BAR FOLLOW
        // =====================================================

        HealthBarFollow follow =
            myHealthBar.GetComponent<HealthBarFollow>();

        if (follow != null)
        {
            follow.target =
                healthBarAnchor;
        }

        // =====================================================
        // SZUKANIE FILL
        // =====================================================

        Transform fill =
            myHealthBar.transform.Find(
                "Background/Fill"
            );

        if (fill != null)
        {
            healthBarFill =
                fill.GetComponent<Image>();

            if (healthBarFill != null)
            {
                healthBarFill.fillAmount =
                    1f;

                healthBarFill.color =
                    greenColor;
            }
        }

        // =====================================================
        // SZUKANIE TEKSTU
        // =====================================================

        Transform text =
            myHealthBar.transform.Find(
                "TowerHealthText"
            );

        if (text != null)
        {
            towerHealthText =
                text.GetComponent<TMP_Text>();
        }

        // =====================================================
        // PIERWSZA AKTUALIZACJA
        // =====================================================

        UpdateHealthBar();
    }

    // =========================================================
    // AKTUALIZACJA PASKA
    // =========================================================

    private void UpdateHealthBar()
    {
        if (healthBarFill == null)
            return;

        if (maxHealth <= 0f)
            return;

        float healthPercent =
            currentHealth /
            maxHealth;

        healthPercent =
            Mathf.Clamp01(
                healthPercent
            );

        // =====================================================
        // FILL
        // =====================================================

        healthBarFill.fillAmount =
            healthPercent;

        // =====================================================
        // KOLOR
        // =====================================================

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

        // =====================================================
        // TEKST
        // =====================================================

        if (towerHealthText != null)
        {
            towerHealthText.text =
                $"{Mathf.CeilToInt(currentHealth)}";
        }
    }

    // =========================================================
    // ŚMIERĆ WIEŻY
    // =========================================================

    private void DestroyTower()
    {
        // Zabezpieczenie przed wielokrotnym wywołaniem.
        if (isDestroyed)
            return;

        isDestroyed = true;

        // =====================================================
        // NATYCHMIASTOWE USTAWIENIE HP NA 0
        // =====================================================

        currentHealth = 0f;

        // =====================================================
        // USUNIĘCIE PASKA HP
        // =====================================================

        if (myHealthBar != null)
        {
            Destroy(myHealthBar);

            myHealthBar = null;
        }

        healthBarFill = null;
        towerHealthText = null;

        // =====================================================
        // URUCHOMIENIE WYBUCHU
        // =====================================================

        if (towerExplosionPrefab != null)
        {
            GameObject explosion =
                Instantiate(
                    towerExplosionPrefab,
                    transform.position,
                    Quaternion.identity
                );

            // Efekt nie jest dzieckiem wieży.
            // Dzięki temu pozostanie w scenie po
            // usunięciu wieży.
            explosion.transform.SetParent(null);
        }

        // =====================================================
        // ROZPOCZĘCIE PROCEDURY ZNISZCZENIA
        // =====================================================

        if (destructionCoroutine != null)
        {
            StopCoroutine(
                destructionCoroutine
            );
        }

        destructionCoroutine =
            StartCoroutine(
                DestructionCoroutine()
            );
    }

    // =========================================================
    // PROCEDURA ZNISZCZENIA
    // =========================================================

    private IEnumerator DestructionCoroutine()
    {
        towerModel.SetActive(false);

        float safeDuration =
            Mathf.Max(
                0f,
                explosionDuration
            );

        // =====================================================
        // CZEKAMY NA KONIEC EFEKTU
        // =====================================================

        if (safeDuration > 0f)
        {
            yield return new WaitForSeconds(
                safeDuration
            );
        }

        destructionCoroutine = null;

        // =====================================================
        // GAME OVER
        // =====================================================

        if (gameOverUI == null)
        {
            gameOverUI =
                FindAnyObjectByType<GameOverUI>();
        }

        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver();
        }
        else
        {
            // Jeżeli GameOverUI nie istnieje,
            // przynajmniej zatrzymujemy grę.
            Time.timeScale = 0f;
        }

        // =====================================================
        // USUNIĘCIE WIEŻY
        // =====================================================

        Destroy(gameObject);
    }
}