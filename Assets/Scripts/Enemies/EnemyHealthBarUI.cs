using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarUI : MonoBehaviour
{
    [Header("Warianty pasków")]
    [SerializeField] private GameObject normalBar;
    [SerializeField] private GameObject championBar;
    [SerializeField] private GameObject eliteBar;
    [SerializeField] private GameObject bossBar;

    [Header("Fill HP każdego paska")]
    [SerializeField] private Image normalFill;
    [SerializeField] private Image championFill;
    [SerializeField] private Image eliteFill;
    [SerializeField] private Image bossFill;

    [Header("Shield Fill każdego paska")]
    [SerializeField] private Image normalShieldFill;
    [SerializeField] private Image championShieldFill;
    [SerializeField] private Image eliteShieldFill;
    [SerializeField] private Image bossShieldFill;

    private Image activeFill;
    private Image activeShieldFill;

    private Transform activeAffixContainer;
    private GameObject activeBar;


    // =========================================================
    // SETUP
    // =========================================================

    public void Setup(
        EnemyType.EnemyRank rank)
    {
        DisableAllBars();

        activeFill = null;
        activeShieldFill = null;
        activeAffixContainer = null;
        activeBar = null;


        // =====================================================
        // WYBÓR PASKA
        // =====================================================

        switch (rank)
        {
            case EnemyType.EnemyRank.Normal:

                activeBar = normalBar;
                activeFill = normalFill;
                activeShieldFill = normalShieldFill;

                break;


            case EnemyType.EnemyRank.Champion:

                activeBar = championBar;
                activeFill = championFill;
                activeShieldFill = championShieldFill;

                break;


            case EnemyType.EnemyRank.Elite:

                activeBar = eliteBar;
                activeFill = eliteFill;
                activeShieldFill = eliteShieldFill;

                break;


            case EnemyType.EnemyRank.Boss:

                activeBar = bossBar;
                activeFill = bossFill;
                activeShieldFill = bossShieldFill;

                break;
        }


        // =====================================================
        // AKTYWUJ WYBRANY PASEK
        // =====================================================

        if (activeBar == null)
        {
            return;
        }


        activeBar.SetActive(true);


        // =====================================================
        // SZUKAMY AFFIX ICONS
        //
        // Szukamy w CAŁYM EnemyHealthBarCanvas,
        // a nie tylko w activeBar.
        // =====================================================

        activeAffixContainer =
            FindAffixIconsRecursive(
                transform
            );


        // =====================================================
        // RESET HP
        // =====================================================

        if (activeFill != null)
        {
            activeFill.fillAmount = 1f;
        }


        // =====================================================
        // RESET SHIELD
        // =====================================================

        if (activeShieldFill != null)
        {
            activeShieldFill.fillAmount = 0f;
        }
    }


    // =========================================================
    // SZUKANIE AFFIX ICONS
    // =========================================================

    private Transform FindAffixIconsRecursive(
        Transform root)
    {
        if (root == null)
            return null;


        Transform[] allTransforms =
            root.GetComponentsInChildren<Transform>(
                true
            );


        // =====================================================
        // DOKŁADNA NAZWA
        // =====================================================

        foreach (
            Transform child
            in allTransforms
        )
        {
            if (child == null)
                continue;


            if (child.name == "AffixIcons")
            {
                return child;
            }
        }


        // =====================================================
        // BEZ WZGLĘDU NA WIELKOŚĆ LITER
        // =====================================================

        foreach (
            Transform child
            in allTransforms
        )
        {
            if (child == null)
                continue;


            if (
                child.name.Equals(
                    "AffixIcons",
                    System.StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return child;
            }
        }


        return null;
    }


    // =========================================================
    // WYŁĄCZ WSZYSTKIE
    // =========================================================

    private void DisableAllBars()
    {
        if (normalBar != null)
            normalBar.SetActive(false);

        if (championBar != null)
            championBar.SetActive(false);

        if (eliteBar != null)
            eliteBar.SetActive(false);

        if (bossBar != null)
            bossBar.SetActive(false);
    }


    // =========================================================
    // GET ACTIVE FILL
    // =========================================================

    public Image GetActiveFill()
    {
        return activeFill;
    }


    // =========================================================
    // GET ACTIVE SHIELD
    // =========================================================

    public Image GetActiveShieldFill()
    {
        return activeShieldFill;
    }


    // =========================================================
    // GET ACTIVE AFFIX CONTAINER
    // =========================================================

    public Transform GetActiveAffixContainer()
    {
        return activeAffixContainer;
    }


    // =========================================================
    // GET ACTIVE BAR
    // =========================================================

    public GameObject GetActiveBar()
    {
        return activeBar;
    }
}