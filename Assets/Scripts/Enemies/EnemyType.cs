using UnityEngine;

public class EnemyType : MonoBehaviour
{
    public enum EnemyRank
    {
        Normal,
        Champion,
        Elite,
        Boss
    }

    [Header("Ranga przeciwnika")]
    [SerializeField] private EnemyRank rank = EnemyRank.Normal;

    public EnemyRank Rank => rank;


    // =========================================================
    // ODPORNOŚĆ NA EFEKTY KONTROLI
    //
    // 1.0 = 100% czasu efektu
    // 0.5 = 50% czasu efektu
    // 0.25 = 25% czasu efektu
    // 0.0 = całkowita odporność
    // =========================================================

    [Header("Odporność na efekty kontroli")]

    [Tooltip("1 = normalny czas Slow, 0.5 = połowa czasu, 0 = całkowita odporność.")]
    [SerializeField]
    private float slowEffectMultiplier = 1f;

    [Tooltip("1 = normalny czas Root, 0.5 = połowa czasu, 0 = całkowita odporność.")]
    [SerializeField]
    private float rootEffectMultiplier = 1f;

    [Tooltip("1 = normalny czas Frozen, 0.5 = połowa czasu, 0 = całkowita odporność.")]
    [SerializeField]
    private float frozenEffectMultiplier = 1f;

    [Tooltip("1 = normalny czas Stun, 0.5 = połowa czasu, 0 = całkowita odporność.")]
    [SerializeField]
    private float stunEffectMultiplier = 1f;


    // =========================================================
    // GETTERY
    // =========================================================

    public float SlowEffectMultiplier =>
        Mathf.Clamp01(slowEffectMultiplier);

    public float RootEffectMultiplier =>
        Mathf.Clamp01(rootEffectMultiplier);

    public float FrozenEffectMultiplier =>
        Mathf.Clamp01(frozenEffectMultiplier);

    public float StunEffectMultiplier =>
        Mathf.Clamp01(stunEffectMultiplier);
}