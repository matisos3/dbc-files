using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectileSlowAura : MonoBehaviour
{
    [Header("Ustawienia aury")]
    [SerializeField] private float auraRange = 12f;
    [SerializeField] private float projectileSlowPercent = 60f;
    [SerializeField] private float checkInterval = 0.05f;

    [Header("Efekt wizualny")]
    [SerializeField] private GameObject auraVisualPrefab;

    [Tooltip("Promień wizualnej aury względem auraRange.")]
    [SerializeField] private float visualRangeMultiplier = 1f;

    private GameObject auraVisual;

    private float auraTimer = 0f;

    private readonly HashSet<ProjectileMovement> affectedProjectiles =
        new HashSet<ProjectileMovement>();


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        auraTimer -= Time.deltaTime;

        if (auraTimer > 0f)
            return;

        auraTimer = checkInterval;

        UpdateAura();
    }


    // =========================================================
    // USTAWIENIE PREFABU WIZUALNEGO
    // =========================================================

    public void SetVisualPrefab(GameObject prefab)
    {
        auraVisualPrefab = prefab;

        if (auraVisual != null)
            return;

        if (auraVisualPrefab == null)
            return;


        auraVisual = Instantiate(
            auraVisualPrefab,
            transform.position,
            Quaternion.identity,
            transform
        );


        auraVisual.transform.localPosition =
            new Vector3(
                0f,
                0.02f,
                0f
            );


        auraVisual.transform.localRotation =
            Quaternion.identity;


        UpdateVisualScale();
    }


    // =========================================================
    // SKALA WIZUALNEJ AURY
    // =========================================================

    private void UpdateVisualScale()
{
    if (auraVisual == null)
        return;

    Renderer[] renderers =
        auraVisual.GetComponentsInChildren<Renderer>();

    if (renderers == null || renderers.Length == 0)
    {
        return;
    }

    Bounds bounds =
        renderers[0].bounds;

    for (int i = 1; i < renderers.Length; i++)
    {
        if (renderers[i] != null)
        {
            bounds.Encapsulate(
                renderers[i].bounds
            );
        }
    }

    float currentDiameter =
        Mathf.Max(
            bounds.size.x,
            bounds.size.z
        );

    if (currentDiameter <= 0.0001f)
        return;

    float targetDiameter =
        auraRange * 2f;

    float scaleMultiplier =
        targetDiameter /
        currentDiameter;

    auraVisual.transform.localScale *=
        scaleMultiplier;
}


    // =========================================================
    // PROMIEŃ AURY
    // =========================================================

    private float GetActualAuraRadius()
    {
        return
            (auraRange *
             visualRangeMultiplier) /
            2f;
    }


    // =========================================================
    // SPRAWDZANIE POCISKÓW
    // =========================================================

    private void UpdateAura()
    {
        HashSet<ProjectileMovement> currentProjectiles =
            new HashSet<ProjectileMovement>();


        float actualRadius =
            GetActualAuraRadius();


        float rangeSqr =
            actualRadius *
            actualRadius;


        foreach (
            ProjectileMovement projectile
            in ProjectileRegistry.Projectiles)
        {
            if (projectile == null)
                continue;


            Vector3 offset =
                projectile.transform.position -
                transform.position;


            float distanceSqr =
                offset.sqrMagnitude;


            // =================================================
            // POCISK DOTKNĄŁ / WCHODZI DO AURY
            // =================================================

            if (distanceSqr <= rangeSqr)
            {
                currentProjectiles.Add(
                    projectile
                );
            }
        }


        // =====================================================
        // USUWANIE SLOWA
        // =====================================================

        foreach (
            ProjectileMovement projectile
            in affectedProjectiles)
        {
            if (projectile == null)
                continue;


            if (!currentProjectiles.Contains(
                projectile))
            {
                projectile.RemoveSlowAura(
                    this
                );
            }
        }


        // =====================================================
        // DODAWANIE SLOWA
        // =====================================================

        foreach (
            ProjectileMovement projectile
            in currentProjectiles)
        {
            if (!affectedProjectiles.Contains(
                projectile))
            {
                projectile.AddSlowAura(
                    this,
                    projectileSlowPercent
                );
            }
        }


        // =====================================================
        // AKTUALIZACJA LISTY
        // =====================================================

        affectedProjectiles.Clear();


        foreach (
            ProjectileMovement projectile
            in currentProjectiles)
        {
            affectedProjectiles.Add(
                projectile
            );
        }
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        foreach (
            ProjectileMovement projectile
            in affectedProjectiles)
        {
            if (projectile != null)
            {
                projectile.RemoveSlowAura(
                    this
                );
            }
        }


        affectedProjectiles.Clear();
    }


    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.cyan;


        Gizmos.DrawWireSphere(
            transform.position,
            GetActualAuraRadius()
        );
    }
}