using UnityEngine;

public class SlimeProjectile : ProjectileBase
{
    // =========================================================
    // RUCH
    // =========================================================

    [Header("Slime Projectile")]

    [SerializeField]
    private float speed = 12f;

    [SerializeField]
    private float maxDistance = 8f;


    // =========================================================
    // SKILL
    // =========================================================

    private SkillInstance skill;


    // =========================================================
    // KIERUNEK
    // =========================================================

    private Vector3 direction;


    // =========================================================
    // START
    // =========================================================

    private Vector3 startPosition;


    // =========================================================
    // AKTYWNY SLIME
    // =========================================================

    private bool hasLanded = false;


    // =========================================================
    // INITIALIZE
    // =========================================================

    public override void Initialize(
        SkillInstance skillInstance)
    {
        base.Initialize(
            skillInstance
        );


        skill =
            skillInstance;


        if (skill == null)
        {
            Destroy(gameObject);

            return;
        }


        if (skill.data == null)
        {
            Destroy(gameObject);

            return;
        }


        startPosition =
            transform.position;


        // =====================================================
        // DOMYŚLNY KIERUNEK
        // =====================================================

        direction =
            transform.forward;


        direction.y = 0f;


        if (direction.sqrMagnitude <=
            0.001f)
        {
            direction =
                Vector3.forward;
        }


        direction.Normalize();


        // =====================================================
        // ROTACJA
        // =====================================================

        transform.rotation =
            Quaternion.LookRotation(
                direction
            );
    }


    // =========================================================
    // USTAWIENIE KIERUNKU
    // =========================================================

    public void SetDirection(
        Vector3 newDirection)
    {
        newDirection.y = 0f;


        if (newDirection.sqrMagnitude <=
            0.001f)
        {
            return;
        }


        direction =
            newDirection.normalized;


        transform.rotation =
            Quaternion.LookRotation(
                direction
            );
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (hasLanded)
            return;


        // =====================================================
        // RUCH
        // =====================================================

        transform.position +=
            direction *
            speed *
            Time.deltaTime;


        // =====================================================
        // ODLEGŁOŚĆ
        // =====================================================

        float distance =
            Vector3.Distance(
                startPosition,
                transform.position
            );


        if (distance >= maxDistance)
        {
            Land();
        }
    }


    // =========================================================
    // HIT
    // =========================================================

    public override void Hit(
        GameObject enemy)
    {
        if (hasLanded)
            return;


        Land();
    }


    // =========================================================
    // LAND
    // =========================================================

    private void Land()
    {
        if (hasLanded)
            return;


        hasLanded = true;


        // =====================================================
        // ZATRZYMUJEMY RUCH
        // =====================================================

        enabled = false;


        // =====================================================
        // COLLIDER
        //
        // BARDZO WAŻNE:
        // NIE WYŁĄCZAMY COLLIDERA!
        //
        // SlimeEffect potrzebuje go jako Triggera
        // do wykrywania przeciwników.
        // =====================================================

        Collider[] colliders =
            GetComponentsInChildren<Collider>(
                true
            );


        foreach (
            Collider collider
            in colliders)
        {
            if (collider == null)
                continue;


            collider.enabled = true;


            collider.isTrigger = true;
        }


        // =====================================================
        // SLIME EFFECT
        // =====================================================

        SlimeEffect effect =
            GetComponent<SlimeEffect>();


        if (effect == null)
        {
            effect =
                gameObject.AddComponent<
                    SlimeEffect
                >();
        }


        effect.Initialize(
            skill
        );
    }
}