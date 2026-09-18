using UnityEngine;

public class ShotgunProjectile : ProjectileBase
{
    public override void Hit(GameObject target)
    {
        if (target == null)
            return;


        EnemyHealth enemyHealth =
            target.GetComponent<EnemyHealth>();


        if (enemyHealth == null)
            return;


        enemyHealth.TakeDamage(
            Mathf.RoundToInt(
                skillInstance.damage
            )
        );
    }
}