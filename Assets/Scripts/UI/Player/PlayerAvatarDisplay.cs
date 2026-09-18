using UnityEngine;
using UnityEngine.UI;

public class PlayerAvatarDisplay : MonoBehaviour
{
    [Header("Avatar")]
    [SerializeField] private Image avatarImage;

    [Header("Avatar Sprites")]
    [SerializeField] private Sprite[] avatarSprites;

    [Header("Settings")]
    [SerializeField] private bool hideIfAvatarIsMissing = false;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        RefreshAvatar();
    }


    // =========================================================
    // ODŚWIEŻENIE AVATARA
    // =========================================================

    public void RefreshAvatar()
    {
        if (PlayFabManager.Instance == null)
            return;

        if (!PlayFabManager.Instance.IsLoggedIn)
            return;

        if (!PlayFabManager.Instance.IsProfileLoaded)
            return;

        string avatarId = PlayFabManager.Instance.GetPlayerAvatarId();

        SetAvatar(avatarId);
    }


    // =========================================================
    // USTAWIENIE AVATARA PO ID
    // =========================================================

    public void SetAvatar(string avatarId)
    {
        if (avatarImage == null)
            return;

        if (string.IsNullOrEmpty(avatarId))
        {
            HandleMissingAvatar();
            return;
        }

        int avatarIndex = GetAvatarIndex(avatarId);

        if (avatarIndex < 0 ||
            avatarIndex >= avatarSprites.Length ||
            avatarSprites[avatarIndex] == null)
        {
            HandleMissingAvatar();
            return;
        }

        avatarImage.sprite = avatarSprites[avatarIndex];
        avatarImage.enabled = true;
    }


    // =========================================================
    // ZAMIANA ID NA INDEX
    // =========================================================

    private int GetAvatarIndex(string avatarId)
    {
        const string prefix = "Avatar_";

        if (!avatarId.StartsWith(prefix))
            return -1;

        string numberPart = avatarId.Substring(prefix.Length);

        int avatarNumber;

        if (!int.TryParse(numberPart, out avatarNumber))
            return -1;

        return avatarNumber - 1;
    }


    // =========================================================
    // BRAK AVATARA
    // =========================================================

    private void HandleMissingAvatar()
    {
        if (hideIfAvatarIsMissing)
        {
            avatarImage.enabled = false;
        }
        else
        {
            avatarImage.sprite = null;
            avatarImage.enabled = true;
        }
    }
}