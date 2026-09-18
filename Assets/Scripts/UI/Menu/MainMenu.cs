using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;

public class MainMenu : MonoBehaviour
{
    // =========================================================
    // MAIN MENU
    // =========================================================

    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button playButton;


    // =========================================================
    // PROFILE EDITOR
    // =========================================================

    [Header("Profile Editor")]
    [SerializeField] private GameObject profilePanel;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_Text profileMessageText;
    [SerializeField] private Button confirmProfileButton;


    // =========================================================
    // PLAYER PROFILE
    // =========================================================

    [Header("Player Profile")]
    [SerializeField] private GameObject playerProfilePanel;
    [SerializeField] private PlayerAvatarDisplay playerAvatarDisplay;


    // =========================================================
    // AVATAR SELECTION
    // =========================================================

    [Header("Avatar Selection")]
    [SerializeField] private Transform avatarContainer;
    [SerializeField] private AvatarButton avatarButtonPrefab;
    [SerializeField] private Sprite[] avatarSprites;


    // =========================================================
    // PLAYER PROGRESSION
    // =========================================================

    [Header("Player Progression")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text xpText;
    [SerializeField] private Image xpBarFill;


    // =========================================================
    // GAME
    // =========================================================

    [Header("Game")]
    [SerializeField] private string gameSceneName = "GameScene";


    // =========================================================
    // LOCALIZATION - PROFILE
    // =========================================================

    [Header("Localization - Profile")]
    [SerializeField]
    private LocalizedString profileCreateText;

    [SerializeField]
    private LocalizedString profileEditText;

    [SerializeField]
    private LocalizedString profileSavingText;

    [SerializeField]
    private LocalizedString profileAvatarSelectedText;

    [SerializeField]
    private LocalizedString profileNameMinText;

    [SerializeField]
    private LocalizedString profileNameMaxText;

    [SerializeField]
    private LocalizedString profileSelectAvatarText;

    [SerializeField]
    private LocalizedString profileNameChangeUsedText;


    // =========================================================
    // LOCALIZATION - PROGRESSION
    // =========================================================

    [Header("Localization - Progression")]
    [SerializeField]
    private LocalizedString profileLevelText;

    [SerializeField]
    private LocalizedString profileXPText;

    [SerializeField]
    private LocalizedString profileMaxText;


    // =========================================================
    // STATE
    // =========================================================

    private string selectedAvatarId = "";
    private int selectedAvatarIndex = -1;

    private bool profileSaving = false;

    private bool stateChecked = false;

    private bool progressionCheckStarted = false;

    private bool initializationFinished = false;

    private AvatarButton[] generatedAvatarButtons;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // -----------------------------------------------------
        // LOADING SCREEN
        // -----------------------------------------------------

        if (GameLoadingUI.Instance != null)
            GameLoadingUI.Instance.Show();


        // -----------------------------------------------------
        // HIDE ALL MENU PANELS DURING INITIALIZATION
        // -----------------------------------------------------

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (profilePanel != null)
            profilePanel.SetActive(false);

        if (playerProfilePanel != null)
            playerProfilePanel.SetActive(false);


        // -----------------------------------------------------
        // DISABLE BUTTONS DURING LOADING
        // -----------------------------------------------------

        if (playButton != null)
            playButton.interactable = false;

        if (confirmProfileButton != null)
            confirmProfileButton.interactable = false;


        // -----------------------------------------------------
        // CLEAR UI DURING LOADING
        // -----------------------------------------------------

        if (profileMessageText != null)
            profileMessageText.text = "";

        if (levelText != null)
            levelText.text = "";

        if (xpText != null)
            xpText.text = "";

        if (xpBarFill != null)
            xpBarFill.fillAmount = 0f;


        // -----------------------------------------------------
        // NAME INPUT
        // -----------------------------------------------------

        if (nameInput != null)
        {
            nameInput.onValueChanged.AddListener(
                OnNameChanged
            );
        }


        // -----------------------------------------------------
        // AVATARS
        // -----------------------------------------------------

        GenerateAvatarButtons();


        // -----------------------------------------------------
        // WAIT FOR PLAYFAB
        // -----------------------------------------------------

        InvokeRepeating(
            nameof(CheckPlayFabState),
            0.1f,
            0.2f
        );
    }


