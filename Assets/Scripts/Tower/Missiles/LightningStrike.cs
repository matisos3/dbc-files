using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningStrike : MonoBehaviour
{
    // =========================================================
    // GŁÓWNA FUNKCJA
    // =========================================================

    public void CreateStrike(SkillInstance skill)
    {
        if (skill == null)
        {
            return;
        }

        if (skill.data == null)
        {
            return;
        }

        EnemyHealth target =
            GetRandomTarget(skill.range);

        if (target == null)
        {
            return;
        }

        StartCoroutine(
            StrikeSequence(
                target,
                skill
            )
        );
    }


    // =========================================================
    // SERIA UDERZEŃ
    // =========================================================

    private IEnumerator StrikeSequence(
        EnemyHealth target,
        SkillInstance skill)
    {
        if (target == null)
            yield break;

        int maxHits =
            Mathf.Max(
                1,
                skill.lightningStrikeMaxHits
            );

        float chance =
            Mathf.Clamp01(
                skill.lightningStrikeChance
            );

        float damageReduction =
            Mathf.Clamp01(
                skill.lightningStrikeDamageReduction
            );

        float delay =
            Mathf.Max(
                0f,
                skill.lightningStrikeDelay
            );


        // =====================================================
        // KOLEJNE UDERZENIA
        // =====================================================

        for (int hitNumber = 1; hitNumber <= maxHits; hitNumber++)
        {
            // -------------------------------------------------
            // SPRAWDZENIE PRZECIWNIKA
            // -------------------------------------------------

            if (target == null)
                yield break;

            if (target.IsDying)
                yield break;


            // -------------------------------------------------
            // OBRAŻENIA
            // -------------------------------------------------

            float damageMultiplier =
                Mathf.Pow(
                    1f - damageReduction,
                    hitNumber - 1
                );

            float damage =
                skill.damage *
                damageMultiplier;


            // -------------------------------------------------
            // UDERZENIE
            // -------------------------------------------------

            target.TakeDamage(damage);


            // -------------------------------------------------
            // EFEKT PIORUNA
            // -------------------------------------------------

            SpawnLightningEffect(
                target,
                skill.lightningStrikeEffectPrefab
            );


            // -------------------------------------------------
            // JEŚLI WRÓG UMARŁ
            // -------------------------------------------------

            if (target.IsDying)
            {
                yield break;
            }


            // -------------------------------------------------
            // JEŚLI TO OSTATNIE UDERZENIE
            // -------------------------------------------------

            if (hitNumber >= maxHits)
            {
                yield break;
            }


            // -------------------------------------------------
            // SZANSA NA KOLEJNE UDERZENIE
            // -------------------------------------------------

            float roll =
                Random.value;

            if (roll > chance)
            {
                yield break;
            }


            // -------------------------------------------------
            // OPÓŹNIENIE
            // -------------------------------------------------

            if (delay > 0f)
                yield return new WaitForSeconds(delay);
        }
    }


    // =========================================================
    // LOSOWY CEL
    // =========================================================

    private EnemyHealth GetRandomTarget(float range)
    {
        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies == null ||
            enemies.Length == 0)
        {
            return null;
        }


        List<EnemyHealth> possibleTargets =
            new List<EnemyHealth>();


        float rangeSqr =
            range * range;


        // =====================================================
        // SZUKANIE CELÓW
        // =====================================================

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null)
                continue;


            EnemyHealth enemyHealth =
                enemy.GetComponentInParent<EnemyHealth>();

            if (enemyHealth == null)
                continue;


            if (enemyHealth.IsDying)
                continue;


            float distanceSqr =
                (enemy.transform.position -
                 transform.position).sqrMagnitude;


            if (distanceSqr > rangeSqr)
                continue;


            possibleTargets.Add(
                enemyHealth
            );
        }


        // =====================================================
        // BRAK CELÓW
        // =====================================================

        if (possibleTargets.Count == 0)
            return null;


        // =====================================================
        // LOSOWY CEL
        // =====================================================

        int randomIndex =
            Random.Range(
                0,
                possibleTargets.Count
            );


        return possibleTargets[randomIndex];
    }


    // =========================================================
    // EFEKT PIORUNA
    // =========================================================

    private void SpawnLightningEffect(
        EnemyHealth target,
        GameObject effectPrefab)
    {
        if (target == null)
            return;


        if (effectPrefab == null)
        {
            return;
        }


        // =====================================================
        // POZYCJA UDERZENIA
        // =====================================================

        Vector3 spawnPosition;


        if (target.hitPoint != null)
        {
            spawnPosition =
                target.hitPoint.position;
        }
        else
        {
            Renderer[] renderers =
                target.GetComponentsInChildren<Renderer>();


            if (renderers.Length > 0)
            {
                Bounds bounds =
                    renderers[0].bounds;


                for (int i = 1;
                     i < renderers.Length;
                     i++)
                {
                    bounds.Encapsulate(
                        renderers[i].bounds
                    );
                }


                spawnPosition =
                    bounds.center;

                spawnPosition.y =
                    bounds.max.y;
            }
            else
            {
                spawnPosition =
                    target.transform.position;

                spawnPosition.y += 1f;
            }
        }


        // =====================================================
        // UTWORZENIE VFX
        // =====================================================

        GameObject effect =
            Instantiate(
                effectPrefab,
                spawnPosition,
                Quaternion.identity
            );


        // =====================================================
        // AUTOMATYCZNE USUNIĘCIE VFX
        // =====================================================

        DestroyEffectAfterDuration(
            effect
        );
    }


    // =========================================================
    // USUWANIE EFEKTU
    // =========================================================

    private void DestroyEffectAfterDuration(
        GameObject effect)
    {
        if (effect == null)
            return;


        ParticleSystem[] particles =
            effect.GetComponentsInChildren<ParticleSystem>();


        float longestDuration =
            0f;


        foreach (ParticleSystem particle in particles)
        {
            if (particle == null)
                continue;


            ParticleSystem.MainModule main =
                particle.main;


            float duration =
                main.duration;


            if (main.loop)
            {
                duration = 1f;
            }


            if (duration > longestDuration)
                longestDuration = duration;
        }


        if (longestDuration <= 0f)
            longestDuration = 1f;


        Destroy(
            effect,
            longestDuration + 0.2f
        );
    }
}
