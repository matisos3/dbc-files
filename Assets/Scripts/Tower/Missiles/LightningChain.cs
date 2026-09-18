using UnityEngine;

public class LightningChain : MonoBehaviour
{
    // =========================================================
    // CELE
    // =========================================================

    private Transform startTarget;
    private Transform endTarget;

    private bool startIsEnemy;
    private bool endIsEnemy;

    // =========================================================
    // USTAWIENIA VFX
    // =========================================================

    [Header("Lightning VFX")]
    [SerializeField] private Transform lightningVisual;

    [Tooltip("Oś Quada odpowiadająca za jego długość.")]
    [SerializeField] private Axis lengthAxis = Axis.X;

    [Tooltip("Szerokość błyskawicy.")]
    [SerializeField] private float width = 1f;

    [Tooltip("Stała rotacja X efektu.")]
    [SerializeField] private float fixedRotationX = 90f;

    [Header("Duration")]
    [SerializeField] private float duration = 0.15f;

    private float timer;

    // =========================================================
    // PUNKT STARTOWY
    // =========================================================

    private Vector3 storedStartPosition;

    // =========================================================
    // ZAPAMIĘTANE POZYCJE CELÓW
    // =========================================================

    private Vector3 storedStartTargetPosition;
    private Vector3 storedEndTargetPosition;

    private bool hasStoredStartPosition;
    private bool hasStoredEndPosition;

    // =========================================================
    // OSIE
    // =========================================================

    public enum Axis
    {
        X,
        Y,
        Z
    }


    // =========================================================
    // INITIALIZE
    // WRÓG → WRÓG
    // =========================================================

    public void Initialize(
        Transform start,
        Transform end)
    {
        startTarget = start;
        endTarget = end;

        startIsEnemy =
            IsEnemy(startTarget);

        endIsEnemy =
            IsEnemy(endTarget);

        Setup();
    }


    // =========================================================
    // INITIALIZE
    // PUNKT → WRÓG
    // =========================================================

    public void InitializeFromPoint(
        Vector3 startPosition,
        Transform end)
    {
        startTarget = null;
        endTarget = end;

        startIsEnemy = false;

        endIsEnemy =
            IsEnemy(endTarget);

        storedStartPosition =
            startPosition;

        FindLightningVisual();

        timer =
            duration;

        if (endTarget != null)
        {
            storedEndTargetPosition =
                GetHitPoint(endTarget);

            hasStoredEndPosition = true;
        }

        UpdateLightningFromPoint(
            storedStartPosition
        );
    }


    // =========================================================
    // SETUP
    // =========================================================

    private void Setup()
    {
        FindLightningVisual();

        timer =
            duration;

        if (startTarget == null ||
            endTarget == null)
        {
            Destroy(gameObject);
            return;
        }

        storedStartTargetPosition =
            GetHitPoint(startTarget);

        storedEndTargetPosition =
            GetHitPoint(endTarget);

        hasStoredStartPosition = true;
        hasStoredEndPosition = true;

        UpdateLightning();
    }


    // =========================================================
    // ZNAJDOWANIE QUADA
    // =========================================================