    // =========================================================
    // AVATAR BUTTONS
    // =========================================================

    private void GenerateAvatarButtons()
    {
        if (avatarContainer == null)
            return;

        if (avatarButtonPrefab == null)
            return;

        if (avatarSprites == null ||
            avatarSprites.Length == 0)
            return;

        ClearAvatarButtons();

        generatedAvatarButtons =
            new AvatarButton[avatarSprites.Length];

        for (int i = 0;
             i < avatarSprites.Length;
             i++)
        {
            if (avatarSprites[i] == null)
                continue;

            AvatarButton newButton =
                Instantiate(
                    avatarButtonPrefab,
                    avatarContainer
                );

            newButton.name =
                "AvatarButton_" + (i + 1);

            newButton.Initialize(
                this,
                i,
                avatarSprites[i]
            );

            generatedAvatarButtons[i] =
                newButton;
        }
    }


    private void ClearAvatarButtons()
    {
        if (avatarContainer == null)
            return;

        for (int i = avatarContainer.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(
                avatarContainer.GetChild(i).gameObject
            );
        }
    }


    // =========================================================
    // PLAYFAB STATE
    // =========================================================

    private void CheckPlayFabState()
    {
        if (stateChecked)
            return;

        if (PlayFabManager.Instance == null)
            return;

        if (!PlayFabManager.Instance.IsLoggedIn)
            return;

        if (!PlayFabManager.Instance.IsProfileLoaded)
            return;


        // -----------------------------------------------------
        // PROFILE IS READY
        // -----------------------------------------------------

        stateChecked = true;

        CancelInvoke(
            nameof(CheckPlayFabState)
        );


        // -----------------------------------------------------
        // WAIT FOR PROGRESSION TOO
        // -----------------------------------------------------

        StartProgressionCheck();
    }


    // =========================================================
    // PROGRESSION CHECK
    // =========================================================

    private void StartProgressionCheck()
    {
        if (progressionCheckStarted)
            return;

        progressionCheckStarted = true;

        InvokeRepeating(
            nameof(UpdateProgressionWhenReady),
            0.1f,
            0.2f
        );
    }


    private void UpdateProgressionWhenReady()
    {
        if (initializationFinished)
            return;

        if (PlayFabManager.Instance == null)
            return;

        if (!PlayFabManager.Instance.IsLoggedIn)
            return;

        if (!PlayFabManager.Instance.IsProgressionLoaded)
            return;


        // -----------------------------------------------------
        // PROGRESSION IS READY
        // -----------------------------------------------------

        CancelInvoke(
            nameof(UpdateProgressionWhenReady)
        );


        // -----------------------------------------------------
        // PREPARE FINAL UI
        // -----------------------------------------------------

        PrepareFinalUI();


        // -----------------------------------------------------
        // INITIALIZATION FINISHED
        // -----------------------------------------------------

        initializationFinished = true;


        // -----------------------------------------------------
        // HIDE LOADING SCREEN LAST
        // -----------------------------------------------------

        if (GameLoadingUI.Instance != null)
            GameLoadingUI.Instance.Hide();
    }


    // =========================================================
    // PREPARE FINAL UI
    // =========================================================

    private void PrepareFinalUI()
    {
        if (PlayFabManager.Instance == null)
            return;


        bool profileComplete =
            PlayFabManager.Instance
                .IsPlayerProfileComplete();


        // =====================================================
        // COMPLETE PROFILE
        // =====================================================

        if (profileComplete)
        {
            PrepareMainMenu();
        }


        // =====================================================
        // INCOMPLETE PROFILE
        // =====================================================

        else
        {
            PrepareProfileCreation();
        }
    }


    // =========================================================
    // PREPARE MAIN MENU
    // =========================================================

