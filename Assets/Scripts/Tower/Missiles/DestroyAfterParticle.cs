using UnityEngine;

public class DestroyAfterParticle : MonoBehaviour
{
    private ParticleSystem[] particles;

    private void Start()
    {
        particles =
            GetComponentsInChildren<ParticleSystem>();

        if (particles.Length == 0)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (particles == null ||
            particles.Length == 0)
        {
            return;
        }

        foreach (ParticleSystem particle in particles)
        {
            if (particle != null &&
                particle.IsAlive(true))
            {
                return;
            }
        }

        Destroy(gameObject);
    }
}