using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance { get; private set; }

    [Header("Ustawienia")]
    [SerializeField] private int fallbackLocaleIndex = 0;

    private const string LanguagePreferenceKey = "SelectedLanguage";

    private TMP_Dropdown languageDropdown;

    private bool isInitializing = false;
    private bool ignoreDropdownCallback = false;
    private bool localizationInitialized = false;


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
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        StartCoroutine(InitializeLanguage());
    }


    // =========================================================
    // INICJALIZACJA JĘZYKA
    // =========================================================

    private IEnumerator InitializeLanguage()
    {
        isInitializing = true;

        yield return LocalizationSettings.InitializationOperation;

        localizationInitialized = true;

        // Szukamy Dropdowna w aktualnej scenie.
        FindLanguageDropdown();

        // Budujemy Dropdown, jeśli istnieje.
        BuildDropdown();

        // =====================================================
        // SPRAWDZAMY ZAPISANY JĘZYK
        // =====================================================

        if (PlayerPrefs.HasKey(LanguagePreferenceKey))
        {
            string savedLocaleCode =
                PlayerPrefs.GetString(LanguagePreferenceKey);

            Locale savedLocale =
                FindLocaleByCode(savedLocaleCode);

            if (savedLocale != null)
            {
                LocalizationSettings.SelectedLocale =
                    savedLocale;
            }
            else
            {
                SelectSystemLanguage();
            }
        }
        else
        {
            // Pierwsze uruchomienie gry.
            SelectSystemLanguage();
        }

        UpdateDropdown();

        RegisterDropdownListener();

        isInitializing = false;
    }


    // =========================================================
    // ZMIANA SCENY
    // =========================================================

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        if (!localizationInitialized)
            return;

        StartCoroutine(SetupDropdownAfterSceneLoad());
    }


    // =========================================================
    // KONFIGURACJA DROPDOWNA PO ZAŁADOWANIU SCENY
    // =========================================================

    private IEnumerator SetupDropdownAfterSceneLoad()
    {
        // Dajemy Unity chwilę na utworzenie całego UI sceny.
        yield return null;

        FindLanguageDropdown();

        if (languageDropdown == null)
            yield break;

        BuildDropdown();

        UpdateDropdown();

        RegisterDropdownListener();
    }


    // =========================================================
    // ZNAJDŹ DROPDOWN W AKTUALNEJ SCENIE
    // =========================================================

