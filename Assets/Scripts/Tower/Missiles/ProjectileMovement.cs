using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ProjectileBase))]
public class ProjectileMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform model;

    [Header("Movement")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float arcHeight = 2f;

    [Header("Model Rotation")]
    [SerializeField] private Vector3 modelRotationOffset;

    [Header("Point Target Collision")]
    [SerializeField] private float collisionRadius = 0.15f;

    [Header("Ground Roll")]
    [SerializeField] private float groundCollisionRadius = 0.3f;
    [SerializeField] private float groundOffset = 0.05f;

    private Transform target;
    private ProjectileBase projectile;

    private Vector3 startPosition;
    private Vector3 currentTargetPosition;

    private float totalDistance;
    private float travelledDistance;

    private bool usePositionTarget = false;

    private Vector3 previousPosition;

    private bool hitProcessed = false;


    // =========================================================
    // ICE SHOT - MODEL SCALE
    // =========================================================

    private Vector3 iceShotBaseModelScale;

    private bool iceShotModelScaleInitialized = false;


    // =========================================================
    // ICE SHOT - TOCZENIE PO ZIEMI
    // =========================================================

    private bool groundRoll = false;

    private float groundY;

    private Vector3 groundDirection;


    // =========================================================
    // SLOW AURA
    // =========================================================

    private Dictionary<EnemyProjectileSlowAura, float> slowAuras =
        new Dictionary<EnemyProjectileSlowAura, float>();


    // =========================================================
    // ICE SHOT - ROZPOZNANIE
    // =========================================================

    private bool IsIceShot()
    {
        return projectile is IceShotProjectile;
    }


    // =========================================================
    // ICE SHOT - POBRANIE PROMIENIA AOE
    // =========================================================

    private float GetIceShotRadius()
    {
        if (!IsIceShot())
            return groundCollisionRadius;

        IceShotProjectile iceShot =
            projectile as IceShotProjectile;

        if (iceShot == null)
            return groundCollisionRadius;

        float radius =
            iceShot.GetIceShotRadius();

        if (radius <= 0f)
            return groundCollisionRadius;

        return radius;
    }


    // =========================================================
    // ICE SHOT - ZAPIS BAZOWEJ SKALI MODELU
    // =========================================================

    public void InitializeIceShotModelScale()
    {
        if (model == null)
            return;

        if (iceShotModelScaleInitialized)
            return;

        iceShotBaseModelScale =
            model.localScale;

        iceShotModelScaleInitialized =
            true;
    }


    // =========================================================
    // ICE SHOT - USTAW SKALĘ MODELU
    // =========================================================

    public void SetIceShotModelScale(
        float multiplier)
    {
        if (model == null)
            return;

        if (!iceShotModelScaleInitialized)
        {
            iceShotBaseModelScale =
                model.localScale;

            iceShotModelScaleInitialized =
                true;
        }

        multiplier =
            Mathf.Max(
                0f,
                multiplier
            );

        model.localScale =
            iceShotBaseModelScale *
            multiplier;
    }


    // =========================================================
    // USTAWIENIE PRĘDKOŚCI
    // =========================================================

    public void SetSpeed(float newSpeed)
    {
        speed =
            Mathf.Max(
                0.01f,
                newSpeed
            );
    }


    // =========================================================
    // INITIALIZE - NORMALNY POCISK
    // =========================================================

    public void Initialize(Transform newTarget)
    {
        projectile =
            GetComponent<ProjectileBase>();

        groundRoll =
            false;

        SetTarget(newTarget);
    }


    // =========================================================
    // INITIALIZE - ICE SHOT
    // =========================================================

    public void InitializeGroundRoll(Transform newTarget)
    {
        projectile =
            GetComponent<ProjectileBase>();

        if (newTarget == null)
        {
            Destroy(gameObject);
            return;
        }

        target =
            newTarget;

        usePositionTarget =
            false;

        groundRoll =
            true;

        hitProcessed =
            false;

        startPosition =
            transform.position;

        previousPosition =
            startPosition;

        travelledDistance =
            0f;

        groundY =
            startPosition.y;

        Vector3 direction =
            newTarget.position -
            startPosition;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction =
                transform.forward;

            direction.y = 0f;
        }

        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction =
                Vector3.forward;
        }

        groundDirection =
            direction.normalized;

        UpdateTargetPosition();

        currentTargetPosition.y =
            groundY;

        totalDistance =
            Vector3.Distance(
                startPosition,
                currentTargetPosition
            );

        if (totalDistance <= 0.001f)
        {
            totalDistance =
                0.001f;
        }
    }


    // =========================================================
    // USTAWIENIE NOWEGO CELU
    // =========================================================

    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null)
            return;

        target =
            newTarget;

        usePositionTarget =
            false;

        hitProcessed =
            false;

        startPosition =
            transform.position;

        previousPosition =
            startPosition;

        travelledDistance =
            0f;

        UpdateTargetPosition();

        totalDistance =
            Vector3.Distance(
                startPosition,
                currentTargetPosition
            );

        if (totalDistance <= 0.001f)
        {
            totalDistance =
                0.001f;
        }
    }


    // =========================================================
    // INITIALIZE - LOT DO PUNKTU
    // =========================================================

    public void Initialize(Vector3 newTargetPosition)
    {
        projectile =
            GetComponent<ProjectileBase>();

        target =
            null;

        usePositionTarget =
            true;

        groundRoll =
            false;

        hitProcessed =
            false;

        startPosition =
            transform.position;

        previousPosition =
            startPosition;

        travelledDistance =
            0f;

        currentTargetPosition =
            newTargetPosition;

        totalDistance =
            Vector3.Distance(
                startPosition,
                currentTargetPosition
            );

        if (totalDistance <= 0.001f)
        {
            totalDistance =
                0.001f;
        }
    }


    // =========================================================
    // REGISTRY
    // =========================================================

    private void OnEnable()
    {
        ProjectileRegistry.Register(this);
    }


    private void OnDisable()
    {
        ProjectileRegistry.Unregister(this);
    }


    // =========================================================
    // SLOW AURA
    // =========================================================

    public void AddSlowAura(
        EnemyProjectileSlowAura aura,
        float slowPercent)
    {
        if (aura == null)
            return;

        slowAuras[aura] =
            slowPercent;
    }


    public void RemoveSlowAura(
        EnemyProjectileSlowAura aura)
    {
        if (aura == null)
            return;

        slowAuras.Remove(aura);
    }


    private float GetSpeedMultiplier()
    {
        float multiplier =
            1f;

        foreach (
            float slowPercent
            in slowAuras.Values)
        {
            multiplier *=
                1f -
                (slowPercent / 100f);
        }

        return Mathf.Clamp(
            multiplier,
            0.1f,
            1f
        );
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (projectile == null)
        {
            projectile =
                GetComponent<ProjectileBase>();
        }

        if (projectile == null)
        {
            Destroy(gameObject);
            return;
        }


        // =====================================================
        // ICE SHOT - TOCZENIE PO ZIEMI
        // =====================================================

        if (groundRoll)
        {
            UpdateGroundRoll();
            return;
        }


        // =====================================================
        // CEL TRANSFORM
        // =====================================================

        if (!usePositionTarget)
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            UpdateTargetPosition();
        }


        // =====================================================
        // BRAK DYSTANSU
        // =====================================================

        if (totalDistance <= 0f)
        {
            Destroy(gameObject);
            return;
        }


        // =====================================================
        // PRĘDKOŚĆ
        // =====================================================

        float speedMultiplier =
            GetSpeedMultiplier();

        travelledDistance +=
            speed *
            speedMultiplier *
            Time.deltaTime;

        float t =
            Mathf.Clamp01(
                travelledDistance /
                totalDistance
            );


        // =====================================================
        // AKTUALNA POZYCJA
        // =====================================================

        Vector3 position =
            Vector3.Lerp(
                startPosition,
                currentTargetPosition,
                t
            );

        float baseY =
            Mathf.Lerp(
                startPosition.y,
                currentTargetPosition.y,
                t
            );

        float arc =
            Mathf.Sin(
                t * Mathf.PI
            ) *
            arcHeight;

        position.y =
            baseY +
            arc;


        // =====================================================
        // NASTĘPNA POZYCJA
        // =====================================================

        float nextT =
            Mathf.Clamp01(
                t + 0.02f
            );

        Vector3 nextPosition =
            Vector3.Lerp(
                startPosition,
                currentTargetPosition,
                nextT
            );

        float nextBaseY =
            Mathf.Lerp(
                startPosition.y,
                currentTargetPosition.y,
                nextT
            );

        float nextArc =
            Mathf.Sin(
                nextT * Mathf.PI
            ) *
            arcHeight;

        nextPosition.y =
            nextBaseY +
            nextArc;


        // =====================================================
        // SHOTGUN
        // =====================================================

        if (usePositionTarget)
        {
            CheckPointTargetCollision(
                previousPosition,
                position
            );
        }


        transform.position =
            position;


        // =====================================================
        // OBRÓT MODELU
        // =====================================================

        if (model != null)
        {
            Vector3 direction =
                nextPosition -
                position;

            if (direction.sqrMagnitude >
                0.0001f)
            {
                model.rotation =
                    Quaternion.LookRotation(
                        direction
                    ) *
                    Quaternion.Euler(
                        modelRotationOffset
                    );
            }
        }


        previousPosition =
            position;


        // =====================================================
        // TRAFIENIE NORMALNEGO POCISKU
        // =====================================================

        if (!usePositionTarget &&
            t >= 1f &&
            !hitProcessed)
        {
            hitProcessed =
                true;

            Transform oldTarget =
                target;

            if (target != null)
            {
                projectile.Hit(
                    target.gameObject
                );
            }

            if (this == null)
                return;

            if (gameObject == null)
                return;

            if (target != null &&
                target != oldTarget)
            {
                return;
            }

            Destroy(gameObject);

            return;
        }


        // =====================================================
        // KONIEC LOTU SHOTGUNA
        // =====================================================

        if (usePositionTarget &&
            t >= 1f)
        {
            Destroy(gameObject);
        }
    }


    // =========================================================
    // ICE SHOT - RUCH PO ZIEMI
    // =========================================================

    private void UpdateGroundRoll()
    {
        if (projectile == null)
        {
            projectile =
                GetComponent<ProjectileBase>();
        }

        if (projectile == null)
        {
            Destroy(gameObject);
            return;
        }


        // =====================================================
        // NORMALNE POCISKI
        // =====================================================

        if (!IsIceShot() &&
            hitProcessed)
        {
            return;
        }


        // =====================================================
        // ICE SHOT - KONIEC TRASY
        // =====================================================

        if (IsIceShot() &&
            travelledDistance >= totalDistance)
        {
            Destroy(gameObject);
            return;
        }


        // =====================================================
        // KIERUNEK
        // =====================================================

        Vector3 direction =
            groundDirection;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction =
                Vector3.forward;
        }

        direction.Normalize();


        // =====================================================
        // PRĘDKOŚĆ
        // =====================================================

        float speedMultiplier =
            GetSpeedMultiplier();

        float moveDistance =
            speed *
            speedMultiplier *
            Time.deltaTime;


        // =====================================================
        // NIE PRZEKRACZAJ KOŃCA TRASY
        // =====================================================

        if (IsIceShot())
        {
            float remainingDistance =
                totalDistance -
                travelledDistance;

            moveDistance =
                Mathf.Min(
                    moveDistance,
                    remainingDistance
                );
        }


        Vector3 from =
            transform.position;

        Vector3 to =
            from +
            direction *
            moveDistance;


        // =====================================================
        // PROMIEŃ DETEKCJI
        // =====================================================

        float currentCollisionRadius =
            GetIceShotRadius();


        // =====================================================
        // WYKRYWANIE WROGÓW
        // =====================================================

        if (moveDistance > 0f)
        {
            RaycastHit[] hits =
                Physics.SphereCastAll(
                    from,
                    currentCollisionRadius,
                    direction,
                    moveDistance
                );


            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null)
                    continue;

                EnemyHealth enemyHealth =
                    hit.collider.GetComponent<EnemyHealth>();

                if (enemyHealth == null)
                {
                    enemyHealth =
                        hit.collider.GetComponentInParent<EnemyHealth>();
                }

                if (enemyHealth == null)
                    continue;

                if (enemyHealth.IsDying)
                    continue;


                // =================================================
                // ICE SHOT
                // =================================================

                if (IsIceShot())
                {
                    projectile.Hit(
                        enemyHealth.gameObject
                    );
                }
                else
                {
                    ProcessGroundHit(
                        enemyHealth.gameObject,
                        hit.point
                    );

                    return;
                }


                // =================================================
                // JEŚLI KULA ZOSTAŁA ZNISZCZONA
                // =================================================

                if (this == null)
                    return;

                if (gameObject == null)
                    return;
            }
        }


        // =====================================================
        // RUCH
        // =====================================================

        transform.position =
            to;

        transform.position =
            new Vector3(
                transform.position.x,
                groundY + groundOffset,
                transform.position.z
            );


        // =====================================================
        // AKTUALIZUJ DYSTANS
        // =====================================================

        travelledDistance +=
            moveDistance;


        // =====================================================
        // TOCZENIE KULI
        // =====================================================

        float radius =
            Mathf.Max(
                0.01f,
                transform.lossyScale.x * 0.5f
            );

        float rotationAngle =
            (moveDistance /
            radius) *
            Mathf.Rad2Deg;

        Vector3 rollAxis =
            Vector3.Cross(
                Vector3.up,
                direction
            ).normalized;


        if (model != null)
        {
            model.Rotate(
                rollAxis,
                rotationAngle,
                Space.World
            );
        }
        else
        {
            transform.Rotate(
                rollAxis,
                rotationAngle,
                Space.World
            );
        }


        previousPosition =
            transform.position;


        // =====================================================
        // KONIEC TRASY ICE SHOT
        // =====================================================

        if (IsIceShot() &&
            travelledDistance >=
            totalDistance - 0.001f)
        {
            Destroy(gameObject);
        }
    }


    // =========================================================
    // SPRAWDZENIE TRAFIENIA ICE SHOT
    // =========================================================

    private void TryGroundHit()
    {
        float currentCollisionRadius =
            GetIceShotRadius();


        Collider[] colliders =
            Physics.OverlapSphere(
                transform.position,
                currentCollisionRadius
            );

        foreach (Collider collider in colliders)
        {
            if (collider == null)
                continue;

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


            if (IsIceShot())
            {
                projectile.Hit(
                    enemyHealth.gameObject
                );
            }
            else
            {
                ProcessGroundHit(
                    enemyHealth.gameObject,
                    transform.position
                );
            }

            return;
        }
    }


    // =========================================================
    // OBSŁUGA TRAFIENIA NORMALNEGO POCISKU
    // =========================================================

    private void ProcessGroundHit(
        GameObject enemy,
        Vector3 hitPoint)
    {
        if (hitProcessed)
            return;

        hitProcessed =
            true;

        projectile.Hit(enemy);
    }


    // =========================================================
    // COLLISION SHOTGUN
    // =========================================================

    private void CheckPointTargetCollision(
        Vector3 from,
        Vector3 to)
    {
        Vector3 direction =
            to -
            from;

        float distance =
            direction.magnitude;

        if (distance <= 0.0001f)
            return;

        RaycastHit[] hits =
            Physics.SphereCastAll(
                from,
                collisionRadius,
                direction.normalized,
                distance
            );

        foreach (RaycastHit hit in hits)
        {
            EnemyHealth enemyHealth =
                hit.collider.GetComponentInParent<EnemyHealth>();

            if (enemyHealth == null)
                continue;

            if (enemyHealth.IsDying)
                continue;

            projectile.Hit(
                enemyHealth.gameObject
            );

            Destroy(gameObject);

            return;
        }
    }


    // =========================================================
    // UPDATE TARGET POSITION
    // =========================================================

    private void UpdateTargetPosition()
    {
        if (target == null)
            return;

        EnemyHealth enemyHealth =
            target.GetComponent<EnemyHealth>();

        if (enemyHealth != null &&
            enemyHealth.hitPoint != null)
        {
            currentTargetPosition =
                enemyHealth.hitPoint.position;
        }
        else
        {
            currentTargetPosition =
                target.position;
        }
    }


    // =========================================================
    // GET TARGET
    // =========================================================

    public Transform GetTarget()
    {
        return target;
    }


    // =========================================================
    // CZY POCISK TRAFIŁ
    // =========================================================

    public bool HasHitTarget()
    {
        return hitProcessed;
    }
}