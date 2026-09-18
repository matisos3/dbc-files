using UnityEngine;

public class ArrowChaos : ProjectileBase
{
    [Header("Chaos Visual Effect")]
    [SerializeField] private GameObject chaosEffectPrefab;

    private GameObject activeChaosEffect;
    private ChaosDamageOverTime activeDot;

    public override void Hit(GameObject enemy)
    {
        if (enemy == null)
            return;

        EnemyHealth enemyHealth =
            enemy.GetComponentInParent<EnemyHealth>();

        if (enemyHealth == null)
            return;

        // =====================================================
        // NATYCHMIASTOWE OBRAŻENIA
        // =====================================================

        enemyHealth.TakeDamage(
            skillInstance.damage
        );


        // =====================================================
        // CHAOS DOT
        // =====================================================

        ChaosDamageOverTime existingDot =
            enemyHealth.GetComponent<ChaosDamageOverTime>();


        if (existingDot != null)
        {
            // Odśwież istniejący DoT
            existingDot.Refresh(
                skillInstance.dotDamage,
                skillInstance.dotDuration,
                skillInstance.dotTickInterval
            );

            activeDot = existingDot;
        }
        else
        {
            // Utwórz nowy DoT
            ChaosDamageOverTime dot =
                enemyHealth.gameObject.AddComponent<
                    ChaosDamageOverTime
                >();

            dot.Initialize(
                enemyHealth,
                skillInstance.dotDamage,
                skillInstance.dotDuration,
                skillInstance.dotTickInterval
            );

            activeDot = dot;
        }


        // =====================================================
        // EFEKT WIZUALNY
        // =====================================================
        //
        // Tworzymy go tylko raz.
        // Kolejne trafienie nie tworzy drugiego efektu.
        //
        // =====================================================

        if (activeChaosEffect == null &&
            chaosEffectPrefab != null)
        {
            activeChaosEffect =
                Instantiate(
                    chaosEffectPrefab,
                    enemyHealth.transform
                );

            activeChaosEffect.transform.localPosition =
                Vector3.zero;

            activeChaosEffect.transform.localRotation =
                Quaternion.identity;

            activeChaosEffect.transform.localScale =
                Vector3.one;


            ChaosEffectAutoDestroy autoDestroy =
                activeChaosEffect.GetComponent<
                    ChaosEffectAutoDestroy
                >();

            if (autoDestroy == null)
            {
                autoDestroy =
                    activeChaosEffect.AddComponent<
                        ChaosEffectAutoDestroy
                    >();
            }

            autoDestroy.Initialize(
                skillInstance.dotDuration,
                this
            );
        }
    }


    // =========================================================
    // USUNIĘCIE AKTYWNEGO EFEKTU
    // =========================================================

    public void ClearChaosEffect()
    {
        if (activeChaosEffect != null)
        {
            Destroy(
                activeChaosEffect
            );

            activeChaosEffect = null;
        }

        activeDot = null;
    }
}