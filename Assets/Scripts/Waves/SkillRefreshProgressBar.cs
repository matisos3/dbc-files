using UnityEngine;
using UnityEngine.UI;

public class SkillRefreshProgressBar : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private Image fillImage;


    // =========================================================
    // USTAWIENIE PROGRESU
    // =========================================================

    public void SetProgress(float progress)
    {
        if (fillImage == null)
            return;

        fillImage.fillAmount =
            Mathf.Clamp01(progress);
    }


    // =========================================================
    // RESET
    // =========================================================

    public void ResetProgress()
    {
        if (fillImage == null)
            return;

        fillImage.fillAmount = 0f;
    }
}