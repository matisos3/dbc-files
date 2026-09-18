using System.Collections.Generic;
using UnityEngine;

public class EnemyAffixes : MonoBehaviour
{
    public enum AffixType
    {
        ExtraAttackSpeed,
        DamageAura,
        DamageReduction,
        Dodge,
        Enrage,
        ExtraDamage,
        ExtraHealth,
        ExtraMoveSpeed,
        Illusion,
        Reincarnation,
        SharedHealth,
        SlowProjectileAura,
        TimedShield,
        Regeneration
    }


    [Header("Affiksy przeciwnika")]
    [SerializeField]
    private List<AffixType> affixes =
        new List<AffixType>();


    public List<AffixType> Affixes =>
        affixes;


    // =========================================================
    // HAS AFFIX
    // =========================================================

    public bool HasAffix(
        AffixType affix)
    {
        return affixes != null &&
               affixes.Contains(affix);
    }


    // =========================================================
    // CLEAR
    // =========================================================

    public void ClearAffixes()
    {
        if (affixes == null)
        {
            affixes =
                new List<AffixType>();

            return;
        }


        affixes.Clear();
    }


    // =========================================================
    // ADD AFFIX
    // =========================================================

    public void AddAffix(
        AffixType affix)
    {
        if (affixes == null)
        {
            affixes =
                new List<AffixType>();
        }


        // Nie dodajemy tego samego dwa razy.
        if (!affixes.Contains(affix))
        {
            affixes.Add(affix);
        }
    }


    // =========================================================
    // SET AFFIXES
    //
    // Używane np. do Championów,
    // żeby cała grupa miała identyczne affiksy.
    // =========================================================

    public void SetAffixes(
        List<AffixType> newAffixes)
    {
        if (affixes == null)
        {
            affixes =
                new List<AffixType>();
        }


        affixes.Clear();


        if (newAffixes == null)
            return;


        foreach (
            AffixType affix
            in newAffixes
        )
        {
            if (!affixes.Contains(affix))
            {
                affixes.Add(affix);
            }
        }
    }


    // =========================================================
    // COPY AFFIXES
    // =========================================================

    public void CopyAffixesFrom(
        EnemyAffixes source)
    {
        if (source == null)
        {
            ClearAffixes();
            return;
        }


        SetAffixes(
            source.Affixes
        );
    }


    // =========================================================
    // REMOVE
    // =========================================================

    public void RemoveAffix(
        AffixType affix)
    {
        if (affixes == null)
            return;


        affixes.Remove(
            affix
        );
    }


    // =========================================================
    // GET COPY
    //
    // Zwraca kopię listy, dzięki czemu Championy
    // nie będą przypadkiem współdzieliły tego samego
    // obiektu List.
    // =========================================================

    public List<AffixType> GetAffixesCopy()
    {
        if (affixes == null)
        {
            return new List<AffixType>();
        }


        return new List<AffixType>(
            affixes
        );
    }
}