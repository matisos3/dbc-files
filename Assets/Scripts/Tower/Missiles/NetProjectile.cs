using UnityEngine;

public class NetProjectile : ProjectileBase
{
    public override void Hit(GameObject target)
    {
        if (target == null)
            return;

        EnemyMovement enemyMovement =
            target.GetComponent<EnemyMovement>();

        if (enemyMovement != null)
        {
            enemyMovement.ApplyRoot(
                skillInstance.rootDuration
            );
        }
    }
}