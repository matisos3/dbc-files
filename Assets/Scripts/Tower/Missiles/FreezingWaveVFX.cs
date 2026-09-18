using System.Collections.Generic;
using UnityEngine;

public class FreezingWaveVFX : MonoBehaviour
{
    // =========================================================
    // WAVE
    // =========================================================

    [Header("Wave Shape")]

    [SerializeField]
    private float waveWidth = 4f;

    [SerializeField]
    private float waveHeight = 2.5f;

    [SerializeField]
    private float waveLength = 0.8f;

    [SerializeField]
    private int widthSegments = 18;

    [SerializeField]
    private int heightSegments = 8;


    // =========================================================
    // WAVE ANIMATION
    // =========================================================

    [Header("Wave Animation")]

    [SerializeField]
    private float waveMoveSpeed = 5f;

    [SerializeField]
    private float waveAmplitude = 0.25f;

    [SerializeField]
    private float waveFrequency = 2.5f;


    // =========================================================
    // MATERIAL
    // =========================================================

    [Header("Material")]

    [SerializeField]
    private Material waveMaterial;


    // =========================================================
    // CRYSTALS
    // =========================================================

    [Header("Ice Crystals")]

    [SerializeField]
    private int crystalCount = 18;

    [SerializeField]
    private float crystalSize = 0.18f;

    [SerializeField]
    private float crystalSpeed = 1.5f;


    // =========================================================
    // CRYSTAL MATERIAL
    // =========================================================

    [SerializeField]
    private Material crystalMaterial;


    // =========================================================
    // INTERNAL
    // =========================================================

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private Mesh waveMesh;

    private Vector3[] baseVertices;

    private readonly List<GameObject> crystals =
        new List<GameObject>();


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        CreateWave();

        CreateCrystals();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        AnimateWave();

