using UnityEngine;

public class EnemyDodgeVisual : MonoBehaviour
{
    [Header("Przezroczystość")]
    [SerializeField] private float minAlpha = 0.20f;
    [SerializeField] private float maxAlpha = 1f;

    [Header("Mruganie")]
    [SerializeField] private float blinkSpeed = 8f;

    private Renderer[] renderers;
    private Material[] materials;
    private Color[] originalColors;

    private void Awake()
    {
        renderers =
            GetComponentsInChildren<Renderer>();

        materials =
            new Material[renderers.Length];

        originalColors =
            new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;

            // Tworzymy własną kopię materiału
            // tylko dla tego przeciwnika.
            materials[i] =
                renderers[i].material;

            if (materials[i] == null)
                continue;

            if (materials[i].HasProperty("_Color"))
            {
                originalColors[i] =
                    materials[i].color;
            }

            SetupTransparentMaterial(
                materials[i]
            );
        }
    }


    private void Update()
    {
        float wave =
            (Mathf.Sin(
                Time.time * blinkSpeed
            ) + 1f) * 0.5f;

        float alpha =
            Mathf.Lerp(
                minAlpha,
                maxAlpha,
                wave
            );

        ApplyAlpha(alpha);
    }


    // =========================================================
    // TRANSPARENT MATERIAL
    // =========================================================

    private void SetupTransparentMaterial(
        Material material)
    {
        if (material == null)
            return;


        // =====================================================
        // URP LIT
        // =====================================================

        if (material.HasProperty(
            "_Surface"))
        {
            material.SetFloat(
                "_Surface",
                1f
            );
        }

        if (material.HasProperty(
            "_Blend"))
        {
            material.SetFloat(
                "_Blend",
                0f
            );
        }

        if (material.HasProperty(
            "_AlphaClip"))
        {
            material.SetFloat(
                "_AlphaClip",
                0f
            );
        }


        // =====================================================
        // STANDARD SHADER
        // =====================================================

        if (material.HasProperty(
            "_Mode"))
        {
            material.SetFloat(
                "_Mode",
                3f
            );
        }


        // =====================================================
        // RENDER QUEUE
        // =====================================================

        material.renderQueue =
            3000;


        // =====================================================
        // BLENDING
        // =====================================================

        if (material.HasProperty(
            "_SrcBlend"))
        {
            material.SetInt(
                "_SrcBlend",
                (int)UnityEngine.Rendering.BlendMode.SrcAlpha
            );
        }

        if (material.HasProperty(
            "_DstBlend"))
        {
            material.SetInt(
                "_DstBlend",
                (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha
            );
        }

        if (material.HasProperty(
            "_ZWrite"))
        {
            material.SetInt(
                "_ZWrite",
                0
            );
        }


        // =====================================================
        // KEYWORDY
        // =====================================================

        material.EnableKeyword(
            "_SURFACE_TYPE_TRANSPARENT"
        );

        material.EnableKeyword(
            "_ALPHAPREMULTIPLY_ON"
        );

        material.DisableKeyword(
            "_ALPHATEST_ON"
        );
    }


    // =========================================================
    // ALPHA
    // =========================================================

    private void ApplyAlpha(
        float alpha)
    {
        for (int i = 0;
             i < materials.Length;
             i++)
        {
            Material material =
                materials[i];

            if (material == null)
                continue;


            if (material.HasProperty(
                "_Color"))
            {
                Color color =
                    originalColors[i];

                color.a =
                    alpha;

                material.color =
                    color;
            }


            // URP Base Color
            if (material.HasProperty(
                "_BaseColor"))
            {
                Color color =
                    originalColors[i];

                color.a =
                    alpha;

                material.SetColor(
                    "_BaseColor",
                    color
                );
            }
        }
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void OnDestroy()
    {
        if (materials == null)
            return;


        for (int i = 0;
             i < materials.Length;
             i++)
        {
            if (materials[i] == null)
                continue;


            if (materials[i].HasProperty(
                "_Color"))
            {
                Color color =
                    originalColors[i];

                color.a = 1f;

                materials[i].color =
                    color;
            }


            if (materials[i].HasProperty(
                "_BaseColor"))
            {
                Color color =
                    originalColors[i];

                color.a = 1f;

                materials[i].SetColor(
                    "_BaseColor",
                    color
                );
            }
        }
    }
}