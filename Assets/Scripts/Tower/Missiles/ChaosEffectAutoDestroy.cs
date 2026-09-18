using UnityEngine;

public class ChaosEffectAutoDestroy : MonoBehaviour
{
    private float timer;
    private ArrowChaos owner;

    public void Initialize(
        float duration,
        ArrowChaos chaosArrow)
    {
        timer = duration;
        owner = chaosArrow;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            if (owner != null)
            {
                owner.ClearChaosEffect();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}