using UnityEngine;
using UnityEngine.UI;

public class AvatarButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image avatarImage;
    [SerializeField] private GameObject selectionObject;

    private int avatarIndex;
    private MainMenu mainMenu;


    // =========================================================
    // INICJALIZACJA
    // =========================================================

    public void Initialize(
        MainMenu menu,
        int index,
        Sprite avatarSprite)
    {
        mainMenu = menu;
        avatarIndex = index;

        if (avatarImage != null)
            avatarImage.sprite = avatarSprite;

        SetSelected(false);

        Button button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }
    }


    // =========================================================
    // KLIKNIĘCIE
    // =========================================================

    private void OnClicked()
    {
        if (mainMenu == null)
            return;

        mainMenu.SelectAvatar(avatarIndex);
    }


    // =========================================================
    // ZAZNACZENIE
    // =========================================================

    public void SetSelected(bool selected)
    {
        if (selectionObject != null)
            selectionObject.SetActive(selected);
    }
}