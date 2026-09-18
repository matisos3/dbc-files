using System.Collections.Generic;
using UnityEngine;

public class SlimeEffect : MonoBehaviour
{
    // =========================================================
    // SKILL
    // =========================================================

    private SkillInstance skill;


    // =========================================================
    // CZAS ŻYCIA SLIME
    // =========================================================

    [Header("Czas działania")]

    [Tooltip("Jak długo slime pozostaje w pełnym rozmiarze.")]
    [SerializeField]
    private float activeDuration = 5f;

    [Tooltip("Jak długo slime zmniejsza się do zera.")]
    [SerializeField]
    private float shrinkDuration = 6f;


    // =========================================================
    // PRZECIWNICY W SLIME
    // =========================================================

    private HashSet<EnemyMovement> enemiesInside =
        new HashSet<EnemyMovement>();


    // =========================================================
    // CZAS
    // =========================================================

    private float lifeTimer;

    private bool isShrinking = false;

    private bool initialized = false;


    // =========================================================
    // SKALA
    // =========================================================

    private Vector3 originalScale;


    // =========================================================
    // COLLIDER
    // =========================================================

    private Collider slimeCollider;

    private Vector3 originalColliderScale;


    // =========================================================
    // INITIALIZE
    // =========================================================

    public void Initialize(
        SkillInstance skillInstance)
    {
        if (skillInstance == null)
        {
            Destroy(gameObject);

            return;
        }


        if (skillInstance.data == null)
        {
            Destroy(gameObject);

            return;
        }


        skill =
            skillInstance;


        // =====================================================
        // SKALA SLIME
        // =====================================================

        originalScale =
            transform.localScale;


        float diameter =
            skill.slimeRadius * 2f;


        transform.localScale =
            new Vector3(
                diameter,
                originalScale.y,
                diameter
            );


        originalScale =
            transform.localScale;


        // =====================================================
        // COLLIDER
        // =====================================================

        slimeCollider =
            GetComponent<Collider>();


        if (slimeCollider == null)
        {
            slimeCollider =
                GetComponentInChildren<Collider>();
        }


        if (slimeCollider == null)
        {
            Destroy(gameObject);

            return;
        }


        // =====================================================
        // TRIGGER
        // =====================================================

        slimeCollider.isTrigger = true;


        // =====================================================
        // RESET CZASU
        // =====================================================

        lifeTimer = 0f;

        isShrinking = false;

        initialized = true;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!initialized)
            return;


        // =====================================================
        // CZAS
        // =====================================================

        lifeTimer +=
            Time.deltaTime;


        // =====================================================
        // NORMALNA FAZA
        // =====================================================

        if (!isShrinking)
        {
            if (lifeTimer >= activeDuration)
            {
                StartShrinking();
            }
        }


        // =====================================================
        // ZMNIEJSZANIE
        // =====================================================

        if (isShrinking)
        {
            UpdateShrink();
        }


        // =====================================================
        // CZYSZCZENIE
        // =====================================================

