using UnityEngine;
using TMPro;

public class PlayerNameDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;


    private void Start()
    {
        RefreshName();
    }


    public void RefreshName()
    {
        if (playerNameText == null)
            return;

        if (PlayFabManager.Instance == null)
            return;

        if (!PlayFabManager.Instance.IsLoggedIn)
            return;

        if (!PlayFabManager.Instance.IsProfileLoaded)
            return;

        playerNameText.text =
            PlayFabManager.Instance.GetPlayerDisplayName();
    }
}