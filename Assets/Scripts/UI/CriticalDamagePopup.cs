using UnityEngine;
using TMPro;

public class CriticalDamagePopup : MonoBehaviour
{
    [Header("Tekst")]
    [SerializeField]
    private TMP_Text damageText;

    [Header("Kolor")]
    [SerializeField]
    private Color critColor =
        new Color(
            1f,
            0.72f,
            0.05f,
            1f
        );

    [Header("Pozycja")]
    [SerializeField]
    private float heightOffset = 0.35f;

    [SerializeField]
    private float moveHeight = 1.1f;

    [Header("Czas")]
    [SerializeField]
    private float lifetime = 0.9f;

    [Header("Animacja POP")]
    [SerializeField]
    private float startScale = 0.15f;

    [SerializeField]
    private float popScale = 1.35f;

    [SerializeField]
    private float finalScale = 1f;

    [SerializeField]
    private float popDuration = 0.16f;

    [SerializeField]
    private float settleDuration = 0.14f;

    [Header("Stała rotacja")]
    [SerializeField]
    private Vector3 fixedRotation =
        new Vector3(80f, 0f, 0f);

    [Header("Unoszenie")]
    [SerializeField]
    private AnimationCurve movementCurve =
        AnimationCurve.EaseInOut(
            0f,
            0f,
            1f,
            1f
        );

    [Header("Znikanie")]
    [SerializeField]
    private AnimationCurve fadeCurve =
        AnimationCurve.EaseInOut(
            0f,
            0f,
            1f,
            1f
        );

    private Vector3 startPosition;
    private float timer;

    private void Awake()
    {
        // Ustawiamy rotację tylko raz.
        // Kod nigdy później jej nie zmienia.
        transform.rotation =
            Quaternion.Euler(
                fixedRotation
            );
    }

    public void Initialize(float damage)
    {
        timer = 0f;

        // Wymuszenie dokładnie stałej rotacji 90°.
        // To jest ostatnie miejsce w kodzie,
        // w którym ustawiamy rotację.
        transform.rotation =
            Quaternion.Euler(
                80f,
                0f,
                0f
            );

        startPosition =
            transform.position +
            Vector3.up * heightOffset;

        transform.position =
            startPosition;

        transform.localScale =
            Vector3.one *
            startScale;

        SetupText(damage);
    }

    private void SetupText(float damage)
    {
        if (damageText == null)
            return;

        int roundedDamage =
            Mathf.RoundToInt(damage);

        damageText.text =
            roundedDamage.ToString();

        damageText.alignment =
            TextAlignmentOptions.Center;

        damageText.color =
            critColor;

        damageText.fontStyle =
            FontStyles.Bold;

        damageText.richText =
            true;

        damageText.textWrappingMode =
            TextWrappingModes.NoWrap;

        damageText.outlineWidth =
            0.2f;

        damageText.outlineColor =
            new Color(
                0.15f,
                0.08f,
                0f,
                1f
            );
    }

    private void Update()
    {
        timer +=
            Time.deltaTime;

        AnimateMovement();

        AnimateScale();

        AnimateFade();

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void AnimateMovement()
    {
        if (lifetime <= 0f)
            return;

        float progress =
            Mathf.Clamp01(
                timer / lifetime
            );

        float curvedProgress =
            movementCurve.Evaluate(
                progress
            );

        transform.position =
            startPosition +
            Vector3.up *
            (moveHeight * curvedProgress);
    }

    private void AnimateScale()
    {
        if (popDuration <= 0f)
        {
            transform.localScale =
                Vector3.one *
                finalScale;

            return;
        }

        if (timer < popDuration)
        {
            float progress =
                Mathf.Clamp01(
                    timer / popDuration
                );

            float eased =
                EaseOutBack(progress);

            float scale =
                Mathf.LerpUnclamped(
                    startScale,
                    popScale,
                    eased
                );

            transform.localScale =
                Vector3.one *
                scale;

            return;
        }

        float settleStart =
            popDuration;

        float settleEnd =
            popDuration +
            settleDuration;

        if (timer < settleEnd)
        {
            float progress =
                Mathf.Clamp01(
                    (timer - settleStart) /
                    settleDuration
                );

            float eased =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress
                );

            float scale =
                Mathf.Lerp(
                    popScale,
                    finalScale,
                    eased
                );

            transform.localScale =
                Vector3.one *
                scale;

            return;
        }

        transform.localScale =
            Vector3.one *
            finalScale;
    }

    private void AnimateFade()
    {
        if (damageText == null)
            return;

        float fadeStart =
            lifetime * 0.55f;

        if (timer < fadeStart)
        {
            SetTextAlpha(1f);
            return;
        }

        float fadeDuration =
            lifetime - fadeStart;

        if (fadeDuration <= 0f)
        {
            SetTextAlpha(0f);
            return;
        }

        float fadeProgress =
            Mathf.Clamp01(
                (timer - fadeStart) /
                fadeDuration
            );

        float curveValue =
            fadeCurve.Evaluate(
                fadeProgress
            );

        float alpha =
            Mathf.Lerp(
                1f,
                0f,
                curveValue
            );

        SetTextAlpha(alpha);
    }

    private void SetTextAlpha(float alpha)
    {
        if (damageText == null)
            return;

        Color color =
            damageText.color;

        color.a =
            alpha;

        damageText.color =
            color;
    }

    private float EaseOutBack(float t)
    {
        const float c1 =
            1.70158f;

        const float c3 =
            c1 + 1f;

        return
            1f +
            c3 *
            Mathf.Pow(
                t - 1f,
                3f
            ) +
            c1 *
            Mathf.Pow(
                t - 1f,
                2f
            );
    }
}