        CleanupEnemies();
    }


    // =========================================================
    // START ZMNIEJSZANIA
    // =========================================================

    private void StartShrinking()
    {
        if (isShrinking)
            return;


        isShrinking = true;
    }


    // =========================================================
    // ZMNIEJSZANIE
    // =========================================================

    private void UpdateShrink()
    {
        if (shrinkDuration <= 0f)
        {
            DestroySlime();

            return;
        }


        float shrinkTime =
            lifeTimer -
            activeDuration;


        float progress =
            Mathf.Clamp01(
                shrinkTime /
                shrinkDuration
            );


        // =====================================================
        // SKALA
        // =====================================================

        float scaleMultiplier =
            1f -
            progress;


        transform.localScale =
            new Vector3(
                originalScale.x *
                scaleMultiplier,

                originalScale.y,

                originalScale.z *
                scaleMultiplier
            );


        // =====================================================
        // COLLIDER
        // =====================================================

        // Collider jest dzieckiem / częścią Slime'a,
        // więc zmniejsza się razem z transformem.
        //
        // Nie zmieniamy już jego radiusu.
        // Dzięki temu wykrywanie jest oparte
        // bezpośrednio na fizycznym kształcie collidera.


        // =====================================================
        // KONIEC
        // =====================================================

        if (progress >= 1f)
        {
            DestroySlime();
        }
    }


    // =========================================================
    // TRIGGER ENTER
    // =========================================================

    private void OnTriggerEnter(
        Collider other)
    {
        if (!initialized)
            return;


        EnemyMovement enemy =
            other.GetComponentInParent<EnemyMovement>();


        if (enemy == null)
            return;


        EnemyHealth health =
            enemy.GetComponent<EnemyHealth>();


        if (health == null)
            return;


        if (health.IsDying)
            return;


        // =====================================================
        // DODAJEMY DO LISTY
        // =====================================================

        if (enemiesInside.Contains(enemy))
            return;


        enemiesInside.Add(
            enemy
        );


        // =====================================================
        // SLOW
        // =====================================================

        ApplySlow(
            enemy
        );
    }


    // =========================================================
    // TRIGGER EXIT
    // =========================================================

    private void OnTriggerExit(
        Collider other)
    {
        if (!initialized)
            return;


        EnemyMovement enemy =
            other.GetComponentInParent<EnemyMovement>();


        if (enemy == null)
            return;


        if (!enemiesInside.Contains(enemy))
            return;


        // =====================================================
        // USUWAMY Z LISTY
        // =====================================================

        enemiesInside.Remove(
            enemy
        );


        // =====================================================
        // USUWAMY SLOW
        // =====================================================

        RemoveSlow(
            enemy
        );


        // =====================================================
        // DOT PO OPUSZCZENIU
        // =====================================================

        ApplyDot(
            enemy
        );
    }


    // =========================================================
    // APPLY SLOW
    // =========================================================

    private void ApplySlow(
        EnemyMovement enemy)
    {
        if (enemy == null)
            return;


        if (skill == null)
            return;


        float slow =
            Mathf.Clamp01(
                skill.slimeSlowPercent
            );


        enemy.ApplySlimeSlow(
            slow
        );
    }


    // =========================================================
    // REMOVE SLOW
    // =========================================================

    private void RemoveSlow(
        EnemyMovement enemy)
    {
        if (enemy == null)
            return;


        enemy.RemoveSlimeSlow();
    }


    // =========================================================
    // APPLY DOT
    // =========================================================

    private void ApplyDot(
        EnemyMovement enemy)
    {
        if (enemy == null)
            return;


        EnemyHealth health =
            enemy.GetComponent<EnemyHealth>();


        if (health == null)
            return;


        if (health.IsDying)
            return;


        // =====================================================
        // SZUKAMY ISTNIEJĄCEGO DOT
        // =====================================================

        ChaosDamageOverTime dot =
            enemy.GetComponent<ChaosDamageOverTime>();


        // =====================================================
        // JEŚLI NIE MA — TWORZYMY
        // =====================================================

        if (dot == null)
        {
            dot =
                enemy.gameObject.AddComponent<
                    ChaosDamageOverTime
                >();


            dot.Initialize(
                health,
                skill.slimeDotDamage,
                skill.slimeDotDuration,
                skill.slimeDotTickInterval
            );
        }
        else
        {
            // =================================================
            // ODNOWIENIE DOT
            // =================================================

            dot.Refresh(
                skill.slimeDotDamage,
                skill.slimeDotDuration,
                skill.slimeDotTickInterval
            );
        }
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void CleanupEnemies()
    {
        if (enemiesInside.Count == 0)
            return;


        List<EnemyMovement> toRemove =
            new List<EnemyMovement>();


        foreach (
            EnemyMovement enemy
            in enemiesInside)
        {
            if (enemy == null)
            {
                toRemove.Add(enemy);

                continue;
            }


            EnemyHealth health =
                enemy.GetComponent<EnemyHealth>();


            if (health == null ||
                health.IsDying)
            {
                toRemove.Add(enemy);
            }
        }


        foreach (
            EnemyMovement enemy
            in toRemove)
        {
            if (enemy != null)
            {
                RemoveSlow(enemy);
            }


            enemiesInside.Remove(enemy);
        }
    }


    // =========================================================
    // DESTROY SLIME
    // =========================================================

    private void DestroySlime()
    {
        // =====================================================
        // USUWAMY SLOW
        // =====================================================

        foreach (
            EnemyMovement enemy
            in enemiesInside)
        {
            if (enemy == null)
                continue;


            RemoveSlow(enemy);
        }


        enemiesInside.Clear();


        // =====================================================
        // USUWAMY OBIEKT
        // =====================================================

        Destroy(gameObject);
    }


    // =========================================================
    // ON DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (enemiesInside == null)
            return;


        foreach (
            EnemyMovement enemy
            in enemiesInside)
        {
            if (enemy == null)
                continue;


            RemoveSlow(enemy);
        }


        enemiesInside.Clear();
    }


    // =========================================================
    // GIZMO
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        // Celowo brak SphereGizmo.
        //
        // Obszar wykrywania jest teraz wyznaczany
        // przez rzeczywisty Collider Slime'a.
    }
}