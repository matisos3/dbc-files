using UnityEngine;

public class CorruptedVisualEffect : MonoBehaviour
{
    private float remainingTime;


    // =========================================================
    // INITIALIZE
    // =========================================================

    public void Initialize(
        float duration)
    {
        remainingTime =
            Mathf.Max(
                0.01f,
                duration
            );
    }


    // =========================================================
    // REFRESH
    // =========================================================

    public void Refresh(
        float duration)
    {
        remainingTime =
            Mathf.Max(
                0.01f,
                duration
            );
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        remainingTime -=
            Time.deltaTime;


        if (remainingTime <= 0f)
        {
            Destroy(gameObject);
        }
    }
}