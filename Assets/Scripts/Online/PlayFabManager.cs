using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance;


    // =========================================================
    // LOGIN
    // =========================================================

    public bool IsLoggedIn { get; private set; }
    public string PlayFabId { get; private set; }


    // =========================================================
    // PROFIL
    // =========================================================

    public string PlayerDisplayName { get; private set; } = "";
    public string PlayerAvatarId { get; private set; } = "";

    public bool IsProfileLoaded { get; private set; } = false;
    public bool HasPlayerProfile { get; private set; } = false;

    private const string PLAYER_AVATAR_KEY = "Avatar";

    // Jednorazowa zmiana nazwy
    private const string PLAYER_NAME_CHANGE_USED_KEY =
        "PlayerNameChangeUsed";

    public bool PlayerNameChangeUsed { get; private set; } = false;


    // =========================================================
    // PROGRESJA
    // =========================================================

    [Header("Player Progression")]
    [SerializeField] private int startingLevel = 1;
    [SerializeField] private int maximumLevel = 100;
    [SerializeField] private double baseXPToLevel2 = 1000.0;
    [SerializeField] private double xpMultiplier = 1.1;

    private const string PLAYER_XP_KEY = "PlayerXP";
    private const string PLAYER_LEVEL_KEY = "PlayerLevel";

    public double PlayerXP { get; private set; } = 0.0;
    public int PlayerLevel { get; private set; } = 1;

    public bool IsProgressionLoaded { get; private set; } = false;


    // =========================================================
    // DOSTĘP DO GRY
    // =========================================================

    public bool IsGameEnabled { get; private set; } = false;
    public bool GameAccessChecked { get; private set; } = false;

    private const string GAME_ENABLED_KEY = "GameEnabled";

    private GameAccessBlockUI gameAccessUI;


    // =========================================================
    // PENDING GAME OVER
    // =========================================================

    private bool pendingGameOverStats = false;

    private int pendingEnemiesKilled = 0;
    private double pendingDamageDealt = 0.0;
    private int pendingHighestWave = 0;
    private int pendingGoldEarned = 0;
    private int pendingGameDuration = 0;
    private int pendingSkillsPurchased = 0;
    private int pendingUpgradesPurchased = 0;

    private string pendingBuildSkills = "";
    private string pendingBuildUpgrades = "";
    private string pendingBuildSummary = "";

    private bool gameOverStatsSaved = false;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        transform.SetParent(null);

        DontDestroyOnLoad(gameObject);
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        Login();
    }


    // =========================================================
    // LOGIN
    // =========================================================

    public void Login()
    {
        IsLoggedIn = false;

        GameAccessChecked = false;
        IsGameEnabled = false;

        IsProfileLoaded = false;
        HasPlayerProfile = false;

        IsProgressionLoaded = false;

        PlayerDisplayName = "";
        PlayerAvatarId = "";

        PlayerNameChangeUsed = false;

        PlayerXP = 0.0;

        PlayerLevel =
            Mathf.Max(
                1,
                startingLevel
            );


        string customId =
            GetOrCreateLocalPlayerId();


        LoginWithCustomIDRequest request =
            new LoginWithCustomIDRequest
            {
                CustomId = customId,
                CreateAccount = true
            };


        PlayFabClientAPI.LoginWithCustomID(
            request,
            OnLoginSuccess,
            OnLoginFailure
        );
    }


    // =========================================================
    // LOCAL PLAYER ID
    // =========================================================

    private string GetOrCreateLocalPlayerId()
    {
        const string key =
            "PLAYFAB_LOCAL_PLAYER_ID";


        string existingId =
            PlayerPrefs.GetString(
                key,
                ""
            );


        if (!string.IsNullOrEmpty(existingId))
            return existingId;


        string newId =
            Guid.NewGuid().ToString();


        PlayerPrefs.SetString(
            key,
            newId
        );

        PlayerPrefs.Save();


        return newId;
    }


    // =========================================================
    // LOGIN SUCCESS
    // =========================================================

    private void OnLoginSuccess(
        LoginResult result)
    {
        IsLoggedIn = true;

        PlayFabId =
            result.PlayFabId;


        if (result.NewlyCreated)
            CreateInitialPlayerData();
        else
            LoadPlayerData();


        LoadPlayerProgression();

        CheckGameAccess();


        if (pendingGameOverStats)
            SavePendingGameOverStats();
    }


    // =========================================================
    // LOGIN FAILURE
    // =========================================================

    private void OnLoginFailure(
        PlayFabError error)
    {
        IsLoggedIn = false;

        GameAccessChecked = true;
        IsGameEnabled = false;

        IsProfileLoaded = false;
        HasPlayerProfile = false;

        IsProgressionLoaded = false;

        ApplyGameAccessToUI();
    }


    // =========================================================
    // LOAD PLAYER DATA
    // =========================================================

    private void LoadPlayerData()
    {
        LoadPlayerProfile();
    }


    // =========================================================
    // CREATE INITIAL PLAYER DATA
    // =========================================================

    private void CreateInitialPlayerData()
    {
        LoadPlayerProfile();
    }


    // =========================================================
    // LOAD PROFILE
    // =========================================================

    public void LoadPlayerProfile()
    {
        if (!IsLoggedIn)
        {
            IsProfileLoaded = false;
            HasPlayerProfile = false;

            return;
        }


        IsProfileLoaded = false;
        HasPlayerProfile = false;

        PlayerDisplayName = "";
        PlayerAvatarId = "";

        PlayerNameChangeUsed = false;


        PlayFabClientAPI.GetAccountInfo(
            new GetAccountInfoRequest(),
            OnPlayerAccountInfoReceived,
            OnPlayerAccountInfoFailed
        );
    }


    // =========================================================
    // ACCOUNT INFO RECEIVED
    // =========================================================

    private void OnPlayerAccountInfoReceived(
        GetAccountInfoResult result)
    {
        if (
            result != null &&
            result.AccountInfo != null &&
            result.AccountInfo.TitleInfo != null)
        {
            PlayerDisplayName =
                result.AccountInfo.TitleInfo.DisplayName ?? "";
        }


        LoadPlayerAvatar();
    }


    // =========================================================
    // ACCOUNT INFO FAILED
    // =========================================================

    private void OnPlayerAccountInfoFailed(
        PlayFabError error)
    {
        PlayerDisplayName = "";

        LoadPlayerAvatar();
    }


    // =========================================================
    // LOAD USER DATA
    // =========================================================

    private void LoadPlayerAvatar()
    {
        if (!IsLoggedIn)
        {
            FinishProfileLoading();

            return;
        }


        PlayFabClientAPI.GetUserData(
            new GetUserDataRequest(),
            OnPlayerUserDataReceived,
            OnPlayerUserDataFailed
        );
    }


    // =========================================================
    // USER DATA RECEIVED
    // =========================================================

    private void OnPlayerUserDataReceived(
        GetUserDataResult result)
    {
        PlayerAvatarId = "";

        PlayerNameChangeUsed = false;


        if (
            result != null &&
            result.Data != null)
        {
            // -------------------------------------------------
            // AVATAR
            // -------------------------------------------------

            if (
                result.Data.ContainsKey(
                    PLAYER_AVATAR_KEY
                ))
            {
                UserDataRecord avatarRecord =
                    result.Data[
                        PLAYER_AVATAR_KEY
                    ];


                if (avatarRecord != null)
                {
                    PlayerAvatarId =
                        avatarRecord.Value ?? "";
                }
            }


            // -------------------------------------------------
            // NAME CHANGE FLAG
            // -------------------------------------------------

            if (
                result.Data.ContainsKey(
                    PLAYER_NAME_CHANGE_USED_KEY
                ))
            {
                UserDataRecord nameChangeRecord =
                    result.Data[
                        PLAYER_NAME_CHANGE_USED_KEY
                    ];


                if (nameChangeRecord != null)
                {
                    bool parsedValue;

                    if (
                        bool.TryParse(
                            nameChangeRecord.Value,
                            out parsedValue))
                    {
                        PlayerNameChangeUsed =
                            parsedValue;
                    }
                }
            }
        }


        FinishProfileLoading();
    }


    // =========================================================
    // USER DATA FAILED
    // =========================================================

    private void OnPlayerUserDataFailed(
        PlayFabError error)
    {
        PlayerAvatarId = "";

        PlayerNameChangeUsed = false;

        FinishProfileLoading();
    }


    // =========================================================
    // FINISH PROFILE LOADING
    // =========================================================

    private void FinishProfileLoading()
    {
        IsProfileLoaded = true;


        HasPlayerProfile =
            !string.IsNullOrWhiteSpace(
                PlayerDisplayName
            ) &&
            !string.IsNullOrWhiteSpace(
                PlayerAvatarId
            );
    }


    // =========================================================
    // CAN CHANGE PLAYER NAME
    // =========================================================

    public bool CanChangePlayerName()
    {
        return !PlayerNameChangeUsed;
    }


    // =========================================================
    // SET PLAYER PROFILE
    // =========================================================

