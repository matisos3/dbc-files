using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameUpdater : MonoBehaviour
{
    // =========================================================
    // KONFIGURACJA
    // =========================================================

    [Header("Version")]
    [SerializeField]
    private string versionJsonUrl =
        "https://raw.githubusercontent.com/matisos3/Chaos_Tower_Defense/main/Version.json";

    // =========================================================
    // UI
    // =========================================================

    [Header("UI")]
    [SerializeField] private GameObject updateWindow;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text versionText;
    [SerializeField] private TMP_Text progressText;

    [SerializeField] private Slider progressSlider;

    [SerializeField] private Button updateButton;
    [SerializeField] private Button laterButton;

    // =========================================================
    // DANE AKTUALIZACJI
    // =========================================================

    [Serializable]
    private class VersionInfo
    {
        public string version;
        public int versionCode;
        public string apkUrl;
        public bool mandatory;
        public string message;
    }

    private VersionInfo latestVersion;

    private string apkPath;

    private bool updateAvailable;
    private bool isDownloading;
    private bool isInstalling;

    // =========================================================
    // OCZEKIWANIE NA POWRÓT Z USTAWIEŃ ANDROIDA
    // =========================================================

    private bool waitingForInstallPermission;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        if (updateWindow != null)
            updateWindow.SetActive(false);

        StartCoroutine(CheckForUpdate());

#else

        if (updateWindow != null)
            updateWindow.SetActive(false);

#endif
    }

    // =========================================================
    // POWRÓT DO APLIKACJI
    // =========================================================

    private void OnApplicationFocus(bool hasFocus)
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        if (!hasFocus)
            return;

        if (!waitingForInstallPermission)
            return;

        waitingForInstallPermission = false;

        StartCoroutine(
            ContinueInstallationAfterSettings()
        );

