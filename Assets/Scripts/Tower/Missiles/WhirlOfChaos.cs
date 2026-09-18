using System.Collections.Generic;
using UnityEngine;

public class WhirlOfChaos : MonoBehaviour
{
    private Tower tower;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        tower =
            GetComponent<Tower>();
    }


    // =========================================================
    // CREATE WHIRL
    // =========================================================

    public void CreateWhirl(
        SkillInstance skill)
    {
        if (tower == null)
            return;


        if (skill == null)
            return;


        if (skill.data == null)
            return;


        // =====================================================
        // PREFAB
        // =====================================================

        GameObject whirlPrefab =
            skill.data.whirlEffectPrefab;


        if (whirlPrefab == null)
        {
            return;
        }


        // =====================================================
        // WROGOWIE
        // =====================================================

        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );


        if (enemies.Length == 0)
            return;


        // =====================================================
        // NAJLEPSZA POZYCJA
        // =====================================================

        Vector3 bestPosition =
            Vector3.zero;

        int bestEnemyCount =
            0;


        // =====================================================
        // SZUKAMY NAJWIĘKSZEGO SKUPISKA
        // =====================================================

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null)
                continue;


            EnemyHealth health =
                enemy.GetComponent<EnemyHealth>();


            if (health == null)
                continue;


            if (health.IsDying)
                continue;


            // =================================================
            // MUSI BYĆ W ZASIĘGU WIEŻY
            // =================================================

            float distanceFromTower =
                Vector3.Distance(
                    tower.transform.position,
                    enemy.transform.position
                );


            if (distanceFromTower > skill.range)
                continue;


            Vector3 testPosition =
                enemy.transform.position;


            int enemyCount =
                0;


            // =================================================
            // LICZYMY WROGÓW W PROMIENIU WIRU
            // =================================================

            foreach (GameObject otherEnemy in enemies)
            {
                if (otherEnemy == null)
                    continue;


                EnemyHealth otherHealth =
                    otherEnemy.GetComponent<EnemyHealth>();


                if (otherHealth == null)
                    continue;


                if (otherHealth.IsDying)
                    continue;


                float distance =
                    Vector3.Distance(
                        testPosition,
                        otherEnemy.transform.position
                    );


                if (distance <= skill.whirlRadius)
                {
                    enemyCount++;
                }
            }


            // =================================================
            // NAJLEPSZE SKUPISKO
            // =================================================

            if (enemyCount > bestEnemyCount)
            {
                bestEnemyCount =
                    enemyCount;

                bestPosition =
                    testPosition;
            }
        }


        // =====================================================
        // BRAK WROGÓW
        // =====================================================

        if (bestEnemyCount <= 0)
            return;


        // =====================================================
        // TWORZYMY WIR
        // =====================================================

        GameObject whirl =
            Instantiate(
                whirlPrefab,
                bestPosition,
                Quaternion.identity
            );


        if (whirl == null)
        {
            return;
        }


        // =====================================================
        // KOMPONENT
        // =====================================================

        WhirlOfChaosEffect effect =
            whirl.GetComponent<WhirlOfChaosEffect>();


        if (effect == null)
        {
            Destroy(whirl);

            return;
        }


        // =====================================================
        // INITIALIZE
        // =====================================================

        effect.Initialize(
            skill
        );
    }
}