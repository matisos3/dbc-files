using System.Collections.Generic;
using UnityEngine;

public class FlamingCircle : MonoBehaviour
{
    private SkillInstance skill;

    private float currentRadius;
    private float previousRadius;

    private float maxRadius;
    private float duration;

    private float expansionTime;
    private float expansionTimer;

    private bool initialized;

    // Przeciwnicy, których pierścień już dotknął
    private readonly HashSet<EnemyHealth> affectedEnemies =
        new HashSet<EnemyHealth>();

    // =========================================================
    // INICJALIZACJA
    // =========================================================

    public void Initialize(SkillInstance skillInstance)
    {
        skill = skillInstance;

        if (skill == null)
        {
            Destroy(gameObject);
            return;
        }

        maxRadius =
            Mathf.Max(
                0.01f,
                skill.flamingCircleRadius
            );

        duration =
            Mathf.Max(
                0.01f,
                skill.flamingCircleDuration
            );

        // =====================================================
        // BARDZO SZYBKIE ROZSZERZANIE
        // =====================================================
        //
        // Pierścień osiąga maksymalny rozmiar
        // w maksymalnie 0.75 sekundy.
        //
        // Jeśli skill trwa krócej, wykorzystujemy
        // cały czas jego trwania.
        //

        expansionTime =
            Mathf.Min(
                0.75f,
                duration
            );

        currentRadius = 0.01f;
        previousRadius = 0.01f;

        expansionTimer = 0f;

        initialized = true;

        UpdateVisualScale();
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!initialized)
            return;

        // =====================================================
        // ROZSZERZANIE PIERŚCIENIA
        // =====================================================

        expansionTimer += Time.deltaTime;

        previousRadius = currentRadius;

        float progress =
            Mathf.Clamp01(
                expansionTimer /
                expansionTime
            );

        // SmoothStep daje szybkie, płynne rozszerzenie
        float smoothProgress =
            Mathf.SmoothStep(
                0f,
                1f,
                progress
            );

        currentRadius =
            Mathf.Lerp(
                0.01f,
                maxRadius,
                smoothProgress
            );

        UpdateVisualScale();

        // =====================================================
        // SPRAWDZANIE KONTAKTU Z PIERŚCIENIEM
        // =====================================================

        DetectEnemiesHitByRing();

        // =====================================================
        // KONIEC ROZSZERZANIA
        // =====================================================

        if (progress >= 1f)
        {
            Destroy(gameObject);
        }
    }

    // =========================================================
    // WIZUALNY ROZMIAR PIERŚCIENIA
    // =========================================================

    private void UpdateVisualScale()
    {
        /*
         * Zakładamy, że prefab Flaming Circle ma
         * podstawowy rozmiar 1x1.
         *
         * currentRadius jest promieniem,
         * więc średnica = radius * 2.
         */

        float diameter =
            currentRadius * 2f;

        transform.localScale =
            new Vector3(
                diameter,
                1f,
                diameter
            );
    }

    // =========================================================
    // WYKRYWANIE PRZECIWNIKÓW
    // =========================================================

    private void DetectEnemiesHitByRing()
    {
        if (currentRadius <= previousRadius)
            return;

        // =====================================================
        // MARGINES KONTAKTU
        // =====================================================
        //
        // Dzięki temu nie musimy znać dokładnego rozmiaru
        // collidera każdego przeciwnika.
        //

        float contactThickness = 0.6f;

        float searchRadius =
            currentRadius +
            contactThickness;

        Collider[] colliders =
            Physics.OverlapSphere(
                transform.position,
                searchRadius
            );

        if (colliders == null ||
            colliders.Length == 0)
        {
            return;
        }

        for (int i = 0;
             i < colliders.Length;
             i++)
        {
            Collider collider =
                colliders[i];

            if (collider == null)
                continue;

            EnemyHealth enemy =
                collider.GetComponent<EnemyHealth>();

            if (enemy == null)
            {
                enemy =
                    collider.GetComponentInParent<EnemyHealth>();
            }

            if (enemy == null)
                continue;

            if (enemy.IsDying)
                continue;

            if (affectedEnemies.Contains(enemy))
                continue;

            // =================================================
            // ODLEGŁOŚĆ PRZECIWNIKA OD ŚRODKA
            // =================================================

            Vector3 enemyPosition =
                enemy.transform.position;

            Vector3 flatPosition =
                enemyPosition -
                transform.position;

            flatPosition.y = 0f;

            float distance =
                flatPosition.magnitude;

            // =================================================
            // CZY FRONT PIERŚCIENIA DOTKNĄŁ PRZECIWNIKA?
            // =================================================
            //
            // Poprzedni promień był jeszcze przed przeciwnikiem,
            // a obecny promień już go przekroczył.
            //

            bool crossedRing =
                distance >=
                previousRadius -
                contactThickness;

            bool reachedRing =
                distance <=
                currentRadius +
                contactThickness;

            if (!crossedRing ||
                !reachedRing)
            {
                continue;
            }

            AffectEnemy(enemy);
        }
    }

    // =========================================================
    // PODPALENIE PRZECIWNIKA
    // =========================================================

    private void AffectEnemy(
        EnemyHealth enemy)
    {
        if (enemy == null)
            return;

        if (enemy.IsDying)
            return;

        affectedEnemies.Add(enemy);

        // =====================================================
        // ZNAJDŹ / UTWÓRZ BURN EFFECT
        // =====================================================

        BurnEffect burn =
            enemy.GetComponent<BurnEffect>();

        if (burn == null)
        {
            burn =
                enemy.gameObject.AddComponent<BurnEffect>();
        }

        if (burn == null)
            return;

        // =====================================================
        // PODPALENIE
        // =====================================================

        burn.ApplyBurn(
            skill.flamingCircleDamage,
            skill.flamingCircleDuration,
            skill.flamingCircleTickInterval
        );
    }

    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            maxRadius
        );
    }
}