#endif
    }

    // =========================================================
    // KONTYNUOWANIE INSTALACJI
    // =========================================================

    private IEnumerator ContinueInstallationAfterSettings()
    {
        /*
         * Dajemy Androidowi chwilę na zakończenie
         * powrotu z ekranu ustawień.
         */
        yield return new WaitForSeconds(0.3f);

        if (string.IsNullOrEmpty(apkPath))
        {
            Debug.LogWarning(
                "GameUpdater: Brak ścieżki do pobranego APK."
            );

            yield break;
        }

        if (!File.Exists(apkPath))
        {
            Debug.LogWarning(
                "GameUpdater: Pobrany APK nie istnieje po powrocie "
                + "z ustawień Androida."
            );

            yield break;
        }

        bool canInstall =
            CanInstallUnknownApps();

        if (!canInstall)
        {
            Debug.LogWarning(
                "GameUpdater: Użytkownik nadal nie zezwolił "
                + "na instalowanie aplikacji z nieznanych źródeł."
            );

            if (messageText != null)
            {
                messageText.text =
                    "Aby zainstalować aktualizację, "
                    + "zezwól aplikacji na instalowanie "
                    + "z nieznanych źródeł.";
            }

            if (updateButton != null)
            {
                updateButton.interactable = true;
            }

            if (laterButton != null &&
                latestVersion != null)
            {
                laterButton.interactable =
                    !latestVersion.mandatory;
            }

            yield break;
        }

        Debug.Log(
            "GameUpdater: Zgoda została udzielona. "
            + "Uruchamiam instalację APK."
        );

        InstallAPK();
    }

    // =========================================================
    // SPRAWDZANIE AKTUALIZACJI
    // =========================================================

    private IEnumerator CheckForUpdate()
    {
        using (UnityWebRequest request =
               UnityWebRequest.Get(versionJsonUrl))
        {
            request.timeout = 15;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning(
                    "GameUpdater: Nie udało się pobrać Version.json: "
                    + request.error
                );

                yield break;
            }

            string json =
                request.downloadHandler.text;

            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning(
                    "GameUpdater: Version.json jest pusty."
                );

                yield break;
            }

            try
            {
                latestVersion =
                    JsonUtility.FromJson<VersionInfo>(json);
            }
            catch (Exception e)
            {
                Debug.LogError(
                    "GameUpdater: Błąd parsowania Version.json: "
                    + e.Message
                );

                yield break;
            }

            if (latestVersion == null)
            {
                Debug.LogError(
                    "GameUpdater: Nie udało się utworzyć VersionInfo."
                );

                yield break;
            }

            if (string.IsNullOrEmpty(latestVersion.apkUrl))
            {
                Debug.LogError(
                    "GameUpdater: Version.json nie zawiera apkUrl."
                );

                yield break;
            }

            int installedVersionCode =
                GetInstalledVersionCode();

            Debug.Log(
                "GameUpdater: Zainstalowana wersja: "
                + installedVersionCode
                + " | Najnowsza wersja: "
                + latestVersion.versionCode
            );

            if (latestVersion.versionCode >
                installedVersionCode)
            {
                updateAvailable = true;

                ShowUpdateWindow();
            }
            else
            {
                updateAvailable = false;

                Debug.Log(
                    "GameUpdater: Gra jest aktualna."
                );
            }
        }
    }

    // =========================================================
    // VERSION CODE
    // =========================================================

    private int GetInstalledVersionCode()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        try
        {
            using (AndroidJavaClass updaterClass =
                   new AndroidJavaClass(
                       "com.MatisosGame.gameupdater.GameUpdater"))
            {
                using (AndroidJavaClass unityPlayer =
                       new AndroidJavaClass(
                           "com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity =
                           unityPlayer.GetStatic<AndroidJavaObject>(
                               "currentActivity"))
                    {
                        string versionString =
                            updaterClass.CallStatic<string>(
                                "getInstalledVersion",
                                activity
                            );

                        Debug.Log(
                            "GameUpdater: Android zwrócił versionCode: "
                            + versionString
                        );

                        if (int.TryParse(
                                versionString,
                                out int versionCode))
                        {
                            return versionCode;
                        }

                        Debug.LogError(
                            "GameUpdater: Nie udało się przekonwertować "
                            + "versionCode na int: "
                            + versionString
                        );
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(
                "GameUpdater: Nie udało się pobrać versionCode: "
                + e
            );
        }

#endif

        return 0;
    }

    // =========================================================
    // SPRAWDZENIE ZGODY
    // =========================================================

    private bool CanInstallUnknownApps()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        try
        {
            using (AndroidJavaClass updaterClass =
                   new AndroidJavaClass(
                       "com.MatisosGame.gameupdater.GameUpdater"))
            {
                using (AndroidJavaClass unityPlayer =
                       new AndroidJavaClass(
                           "com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity =
                           unityPlayer.GetStatic<AndroidJavaObject>(
                               "currentActivity"))
                    {
                        return updaterClass.CallStatic<bool>(
                            "canInstallUnknownApps",
                            activity
                        );
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(
                "GameUpdater: Nie udało się sprawdzić zgody "
                + "na instalowanie APK: "
                + e
            );

            return false;
        }

#else

        return true;

#endif
    }

    // =========================================================
    // POKAZANIE OKNA
    // =========================================================

    private void ShowUpdateWindow()
    {
        if (updateWindow == null)
        {
            Debug.LogWarning(
                "GameUpdater: updateWindow nie jest przypisane."
            );

            return;
        }

        updateWindow.SetActive(true);

        if (titleText != null)
            titleText.text =
                "Dostępna aktualizacja";

        if (versionText != null)
        {
            versionText.text =
                "Wersja: "
                + Application.version
                + " → "
                + latestVersion.version;
        }

        if (messageText != null)
        {
            messageText.text =
                string.IsNullOrEmpty(
                    latestVersion.message
                )
                    ? "Dostępna jest nowa wersja gry."
                    : latestVersion.message;
        }

        if (progressSlider != null)
        {
            progressSlider.value = 0f;

            progressSlider.gameObject.SetActive(
                false
            );
        }

        if (progressText != null)
        {
            progressText.text = "";

            progressText.gameObject.SetActive(
                false
            );
        }

        if (updateButton != null)
        {
            updateButton.gameObject.SetActive(true);

            updateButton.interactable = true;

            updateButton.onClick.RemoveAllListeners();

            updateButton.onClick.AddListener(
                StartUpdate
            );
        }

        if (laterButton != null)
        {
            laterButton.gameObject.SetActive(
                !latestVersion.mandatory
            );

            laterButton.interactable = true;

            laterButton.onClick.RemoveAllListeners();

            laterButton.onClick.AddListener(
                CloseUpdateWindow
            );
        }
    }

    // =========================================================
    // START AKTUALIZACJI
    // =========================================================

    public void StartUpdate()
    {
        if (!updateAvailable)
            return;

        if (isDownloading ||
            isInstalling)
            return;

        StartCoroutine(
            DownloadUpdate()
        );
    }

    // =========================================================
    // POBIERANIE APK
    // =========================================================

    private IEnumerator DownloadUpdate()
    {
        isDownloading = true;

        if (updateButton != null)
            updateButton.interactable = false;

        if (laterButton != null)
            laterButton.interactable = false;

        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(true);

            progressSlider.value = 0f;
        }

        if (progressText != null)
        {
            progressText.gameObject.SetActive(true);

            progressText.text = "0%";
        }

        string fileName =
            "ChaosTowerDefense_Update.apk";

        apkPath =
            Path.Combine(
                Application.temporaryCachePath,
                fileName
            );

        // =====================================================
        // USUNIĘCIE STAREGO APK
        // =====================================================

        if (File.Exists(apkPath))
        {
            try
            {
                File.Delete(apkPath);
            }
            catch (Exception e)
            {
                Debug.LogWarning(
                    "GameUpdater: Nie udało się usunąć starego APK: "
                    + e.Message
                );
            }
        }

        // =====================================================
        // POBIERANIE
        // =====================================================

        Debug.Log(
            "GameUpdater: Pobieranie APK z: "
            + latestVersion.apkUrl
        );

        using (UnityWebRequest request =
               UnityWebRequest.Get(
                   latestVersion.apkUrl
               ))
        {
            request.timeout = 120;

            request.downloadHandler =
                new DownloadHandlerFile(
                    apkPath
                );

            request.SendWebRequest();

            while (!request.isDone)
            {
                float progress =
                    request.downloadProgress;

                if (progressSlider != null)
                    progressSlider.value =
                        progress;

                if (progressText != null)
                {
                    int percent =
                        Mathf.RoundToInt(
                            progress * 100f
                        );

                    progressText.text =
                        percent + "%";
                }

                yield return null;
            }

            if (request.result !=
                UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    "GameUpdater: Błąd pobierania APK: "
                    + request.error
                );

                isDownloading = false;

                ShowDownloadError(
                    request.error
                );

                yield break;
            }
        }

        isDownloading = false;

        // =====================================================
        // SPRAWDZENIE PLIKU
        // =====================================================

        if (!File.Exists(apkPath))
        {
            Debug.LogError(
                "GameUpdater: APK nie istnieje po pobraniu."
            );

            ShowDownloadError(
                "Nie znaleziono pobranego pliku APK."
            );

            yield break;
        }

        if (progressSlider != null)
            progressSlider.value = 1f;

        if (progressText != null)
            progressText.text = "100%";

        Debug.Log(
            "GameUpdater: APK pobrane: "
            + apkPath
        );

        yield return new WaitForSeconds(
            0.2f
        );

        InstallAPK();
    }

    // =========================================================
    // INSTALACJA APK
    // =========================================================

    private void InstallAPK()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        if (isInstalling)
            return;

        if (string.IsNullOrEmpty(apkPath))
        {
            Debug.LogError(
                "GameUpdater: apkPath jest pusty."
            );

            return;
        }

        if (!File.Exists(apkPath))
        {
            Debug.LogError(
                "GameUpdater: APK nie istnieje: "
                + apkPath
            );

            return;
        }

        isInstalling = true;

        try
        {
            using (AndroidJavaClass updaterClass =
                   new AndroidJavaClass(
                       "com.MatisosGame.gameupdater.GameUpdater"))
            {
                using (AndroidJavaClass unityPlayer =
                       new AndroidJavaClass(
                           "com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity =
                           unityPlayer.GetStatic<AndroidJavaObject>(
                               "currentActivity"))
                    {
                        bool canInstall =
                            updaterClass.CallStatic<bool>(
                                "canInstallUnknownApps",
                                activity
                            );

                        // =====================================
                        // BRAK ZGODY
                        // =====================================

                        if (!canInstall)
                        {
                            Debug.Log(
                                "GameUpdater: Brak zgody na "
                                + "instalowanie aplikacji z nieznanych źródeł."
                            );

                            /*
                             * Zapamiętujemy, że czekamy
                             * na powrót z ustawień Androida.
                             */
                            waitingForInstallPermission =
                                true;

                            /*
                             * Instalacja nie jest już aktywna.
                             * Po powrocie z ustawień wywołamy ją ponownie.
                             */
                            isInstalling = false;

                            updaterClass.CallStatic(
                                "openUnknownAppsSettings",
                                activity
                            );

                            return;
                        }

                        // =====================================
                        // JEST ZGODA
                        // =====================================

                        Debug.Log(
                            "GameUpdater: Zgoda na instalowanie APK "
                            + "jest aktywna."
                        );

                        Debug.Log(
                            "GameUpdater: Uruchamianie instalatora APK."
                        );

                        updaterClass.CallStatic(
                            "installAPK",
                            activity,
                            apkPath
                        );
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(
                "GameUpdater: Błąd instalacji APK: "
                + e
            );

            isInstalling = false;
        }

#endif
    }

    // =========================================================
    // BŁĄD POBIERANIA
    // =========================================================

    private void ShowDownloadError(
        string error
    )
    {
        if (titleText != null)
            titleText.text =
                "Błąd aktualizacji";

        if (messageText != null)
        {
            messageText.text =
                "Nie udało się pobrać aktualizacji.\n\n"
                + error;
        }

        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(
                false
            );
        }

        if (progressText != null)
        {
            progressText.gameObject.SetActive(
                false
            );
        }

        if (updateButton != null)
        {
            updateButton.interactable = true;

            updateButton.gameObject.SetActive(
                !latestVersion.mandatory
            );
        }

        if (laterButton != null)
        {
            laterButton.gameObject.SetActive(true);

            laterButton.interactable = true;

            laterButton.onClick.RemoveAllListeners();

            laterButton.onClick.AddListener(
                CloseUpdateWindow
            );
        }
    }

    // =========================================================
    // ZAMKNIĘCIE OKNA
    // =========================================================

    private void CloseUpdateWindow()
    {
        if (latestVersion != null &&
            latestVersion.mandatory)
        {
            return;
        }

        if (updateWindow != null)
            updateWindow.SetActive(false);
    }

    // =========================================================
    // PUBLICZNE PONOWNE SPRAWDZENIE
    // =========================================================

    public void CheckAgain()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        if (isDownloading ||
            isInstalling)
            return;

        StartCoroutine(
            CheckForUpdate()
        );

#endif
    }
}