    private void FindLightningVisual()
    {
        if (lightningVisual != null)
            return;

        // -----------------------------------------------------
        // Jeżeli prefab główny posiada MeshRenderer,
        // używamy jego Transform.
        // -----------------------------------------------------

        if (GetComponent<MeshRenderer>() != null)
        {
            lightningVisual =
                transform;

            return;
        }

        // -----------------------------------------------------
        // W przeciwnym przypadku szukamy Quada w dzieciach.
        // -----------------------------------------------------

        MeshRenderer renderer =
            GetComponentInChildren<MeshRenderer>();

        if (renderer != null)
        {
            lightningVisual =
                renderer.transform;

            return;
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (lightningVisual == null)
        {
            FindLightningVisual();

            if (lightningVisual == null)
            {
                Destroy(gameObject);
                return;
            }
        }


        // =====================================================
        // PUNKT → WRÓG
        // =====================================================

        if (startTarget == null)
        {
            UpdateLightningFromPoint(
                storedStartPosition
            );
        }


        // =====================================================
        // WRÓG → WRÓG
        // =====================================================

        else
        {
            UpdateLightning();
        }


        // =====================================================
        // TIMER
        // =====================================================

        timer -=
            Time.deltaTime;


        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }


    // =========================================================
    // WRÓG → WRÓG
    // =========================================================

    private void UpdateLightning()
    {
        Vector3 start;
        Vector3 end;


        // -----------------------------------------------------
        // START
        // -----------------------------------------------------

        if (startTarget != null)
        {
            start =
                GetHitPoint(startTarget);

            storedStartTargetPosition =
                start;

            hasStoredStartPosition = true;
        }
        else
        {
            if (!hasStoredStartPosition)
            {
                Destroy(gameObject);
                return;
            }

            start =
                storedStartTargetPosition;
        }


        // -----------------------------------------------------
        // END
        // -----------------------------------------------------

        if (endTarget != null)
        {
            end =
                GetHitPoint(endTarget);

            storedEndTargetPosition =
                end;

            hasStoredEndPosition = true;
        }
        else
        {
            if (!hasStoredEndPosition)
            {
                Destroy(gameObject);
                return;
            }

            end =
                storedEndTargetPosition;
        }


        UpdateLightningVisual(
            start,
            end
        );
    }


    // =========================================================
    // PUNKT → WRÓG
    // =========================================================

    private void UpdateLightningFromPoint(
        Vector3 start)
    {
        Vector3 end;


        // -----------------------------------------------------
        // CEL ISTNIEJE
        // -----------------------------------------------------

        if (endTarget != null)
        {
            end =
                GetHitPoint(endTarget);

            storedEndTargetPosition =
                end;

            hasStoredEndPosition = true;
        }


        // -----------------------------------------------------
        // CEL ZNIKNĄŁ
        // -----------------------------------------------------

        else
        {
            if (!hasStoredEndPosition)
            {
                Destroy(gameObject);
                return;
            }

            end =
                storedEndTargetPosition;
        }


        UpdateLightningVisual(
            start,
            end
        );
    }


    // =========================================================
    // USTAWIENIE QUADA
    // =========================================================

    private void UpdateLightningVisual(
        Vector3 start,
        Vector3 end)
    {
        if (lightningVisual == null)
            return;


        Vector3 direction =
            end - start;

        float distance =
            direction.magnitude;


        if (distance <= 0.001f)
            return;


        Vector3 midpoint =
            (start + end) * 0.5f;


        // -----------------------------------------------------
        // POZYCJA
        // -----------------------------------------------------

        lightningVisual.position =
            midpoint;


        // -----------------------------------------------------
        // ROTACJA
        //
        // X ZAWSZE = 90°
        //
        // Kierunek pomiędzy punktami wyznaczamy przez Y.
        // Zostawiamy X całkowicie niezależne od kierunku.
        // -----------------------------------------------------

        Vector3 flatDirection =
            direction;

        flatDirection.y = 0f;


        if (flatDirection.sqrMagnitude > 0.001f)
        {
            float angle =
                Mathf.Atan2(
                    flatDirection.z,
                    flatDirection.x
                ) * Mathf.Rad2Deg;


            lightningVisual.rotation =
                Quaternion.Euler(
                    fixedRotationX,
                    -angle+90f,
                    0f
                );
        }
        else
        {
            lightningVisual.rotation =
                Quaternion.Euler(
                    fixedRotationX,
                    lightningVisual.eulerAngles.y+90f,
                    0f
                );
        }


        // -----------------------------------------------------
        // SKALA
        // -----------------------------------------------------

        Vector3 scale =
            lightningVisual.localScale;


        switch (lengthAxis)
        {
            case Axis.X:

                scale.x =
                    distance;

                scale.y =
                    width;

                break;


            case Axis.Y:

                scale.x =
                    width;

                scale.y =
                    distance;

                break;


            case Axis.Z:

                scale.x =
                    width;

                scale.z =
                    distance;

                break;
        }


        lightningVisual.localScale =
            scale;
    }


    // =========================================================
    // CZY WRÓG
    // =========================================================

    private bool IsEnemy(
        Transform target)
    {
        if (target == null)
            return false;

        EnemyHealth enemyHealth =
            target.GetComponentInParent<EnemyHealth>();

        return enemyHealth != null;
    }


    // =========================================================
    // HIT POINT
    // =========================================================

    private Vector3 GetHitPoint(
        Transform target)
    {
        if (target == null)
            return Vector3.zero;


        EnemyHealth enemyHealth =
            target.GetComponentInParent<EnemyHealth>();


        if (enemyHealth != null &&
            enemyHealth.hitPoint != null)
        {
            return enemyHealth.hitPoint.position;
        }


        return target.position;
    }


    // =========================================================
    // USTAWIENIE STARTU
    // =========================================================

    public void SetStartPosition(
        Vector3 position)
    {
        storedStartPosition =
            position;
    }
}