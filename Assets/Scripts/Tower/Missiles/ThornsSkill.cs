using UnityEngine;

public class ThornsSkill : MonoBehaviour
{
    private Tower tower;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        tower = GetComponent<Tower>();
    }


    // =========================================================
    // ODBICIE OBRAŻEŃ
    // =========================================================

    public void DealReflectDamage(
        EnemyMovement attacker)
    {
        if (tower == null)
            return;

        if (attacker == null)
            return;

        if (tower.activeSkills == null)
            return;


        // =====================================================
        // SZUKAMY SKILLA THORNS
        // =====================================================

        foreach (SkillInstance skill in tower.activeSkills)
        {
            if (skill == null)
                continue;

            if (skill.data == null)
                continue;


            // =================================================
            // THORNS
            // =================================================

            if (skill.data.skillID != "Thorns")
                continue;


            // =================================================
            // OBRAŻENIA Z AKTUALNEGO POZIOMU
            // =================================================
            //
            // NIE:
            // skill.data.damage
            //
            // Tylko:
            // skill.damage
            //
            // skill.damage zawiera już upgrade'y.
            //
            // Przykład:
            //
            // poziom 1 = 10
            // poziom 2 = 12
            // poziom 3 = 14
            // poziom 4 = 16
            //
            // jeśli damageUpgrade = 2
            // =================================================

            float thornsDamage =
                skill.damage;


            if (thornsDamage <= 0f)
                continue;


            // =================================================
            // ENEMY HEALTH
            // =================================================

            EnemyHealth enemyHealth =
                attacker.GetComponent<EnemyHealth>();


            if (enemyHealth == null)
                continue;


            if (enemyHealth.IsDying)
                continue;


            // =================================================
            // ZADAJEMY OBRAŻENIA
            // =================================================

            enemyHealth.TakeDamage(
                thornsDamage
            );

            // Jeden aktywny Thorns wystarczy.
            return;
        }
    }
}