    private void PrepareMainMenu()
    {
        // -----------------------------------------------------
        // PANELS
        // -----------------------------------------------------

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (profilePanel != null)
            profilePanel.SetActive(false);

        if (playerProfilePanel != null)
            playerProfilePanel.SetActive(true);


        // -----------------------------------------------------
        // PLAY BUTTON
        // -----------------------------------------------------

        if (playButton != null)
            playButton.interactable = true;


        // -----------------------------------------------------
        // AVATAR
        // -----------------------------------------------------

        if (playerAvatarDisplay != null)
            playerAvatarDisplay.RefreshAvatar();


        // -----------------------------------------------------
        // PROGRESSION
        // -----------------------------------------------------

        UpdateProgressionUI();
    }


    // =========================================================
    // PREPARE PROFILE CREATION
    // =========================================================

    private void PrepareProfileCreation()
    {
        // -----------------------------------------------------
        // PANELS
        // -----------------------------------------------------

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (playerProfilePanel != null)
            playerProfilePanel.SetActive(false);

        if (profilePanel != null)
            profilePanel.SetActive(true);


        // -----------------------------------------------------
        // PROFILE DATA
        // -----------------------------------------------------

        selectedAvatarId = "";
        selectedAvatarIndex = -1;


        // -----------------------------------------------------
        // NAME
        // -----------------------------------------------------

        if (nameInput != null)
        {
            nameInput.text = "";
            nameInput.interactable = true;
        }


        // -----------------------------------------------------
        // MESSAGE
        // -----------------------------------------------------

        ShowProfileMessage(
            GetLocalizedText(
                profileCreateText
            )
        );


        // -----------------------------------------------------
        // CONFIRM BUTTON
        // -----------------------------------------------------

        if (confirmProfileButton != null)
            confirmProfileButton.interactable = false;


        profileSaving = false;


        // -----------------------------------------------------
        // AVATAR
        // -----------------------------------------------------

        ClearAvatarSelection();
    }


    // =========================================================
    // PROGRESSION UI
    // =========================================================

    private void UpdateProgressionUI()
    {
        if (PlayFabManager.Instance == null)
            return;

        if (!PlayFabManager.Instance.IsProgressionLoaded)
            return;


        int currentLevel =
            PlayFabManager.Instance.PlayerLevel;

        int maximumLevel =
            PlayFabManager.Instance.GetMaximumLevel();


        // -----------------------------------------------------
        // LEVEL
        // -----------------------------------------------------

        if (levelText != null)
        {
            levelText.text =
                GetLocalizedText(
                    profileLevelText,
                    currentLevel
                );
        }


        // -----------------------------------------------------
        // MAX LEVEL
        // -----------------------------------------------------

        if (currentLevel >= maximumLevel)
        {
            if (xpBarFill != null)
                xpBarFill.fillAmount = 1f;

            if (xpText != null)
            {
                xpText.text =
                    GetLocalizedText(
                        profileMaxText
                    );
            }

            return;
        }


        // -----------------------------------------------------
        // XP
        // -----------------------------------------------------

        double xpIntoLevel =
            PlayFabManager.Instance
                .GetXPIntoCurrentLevel();

        double xpRequired =
            PlayFabManager.Instance
                .GetXPRequiredForNextLevel();


        float fillAmount = 0f;

        if (xpRequired > 0.0)
        {
            fillAmount =
                (float)(
                    xpIntoLevel /
                    xpRequired
                );
        }


        // -----------------------------------------------------
        // XP BAR
        // -----------------------------------------------------

        if (xpBarFill != null)
        {
            xpBarFill.fillAmount =
                Mathf.Clamp01(
                    fillAmount
                );
        }


        // -----------------------------------------------------
        // XP TEXT
        // -----------------------------------------------------

        if (xpText != null)
        {
            string currentXP =
                FormatXP(xpIntoLevel);

            string requiredXP =
                FormatXP(xpRequired);

            xpText.text =
                GetLocalizedText(
                    profileXPText,
                    currentXP,
                    requiredXP
                );
        }
    }


    // =========================================================
    // FORMAT XP
    // =========================================================

