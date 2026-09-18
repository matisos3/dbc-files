using UnityEngine;

public class IceSpikes : MonoBehaviour
{
    // =========================================================
    // CREATE SPIKES
    // =========================================================

    public void CreateSpikes(
        EnemyHealth mainTarget,
        SkillInstance skill)
    {
        if (mainTarget == null)
        {
            return;
        }


        if (skill == null)
        {
            return;
        }


        if (skill.data == null)
        {
            return;
        }


        // =====================================================
        // PREFAB ODŁAMKA
        // =====================================================

        GameObject spikePrefab =
            skill.iceSpikeProjectilePrefab;


        if (spikePrefab == null)
        {
            return;
        }


        // =====================================================
        // LICZBA ODŁAMKÓW
        // =====================================================

        int spikeCount =
            Mathf.Max(
                1,
                skill.iceSpikeCount
            );


        float angleStep =
            360f /
            spikeCount;


        // =====================================================
        // POZYCJA GŁÓWNEGO CELU
        // =====================================================

        Vector3 targetPosition =
            mainTarget.transform.position;


        // =====================================================
        // TWORZENIE ODŁAMKÓW
        // =====================================================

        for (
            int i = 0;
            i < spikeCount;
            i++)
        {
            // =================================================
            // KĄT
            // =================================================

            float angle =
                angleStep * i;


            // =================================================
            // KIERUNEK
            // =================================================

            Vector3 direction =
                Quaternion.Euler(
                    0f,
                    angle,
                    0f
                ) *
                Vector3.forward;


            direction.y = 0f;


            if (direction.sqrMagnitude <=
                0.0001f)
            {
                direction =
                    Vector3.forward;
            }


            direction.Normalize();


            // =================================================
            // POZYCJA
            //
            // IceSpikes.cs nie ustawia już odległości.
            // Odległość jest obsługiwana przez
            // IceSpikesProjectile.cs.
            // =================================================

            Vector3 spawnPosition =
                targetPosition;


            // =================================================
            // ROTACJA
            // =================================================

            Quaternion rotation =
                Quaternion.LookRotation(
                    direction
                );


            // =================================================
            // SPAWN
            // =================================================

            GameObject spikeObject =
                Instantiate(
                    spikePrefab,
                    spawnPosition,
                    rotation
                );


            if (spikeObject == null)
                continue;


            // =================================================
            // COMPONENT
            // =================================================

            IceSpikeProjectile spike =
                spikeObject.GetComponent<
                    IceSpikeProjectile
                >();


            if (spike == null)
            {
                Destroy(
                    spikeObject
                );


                continue;
            }


            // =================================================
            // INITIALIZE
            // =================================================

            spike.Initialize(
                skill,
                direction
            );
        }
    }
}