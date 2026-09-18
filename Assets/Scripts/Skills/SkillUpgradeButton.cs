using UnityEngine;
using UnityEngine.UI;

public class SkillUpgradeButton : MonoBehaviour
{
public GameObject highlight;

private Image image;

private SkillUpgradeData upgrade;
private SkillUpgradeSelectionManager manager;


// =========================================================
// AWAKE
// =========================================================

private void Awake()
{
    image = GetComponent<Image>();
}


// =========================================================
// SETUP
// =========================================================

public void Setup(
    SkillUpgradeData newUpgrade,
    SkillUpgradeSelectionManager newManager)
{
    upgrade = newUpgrade;
    manager = newManager;

    if (upgrade == null)
    {
        Debug.LogWarning(
            "SkillUpgradeButton: brak SkillUpgradeData."
        );

        gameObject.SetActive(false);

        return;
    }

    // -----------------------------------------------------
    // IKONA
    // -----------------------------------------------------

    if (image != null)
    {
        image.sprite = upgrade.icon;
    }

    // -----------------------------------------------------
    // RESET PODŚWIETLENIA
    // -----------------------------------------------------

    RemoveHighlight();

    // -----------------------------------------------------
    // AKTYWUJ
    // -----------------------------------------------------

    gameObject.SetActive(true);
}


// =========================================================
// CLICK
// =========================================================

public void Click()
{
    if (upgrade == null)
    {
        Debug.LogWarning(
            "SkillUpgradeButton: brak ulepszenia."
        );

        return;
    }

    if (manager == null)
    {
        Debug.LogWarning(
            "SkillUpgradeButton: brak managera."
        );

        return;
    }

    manager.ButtonClicked(
        this,
        upgrade
    );
}


// =========================================================
// HIGHLIGHT
// =========================================================

public void Highlight()
{
    if (highlight != null)
    {
        highlight.SetActive(true);
    }
}


// =========================================================
// REMOVE HIGHLIGHT
// =========================================================

public void RemoveHighlight()
{
    if (highlight != null)
    {
        highlight.SetActive(false);
    }
}

}