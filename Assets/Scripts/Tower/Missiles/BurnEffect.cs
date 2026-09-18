using System;
using System.Collections.Generic;
using UnityEngine;

public class BurnEffect : MonoBehaviour
{
    private EnemyHealth enemyHealth;

    private float burnDamage;
    private float remainingDuration;
    private float tickInterval;
    private float tickTimer;

    private bool active;

    // =========================================================
    // CALLBACK PO ZAKOŃCZENIU PODPALENIA
    // =========================================================

    private Action onBurnFinished;


    // =========================================================
    // SEARING SHOT
    // =========================================================

    private bool searingShotActive;

    private float searingShotExplosionDamage;
    private float searingShotExplosionRadius;

    private GameObject searingShotExplosionEffectPrefab;


    // =========================================================
    // KOLOR PODPALENIA
    // =========================================================

    private Renderer[] renderers;
    private Color[] originalColors;

    private readonly Color burnColor =
        new Color(1f, 0.65f, 0.05f, 1f);


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        enemyHealth =
            GetComponent<EnemyHealth>();

        CacheRenderers();

        active = false;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!active)
            return;


        if (enemyHealth == null)
        {
            StopBurn(false);
            return;
        }


        // =====================================================
        // ŚMIERĆ PRZECIWNIKA
        // =====================================================

        if (enemyHealth.IsDying)
        {
            // Searing Shot eksploduje w momencie śmierci.
            if (searingShotActive)
            {
                CreateSearingShotExplosion();

                searingShotActive = false;
            }


            StopBurn(false);

            return;
        }


        // =====================================================
        // CZAS PODPALENIA
        // =====================================================

        remainingDuration -=
            Time.deltaTime;


        // =====================================================
        // OBRAŻENIA PODPALENIA
        // =====================================================

        tickTimer -=
            Time.deltaTime;


        if (tickTimer <= 0f)
        {
            enemyHealth.TakeDamage(
                burnDamage
            );

            tickTimer +=
                tickInterval;


            // Wróg mógł umrzeć od obrażeń podpalenia.
            if (enemyHealth.IsDying)
            {
                if (searingShotActive)
                {
                    CreateSearingShotExplosion();

                    searingShotActive = false;
                }


                StopBurn(false);

                return;
            }
        }


        // =====================================================
        // KONIEC CZASU PODPALENIA
        // =====================================================

        if (remainingDuration <= 0f)
        {
            StopBurn(true);
        }
    }


    // =========================================================
    // ZWYKŁY APPLY BURN
    // =========================================================

    public void ApplyBurn(
        float damage,
        float duration,
        float interval)
    {
        ApplyBurn(
            damage,
            duration,
            interval,
            null
        );
    }


    // =========================================================
    // APPLY BURN + CALLBACK
    // =========================================================

    public void ApplyBurn(
        float damage,
        float duration,
        float interval,
        Action burnFinishedCallback)
    {
        if (enemyHealth == null)
        {
            enemyHealth =
                GetComponent<EnemyHealth>();
        }


        if (enemyHealth == null)
            return;


        if (enemyHealth.IsDying)
            return;


        // =====================================================
        // ZWYKŁY BURN
        // =====================================================

        burnDamage =
            Mathf.Max(
                0f,
                damage
            );


        remainingDuration =
            Mathf.Max(
                0f,
                duration
            );


        tickInterval =
            Mathf.Max(
                0.01f,
                interval
            );


        // =====================================================
        // CALLBACK
        // =====================================================

        onBurnFinished =
            burnFinishedCallback;


        // Zwykły Burn nie jest Searing Shot.
        searingShotActive = false;


        // =====================================================
        // WŁĄCZENIE PODPALENIA
        // =====================================================

        if (!active)
        {
            CacheRenderers();

            tickTimer =
                tickInterval;

            active =
                true;

            SetBurnColor();

            enabled =
                true;
        }
        else
        {
            SetBurnColor();
        }
    }


    // =========================================================
    // SEARING SHOT BURN
    // =========================================================
    //
    // Podpalenie specjalne dla Searing Shot.
    //
    // Eksplozja następuje dopiero wtedy,
    // gdy przeciwnik umrze.
    //

    public void ApplySearingShotBurn(
        float damage,
        float duration,
        float interval,
        float explosionDamage,
        float explosionRadius,
        GameObject explosionEffectPrefab)
    {
        if (enemyHealth == null)
        {
            enemyHealth =
                GetComponent<EnemyHealth>();
        }


        if (enemyHealth == null)
            return;


        if (enemyHealth.IsDying)
            return;


        // =====================================================
        // BURN
        // =====================================================

        burnDamage =
            Mathf.Max(
                0f,
                damage
            );


        remainingDuration =
            Mathf.Max(
                0f,
                duration
            );


        tickInterval =
            Mathf.Max(
                0.01f,
                interval
            );


        // =====================================================
        // SEARING SHOT
        // =====================================================

        searingShotActive =
            true;


        searingShotExplosionDamage =
            Mathf.Max(
                0f,
                explosionDamage
            );


        searingShotExplosionRadius =
            Mathf.Max(
                0f,
                explosionRadius
            );


        searingShotExplosionEffectPrefab =
            explosionEffectPrefab;


        // Searing Shot nie korzysta z callbacka
        // po zakończeniu czasu podpalenia.
        onBurnFinished =
            null;


        // =====================================================
        // WŁĄCZENIE
        // =====================================================

        if (!active)
        {
            CacheRenderers();

            tickTimer =
                tickInterval;

            active =
                true;

            SetBurnColor();

            enabled =
                true;
        }
        else
        {
            SetBurnColor();
        }
    }


    // =========================================================
    // EKSPLOZJA SEARING SHOT
    // =========================================================

    private void CreateSearingShotExplosion()
    {
        if (enemyHealth == null)
            return;


        Vector3 explosionPosition =
            enemyHealth.transform.position;


        if (enemyHealth.hitPoint != null)
        {
            explosionPosition =
                enemyHealth.hitPoint.position;
        }


        // =====================================================
        // WYSZUKIWANIE PRZECIWNIKÓW
        // =====================================================

        Collider[] colliders =
            Physics.OverlapSphere(
                explosionPosition,
                searingShotExplosionRadius
            );


        HashSet<EnemyHealth> affectedEnemies =
            new HashSet<EnemyHealth>();


        // =====================================================
        // OBRAŻENIA
        // =====================================================

        foreach (Collider collider in colliders)
        {
            if (collider == null)
                continue;


            EnemyHealth target =
                collider.GetComponent<EnemyHealth>();


            if (target == null)
            {
                target =
                    collider.GetComponentInParent<EnemyHealth>();
            }


            if (target == null)
                continue;


            if (target.IsDying)
                continue;


            if (affectedEnemies.Contains(target))
                continue;


            affectedEnemies.Add(
                target
            );


            target.TakeDamage(
                searingShotExplosionDamage
            );
        }


        // =====================================================
        // EFEKT EKSPLOZJI
        // =====================================================

        if (searingShotExplosionEffectPrefab != null)
        {
            GameObject effect =
                Instantiate(
                    searingShotExplosionEffectPrefab,
                    explosionPosition,
                    Quaternion.identity
                );


            if (effect != null)
            {
                float diameter =
                    searingShotExplosionRadius *
                    2f;


                effect.transform.localScale =
                    Vector3.one *
                    diameter;
            }
        }
    }


    // =========================================================
    // STOP BURN
    // =========================================================

    private void StopBurn(
        bool invokeCallback)
    {
        active =
            false;


        remainingDuration =
            0f;


        tickTimer =
            0f;


        RestoreOriginalColors();


        Action callback =
            onBurnFinished;


        onBurnFinished =
            null;


        enabled =
            false;


        // =====================================================
        // CALLBACK
        // =====================================================

        if (invokeCallback &&
            callback != null)
        {
            callback.Invoke();
        }
    }


    // =========================================================
    // ZNALEZIENIE RENDERERÓW
    // =========================================================

    private void CacheRenderers()
    {
        renderers =
            GetComponentsInChildren<Renderer>();


        if (renderers == null)
            return;


        originalColors =
            new Color[
                renderers.Length
            ];


        for (int i = 0;
             i < renderers.Length;
             i++)
        {
            if (renderers[i] == null)
                continue;


            Material material =
                renderers[i].material;


            if (material == null)
                continue;


            if (material.HasProperty("_Color"))
            {
                originalColors[i] =
                    material.color;
            }
        }
    }


    // =========================================================
    // KOLOR PODPALENIA
    // =========================================================

    private void SetBurnColor()
    {
        if (renderers == null)
            return;


        for (int i = 0;
             i < renderers.Length;
             i++)
        {
            if (renderers[i] == null)
                continue;


            Material material =
                renderers[i].material;


            if (material == null)
                continue;


            if (material.HasProperty("_Color"))
            {
                material.color =
                    burnColor;
            }
        }
    }


    // =========================================================
    // PRZYWRÓCENIE KOLORÓW
    // =========================================================

    private void RestoreOriginalColors()
    {
        if (renderers == null ||
            originalColors == null)
        {
            return;
        }


        for (int i = 0;
             i < renderers.Length;
             i++)
        {
            if (renderers[i] == null)
                continue;


            if (i >= originalColors.Length)
                continue;


            Material material =
                renderers[i].material;


            if (material == null)
                continue;


            if (material.HasProperty("_Color"))
            {
                material.color =
                    originalColors[i];
            }
        }
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        RestoreOriginalColors();

        onBurnFinished =
            null;
    }


    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (!searingShotActive)
            return;


        Gizmos.DrawWireSphere(
            transform.position,
            searingShotExplosionRadius
        );
    }
}