private void FindLanguageDropdown()
{
    languageDropdown = null;

    TMP_Dropdown[] dropdowns =
        FindObjectsByType<TMP_Dropdown>(
            FindObjectsInactive.Include
        );

    foreach (TMP_Dropdown dropdown in dropdowns)
    {
        if (dropdown == null)
            continue;

        if (dropdown.gameObject.name == "LanguageDropdown")
        {
            languageDropdown = dropdown;
            break;
        }
    }

    if (languageDropdown == null)
    {
        return;
    }
}


    // =========================================================
    // LISTENER
    // =========================================================

    private void RegisterDropdownListener()
    {
        if (languageDropdown == null)
            return;

        languageDropdown.onValueChanged
            .RemoveListener(OnDropdownValueChanged);

        languageDropdown.onValueChanged
            .AddListener(OnDropdownValueChanged);
    }


    // =========================================================
    // BUDOWANIE DROPDOWNA
    // =========================================================

    private void BuildDropdown()
    {
        if (languageDropdown == null)
            return;

        languageDropdown.ClearOptions();

        var locales =
            LocalizationSettings.AvailableLocales.Locales;

        if (locales == null ||
            locales.Count == 0)
        {
            return;
        }

        foreach (Locale locale in locales)
        {
            if (locale == null)
                continue;

            string languageName =
                GetLanguageName(locale);

            languageDropdown.options.Add(
                new TMP_Dropdown.OptionData(languageName)
            );
        }

        languageDropdown.RefreshShownValue();
    }


    // =========================================================
    // NAZWA JĘZYKA
    // =========================================================

    private string GetLanguageName(Locale locale)
    {
        if (locale == null)
            return "Unknown";

        string name =
            locale.LocaleName;

        if (!string.IsNullOrEmpty(name))
            return name;

        return locale.Identifier.Code;
    }


    // =========================================================
    // AUTOMATYCZNE WYKRYWANIE JĘZYKA
    // =========================================================

    private void SelectSystemLanguage()
    {
        var locales =
            LocalizationSettings.AvailableLocales.Locales;

        if (locales == null ||
            locales.Count == 0)
        {
            return;
        }

        SystemLanguage systemLanguage =
            Application.systemLanguage;

        string desiredCode = "en";

        switch (systemLanguage)
        {
            case SystemLanguage.Polish:
                desiredCode = "pl";
                break;

            case SystemLanguage.German:
                desiredCode = "de";
                break;

            case SystemLanguage.English:
                desiredCode = "en";
                break;

            default:
                desiredCode = "en";
                break;
        }

        Locale detectedLocale =
            FindLocaleByCode(desiredCode);

        if (detectedLocale != null)
        {
            LocalizationSettings.SelectedLocale =
                detectedLocale;

            PlayerPrefs.SetString(
                LanguagePreferenceKey,
                detectedLocale.Identifier.Code
            );

            PlayerPrefs.Save();
        }
        else
        {
            int safeIndex =
                Mathf.Clamp(
                    fallbackLocaleIndex,
                    0,
                    locales.Count - 1
                );

            Locale fallbackLocale =
                locales[safeIndex];

            LocalizationSettings.SelectedLocale =
                fallbackLocale;

            PlayerPrefs.SetString(
                LanguagePreferenceKey,
                fallbackLocale.Identifier.Code
            );

            PlayerPrefs.Save();
        }
    }


    // =========================================================
    // ZNAJDŹ LOCALE PO KODZIE
    // =========================================================

    private Locale FindLocaleByCode(string code)
    {
        if (string.IsNullOrEmpty(code))
            return null;

        var locales =
            LocalizationSettings.AvailableLocales.Locales;

        if (locales == null)
            return null;

        foreach (Locale locale in locales)
        {
            if (locale == null)
                continue;

            if (string.Equals(
                locale.Identifier.Code,
                code,
                System.StringComparison.OrdinalIgnoreCase))
            {
                return locale;
            }
        }

        return null;
    }


    // =========================================================
    // ZMIANA JĘZYKA
    // =========================================================

    private void OnDropdownValueChanged(int index)
    {
        if (isInitializing ||
            ignoreDropdownCallback)
        {
            return;
        }

        var locales =
            LocalizationSettings.AvailableLocales.Locales;

        if (locales == null ||
            index < 0 ||
            index >= locales.Count)
        {
            return;
        }

        Locale selectedLocale =
            locales[index];

        if (selectedLocale == null)
            return;

        SetLanguage(selectedLocale);
    }


    // =========================================================
    // USTAW JĘZYK
    // =========================================================

    private void SetLanguage(Locale locale)
    {
        if (locale == null)
            return;

        LocalizationSettings.SelectedLocale =
            locale;

        PlayerPrefs.SetString(
            LanguagePreferenceKey,
            locale.Identifier.Code
        );

        PlayerPrefs.Save();

        UpdateDropdown();
    }


    // =========================================================
    // AKTUALIZACJA DROPDOWNA
    // =========================================================

    private void UpdateDropdown()
    {
        if (languageDropdown == null)
            return;

        Locale selectedLocale =
            LocalizationSettings.SelectedLocale;

        if (selectedLocale == null)
            return;

        var locales =
            LocalizationSettings.AvailableLocales.Locales;

        int selectedIndex = -1;

        for (int i = 0; i < locales.Count; i++)
        {
            if (locales[i] == selectedLocale)
            {
                selectedIndex = i;
                break;
            }
        }

        if (selectedIndex < 0)
            return;

        ignoreDropdownCallback = true;

        languageDropdown.SetValueWithoutNotify(
            selectedIndex
        );

        languageDropdown.RefreshShownValue();

        ignoreDropdownCallback = false;
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (languageDropdown != null)
        {
            languageDropdown.onValueChanged
                .RemoveListener(OnDropdownValueChanged);
        }
    }
}