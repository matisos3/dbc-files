using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    // =========================================================
    // PLAYER PREFS KEYS
    // =========================================================

    private const string SHOW_CRITICAL_DAMAGE_KEY =
        "ShowCriticalDamage";


    // =========================================================
    // USTAWIENIA
    // =========================================================

    [Header("Widoczność obrażeń krytycznych")]
    [SerializeField]
    private bool showCriticalDamage = true;


    // =========================================================
    // PUBLIC ACCESS
    // =========================================================

    public bool ShowCriticalDamage
    {
        get
        {
            /*
             * PlayerPrefs jest tutaj źródłem prawdy.
             *
             * Dzięki temu nawet jeśli jakieś inne miejsce
             * zmieni stan obiektu, ustawienie zostanie
             * odczytane zgodnie z zapisanym wyborem gracza.
             */
            return PlayerPrefs.GetInt(
                SHOW_CRITICAL_DAMAGE_KEY,
                1
            ) == 1;
        }
    }


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        LoadSettings();
    }


    // =========================================================
    // USTAWIENIE WARTOŚCI
    // =========================================================

    public void SetShowCriticalDamage(bool value)
    {
        /*
         * Aktualizujemy również pole widoczne w Inspectorze
         * podczas działania gry.
         */
        showCriticalDamage = value;

        /*
         * Zapis ustawienia.
         */
        PlayerPrefs.SetInt(
            SHOW_CRITICAL_DAMAGE_KEY,
            value ? 1 : 0
        );

        PlayerPrefs.Save();
    }


    // =========================================================
    // WCZYTYWANIE
    // =========================================================

    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey(
            SHOW_CRITICAL_DAMAGE_KEY))
        {
            showCriticalDamage =
                PlayerPrefs.GetInt(
                    SHOW_CRITICAL_DAMAGE_KEY
                ) == 1;
        }
        else
        {
            /*
             * Pierwsze uruchomienie:
             * domyślnie pokazujemy krytyki.
             */
            showCriticalDamage = true;

            PlayerPrefs.SetInt(
                SHOW_CRITICAL_DAMAGE_KEY,
                1
            );

            PlayerPrefs.Save();
        }
    }
}