using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("Menu pauzy")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject optionsMenu;

    private bool isPaused = false;


    private void Start()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }

        Time.timeScale = 1f;
    }


    private void Update()
    {
        // =====================================================
        // ESC - PAUZA
        // =====================================================

        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }


    // =========================================================
    // PAUZA
    // =========================================================

    public void PauseGame()
    {
        isPaused = true;

        Time.timeScale = 0f;

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
        }
    }


    // =========================================================
    // WZNÓW
    // =========================================================

    public void ResumeGame()
    {
        isPaused = false;

        Time.timeScale = 1f;

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
    }


    // =========================================================
    // OPCJE
    // =========================================================

    public void OpenOptions()
{
    if (pauseMenu != null)
        pauseMenu.SetActive(false);

    if (optionsMenu != null)
        optionsMenu.SetActive(true);
}


public void CloseOptions()
{
    if (optionsMenu != null)
        optionsMenu.SetActive(false);

    if (pauseMenu != null)
        pauseMenu.SetActive(true);
}


    // =========================================================
    // WYJŚCIE Z GRY
    // =========================================================

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}