// =========================================================
// SET PLAYER PROFILE
// =========================================================

public void SetPlayerProfile(
    string displayName,
    string avatarId,
    Action<bool, string> callback = null)
{
    if (!IsLoggedIn)
    {
        callback?.Invoke(
            false,
            "Gracz nie jest zalogowany."
        );

        return;
    }


    if (string.IsNullOrWhiteSpace(displayName))
    {
        callback?.Invoke(
            false,
            "Nazwa gracza nie może być pusta."
        );

        return;
    }


    displayName =
        displayName.Trim();


    if (displayName.Length < 3)
    {
        callback?.Invoke(
            false,
            "Nazwa musi mieć co najmniej 3 znaki."
        );

        return;
    }


    if (displayName.Length > 25)
    {
        callback?.Invoke(
            false,
            "Nazwa może mieć maksymalnie 25 znaków."
        );

        return;
    }


    if (string.IsNullOrWhiteSpace(avatarId))
    {
        callback?.Invoke(
            false,
            "Wybierz avatar."
        );

        return;
    }


    // =====================================================
    // CZY PROFIL JUŻ ISTNIEJE?
    // =====================================================

    bool hasExistingProfile =
        HasPlayerProfile;


    // =====================================================
    // NOWY PROFIL
    // =====================================================

    if (!hasExistingProfile)
    {
        // -------------------------------------------------
        // NOWY GRACZ:
        //
        // 1. Ustawiamy nazwę
        // 2. Zapisujemy avatar
        //
        // Pierwsze utworzenie profilu NIE zużywa
        // jednorazowej zmiany nazwy.
        // -------------------------------------------------

        PlayFabClientAPI.UpdateUserTitleDisplayName(
            new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = displayName
            },
            result =>
            {
                if (result == null ||
                    string.IsNullOrEmpty(result.DisplayName))
                {
                    callback?.Invoke(
                        false,
                        "Nie udało się zapisać nazwy profilu."
                    );

                    return;
                }


                // -------------------------------------------------
                // NAZWA ZAPISANA
                // -------------------------------------------------

                PlayerDisplayName =
                    result.DisplayName;


                // -------------------------------------------------
                // WAŻNE:
                // Pierwsze utworzenie profilu NIE zużywa
                // zmiany nazwy.
                // -------------------------------------------------

                PlayerNameChangeUsed = false;


                // -------------------------------------------------
                // TERAZ ZAPISUJEMY AVATAR
                // -------------------------------------------------

                SavePlayerAvatar(
                    avatarId,
                    callback
                );
            },
            error =>
            {
                if (
                    error != null &&
                    error.Error ==
                    PlayFabErrorCode.NameNotAvailable)
                {
                    callback?.Invoke(
                        false,
                        "Ta nazwa jest już zajęta."
                    );

                    return;
                }


                callback?.Invoke(
                    false,
                    GetProfileErrorMessage(
                        error
                    )
                );
            }
        );


        return;
    }


    // =====================================================
    // ISTNIEJĄCY PROFIL
    // =====================================================

    bool isNameChange =
        !string.Equals(
            displayName,
            PlayerDisplayName,
            StringComparison.Ordinal
        );


    // =====================================================
    // NAZWA SIĘ NIE ZMIENIŁA
    // =====================================================

    if (!isNameChange)
    {
        // -------------------------------------------------
        // Użytkownik zmienia tylko avatar
        // -------------------------------------------------

        SavePlayerAvatar(
            avatarId,
            callback
        );

        return;
    }


    // =====================================================
    // DRUGA ZMIANA NAZWY — BLOKADA
    // =====================================================

    if (PlayerNameChangeUsed)
    {
        callback?.Invoke(
            false,
            "Wykorzystałeś już jedyną zmianę nazwy profilu."
        );

        return;
    }


    // =====================================================
    // ZMIANA NAZWY ISTNIEJĄCEGO PROFILU
    // =====================================================

    PlayFabClientAPI.UpdateUserTitleDisplayName(
        new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = displayName
        },
        result =>
        {
            if (result == null ||
                string.IsNullOrEmpty(result.DisplayName))
            {
                callback?.Invoke(
                    false,
                    "Nie udało się zmienić nazwy profilu."
                );

                return;
            }


            PlayerDisplayName =
                result.DisplayName;


            // -------------------------------------------------
            // Zmiana nazwy została wykorzystana
            // -------------------------------------------------

            PlayerNameChangeUsed = true;


            // -------------------------------------------------
            // Zapisujemy avatar + flagę zmiany nazwy
            // -------------------------------------------------

            SavePlayerAvatarAndNameChangeFlag(
                avatarId,
                callback
            );
        },
        error =>
        {
            if (
                error != null &&
                error.Error ==
                PlayFabErrorCode.NameNotAvailable)
            {
                callback?.Invoke(
                    false,
                    "Ta nazwa jest już zajęta."
                );

                return;
            }


            callback?.Invoke(
                false,
                GetProfileErrorMessage(
                    error
                )
            );
        }
    );
}


    // =========================================================
    // SAVE AVATAR + NAME CHANGE FLAG
    // =========================================================

    private void SavePlayerAvatarAndNameChangeFlag(
        string avatarId,
        Action<bool, string> callback)
    {
        Dictionary<string, string> data =
            new Dictionary<string, string>
            {
                {
                    PLAYER_AVATAR_KEY,
                    avatarId
                },
                {
                    PLAYER_NAME_CHANGE_USED_KEY,
                    "true"
                }
            };


        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest
            {
                Data = data
            },
            result =>
            {
                PlayerAvatarId =
                    avatarId;


                PlayerNameChangeUsed =
                    true;


                IsProfileLoaded = true;


                HasPlayerProfile =
                    !string.IsNullOrWhiteSpace(
                        PlayerDisplayName
                    ) &&
                    !string.IsNullOrWhiteSpace(
                        PlayerAvatarId
                    );


                callback?.Invoke(
                    true,
                    ""
                );
            },
            error =>
            {
                callback?.Invoke(
                    false,
                    GetProfileErrorMessage(
                        error
                    )
                );
            }
        );
    }


    // =========================================================
    // SAVE AVATAR
    // =========================================================

    private void SavePlayerAvatar(
        string avatarId,
        Action<bool, string> callback)
    {
        Dictionary<string, string> data =
            new Dictionary<string, string>
            {
                {
                    PLAYER_AVATAR_KEY,
                    avatarId
                }
            };


        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest
            {
                Data = data
            },
            result =>
            {
                PlayerAvatarId =
                    avatarId;


                IsProfileLoaded = true;


                HasPlayerProfile =
                    !string.IsNullOrWhiteSpace(
                        PlayerDisplayName
                    ) &&
                    !string.IsNullOrWhiteSpace(
                        PlayerAvatarId
                    );


                callback?.Invoke(
                    true,
                    ""
                );
            },
            error =>
            {
                callback?.Invoke(
                    false,
                    GetProfileErrorMessage(
                        error
                    )
                );
            }
        );
    }


    // =========================================================
    // SET PLAYER AVATAR
    // =========================================================

    public void SetPlayerAvatar(
        string avatarId,
        Action<bool, string> callback = null)
    {
        if (!IsLoggedIn)
        {
            callback?.Invoke(
                false,
                "Gracz nie jest zalogowany."
            );

            return;
        }


        if (string.IsNullOrWhiteSpace(avatarId))
        {
            callback?.Invoke(
                false,
                "Nieprawidłowy avatar."
            );

            return;
        }


        SavePlayerAvatar(
            avatarId,
            callback
        );
    }


    // =========================================================
    // SET PLAYER DISPLAY NAME
    // =========================================================

    public void SetPlayerDisplayName(
        string displayName,
        Action<bool, string> callback = null)
    {
        if (!IsLoggedIn)
        {
            callback?.Invoke(
                false,
                "Gracz nie jest zalogowany."
            );

            return;
        }


        if (string.IsNullOrWhiteSpace(displayName))
        {
            callback?.Invoke(
                false,
                "Nazwa gracza nie może być pusta."
            );

            return;
        }


        displayName =
            displayName.Trim();


        if (displayName.Length < 3)
        {
            callback?.Invoke(
                false,
                "Nazwa musi mieć co najmniej 3 znaki."
            );

            return;
        }


        if (displayName.Length > 25)
        {
            callback?.Invoke(
                false,
                "Nazwa może mieć maksymalnie 25 znaków."
            );

            return;
        }


        // -----------------------------------------------------
        // TA SAMA NAZWA = BRAK ZMIANY
        // -----------------------------------------------------

        if (
            string.Equals(
                displayName,
                PlayerDisplayName,
                StringComparison.Ordinal
            ))
        {
            callback?.Invoke(
                true,
                ""
            );

            return;
        }


        // -----------------------------------------------------
        // BLOKADA
        // -----------------------------------------------------

        if (PlayerNameChangeUsed)
        {
            callback?.Invoke(
                false,
                "Wykorzystałeś już jedyną zmianę nazwy profilu."
            );

            return;
        }


        // -----------------------------------------------------
        // ZMIANA NAZWY
        // -----------------------------------------------------

        PlayFabClientAPI.UpdateUserTitleDisplayName(
            new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = displayName
            },
            result =>
            {
                PlayerDisplayName =
                    result.DisplayName;


                PlayerNameChangeUsed =
                    true;


                SaveNameChangeFlag(
                    callback
                );
            },
            error =>
            {
                if (
                    error.Error ==
                    PlayFabErrorCode.NameNotAvailable)
                {
                    callback?.Invoke(
                        false,
                        "Ta nazwa jest już zajęta."
                    );

                    return;
                }


                callback?.Invoke(
                    false,
                    GetProfileErrorMessage(
                        error
                    )
                );
            }
        );
    }


    // =========================================================
    // SAVE NAME CHANGE FLAG
    // =========================================================

    private void SaveNameChangeFlag(
        Action<bool, string> callback)
    {
        Dictionary<string, string> data =
            new Dictionary<string, string>
            {
                {
                    PLAYER_NAME_CHANGE_USED_KEY,
                    "true"
                }
            };


        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest
            {
                Data = data
            },
            result =>
            {
                PlayerNameChangeUsed =
                    true;


                IsProfileLoaded = true;


                HasPlayerProfile =
                    !string.IsNullOrWhiteSpace(
                        PlayerDisplayName
                    ) &&
                    !string.IsNullOrWhiteSpace(
                        PlayerAvatarId
                    );


                callback?.Invoke(
                    true,
                    ""
                );
            },
            error =>
            {
                callback?.Invoke(
                    false,
                    GetProfileErrorMessage(
                        error
                    )
                );
            }
        );
    }


    // =========================================================
    // PROFILE ERROR MESSAGE
    // =========================================================

    private string GetProfileErrorMessage(
        PlayFabError error)
    {
        if (error == null)
            return "Wystąpił nieznany błąd.";


        if (
            error.Error ==
            PlayFabErrorCode.ProfaneDisplayName)
        {
            return "Ta nazwa nie może zostać użyta.";
        }


        if (
            error.Error ==
            PlayFabErrorCode.NameNotAvailable)
        {
            return "Ta nazwa jest już zajęta.";
        }


        if (!string.IsNullOrEmpty(
            error.ErrorMessage))
        {
            return error.ErrorMessage;
        }


        return "Nie udało się zapisać profilu.";
    }


    // =========================================================
    // PROFILE STATUS
    // =========================================================

    public bool IsPlayerProfileComplete()
    {
        return
            IsLoggedIn &&
            IsProfileLoaded &&
            HasPlayerProfile;
    }


    public string GetPlayerDisplayName()
    {
        return PlayerDisplayName;
    }


    public string GetPlayerAvatarId()
    {
        return PlayerAvatarId;
    }


    // =========================================================
    // PROGRESJA - LOAD
    // =========================================================

    public void LoadPlayerProgression()
    {
        if (!IsLoggedIn)
        {
            IsProgressionLoaded = false;

            return;
        }


        IsProgressionLoaded = false;


        PlayFabClientAPI.GetUserData(
            new GetUserDataRequest(),
            OnProgressionDataReceived,
            OnProgressionDataFailed
        );
    }


    // =========================================================
    // PROGRESJA - DATA RECEIVED
    // =========================================================

    private void OnProgressionDataReceived(
        GetUserDataResult result)
    {
        double loadedXP = 0.0;


        int loadedLevel =
            Mathf.Max(
                1,
                startingLevel
            );


        bool hasXP = false;
        bool hasLevel = false;


        if (
            result != null &&
            result.Data != null)
        {
            // -------------------------------------------------
            // XP
            // -------------------------------------------------

            if (
                result.Data.ContainsKey(
                    PLAYER_XP_KEY
                ))
            {
                string xpValue =
                    result.Data[
                        PLAYER_XP_KEY
                    ].Value;


                if (
                    double.TryParse(
                        xpValue,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out double parsedXP))
                {
                    if (
                        !double.IsNaN(parsedXP) &&
                        !double.IsInfinity(parsedXP) &&
                        parsedXP >= 0.0)
                    {
                        loadedXP =
                            parsedXP;

                        hasXP = true;
                    }
                }
            }


            // -------------------------------------------------
            // LEVEL
            // -------------------------------------------------

            if (
                result.Data.ContainsKey(
                    PLAYER_LEVEL_KEY
                ))
            {
                string levelValue =
                    result.Data[
                        PLAYER_LEVEL_KEY
                    ].Value;


                if (
                    int.TryParse(
                        levelValue,
                        out int parsedLevel))
                {
                    loadedLevel =
                        Mathf.Clamp(
                            parsedLevel,
                            1,
                            maximumLevel
                        );

                    hasLevel = true;
                }
            }
        }


        PlayerXP =
            loadedXP;


        PlayerLevel =
            Mathf.Clamp(
                loadedLevel,
                1,
                maximumLevel
            );


        if (!hasXP || !hasLevel)
            SaveInitialProgressionData();
        else
            IsProgressionLoaded = true;
    }


    // =========================================================
    // PROGRESJA - LOAD FAILED
    // =========================================================

    private void OnProgressionDataFailed(
        PlayFabError error)
    {
        PlayerXP = 0.0;


        PlayerLevel =
            Mathf.Max(
                1,
                startingLevel
            );


        IsProgressionLoaded = false;
    }


    // =========================================================
    // PROGRESJA - INITIAL DATA
    // =========================================================

    private void SaveInitialProgressionData()
    {
        if (!IsLoggedIn)
            return;


        PlayerXP = 0.0;


        PlayerLevel =
            Mathf.Clamp(
                startingLevel,
                1,
                maximumLevel
            );


        Dictionary<string, string> data =
            new Dictionary<string, string>
            {
                {
                    PLAYER_XP_KEY,
                    PlayerXP.ToString(
                        CultureInfo.InvariantCulture
                    )
                },
                {
                    PLAYER_LEVEL_KEY,
                    PlayerLevel.ToString()
                }
            };


        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest
            {
                Data = data
            },
            result =>
            {
                IsProgressionLoaded = true;
            },
            error =>
            {
                IsProgressionLoaded = false;
            }
        );
    }


    // =========================================================
    // XP REQUIRED
    // =========================================================

    public double GetXPRequiredForNextLevel()
    {
        if (PlayerLevel >= maximumLevel)
            return 0.0;


        return GetXPRequiredForLevel(
            PlayerLevel
        );
    }


    public double GetXPRequiredForLevel(
        int level)
    {
        level =
            Mathf.Clamp(
                level,
                1,
                maximumLevel
            );


        if (level >= maximumLevel)
            return 0.0;


        if (baseXPToLevel2 <= 0.0)
            return 0.0;


        if (xpMultiplier <= 0.0)
            return baseXPToLevel2;


        int exponent =
            level - 1;


        return
            baseXPToLevel2 *
            Math.Pow(
                xpMultiplier,
                exponent
            );
    }


    // =========================================================
    // XP INTO CURRENT LEVEL
    // =========================================================

    public double GetXPIntoCurrentLevel()
    {
        if (PlayerLevel >= maximumLevel)
            return 0.0;


        double xpBeforeCurrentLevel =
            GetTotalXPRequiredForLevel(
                PlayerLevel
            );


        return Math.Max(
            0.0,
            PlayerXP -
            xpBeforeCurrentLevel
        );
    }


    // =========================================================
    // XP REMAINING
    // =========================================================

    public double GetXPRemainingToNextLevel()
    {
        if (PlayerLevel >= maximumLevel)
            return 0.0;


        double currentLevelStartXP =
            GetTotalXPRequiredForLevel(
                PlayerLevel
            );


        double nextLevelStartXP =
            GetTotalXPRequiredForLevel(
                PlayerLevel + 1
            );


        return Math.Max(
            0.0,
            nextLevelStartXP -
            PlayerXP
        );
    }


    // =========================================================
    // TOTAL XP REQUIRED
    // =========================================================

    private double GetTotalXPRequiredForLevel(
        int level)
    {
        if (level <= 1)
            return 0.0;


        double totalXP = 0.0;


        for (
            int i = 1;
            i < level;
            i++)
        {
            totalXP +=
                GetXPRequiredForLevel(
                    i
                );
        }


        return totalXP;
    }


    // =========================================================
    // ADD GAME OVER XP
    // =========================================================

    public void AddGameOverXP(
        double damageDealt,
        Action<XPResult> callback = null)
    {
        if (!IsLoggedIn)
        {
            callback?.Invoke(
                new XPResult
                {
                    success = false
                }
            );

            return;
        }


        if (
            double.IsNaN(damageDealt) ||
            double.IsInfinity(damageDealt) ||
            damageDealt < 0.0)
        {
            damageDealt = 0.0;
        }


        PlayFabClientAPI.GetUserData(
            new GetUserDataRequest(),
            result =>
            {
                ProcessGameOverXP(
                    result,
                    damageDealt,
                    callback
                );
            },
            error =>
            {
                callback?.Invoke(
                    new XPResult
                    {
                        success = false
                    }
                );
            }
        );
    }


    // =========================================================
    // PROCESS GAME OVER XP
    // =========================================================

    private void ProcessGameOverXP(
        GetUserDataResult result,
        double damageDealt,
        Action<XPResult> callback)
    {
        double currentXP = 0.0;


        int currentLevel =
            Mathf.Clamp(
                startingLevel,
                1,
                maximumLevel
            );


        if (
            result != null &&
            result.Data != null &&
            result.Data.ContainsKey(
                PLAYER_XP_KEY))
        {
            string xpValue =
                result.Data[
                    PLAYER_XP_KEY
                ].Value;


            if (
                double.TryParse(
                    xpValue,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double parsedXP))
            {
                if (
                    !double.IsNaN(parsedXP) &&
                    !double.IsInfinity(parsedXP) &&
                    parsedXP >= 0.0)
                {
                    currentXP =
                        parsedXP;
                }
            }
        }


        if (
            result != null &&
            result.Data != null &&
            result.Data.ContainsKey(
                PLAYER_LEVEL_KEY))
        {
            string levelValue =
                result.Data[
                    PLAYER_LEVEL_KEY
                ].Value;


            if (
                int.TryParse(
                    levelValue,
                    out int parsedLevel))
            {
                currentLevel =
                    Mathf.Clamp(
                        parsedLevel,
                        1,
                        maximumLevel
                    );
            }
        }


        double oldXP =
            currentXP;


        int oldLevel =
            currentLevel;


        currentXP +=
            damageDealt;


        while (
            currentLevel <
            maximumLevel)
        {
            double requiredTotalXP =
                GetTotalXPRequiredForLevel(
                    currentLevel + 1
                );


            if (currentXP < requiredTotalXP)
                break;


            currentLevel++;
        }


        if (currentLevel >= maximumLevel)
            currentLevel = maximumLevel;


        PlayerXP =
            currentXP;


        PlayerLevel =
            currentLevel;


        IsProgressionLoaded = true;


        Dictionary<string, string> data =
            new Dictionary<string, string>
            {
                {
                    PLAYER_XP_KEY,
                    currentXP.ToString(
                        CultureInfo.InvariantCulture
                    )
                },
                {
                    PLAYER_LEVEL_KEY,
                    currentLevel.ToString()
                }
            };


        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest
            {
                Data = data
            },
            saveResult =>
            {
                callback?.Invoke(
                    new XPResult
                    {
                        success = true,
                        xpGained = damageDealt,
                        oldXP = oldXP,
                        newXP = currentXP,
                        oldLevel = oldLevel,
                        newLevel = currentLevel,
                        levelsGained =
                            currentLevel -
                            oldLevel
                    }
                );
            },
            error =>
            {
                callback?.Invoke(
                    new XPResult
                    {
                        success = false,
                        xpGained = damageDealt,
                        oldXP = oldXP,
                        newXP = currentXP,
                        oldLevel = oldLevel,
                        newLevel = currentLevel,
                        levelsGained =
                            currentLevel -
                            oldLevel
                    }
                );
            }
        );
    }


    // =========================================================
    // PROGRESJA GETTERS
    // =========================================================

    public double GetPlayerXP()
    {
        return PlayerXP;
    }


    public int GetPlayerLevel()
    {
        return PlayerLevel;
    }


    public int GetMaximumLevel()
    {
        return maximumLevel;
    }


    public double GetBaseXPToLevel2()
    {
        return baseXPToLevel2;
    }


    public double GetXPMultiplier()
    {
        return xpMultiplier;
    }


    // =========================================================
    // XP RESULT
    // =========================================================

    public class XPResult
    {
        public bool success;

        public double xpGained;

        public double oldXP;
        public double newXP;

        public int oldLevel;
        public int newLevel;

        public int levelsGained;
    }


    // =========================================================
    // GAME ACCESS UI
    // =========================================================

    public void RegisterGameAccessUI(
        GameAccessBlockUI ui)
    {
        if (ui == null)
            return;


        gameAccessUI = ui;


        if (GameAccessChecked)
            ApplyGameAccessToUI();
    }


    public void UnregisterGameAccessUI(
        GameAccessBlockUI ui)
    {
        if (gameAccessUI == ui)
            gameAccessUI = null;
    }


    // =========================================================
    // CHECK GAME ACCESS
    // =========================================================

    private void CheckGameAccess()
    {
        if (!IsLoggedIn)
        {
            GameAccessChecked = true;

            IsGameEnabled = false;

            ApplyGameAccessToUI();

            return;
        }


        PlayFabClientAPI.GetTitleData(
            new GetTitleDataRequest
            {
                Keys = new List<string>
                {
                    GAME_ENABLED_KEY
                }
            },
            OnGameAccessReceived,
            OnGameAccessCheckFailed
        );
    }


    // =========================================================
    // GAME ACCESS RECEIVED
    // =========================================================

    private void OnGameAccessReceived(
        GetTitleDataResult result)
    {
        GameAccessChecked = true;


        if (
            result == null ||
            result.Data == null ||
            !result.Data.ContainsKey(
                GAME_ENABLED_KEY))
        {
            IsGameEnabled = false;

            ApplyGameAccessToUI();

            return;
        }


        string value =
            result.Data[
                GAME_ENABLED_KEY
            ];


        bool enabled;


        if (
            !bool.TryParse(
                value,
                out enabled))
        {
            IsGameEnabled = false;

            ApplyGameAccessToUI();

            return;
        }


        IsGameEnabled =
            enabled;


        ApplyGameAccessToUI();
    }


    // =========================================================
    // GAME ACCESS FAILED
    // =========================================================

    private void OnGameAccessCheckFailed(
        PlayFabError error)
    {
        GameAccessChecked = true;

        IsGameEnabled = false;

        ApplyGameAccessToUI();
    }


    // =========================================================
    // APPLY GAME ACCESS
    // =========================================================

    private void ApplyGameAccessToUI()
    {
        if (gameAccessUI == null)
            return;


        if (IsGameEnabled)
        {
            gameAccessUI.Hide();
        }
        else
        {
            gameAccessUI.Show(
                "Testy zostały zakończone.\n\n" +
                "Dziękuję!"
            );
        }
    }


    // =========================================================
    // GAME OVER
    // =========================================================

    public void SaveGameOverStats(
        int enemiesKilled,
        double damageDealt,
        int highestWave,
        int goldEarned,
        int gameDuration,
        int skillsPurchased,
        int upgradesPurchased)
    {
        if (enemiesKilled < 0)
            enemiesKilled = 0;


        if (
            double.IsNaN(damageDealt) ||
            double.IsInfinity(damageDealt) ||
            damageDealt < 0.0)
        {
            damageDealt = 0.0;
        }


        if (highestWave < 0)
            highestWave = 0;


        if (goldEarned < 0)
            goldEarned = 0;


        if (gameDuration < 0)
            gameDuration = 0;


        if (skillsPurchased < 0)
            skillsPurchased = 0;


        if (upgradesPurchased < 0)
            upgradesPurchased = 0;


        if (gameOverStatsSaved)
            return;


        string buildSkills = "";
        string buildUpgrades = "";
        string buildSummary = "";


        if (GameStatsManager.Instance != null)
        {
            buildSkills =
                GameStatsManager.Instance
                    .GetPurchasedSkillsSummary();


            buildUpgrades =
                GameStatsManager.Instance
                    .GetPurchasedUpgradesSummary();


            buildSummary =
                GameStatsManager.Instance
                    .GetBuildSummary();
        }


        if (!IsLoggedIn)
        {
            pendingGameOverStats = true;


            pendingEnemiesKilled =
                enemiesKilled;

            pendingDamageDealt =
                damageDealt;

            pendingHighestWave =
                highestWave;

            pendingGoldEarned =
                goldEarned;

            pendingGameDuration =
                gameDuration;

            pendingSkillsPurchased =
                skillsPurchased;

            pendingUpgradesPurchased =
                upgradesPurchased;


            pendingBuildSkills =
                buildSkills;

            pendingBuildUpgrades =
                buildUpgrades;

            pendingBuildSummary =
                buildSummary;


            return;
        }


        SendGameOverStats(
            enemiesKilled,
            damageDealt,
            highestWave,
            goldEarned,
            gameDuration,
            skillsPurchased,
            upgradesPurchased,
            buildSkills,
            buildUpgrades,
            buildSummary
        );
    }


    // =========================================================
    // SAVE PENDING GAME OVER
    // =========================================================

    private void SavePendingGameOverStats()
    {
        if (!pendingGameOverStats)
            return;


        SendGameOverStats(
            pendingEnemiesKilled,
            pendingDamageDealt,
            pendingHighestWave,
            pendingGoldEarned,
            pendingGameDuration,
            pendingSkillsPurchased,
            pendingUpgradesPurchased,
            pendingBuildSkills,
            pendingBuildUpgrades,
            pendingBuildSummary
        );
    }


    // =========================================================
    // SEND GAME OVER STATS
    // =========================================================

    private void SendGameOverStats(
        int enemiesKilled,
        double damageDealt,
        int highestWave,
        int goldEarned,
        int gameDuration,
        int skillsPurchased,
        int upgradesPurchased,
        string buildSkills,
        string buildUpgrades,
        string buildSummary)
    {
        if (!IsLoggedIn)
            return;


        List<StatisticUpdate> statistics =
            new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName =
                        "EnemiesKilled",

                    Value =
                        enemiesKilled
                },

                new StatisticUpdate
                {
                    StatisticName =
                        "HighestWave",

                    Value =
                        highestWave
                },

                new StatisticUpdate
                {
                    StatisticName =
                        "GoldEarned",

                    Value =
                        goldEarned
                },

                new StatisticUpdate
                {
                    StatisticName =
                        "GameDuration",

                    Value =
                        gameDuration
                },

                new StatisticUpdate
                {
                    StatisticName =
                        "GamesPlayed",

                    Value =
                        1
                },

                new StatisticUpdate
                {
                    StatisticName =
                        "TotalEnemiesKilled",

                    Value =
                        enemiesKilled
                },

                new StatisticUpdate
                {
                    StatisticName =
                        "TotalGoldEarned",

                    Value =
                        goldEarned
                },

                new StatisticUpdate
                {
                    StatisticName =
                        "TotalGameTime",

                    Value =
                        gameDuration
                },

                new StatisticUpdate
                {
                    StatisticName =
                        "TotalSkillsPurchased",

                    Value =
                        skillsPurchased
                },

                new StatisticUpdate
                {
                    StatisticName =
                        "TotalUpgradesPurchased",

                    Value =
                        upgradesPurchased
                }
            };


        PlayFabClientAPI.GetPlayerStatistics(
            new GetPlayerStatisticsRequest(),
            result =>
            {
                int previousHighestWave = 0;

                bool hasPreviousHighestWave = false;


                if (
                    result != null &&
                    result.Statistics != null)
                {
                    foreach (
                        StatisticValue statistic
                        in result.Statistics)
                    {
                        if (statistic == null)
                            continue;


                        if (
                            statistic.StatisticName !=
                            "HighestWave")
                        {
                            continue;
                        }


                        previousHighestWave =
                            statistic.Value;


                        hasPreviousHighestWave =
                            true;


                        break;
                    }
                }


                bool isNewHighestWave =
                    !hasPreviousHighestWave ||
                    highestWave >
                    previousHighestWave;


                PlayFabClientAPI.UpdatePlayerStatistics(
                    new UpdatePlayerStatisticsRequest
                    {
                        Statistics =
                            statistics
                    },
                    updateResult =>
                    {
                        SaveDamageDealt(
                            damageDealt
                        );


                        if (isNewHighestWave)
                        {
                            SaveGameBuildData(
                                damageDealt,
                                buildSkills,
                                buildUpgrades,
                                buildSummary
                            );
                        }


                        gameOverStatsSaved =
                            true;

                        pendingGameOverStats =
                            false;
                    },
                    error =>
                    {
                    }
                );
            },
            error =>
            {
            }
        );
    }


    // =========================================================
    // SAVE DAMAGE DEALT
    // =========================================================

    private void SaveDamageDealt(
        double damageDealt)
    {
        if (!IsLoggedIn)
            return;


        Dictionary<string, string> data =
            new Dictionary<string, string>
            {
                {
                    "DamageDealt",
                    damageDealt.ToString(
                        CultureInfo.InvariantCulture
                    )
                }
            };


        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest
            {
                Data = data
            },
            result =>
            {
            },
            error =>
            {
            }
        );
    }


    // =========================================================
    // SAVE GAME BUILD DATA
    // =========================================================

    private void SaveGameBuildData(
        double damageDealt,
        string buildSkills,
        string buildUpgrades,
        string buildSummary)
    {
        if (!IsLoggedIn)
            return;


        Dictionary<string, string> data =
            new Dictionary<string, string>
            {
                {
                    "DamageDealt",
                    damageDealt.ToString(
                        CultureInfo.InvariantCulture
                    )
                },

                {
                    "BuildSkills",
                    buildSkills ?? ""
                },

                {
                    "BuildUpgrades",
                    buildUpgrades ?? ""
                },

                {
                    "BuildSummary",
                    buildSummary ?? ""
                }
            };


        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest
            {
                Data = data
            },
            result =>
            {
            },
            error =>
            {
            }
        );
    }
}