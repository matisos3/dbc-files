using UnityEngine;

public class ChaosDamageOverTime : MonoBehaviour
{
    private EnemyHealth enemy;

    private float damage;
    private float duration;
    private float tickInterval;

    private float remainingTime;
    private float tickTimer;


    // =========================================================
    // KOLOR CHAOS DOT
    // =========================================================

    [Header("Chaos DoT - kolor")]

    [SerializeField]
    private Color chaosColor = Color.green;


    private Renderer[] renderers;

    private Color[] originalColors;

    private bool colorApplied = false;


    // =========================================================
    // INITIALIZE
    // =========================================================

    public void Initialize(
        EnemyHealth target,
        float dotDamage,
        float dotDuration,
        float dotTickInterval)
    {
        enemy = target;

        damage = dotDamage;
        duration = dotDuration;
        tickInterval = dotTickInterval;

        remainingTime = duration;
        tickTimer = tickInterval;


        // =====================================================
        // KOLOR MODELU
        // =====================================================

        SetupChaosColor();
    }


    // =========================================================
    // REFRESH
    // =========================================================

    public void Refresh(
        float dotDamage,
        float dotDuration,
        float dotTickInterval)
    {
        damage = dotDamage;
        duration = dotDuration;
        tickInterval = dotTickInterval;

        // Odświeżamy czas działania
        remainingTime = duration;

        // Nie resetujemy ticka
        if (tickTimer > tickInterval)
        {
            tickTimer = tickInterval;
        }


        // =====================================================
        // UPEWNIAMY SIĘ, ŻE KOLOR NADAL JEST ZIELONY
        // =====================================================

        if (!colorApplied)
        {
            SetupChaosColor();
        }
    }


    // =========================================================
    // SETUP KOLORU
    // =========================================================

    private void SetupChaosColor()
    {
        if (enemy == null)
            return;


        // =====================================================
        // SZUKAMY RENDERERÓW MODELU PRZECIWNIKA
        // =====================================================

        renderers =
            enemy.GetComponentsInChildren<Renderer>();


        if (renderers == null ||
            renderers.Length == 0)
        {
            return;
        }


        // =====================================================
        // ZAPISUJEMY ORYGINALNE KOLORY
        // =====================================================

        originalColors =
            new Color[renderers.Length];


        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;


            Material material =
                renderers[i].material;


            if (material.HasProperty("_Color"))
            {
                originalColors[i] =
                    material.color;
            }
            else
            {
                originalColors[i] =
                    Color.white;
            }
        }


        // =====================================================
        // USTAWIAMY ZIELONY KOLOR
        // =====================================================

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;


            Material material =
                renderers[i].material;


            if (material.HasProperty("_Color"))
            {
                material.color =
                    chaosColor;
            }
        }


        colorApplied = true;
    }


    // =========================================================
    // PRZYWRACANIE KOLORU
    // =========================================================

    private void RestoreOriginalColor()
    {
        if (!colorApplied)
            return;


        if (renderers == null ||
            originalColors == null)
            return;


        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;


            if (i >= originalColors.Length)
                continue;


            Material material =
                renderers[i].material;


            if (material.HasProperty("_Color"))
            {
                material.color =
                    originalColors[i];
            }
        }


        colorApplied = false;
    }


    // =========================================================
    // DETONATE
    // =========================================================

    public void Detonate(float instantDamage)
    {
        if (enemy == null)
            return;

        if (enemy.IsDying)
            return;

        if (instantDamage <= 0f)
            return;


        enemy.TakeDamage(
            instantDamage
        );
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (enemy == null)
        {
            RestoreOriginalColor();

            Destroy(this);

            return;
        }


        if (enemy.IsDying)
        {
            RestoreOriginalColor();

            Destroy(this);

            return;
        }


        // =====================================================
        // CZAS
        // =====================================================

        remainingTime -=
            Time.deltaTime;

        tickTimer -=
            Time.deltaTime;


        // =====================================================
        // TICK DAMAGE
        // =====================================================

        if (tickTimer <= 0f)
        {
            enemy.TakeDamage(
                damage
            );

            tickTimer +=
                tickInterval;
        }


        // =====================================================
        // KONIEC DOT
        // =====================================================

        if (remainingTime <= 0f)
        {
            RestoreOriginalColor();

            Destroy(this);
        }
    }


    // =========================================================
    // ON DESTROY
    // =========================================================

    private void OnDestroy()
    {
        RestoreOriginalColor();
    }
}