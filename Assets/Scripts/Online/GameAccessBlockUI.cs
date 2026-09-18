using TMPro;
using UnityEngine;

public class GameAccessBlockUI : MonoBehaviour
{
    [Header("Panel blokady")]
    [SerializeField] private GameObject panel;

    [Header("Tekst")]
    [SerializeField] private TMP_Text messageText;


    private bool registered = false;


    private void Awake()
    {
        // Na początku panel ma być ukryty.
        if (panel != null)
        {
            panel.SetActive(false);
        }
        else
        {
            Debug.LogError("GameAccessBlockUI: panel nie jest przypisany!");
        }
    }


    private void Update()
    {
        // Jeżeli PlayFabManager jeszcze nie istniał
        // w momencie OnEnable(), próbujemy ponownie.
        if (!registered && PlayFabManager.Instance != null)
        {
            RegisterToPlayFab();
        }
    }


    private void OnEnable()
    {
        RegisterToPlayFab();
    }


    private void OnDisable()
    {
        if (PlayFabManager.Instance != null)
        {
            PlayFabManager.Instance.UnregisterGameAccessUI(this);
        }

        registered = false;
    }


    private void RegisterToPlayFab()
    {
        if (registered)
            return;

        if (PlayFabManager.Instance == null)
            return;

        PlayFabManager.Instance.RegisterGameAccessUI(this);

        registered = true;

        Debug.Log("GameAccessBlockUI: zarejestrowano w PlayFabManager.");
    }


    public void Show(string message)
    {
        Debug.Log("GameAccessBlockUI: SHOW BLOCK PANEL");

        if (messageText != null)
        {
            messageText.text = message;
        }
        else
        {
            Debug.LogError("GameAccessBlockUI: messageText nie jest przypisany!");
        }

        if (panel != null)
        {
            panel.SetActive(true);
        }
        else
        {
            Debug.LogError("GameAccessBlockUI: panel nie jest przypisany!");
        }
    }


    public void Hide()
    {
        Debug.Log("GameAccessBlockUI: HIDE BLOCK PANEL");

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
}