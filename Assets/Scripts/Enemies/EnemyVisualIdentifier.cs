using UnityEngine;

public class EnemyVisualIdentifier : MonoBehaviour
{
    [Header("Kolorystyka rang")]
    [SerializeField] private Color championColor = new Color(0.2f, 0.5f, 1f);
    [SerializeField] private Color eliteColor = new Color(1f, 0.75f, 0.1f);

    private Renderer[] renderers;
    private Color[] originalColors;

    void Start()
    {
        EnemyType enemyType = GetComponent<EnemyType>();

        if (enemyType == null)
            return;

        renderers = GetComponentsInChildren<Renderer>();

        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                originalColors[i] =
                    renderers[i].material.color;
            }
        }

        ApplyRankColor(enemyType.Rank);
    }

    private void ApplyRankColor(EnemyType.EnemyRank rank)
    {
        Color color;

        switch (rank)
        {
            case EnemyType.EnemyRank.Champion:
                color = championColor;
                break;

            case EnemyType.EnemyRank.Elite:
                color = eliteColor;
                break;

            default:
                return;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                renderers[i].material.color = color;
            }
        }
    }
}