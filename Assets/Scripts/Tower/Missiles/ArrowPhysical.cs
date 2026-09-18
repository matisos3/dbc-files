using System.Collections.Generic;
using UnityEngine;

public class ArrowPhysical : ProjectileBase
{
    public override void Hit(GameObject target)
    {
        if (skillInstance.hitEffectPrefab != null)
        {
        Instantiate(
            skillInstance.hitEffectPrefab,
            target.transform.position,
            Quaternion.identity
            );
        }
        if (target == null)
            return;

        // Główny cel
        EnemyHealth mainTarget = target.GetComponent<EnemyHealth>();

        if (mainTarget != null)
        {
            mainTarget.TakeDamage(skillInstance.damage);
        }
    }
}