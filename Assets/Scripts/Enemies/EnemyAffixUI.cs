using UnityEngine;
using UnityEngine.UI;

public class EnemyAffixUI : MonoBehaviour
{
    [Header("Prefab ikony affixu")]
    [SerializeField] private GameObject affixIconPrefab;

    [Header("Automatyczne wyszukiwanie")]
    [SerializeField] private string affixContainerName = "AffixIcons";


    // =========================================================
    // IKONY AFFIXÓW
    // =========================================================

    [Header("Sprite'y affiksów")]

    [SerializeField] private Sprite extraAttackSpeedIcon;
    [SerializeField] private Sprite damageAuraIcon;
    [SerializeField] private Sprite damageReductionIcon;
    [SerializeField] private Sprite dodgeIcon;
    [SerializeField] private Sprite enrageIcon;
    [SerializeField] private Sprite extraDamageIcon;
    [SerializeField] private Sprite extraHealthIcon;
    [SerializeField] private Sprite extraMoveSpeedIcon;
    [SerializeField] private Sprite illusionIcon;
    [SerializeField] private Sprite reincarnationIcon;
    [SerializeField] private Sprite sharedHealthIcon;
    [SerializeField] private Sprite slowProjectileAuraIcon;
    [SerializeField] private Sprite timedShieldIcon;
    [SerializeField] private Sprite regenerationIcon;


    private Transform activeAffixContainer;


    // =========================================================
    // SETUP
    // =========================================================

    public void Setup(EnemyAffixes enemyAffixes)
    {
        if (enemyAffixes == null)
        {
            return;
        }


        // =====================================================
        // ZNAJDŹ AKTYWNY KONTENER
        // =====================================================

        activeAffixContainer =
            FindActiveAffixContainer();


        if (activeAffixContainer == null)
        {
            return;
        }


        // =====================================================
        // WYCZYŚĆ STARE IKONY
        // =====================================================

        ClearIcons();


        // =====================================================
        // SPRAWDŹ LISTĘ AFFIXÓW
        // =====================================================

        if (enemyAffixes.Affixes == null ||
            enemyAffixes.Affixes.Count == 0)
        {
            return;
        }


        // =====================================================
        // UTWÓRZ IKONY
        // =====================================================

        int createdIcons = 0;


        foreach (
            EnemyAffixes.AffixType affix
            in enemyAffixes.Affixes
        )
        {
            if (CreateAffixIcon(affix))
            {
                createdIcons++;
            }
        }
    }


    // =========================================================
    // ZNAJDOWANIE AKTYWNEGO KONTENERA
    // =========================================================

    private Transform FindActiveAffixContainer()
    {
        Transform[] allTransforms =
            GetComponentsInChildren<Transform>(
                true
            );


        foreach (
            Transform t
            in allTransforms
        )
        {
            if (t == null)
                continue;


            if (t.name != affixContainerName)
                continue;


            if (!IsHierarchyActive(t))
                continue;


            return t;
        }


        return null;
    }


    // =========================================================
    // SPRAWDZENIE AKTYWNOŚCI HIERARCHII
    // =========================================================

    private bool IsHierarchyActive(
        Transform target)
    {
        if (target == null)
            return false;


        Transform current =
            target;


        while (current != null)
        {
            if (!current.gameObject.activeSelf)
                return false;


            current =
                current.parent;
        }


        return true;
    }


    // =========================================================
    // TWORZENIE IKONY
    // =========================================================

    private bool CreateAffixIcon(
        EnemyAffixes.AffixType affix)
    {
        if (affixIconPrefab == null)
        {
            return false;
        }


        if (activeAffixContainer == null)
        {
            return false;
        }


        GameObject icon =
            Instantiate(
                affixIconPrefab,
                activeAffixContainer
            );


        if (icon == null)
            return false;


        // =====================================================
        // RESET TRANSFORM
        // =====================================================

        RectTransform rect =
            icon.GetComponent<RectTransform>();


        if (rect != null)
        {
            rect.localScale =
                Vector3.one;

            rect.localPosition =
                Vector3.zero;

            rect.localRotation =
                Quaternion.identity;
        }
        else
        {
            icon.transform.localScale =
                Vector3.one;

            icon.transform.localPosition =
                Vector3.zero;

            icon.transform.localRotation =
                Quaternion.identity;
        }


        // =====================================================
        // ZNAJDŹ IMAGE
        // =====================================================

        Image image =
            icon.GetComponent<Image>();


        if (image == null)
        {
            image =
                icon.GetComponentInChildren<Image>(
                    true
                );
        }


        if (image == null)
        {
            Destroy(icon);

            return false;
        }


        // =====================================================
        // POBIERZ SPRITE
        // =====================================================

        Sprite sprite =
            GetAffixSprite(
                affix
            );


        if (sprite == null)
        {
            Destroy(icon);

            return false;
        }


        // =====================================================
        // USTAW SPRITE
        // =====================================================

        image.sprite =
            sprite;

        image.enabled =
            true;

        image.preserveAspect =
            true;


        // =====================================================
        // IKONA GOTOWA
        // =====================================================

        return true;
    }


    // =========================================================
    // CZYSZCZENIE IKON
    // =========================================================

    private void ClearIcons()
    {
        if (activeAffixContainer == null)
            return;


        for (
            int i =
                activeAffixContainer.childCount - 1;

            i >= 0;

            i--
        )
        {
            Transform child =
                activeAffixContainer.GetChild(i);


            if (child != null)
            {
                Destroy(
                    child.gameObject
                );
            }
        }
    }


    // =========================================================
    // SPRITE AFFIXU
    // =========================================================

    private Sprite GetAffixSprite(
        EnemyAffixes.AffixType affix)
    {
        switch (affix)
        {
            case EnemyAffixes.AffixType.ExtraAttackSpeed:

                return extraAttackSpeedIcon;


            case EnemyAffixes.AffixType.DamageAura:

                return damageAuraIcon;


            case EnemyAffixes.AffixType.DamageReduction:

                return damageReductionIcon;


            case EnemyAffixes.AffixType.Dodge:

                return dodgeIcon;


            case EnemyAffixes.AffixType.Enrage:

                return enrageIcon;


            case EnemyAffixes.AffixType.ExtraDamage:

                return extraDamageIcon;


            case EnemyAffixes.AffixType.ExtraHealth:

                return extraHealthIcon;


            case EnemyAffixes.AffixType.ExtraMoveSpeed:

                return extraMoveSpeedIcon;


            case EnemyAffixes.AffixType.Illusion:

                return illusionIcon;


            case EnemyAffixes.AffixType.Reincarnation:

                return reincarnationIcon;


            case EnemyAffixes.AffixType.SharedHealth:

                return sharedHealthIcon;


            case EnemyAffixes.AffixType.SlowProjectileAura:

                return slowProjectileAuraIcon;


            case EnemyAffixes.AffixType.TimedShield:

                return timedShieldIcon;


            case EnemyAffixes.AffixType.Regeneration:

                return regenerationIcon;
        }


        return null;
    }


    // =========================================================
    // RANGA
    // =========================================================

    private EnemyType.EnemyRank GetEnemyRank(
        EnemyAffixes affixes)
    {
        if (affixes == null)
            return EnemyType.EnemyRank.Normal;


        EnemyType enemyType =
            affixes.GetComponent<EnemyType>();


        if (enemyType == null)
            return EnemyType.EnemyRank.Normal;


        return enemyType.Rank;
    }
}