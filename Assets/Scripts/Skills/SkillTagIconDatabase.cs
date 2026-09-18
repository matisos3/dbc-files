using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Skill Tag Icon Database",
    menuName = "TowerDefense/Skill Tag Icon Database"
)]
public class SkillTagIconDatabase : ScriptableObject
{
    // =========================================================
    // TYP TAGU
    // =========================================================

    public enum TagType
    {
        All,

        Fire,
        Cold,
        Chaos,
        Lightning,
        Physical,

        Projectile,
        Area,
        Piercing,
        Chain,
        DoT,
        Melee
    }


    // =========================================================
    // WPIS
    // =========================================================

    [System.Serializable]
    public class TagIconEntry
    {
        public TagType tag;
        public Sprite icon;
    }


    // =========================================================
    // IKONY
    // =========================================================

    [Header("Ikony tagów")]
    public List<TagIconEntry> icons =
        new List<TagIconEntry>();


    // =========================================================
    // POBIERANIE IKONY
    // =========================================================

    public Sprite GetIcon(TagType tag)
    {
        for (int i = 0; i < icons.Count; i++)
        {
            if (icons[i] == null)
                continue;

            if (icons[i].tag == tag)
                return icons[i].icon;
        }

        return null;
    }
}