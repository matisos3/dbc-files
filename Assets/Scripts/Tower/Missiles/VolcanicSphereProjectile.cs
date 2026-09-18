using UnityEngine;

public class VolcanicSphereProjectile : ProjectileBase
{
    public override void Hit(GameObject target)
    {
        if (target == null)
            return;

        if (skillInstance == null)
            return;

        EnemyHealth enemyHealth =
            target.GetComponent<EnemyHealth>();

        if (enemyHealth == null)
        {
            enemyHealth =
                target.GetComponentInParent<EnemyHealth>();
        }

        if (enemyHealth == null)
            return;

        if (enemyHealth.IsDying)
            return;

        enemyHealth.TakeDamage(
            skillInstance.volcanicSphereDamage
        );
    }
}