        AnimateCrystals();
    }


    // =========================================================
    // CREATE WAVE
    // =========================================================

    private void CreateWave()
    {
        GameObject waveObject =
            new GameObject("IceWaveMesh");

        waveObject.transform.SetParent(
            transform,
            false
        );


        meshFilter =
            waveObject.AddComponent<MeshFilter>();


        meshRenderer =
            waveObject.AddComponent<MeshRenderer>();


        if (waveMaterial != null)
        {
            meshRenderer.material =
                waveMaterial;
        }


        waveMesh =
            new Mesh();

        waveMesh.name =
            "Procedural Ice Wave";


        meshFilter.mesh =
            waveMesh;


        // =====================================================
        // VERTICES
        // =====================================================

        int vertexCount =
            (widthSegments + 1) *
            (heightSegments + 1);


        baseVertices =
            new Vector3[vertexCount];


        Vector3[] vertices =
            new Vector3[vertexCount];


        for (int y = 0;
             y <= heightSegments;
             y++)
        {
            float vertical =
                (float)y /
                heightSegments;


            float localY =
                vertical *
                waveHeight;


            for (int x = 0;
                 x <= widthSegments;
                 x++)
            {
                float horizontal =
                    (float)x /
                    widthSegments;


                float localX =
                    (horizontal - 0.5f) *
                    waveWidth;


                float localZ =
                    0f;


                // =================================================
                // FALOWANIE
                // =================================================

                localZ =
                    Mathf.Sin(
                        horizontal *
                        Mathf.PI *
                        waveFrequency
                    ) *
                    waveAmplitude *
                    vertical;


                int index =
                    y *
                    (widthSegments + 1) +
                    x;


                Vector3 vertex =
                    new Vector3(
                        localX,
                        localY,
                        localZ
                    );


                vertices[index] =
                    vertex;


                baseVertices[index] =
                    vertex;
            }
        }


        // =====================================================
        // TRIANGLES
        // =====================================================

        int quadCount =
            widthSegments *
            heightSegments;


        int[] triangles =
            new int[
                quadCount *
                6
            ];


        int triangleIndex = 0;


        for (int y = 0;
             y < heightSegments;
             y++)
        {
            for (int x = 0;
                 x < widthSegments;
                 x++)
            {
                int current =
                    y *
                    (widthSegments + 1) +
                    x;


                int next =
                    current +
                    widthSegments +
                    1;


                triangles[triangleIndex++] =
                    current;

                triangles[triangleIndex++] =
                    next;

                triangles[triangleIndex++] =
                    current + 1;


                triangles[triangleIndex++] =
                    current + 1;

                triangles[triangleIndex++] =
                    next;

                triangles[triangleIndex++] =
                    next + 1;
            }
        }


        waveMesh.Clear();


        waveMesh.vertices =
            vertices;

        waveMesh.triangles =
            triangles;


        waveMesh.RecalculateNormals();

        waveMesh.RecalculateBounds();
    }


    // =========================================================
    // ANIMATE WAVE
    // =========================================================

    private void AnimateWave()
    {
        if (waveMesh == null)
            return;


        Vector3[] vertices =
            new Vector3[
                baseVertices.Length
            ];


        float time =
            Time.time *
            waveMoveSpeed;


        for (int i = 0;
             i < baseVertices.Length;
             i++)
        {
            Vector3 vertex =
                baseVertices[i];


            float normalizedY =
                waveHeight > 0f
                    ? vertex.y /
                      waveHeight
                    : 0f;


            vertex.z =
                baseVertices[i].z +
                Mathf.Sin(
                    time +
                    vertex.x *
                    waveFrequency
                ) *
                waveAmplitude *
                normalizedY;


            vertices[i] =
                vertex;
        }


        waveMesh.vertices =
            vertices;


        waveMesh.RecalculateNormals();
    }


    // =========================================================
    // CREATE CRYSTALS
    // =========================================================

    private void CreateCrystals()
    {
        for (int i = 0;
             i < crystalCount;
             i++)
        {
            float horizontal =
                Random.Range(
                    -0.5f,
                    0.5f
                );


            float vertical =
                Random.Range(
                    0.15f,
                    0.95f
                );


            float x =
                horizontal *
                waveWidth;


            float y =
                vertical *
                waveHeight;


            float z =
                Random.Range(
                    -waveLength,
                    waveLength
                );


            GameObject crystal =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );


            crystal.name =
                "IceCrystal";


            crystal.transform.SetParent(
                transform,
                false
            );


            crystal.transform.localPosition =
                new Vector3(
                    x,
                    y,
                    z
                );


            float size =
                Random.Range(
                    crystalSize * 0.5f,
                    crystalSize * 1.5f
                );


            crystal.transform.localScale =
                new Vector3(
                    size,
                    Random.Range(
                        crystalSize,
                        crystalSize * 2.5f
                    ),
                    size
                );


            crystal.transform.localRotation =
                Quaternion.Euler(
                    Random.Range(
                        -30f,
                        30f
                    ),
                    Random.Range(
                        0f,
                        360f
                    ),
                    Random.Range(
                        -30f,
                        30f
                    )
                );


            Collider collider =
                crystal.GetComponent<Collider>();


            if (collider != null)
            {
                Destroy(
                    collider
                );
            }


            Renderer renderer =
                crystal.GetComponent<Renderer>();


            if (renderer != null &&
                crystalMaterial != null)
            {
                renderer.material =
                    crystalMaterial;
            }


            crystals.Add(
                crystal
            );
        }
    }


    // =========================================================
    // ANIMATE CRYSTALS
    // =========================================================

    private void AnimateCrystals()
    {
        for (int i = 0;
             i < crystals.Count;
             i++)
        {
            GameObject crystal =
                crystals[i];


            if (crystal == null)
                continue;


            Vector3 position =
                crystal.transform.localPosition;


            position.z =
                Mathf.Sin(
                    Time.time *
                    crystalSpeed +
                    i
                ) *
                waveLength;


            crystal.transform.localPosition =
                position;


            crystal.transform.Rotate(
                0f,
                60f *
                Time.deltaTime,
                20f *
                Time.deltaTime
            );
        }
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void OnDestroy()
    {
        for (int i = 0;
             i < crystals.Count;
             i++)
        {
            if (crystals[i] != null)
            {
                Destroy(
                    crystals[i]
                );
            }
        }


        crystals.Clear();
    }
}