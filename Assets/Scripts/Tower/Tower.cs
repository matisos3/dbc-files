using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Zasięg wieży")]
    public float range = 10f;


    [Header("Punkt strzelecki")]
    public Transform firePoint;


    // =========================================================
    // THORNS
    // =========================================================

    [SerializeField]
    private GameObject thornsPrefab;


    // =========================================================
    // SKILLE
    // =========================================================

    [Header("Aktywne skille")]

    public List<SkillInstance> activeSkills =
        new List<SkillInstance>();


    // =========================================================
    // ULEPSZENIA SKILLI
    // =========================================================

    [Header("Ulepszenia skilli")]

    public SkillUpgradeManager skillUpgradeManager;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        /*
         * Jeżeli nie przypiszemy managera ręcznie
         * w Inspectorze, spróbujemy znaleźć go
         * automatycznie w scenie.
         */

        if (skillUpgradeManager == null)
        {
            skillUpgradeManager =
                FindAnyObjectByType<SkillUpgradeManager>();
        }


        InvokeRepeating(
            nameof(UpdateTarget),
            0f,
            0.2f
        );
    }


    // =========================================================
    // THORNS
    // =========================================================

    public void SetThornsActive(bool active)
    {
        if (thornsPrefab != null)
        {
            thornsPrefab.SetActive(active);
        }
    }


    // =========================================================
    // DODAWANIE / ULEPSZANIE SKILLA
    // =========================================================

    public void AddSkill(SkillData skill)
    {
        if (skill == null)
            return;


        // =====================================================
        // SPRAWDZAMY CZY SKILL JUŻ ISTNIEJE
        // =====================================================

        SkillInstance existingSkill =
            GetSkillInstance(skill);


        // =====================================================
        // SKILL JUŻ ISTNIEJE
        // = ULEPSZENIE
        // =====================================================

        if (existingSkill != null)
{
    existingSkill.Upgrade();

    existingSkill.RebuildStats(this);

    return;
}


        // =====================================================
        // PIERWSZY ZAKUP
        // =====================================================

        SkillInstance newSkill =
            new SkillInstance(skill);


        activeSkills.Add(newSkill);
        newSkill.RebuildStats(this);


        // =====================================================
        // THORNS
        // =====================================================

        if (skill.skillID == "Thorns")
        {
            SetThornsActive(true);
        }
    }


    // =========================================================
    // POBIERANIE SKILLA
    // =========================================================

    public SkillInstance GetSkillInstance(
        SkillData skill)
    {
        if (skill == null)
            return null;


        foreach (
            SkillInstance skillInstance
            in activeSkills)
        {
            if (skillInstance == null)
                continue;


            if (skillInstance.data == null)
                continue;


            if (skillInstance.data == skill)
            {
                return skillInstance;
            }
        }


        return null;
    }


    // =========================================================
    // SPRAWDZANIE CZY WIEŻA POSIADA SKILL
    // =========================================================

    public bool HasSkill(
        SkillData skill)
    {
        return GetSkillInstance(skill) != null;
    }


    // =========================================================
    // WYSZUKIWANIE CELU
    // =========================================================

    private void UpdateTarget()
    {
        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );


        float shortestDistance =
            Mathf.Infinity;


        GameObject nearestEnemy =
            null;


        foreach (
            GameObject enemy
            in enemies)
        {
            if (enemy == null)
                continue;


            float distanceToEnemy =
                Vector3.Distance(
                    transform.position,
                    enemy.transform.position
                );


            if (distanceToEnemy <
                shortestDistance)
            {
                shortestDistance =
                    distanceToEnemy;

                nearestEnemy =
                    enemy;
            }
        }


        if (nearestEnemy != null &&
            shortestDistance <= range)
        {
            // Target może być wykorzystany
            // przez system skilli.
        }
    }


    // =========================================================
    // GIZMO ZASIĘGU
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.red;


        Gizmos.DrawWireSphere(
            transform.position,
            range
        );
    }
}