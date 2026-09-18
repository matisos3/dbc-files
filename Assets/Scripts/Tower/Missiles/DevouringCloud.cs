using UnityEngine;

public class DevouringCloud : MonoBehaviour
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
    // CREATE CLOUD
    // =========================================================

    public void CreateCloud(
        SkillInstance skill)
    {
        if (tower == null)
            return;


        if (skill == null)
            return;


        if (skill.data == null)
            return;


        if (skill.data.cloudEffectPrefab == null)
            return;


        // =====================================================
        // LOSOWY PRZECIWNIK
        // =====================================================

        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );


        GameObject randomEnemy =
            null;


        System.Collections.Generic.List<GameObject>
            validEnemies =
                new System.Collections.Generic.List<GameObject>();


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


            float distance =
                Vector3.Distance(
                    tower.transform.position,
                    enemy.transform.position
                );


            if (distance <= skill.range)
            {
                validEnemies.Add(
                    enemy
                );
            }
        }


        if (validEnemies.Count == 0)
            return;


        randomEnemy =
            validEnemies[
                Random.Range(
                    0,
                    validEnemies.Count
                )
            ];


        // =====================================================
        // POZYCJA CHMURY
        // =====================================================

        Vector3 cloudPosition =
            randomEnemy.transform.position;


        // =====================================================
        // TWORZYMY CHMURĘ
        // =====================================================

        GameObject cloud =
            Instantiate(
                skill.data.cloudEffectPrefab,
                cloudPosition,
                Quaternion.identity
            );


        // =====================================================
        // POBIERAMY KOMPONENT
        // =====================================================

        DevouringCloudEffect cloudEffect =
            cloud.GetComponent<DevouringCloudEffect>();


        if (cloudEffect == null)
        {
            Destroy(
                cloud
            );


            return;
        }


        // =====================================================
        // PRZEKAZUJEMY SKILL INSTANCE
        // =====================================================

        cloudEffect.Initialize(
            skill
        );
    }
}