    private string FormatXP(double xp)
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
            return
                (xp / 1000.0)
                    .ToString("0.##") +
                "K";
        }

        if (xp < 1000000000.0)
        {
            return
                (xp / 1000000.0)
                    .ToString("0.##") +
                "M";
        }

        return
            (xp / 1000000000.0)
                .ToString("0.##") +
            "B";
    }


    // =========================================================
    // OPEN PROFILE EDITOR
    // =========================================================

    public void OpenProfileEditor()
    {
        if (PlayFabManager.Instance == null)
            return;

        if (
            !PlayFabManager.Instance
                .IsPlayerProfileComplete())
        {
            return;
        }


        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (playerProfilePanel != null)
            playerProfilePanel.SetActive(false);

        if (profilePanel != null)
            profilePanel.SetActive(true);


        string currentName =
            PlayFabManager.Instance
                .GetPlayerDisplayName();

        string currentAvatar =
            PlayFabManager.Instance
                .GetPlayerAvatarId();


        // -----------------------------------------------------
        // NAME
        // -----------------------------------------------------

        if (nameInput != null)
        {
            nameInput.text = currentName;

            nameInput.interactable =
                PlayFabManager.Instance
                    .CanChangePlayerName();
        }


        // -----------------------------------------------------
        // AVATAR
        // -----------------------------------------------------

        selectedAvatarId = currentAvatar;

        selectedAvatarIndex =
            GetAvatarIndex(
                currentAvatar
            );

        ClearAvatarSelection();

        if (
            selectedAvatarIndex >= 0 &&
            generatedAvatarButtons != null &&
            selectedAvatarIndex <
                generatedAvatarButtons.Length &&
            generatedAvatarButtons[
                selectedAvatarIndex
            ] != null)
        {
            generatedAvatarButtons[
                selectedAvatarIndex
            ].SetSelected(true);
        }


        // -----------------------------------------------------
        // MESSAGE
        // -----------------------------------------------------

        if (
            PlayFabManager.Instance
                .CanChangePlayerName())
        {
            ShowProfileMessage(
                GetLocalizedText(
                    profileEditText
                )
            );
        }
        else
        {
            ShowProfileMessage(
                GetLocalizedText(
                    profileNameChangeUsedText
                )
            );
        }


        profileSaving = false;

        UpdateConfirmButton();
    }


    // =========================================================
    // NAME CHANGED
    // =========================================================

    private void OnNameChanged(string value)
    {
        UpdateConfirmButton();
    }


    // =========================================================
    // SELECT AVATAR
    // =========================================================

    public void SelectAvatar(int index)
    {
        if (generatedAvatarButtons == null)
            return;

        if (
            index < 0 ||
            index >= generatedAvatarButtons.Length)
        {
            return;
        }

        if (generatedAvatarButtons[index] == null)
            return;


        selectedAvatarIndex = index;

        selectedAvatarId =
            "Avatar_" + (index + 1);


        ClearAvatarSelection();

        generatedAvatarButtons[index]
            .SetSelected(true);


        UpdateConfirmButton();


        ShowProfileMessage(
            GetLocalizedText(
                profileAvatarSelectedText
            )
        );
    }


    // =========================================================
    // GET AVATAR INDEX
    // =========================================================

    private int GetAvatarIndex(string avatarId)
    {
        const string prefix = "Avatar_";

        if (string.IsNullOrEmpty(avatarId))
            return -1;

        if (!avatarId.StartsWith(prefix))
            return -1;

        string numberPart =
            avatarId.Substring(
                prefix.Length
            );

        int avatarNumber;

        if (
            !int.TryParse(
                numberPart,
                out avatarNumber))
        {
            return -1;
        }

        return avatarNumber - 1;
    }


    // =========================================================
    // CLEAR AVATAR SELECTION
    // =========================================================

    private void ClearAvatarSelection()
    {
        if (generatedAvatarButtons == null)
            return;

        for (int i = 0;
             i < generatedAvatarButtons.Length;
             i++)
        {
            if (generatedAvatarButtons[i] == null)
                continue;

            generatedAvatarButtons[i]
                .SetSelected(false);
        }
    }


    // =========================================================
    // CONFIRM BUTTON
    // =========================================================

    private void UpdateConfirmButton()
    {
        if (confirmProfileButton == null)
            return;

        if (profileSaving)
        {
            confirmProfileButton.interactable =
                false;

            return;
        }


        string playerName = "";

        if (nameInput != null)
        {
            playerName =
                nameInput.text.Trim();
        }


        bool validName =
            playerName.Length >= 3 &&
            playerName.Length <= 25;

        bool validAvatar =
            !string.IsNullOrEmpty(
                selectedAvatarId
            );


        confirmProfileButton.interactable =
            validName &&
            validAvatar;
    }


    // =========================================================
    // CONFIRM PROFILE
    // =========================================================

    public void ConfirmProfile()
    {
        if (profileSaving)
            return;

        if (PlayFabManager.Instance == null)
            return;

        if (!PlayFabManager.Instance.IsLoggedIn)
            return;


        string playerName = "";

        if (nameInput != null)
        {
            playerName =
                nameInput.text.Trim();
        }


        // -----------------------------------------------------
        // NAME MIN
        // -----------------------------------------------------

        if (playerName.Length < 3)
        {
            ShowProfileMessage(
                GetLocalizedText(
                    profileNameMinText
                )
            );

            return;
        }


        // -----------------------------------------------------
        // NAME MAX
        // -----------------------------------------------------

        if (playerName.Length > 25)
        {
            ShowProfileMessage(
                GetLocalizedText(
                    profileNameMaxText
                )
            );

            return;
        }


        // -----------------------------------------------------
        // AVATAR
        // -----------------------------------------------------

        if (string.IsNullOrEmpty(selectedAvatarId))
        {
            ShowProfileMessage(
                GetLocalizedText(
                    profileSelectAvatarText
                )
            );

            return;
        }


        // -----------------------------------------------------
        // SAVE
        // -----------------------------------------------------

        profileSaving = true;


        if (confirmProfileButton != null)
        {
            confirmProfileButton.interactable =
                false;
        }


        if (nameInput != null)
            nameInput.interactable = false;


        ShowProfileMessage(
            GetLocalizedText(
                profileSavingText
            )
        );


        PlayFabManager.Instance.SetPlayerProfile(
            playerName,
            selectedAvatarId,
            OnProfileSaved
        );
    }


    // =========================================================
    // PROFILE SAVED
    // =========================================================

    private void OnProfileSaved(
        bool success,
        string message)
    {
        profileSaving = false;


        if (!success)
        {
            if (nameInput != null)
            {
                nameInput.interactable =
                    PlayFabManager.Instance
                        .CanChangePlayerName();
            }


            ShowProfileMessage(message);

            UpdateConfirmButton();

            return;
        }


        // -----------------------------------------------------
        // REFRESH AVATAR
        // -----------------------------------------------------

        if (playerAvatarDisplay != null)
            playerAvatarDisplay.RefreshAvatar();


        // -----------------------------------------------------
        // CLOSE PROFILE
        // -----------------------------------------------------

        if (profilePanel != null)
            profilePanel.SetActive(false);


        // -----------------------------------------------------
        // SHOW MAIN MENU
        // -----------------------------------------------------

        PrepareMainMenu();
    }


    // =========================================================
    // PROFILE MESSAGE
    // =========================================================

    private void ShowProfileMessage(
        string message)
    {
        if (profileMessageText != null)
            profileMessageText.text = message;
    }


    // =========================================================
    // LOCALIZATION HELPER
    // =========================================================

    private string GetLocalizedText(
        LocalizedString localizedString,
        params object[] arguments)
    {
        if (localizedString == null)
            return "";

        if (localizedString.IsEmpty)
            return "";

        return localizedString.GetLocalizedString(
            arguments
        );
    }


    // =========================================================
    // PLAY GAME
    // =========================================================

    public void PlayGame()
    {
        if (PlayFabManager.Instance == null)
            return;

        if (
            !PlayFabManager.Instance
                .IsPlayerProfileComplete())
        {
            return;
        }

        SceneManager.LoadScene(
            gameSceneName
        );
    }


    // =========================================================
    // QUIT
    // =========================================================

    public void QuitGame()
    {
        Application.Quit();
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        CancelInvoke(
            nameof(CheckPlayFabState)
        );

        CancelInvoke(
            nameof(UpdateProgressionWhenReady)
        );

        if (nameInput != null)
        {
            nameInput.onValueChanged.RemoveListener(
                OnNameChanged
            );
        }
    }
}