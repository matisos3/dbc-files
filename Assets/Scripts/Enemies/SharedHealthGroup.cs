using System.Collections.Generic;
using UnityEngine;

public class SharedHealthGroup : MonoBehaviour
{
    private readonly List<EnemyHealth> members =
        new List<EnemyHealth>();

    private float sharedMaxHealth;
    private float sharedCurrentHealth;


    // =========================================================
    // REGISTER
    // =========================================================

    public void Register(
        EnemyHealth enemy)
    {
        if (enemy == null)
            return;

        if (members.Contains(enemy))
            return;


        members.Add(enemy);


        sharedMaxHealth +=
            enemy.maxHealth;


        sharedCurrentHealth +=
            enemy.maxHealth;


        UpdateAllHealthBars();
    }


    // =========================================================
    // UNREGISTER
    // =========================================================

    public void Unregister(
        EnemyHealth enemy)
    {
        if (enemy == null)
            return;


        members.Remove(
            enemy
        );


        if (members.Count == 0)
        {
            sharedMaxHealth = 0f;
            sharedCurrentHealth = 0f;

            Destroy(
                gameObject
            );

            return;
        }


        UpdateAllHealthBars();
    }


    // =========================================================
    // DAMAGE
    // =========================================================

    public void TakeDamage(
        float damage)
    {
        if (damage <= 0f)
            return;


        if (sharedCurrentHealth <= 0f)
            return;


        sharedCurrentHealth -=
            damage;


        sharedCurrentHealth =
            Mathf.Max(
                0f,
                sharedCurrentHealth
            );


        UpdateAllHealthBars();

        if (sharedCurrentHealth <= 0f)
        {
            KillAllMembers();
        }
    }


    // =========================================================
    // REGENERATION
    // =========================================================

    public void Regenerate(
        float amount)
    {
        if (amount <= 0f)
            return;


        if (sharedCurrentHealth <= 0f)
            return;


        sharedCurrentHealth +=
            amount;


        sharedCurrentHealth =
            Mathf.Min(
                sharedCurrentHealth,
                sharedMaxHealth
            );


        UpdateAllHealthBars();
    }


    // =========================================================
    // FIRST MEMBER
    // =========================================================

    public bool IsFirstMember(
        EnemyHealth enemy)
    {
        if (members.Count == 0)
            return false;


        return members[0] == enemy;
    }


    // =========================================================
    // UPDATE ALL HEALTH BARS
    // =========================================================

    private void UpdateAllHealthBars()
    {
        if (sharedMaxHealth <= 0f)
            return;


        float healthPercent =
            sharedCurrentHealth /
            sharedMaxHealth;


        healthPercent =
            Mathf.Clamp01(
                healthPercent
            );


        for (int i = members.Count - 1;
             i >= 0;
             i--)
        {
            EnemyHealth enemy =
                members[i];


            if (enemy == null)
            {
                members.RemoveAt(i);
                continue;
            }


            enemy.SetSharedHealthDisplay(
                healthPercent
            );
        }
    }


    // =========================================================
    // KILL ALL
    // =========================================================

    private void KillAllMembers()
    {
        EnemyHealth[] enemies =
            members.ToArray();


        members.Clear();


        sharedMaxHealth = 0f;
        sharedCurrentHealth = 0f;


        foreach (EnemyHealth enemy
                 in enemies)
        {
            if (enemy != null)
            {
                enemy.DieFromSharedHealth();
            }
        }


        Destroy(
            gameObject
        );
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void OnDestroy()
    {
        members.Clear();
    }
}