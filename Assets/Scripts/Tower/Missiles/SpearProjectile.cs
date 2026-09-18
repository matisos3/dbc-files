using System.Collections.Generic;
using UnityEngine;

public class SpearProjectile : ProjectileBase
{
    [Header("Movement")]
    [SerializeField] private float speed = 20f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 720f;


    // =========================================================
    // RUCH
    // =========================================================

    private Vector3 moveDirection;

    private bool initialized = false;


    // =========================================================
    // PIERCING
    // =========================================================

    // Liczba przeciwników, których włócznia może jeszcze trafić.
    private int remainingPiercing;


    // Przeciwnicy już trafieni przez tę włócznię.
    private readonly HashSet<GameObject> hitEnemies =
        new HashSet<GameObject>();


    // =========================================================
    // INITIALIZE
    // =========================================================

    public override void Initialize(
        SkillInstance instance)
    {
        base.Initialize(instance);


        if (skillInstance == null ||
            skillInstance.data == null)
        {
            initialized = false;

            return;
        }


        // =====================================================
        // PIERCING
        // =====================================================

        remainingPiercing =
            Mathf.Max(
                1,
                skillInstance.piercingCount
            );


        hitEnemies.Clear();


        initialized = true;
    }


    // =========================================================
    // USTAWIENIE KIERUNKU
    // =========================================================

    public void SetDirection(
        Vector3 direction)
    {
        direction.y = 0f;


        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction =
                transform.forward;

            direction.y = 0f;
        }


        moveDirection =
            direction.normalized;


        RotateTowardsDirection();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!initialized)
            return;


        // =====================================================
        // RUCH W PROSTEJ LINII
        // =====================================================

        transform.position +=
            moveDirection *
            speed *
            Time.deltaTime;


        // =====================================================
        // OBRÓT WOKÓŁ WŁASNEJ OSI
        // =====================================================

        transform.Rotate(
            0f,
            0f,
            rotationSpeed * Time.deltaTime,
            Space.Self
        );


        // =====================================================
        // SPRAWDZANIE PRZECIWNIKÓW
        // =====================================================

        CheckForEnemies();
    }


    // =========================================================
    // SPRAWDZANIE TRAFIENIA
    // =========================================================

    private void CheckForEnemies()
    {
        if (remainingPiercing <= 0)
            return;


        Collider[] colliders =
            Physics.OverlapSphere(
                transform.position,
                0.4f
            );


        foreach (Collider collider in colliders)
        {
            if (collider == null)
                continue;


            // =================================================
            // SZUKAMY ENEMY
            // =================================================

            EnemyHealth enemyHealth =
                collider.GetComponent<EnemyHealth>();


            if (enemyHealth == null)
            {
                enemyHealth =
                    collider.GetComponentInParent<EnemyHealth>();
            }


            if (enemyHealth == null)
                continue;


            if (enemyHealth.IsDying)
                continue;


            GameObject enemy =
                enemyHealth.gameObject;


            // =================================================
            // NIE TRAFIAMY TEGO SAMEGO WROGA PONOWNIE
            // =================================================

            if (hitEnemies.Contains(enemy))
                continue;


            // =================================================
            // BRAK PIERCINGU
            // =================================================

            if (remainingPiercing <= 0)
            {
                return;
            }


            // =================================================
            // TRAFIENIE
            // =================================================

            Hit(enemy);


            // =================================================
            // JEŻELI WŁÓCZNIA ZOSTAŁA USUNIĘTA
            // =================================================

            if (this == null)
                return;


            // =================================================
            // KONIEC PIERCINGU
            // =================================================

            if (remainingPiercing <= 0)
                return;
        }
    }


    // =========================================================
    // HIT
    // =========================================================

    public override void Hit(
        GameObject target)
    {
        if (!initialized)
            return;


        if (target == null)
            return;


        if (skillInstance == null ||
            skillInstance.data == null)
        {
            Destroy(gameObject);

            return;
        }


        // =====================================================
        // SZUKAMY ENEMY HEALTH
        // =====================================================

        EnemyHealth enemyHealth =
            target.GetComponent<EnemyHealth>();


        if (enemyHealth == null)
        {
            enemyHealth =
                target.GetComponentInParent<EnemyHealth>();
        }


        if (enemyHealth == null)
            return;


        if (enemyHealth.IsDying)
            return;


        GameObject enemy =
            enemyHealth.gameObject;


        // =====================================================
        // TEN SAM WRÓG NIE MOŻE ZUŻYĆ DRUGIEGO PIERCINGU
        // =====================================================

        if (hitEnemies.Contains(enemy))
            return;


        // =====================================================
        // BRAK PIERCINGU
        // =====================================================

        if (remainingPiercing <= 0)
            return;


        // =====================================================
        // ZAPISUJEMY TRAFIENIE
        // =====================================================

        hitEnemies.Add(
            enemy
        );


        // =====================================================
        // ZUŻYCIE PIERCINGU
        // =====================================================

        remainingPiercing--;


        // =====================================================
        // OBRAŻENIA
        // =====================================================

        enemyHealth.TakeDamage(
            skillInstance.data.damage
        );


        // =====================================================
        // KONIEC
        // =====================================================

        if (remainingPiercing <= 0)
        {
            Destroy(
                gameObject
            );
        }
    }


    // =========================================================
    // OBRÓT W KIERUNKU LOTU
    // =========================================================

    private void RotateTowardsDirection()
    {
        if (moveDirection.sqrMagnitude <= 0.0001f)
            return;


        Quaternion targetRotation =
            Quaternion.LookRotation(
                moveDirection
            );


        transform.rotation =
            targetRotation;
    }
}