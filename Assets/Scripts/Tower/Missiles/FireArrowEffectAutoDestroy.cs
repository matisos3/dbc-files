using UnityEngine;

public class FireArrowEffectAutoDestroy : MonoBehaviour
{
    private float timer;

    public void Initialize(
        float duration,
        float explosionRadius)
    {
        timer = duration;

        // =====================================================
        // SKALOWANIE EFEKTU
        // =====================================================

        float diameter =
            explosionRadius * 2f;

        transform.localScale =
            new Vector3(
                diameter,
                transform.localScale.y,
                diameter
            );
    }


    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}