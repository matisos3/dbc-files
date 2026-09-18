using System.Collections.Generic;
using UnityEngine;

public class EnemyDamageAura : MonoBehaviour
{
    [Header("Ustawienia aury")]
    [SerializeField] private float auraRange = 7f;
    [SerializeField] private float damageBonusPercent = 15f;

    [Header("Wizualizacja aury")]
    [SerializeField] private GameObject auraVisualPrefab;

    [Tooltip("Jak duża ma być aura względem modelu przeciwnika.")]
    [SerializeField] private float auraVisualMultiplier = 1f;

    private GameObject auraVisual;

    private readonly HashSet<EnemyMovement> affectedEnemies =
        new HashSet<EnemyMovement>();


    // =========================================================
    // USTAWIENIE PREFABU WIZUALNEGO
    // =========================================================

    public void SetVisualPrefab(GameObject prefab)
    {
        auraVisualPrefab = prefab;

        if (auraVisual == null &&
            auraVisualPrefab != null)
        {
            CreateAuraVisual();
        }
    }


    // =========================================================
    // TWORZENIE WIZUALIZACJI
    // =========================================================

    private void CreateAuraVisual()
    {
        if (auraVisualPrefab == null)
            return;


        auraVisual =
            Instantiate(
                auraVisualPrefab,
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


        // Najpierw ustawiamy bazową skalę.
        auraVisual.transform.localScale =
            Vector3.one;


        AdjustAuraVisualToEnemy();
    }


    // =========================================================
    // DOPASOWANIE AURY DO MODELU
    // =========================================================

    private void AdjustAuraVisualToEnemy()
    {
        if (auraVisual == null)
            return;


        // =====================================================
        // SZUKAMY RENDERERÓW PRZECIWNIKA
        // =====================================================

        Renderer[] allRenderers =
            GetComponentsInChildren<Renderer>(true);


        List<Renderer> enemyRenderers =
            new List<Renderer>();


        foreach (Renderer renderer in allRenderers)
        {
            if (renderer == null)
                continue;


            // =================================================
            // IGNORUJEMY RENDERERY AURY
            // =================================================

            if (renderer.transform.IsChildOf(
                auraVisual.transform))
            {
                continue;
            }


            enemyRenderers.Add(renderer);
        }


        if (enemyRenderers.Count == 0)
        {
            return;
        }


        // =====================================================
        // OBLICZAMY BOUNDS CAŁEGO MODELU
        // =====================================================

        Bounds enemyBounds =
            enemyRenderers[0].bounds;


        for (int i = 1;
             i < enemyRenderers.Count;
             i++)
        {
            enemyBounds.Encapsulate(
                enemyRenderers[i].bounds
            );
        }


        // =====================================================
        // ROZMIAR MODELU
        // =====================================================

        float enemyWidth =
            enemyBounds.size.x;

        float enemyDepth =
            enemyBounds.size.z;


        if (enemyWidth <= 0.001f)
        {
            return;
        }


        if (enemyDepth <= 0.001f)
        {
            return;
        }


        // =====================================================
        // DOCZELOWY ROZMIAR AURY
        // =====================================================

        float targetWidth =
            enemyWidth *
            auraVisualMultiplier;


        float targetDepth =
            enemyDepth *
            auraVisualMultiplier;


        // =====================================================
        // SKALOWANIE
        // =====================================================

        /*
         * Ważne:
         *
         * Nie ustawiamy tutaj:
         *
         * scale = enemyWidth
         *
         * ponieważ prefab AuraDamage może mieć własną
         * geometrię / skalę bazową.
         *
         * Najpierw sprawdzamy rozmiar wizualnego efektu,
         * a następnie wyliczamy skalę potrzebną do osiągnięcia
         * docelowego rozmiaru.
         */


        Renderer[] auraRenderers =
            auraVisual.GetComponentsInChildren<Renderer>(true);


        if (auraRenderers == null ||
            auraRenderers.Length == 0)
        {
            return;
        }


        // =====================================================
        // BOUNDS AURY
        // =====================================================

        Bounds auraBounds =
            auraRenderers[0].bounds;


        for (int i = 1;
             i < auraRenderers.Length;
             i++)
        {
            auraBounds.Encapsulate(
                auraRenderers[i].bounds
            );
        }


        float auraWidth =
            auraBounds.size.x;


        float auraDepth =
            auraBounds.size.z;


        if (auraWidth <= 0.001f)
        {
            return;
        }


        if (auraDepth <= 0.001f)
        {
            return;
        }


        // =====================================================
        // OBLICZENIE SKALI X/Z
        // =====================================================

        float scaleX =
            targetWidth /
            auraWidth;


        float scaleZ =
            targetDepth /
            auraDepth;


        // =====================================================
        // USTAWIENIE SKALI
        // =====================================================

        auraVisual.transform.localScale =
            new Vector3(
                scaleX,
                scaleX,
                scaleZ
            );


        // =====================================================
        // POZYCJA
        // =====================================================

        auraVisual.transform.localPosition =
            new Vector3(
                0f,
                0.02f,
                0f
            );


        auraVisual.transform.localRotation =
            Quaternion.identity;

    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        UpdateAura();
    }


    // =========================================================
    // AKTUALIZACJA AURY
    // =========================================================

    private void UpdateAura()
    {
        HashSet<EnemyMovement> currentEnemies =
            new HashSet<EnemyMovement>();


        Collider[] colliders =
            Physics.OverlapSphere(
                transform.position,
                auraRange
            );


        foreach (Collider col in colliders)
        {
            EnemyMovement enemy =
                col.GetComponentInParent<EnemyMovement>();


            if (enemy == null)
                continue;


            currentEnemies.Add(enemy);
        }


        // =====================================================
        // USUWAMY BONUS PRZECIWNIKOM POZA ZASIĘGIEM
        // =====================================================

        foreach (EnemyMovement enemy in affectedEnemies)
        {
            if (enemy == null)
                continue;


            if (!currentEnemies.Contains(enemy))
            {
                enemy.RemoveDamageAura(this);
            }
        }


        // =====================================================
        // DODAJEMY BONUS NOWYM PRZECIWNIKOM
        // =====================================================

        foreach (EnemyMovement enemy in currentEnemies)
        {
            if (!affectedEnemies.Contains(enemy))
            {
                enemy.AddDamageAura(
                    this,
                    damageBonusPercent
                );
            }
        }


        // =====================================================
        // AKTUALIZUJEMY LISTĘ
        // =====================================================

        affectedEnemies.Clear();


        foreach (EnemyMovement enemy in currentEnemies)
        {
            affectedEnemies.Add(enemy);
        }
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        foreach (EnemyMovement enemy in affectedEnemies)
        {
            if (enemy != null)
            {
                enemy.RemoveDamageAura(this);
            }
        }


        affectedEnemies.Clear();
    }


    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.yellow;


        Gizmos.DrawWireSphere(
            transform.position,
            auraRange
        );
    }
}