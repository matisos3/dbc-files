using UnityEngine;
using TMPro;
using UnityEngine.Localization;

public class GameLoadingUI : MonoBehaviour
{
    public static GameLoadingUI Instance { get; private set; }

    // =========================================================
    // UI
    // =========================================================

    [Header("Loading UI")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_Text loadingText;


    // =========================================================
    // LOCALIZATION
    // =========================================================

    [Header("Localization")]
    [SerializeField] private LocalizedString loadingTextLocalized;


    // =========================================================
    // STATE
    // =========================================================

    private bool isShowing = false;


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

        HideImmediate();
    }


    // =========================================================
    // SHOW
    // =========================================================

    public void Show()
    {
        Show(
            loadingTextLocalized
        );
    }


    public void Show(
        LocalizedString localizedText)
    {
        isShowing = true;

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        SetLoadingText(localizedText);
    }


    // =========================================================
    // HIDE
    // =========================================================

    public void Hide()
    {
        isShowing = false;

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }


    // =========================================================
    // HIDE IMMEDIATE
    // =========================================================

    public void HideImmediate()
    {
        isShowing = false;

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }


    // =========================================================
    // SET TEXT
    // =========================================================

    public void SetLoadingText(
        LocalizedString localizedText)
    {
        if (loadingText == null)
            return;

        if (localizedText == null ||
            localizedText.IsEmpty)
        {
            loadingText.text = "";
            return;
        }

        loadingText.text =
            localizedText.GetLocalizedString();
    }


    // =========================================================
    // SIMPLE TEXT
    // =========================================================

    public void SetLoadingText(
        string text)
    {
        if (loadingText == null)
            return;

        loadingText.text =
            text ?? "";
    }


    // =========================================================
    // STATE
    // =========================================================

    public bool IsShowing()
    {
        return isShowing;
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}