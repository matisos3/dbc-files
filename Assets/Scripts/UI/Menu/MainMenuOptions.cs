using UnityEngine;

public class MainMenuOptions : MonoBehaviour
{
    [Header("Menu główne")]
    [SerializeField] private GameObject mainMenuPanel;

    [Header("Panel opcji")]
    [SerializeField] private GameObject optionsMenu;


    private void Start()
    {
        // Na początku pokazujemy menu główne.
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        // Panel opcji jest zamknięty.
        if (optionsMenu != null)
        {
            optionsMenu.SetActive(false);
        }
    }


    // =========================================================
    // OTWÓRZ OPCJE
    // =========================================================

    public void OpenOptions()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (optionsMenu != null)
        {
            optionsMenu.SetActive(true);
        }
    }


    // =========================================================
    // ZAMKNIJ OPCJE
    // =========================================================

    public void CloseOptions()
    {
        if (optionsMenu != null)
        {
            optionsMenu.SetActive(false);
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }
}