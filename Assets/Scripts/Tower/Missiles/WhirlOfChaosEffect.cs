using System.Collections.Generic;
using UnityEngine;

public class WhirlOfChaosEffect : MonoBehaviour
{
    private SkillInstance skill;

    private float remainingTime;
    private float tickTimer;


    // =========================================================
    // WROGOWIE POD WPŁYWEM WIRU
    // =========================================================

    private HashSet<EnemyMovement> affectedEnemies =
        new HashSet<EnemyMovement>();


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
        // CZAS
        // =====================================================

        remainingTime =
            skill.whirlDuration;


        // =====================================================
        // PIERWSZY TICK NATYCHMIAST
        // =====================================================

        tickTimer =
            0f;


        // =====================================================
        // SKALA
        // =====================================================

        Vector3 currentScale =
            transform.localScale;


        transform.localScale =
            new Vector3(
                skill.whirlRadius * 2f,
                currentScale.y,
                skill.whirlRadius * 2f
            );
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (skill == null)
        {
            EndWhirlForAllEnemies();

            Destroy(gameObject);

            return;
        }


        // =====================================================
        // CZAS
        // =====================================================

        remainingTime -=
            Time.deltaTime;


        // =====================================================
        // WCIĄGANIE
        // =====================================================

        PullEnemies();


        // =====================================================
        // DAMAGE TICK
        // =====================================================

        tickTimer -=
            Time.deltaTime;


        if (tickTimer <= 0f)
        {
            DealDamage();


            if (skill.whirlTickInterval > 0f)
            {
                tickTimer =
                    skill.whirlTickInterval;
            }
            else
            {
                tickTimer =
                    1f;
            }
        }


        // =====================================================
        // KONIEC
        // =====================================================

        if (remainingTime <= 0f)
        {
            EndWhirlForAllEnemies();

            Destroy(gameObject);
        }
    }


    // =========================================================
    // PULL ENEMIES
    // =========================================================

    private void PullEnemies()
    {
        Collider[] colliders =
            Physics.OverlapSphere(
                transform.position,
                skill.whirlRadius
            );


        HashSet<EnemyMovement> enemies =
            new HashSet<EnemyMovement>();


        foreach (Collider collider in colliders)
        {
            if (collider == null)
                continue;


            EnemyMovement enemy =
                collider.GetComponentInParent<EnemyMovement>();


            if (enemy == null)
                continue;


            EnemyHealth health =
                enemy.GetComponent<EnemyHealth>();


            if (health == null)
                continue;


            if (health.IsDying)
                continue;


            enemies.Add(enemy);
        }


        // =====================================================
        // WCIĄGANIE DO ŚRODKA
        // =====================================================

        foreach (EnemyMovement enemy in enemies)
        {
            if (enemy == null)
                continue;


            EnemyHealth health =
                enemy.GetComponent<EnemyHealth>();


            if (health != null &&
                health.IsDying)
            {
                continue;
            }


            // =================================================
            // ZAPAMIĘTUJEMY PRZECIWNIKA
            //
            // UWAGA:
            // NIE wywołujemy BeginWhirlOfChaos().
            //
            // EnemyMovement ma nadal normalnie działać.
            // Wir jedynie dodatkowo przesuwa przeciwnika.
            // =================================================

            affectedEnemies.Add(enemy);


            // =================================================
            // KIERUNEK DO ŚRODKA WIRU
            // =================================================

            Vector3 direction =
                transform.position -
                enemy.transform.position;


            // Nie przyciągamy w osi Y.
            direction.y = 0f;


            // =================================================
            // JEŻELI JEST BARDZO BLISKO ŚRODKA
            // =================================================

            if (direction.sqrMagnitude <= 0.01f)
                continue;


            direction.Normalize();


            // =================================================
            // DODATKOWY RUCH W STRONĘ ŚRODKA
            // =================================================

            enemy.transform.position +=
                direction *
                skill.whirlPullSpeed *
                Time.deltaTime;
        }
    }


    // =========================================================
    // END WHIRL FOR ALL ENEMIES
    // =========================================================

    private void EndWhirlForAllEnemies()
    {
        // =====================================================
        // NIE wywołujemy EndWhirlOfChaos().
        //
        // Whirl nie zmienia stanu EnemyMovement,
        // więc nie ma czego przywracać.
        // =====================================================

        affectedEnemies.Clear();
    }


    // =========================================================
    // DAMAGE
    // =========================================================

    private void DealDamage()
    {
        Collider[] colliders =
            Physics.OverlapSphere(
                transform.position,
                skill.whirlRadius
            );


        HashSet<EnemyHealth> enemies =
            new HashSet<EnemyHealth>();


        foreach (Collider collider in colliders)
        {
            if (collider == null)
                continue;


            EnemyHealth enemy =
                collider.GetComponentInParent<EnemyHealth>();


            if (enemy == null)
                continue;


            if (enemy.IsDying)
                continue;


            enemies.Add(enemy);
        }


        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null)
                continue;


            if (enemy.IsDying)
                continue;


            enemy.TakeDamage(
                skill.whirlDamage
            );
        }
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void OnDestroy()
    {
        EndWhirlForAllEnemies();
    }


    // =========================================================
    // GIZMO
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (skill == null)
            return;


        Gizmos.color =
            Color.magenta;


        Gizmos.DrawWireSphere(
            transform.position,
            skill.whirlRadius
        );
    }
}