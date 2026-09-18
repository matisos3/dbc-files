using UnityEngine;

public class ArrowCold : ProjectileBase
{
    public override void Hit(GameObject target)
    {
        if (target == null)
            return;

        EnemyHealth health = target.GetComponentInParent<EnemyHealth>();

        if (health == null)
            return;

        // Obrażenia tylko głównemu celowi
        health.TakeDamage(skillInstance.damage);

        // Spowolnienie tylko głównego celu
        EnemyMovement movement = health.GetComponent<EnemyMovement>();

        if (movement != null)
        {
            movement.ApplySlow(skillInstance.slowPercent, skillInstance.slowDuration);
        }
    }
}