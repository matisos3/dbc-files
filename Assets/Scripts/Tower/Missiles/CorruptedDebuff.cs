using UnityEngine;

public class CorruptedDebuff : MonoBehaviour
{
    private EnemyMovement enemyMovement;
    private EnemyHealth enemyHealth;


    // =========================================================
    // AKTUALNA INSTANCJA SKILLA
    // =========================================================

    private SkillInstance skillInstance;


    // =========================================================
    // EFEKT WIZUALNY
    // =========================================================

    private CorruptedVisualEffect visualEffect;


    // =========================================================
    // STACKI
    // =========================================================

    private int stacks = 0;


    // =========================================================
    // CZAS
    // =========================================================

    private float remainingTime;


    // =========================================================
    // INITIALIZE
    // =========================================================

    public void Initialize(
        SkillInstance instance)
    {
        if (instance == null)
        {
            return;
        }


        if (instance.data == null)
        {
            return;
        }


        skillInstance =
            instance;


        enemyMovement =
            GetComponent<EnemyMovement>();


        enemyHealth =
            GetComponent<EnemyHealth>();


        // =====================================================
        // JEŚLI KOMPONENT JEST NA DZIECKU
        // =====================================================

        if (enemyMovement == null)
        {
            enemyMovement =
                GetComponentInParent<EnemyMovement>();
        }


        if (enemyHealth == null)
        {
            enemyHealth =
                GetComponentInParent<EnemyHealth>();
        }


        stacks = 1;


        remainingTime =
            skillInstance.corruptedDuration;


        ApplyCurrentStacks();


        // =====================================================
        // EFEKT WIZUALNY
        // =====================================================

        CreateOrRefreshVisualEffect();
    }


    // =========================================================
    // REFRESH / ADD STACK
    // =========================================================

    public void Refresh(
        SkillInstance instance)
    {
        if (instance == null)
            return;


        if (instance.data == null)
            return;


        skillInstance =
            instance;


        if (enemyMovement == null)
        {
            enemyMovement =
                GetComponent<EnemyMovement>();


            if (enemyMovement == null)
            {
                enemyMovement =
                    GetComponentInParent<EnemyMovement>();
            }
        }


        if (enemyHealth == null)
        {
            enemyHealth =
                GetComponent<EnemyHealth>();


            if (enemyHealth == null)
            {
                enemyHealth =
                    GetComponentInParent<EnemyHealth>();
            }
        }


        // =====================================================
        // STACK
        // =====================================================

        if (stacks <
            skillInstance.corruptedMaxStacks)
        {
            stacks++;
        }


        // =====================================================
        // ODNOWIENIE CZASU
        // =====================================================

        remainingTime =
            skillInstance.corruptedDuration;


        // =====================================================
        // AKTUALIZUJEMY PARAMETRY
        // =====================================================

        ApplyCurrentStacks();


        // =====================================================
        // ODNOWIENIE EFEKTU
        // =====================================================

        CreateOrRefreshVisualEffect();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // =====================================================
        // PRZECIWNIK UMIERA
        //
        // WAŻNE:
        // Usuwamy tylko debuff.
        // NIE WOLNO zniszczyć EnemyHealth/GameObject przeciwnika.
        // =====================================================

        if (enemyHealth != null &&
            enemyHealth.IsDying)
        {
            RemoveDebuff();

            return;
        }


        // =====================================================
        // CZAS
        // =====================================================

        remainingTime -=
            Time.deltaTime;


        if (remainingTime <= 0f)
        {
            RemoveDebuff();
        }
    }


    // =========================================================
    // APPLY STACKS
    // =========================================================

    private void ApplyCurrentStacks()
    {
        if (skillInstance == null)
            return;


        // =====================================================
        // ENEMY MOVEMENT
        // =====================================================

        if (enemyMovement != null)
        {
            float movementMultiplier =
                Mathf.Max(
                    0f,
                    1f -
                    skillInstance.corruptedSlowPercent *
                    stacks
                );


            float attackSpeedMultiplier =
                Mathf.Max(
                    0f,
                    1f -
                    skillInstance.corruptedAttackSpeedReduction *
                    stacks
                );


            enemyMovement.SetCorruptedMoveSpeedMultiplier(
                movementMultiplier
            );


            enemyMovement.SetCorruptedAttackSpeedMultiplier(
                attackSpeedMultiplier
            );
        }


        // =====================================================
        // ENEMY HEALTH
        // =====================================================

        if (enemyHealth != null)
        {
            float damageTakenMultiplier =
                1f +
                skillInstance.corruptedDamageTakenIncrease *
                stacks;


            enemyHealth.SetCorruptedDamageTakenMultiplier(
                damageTakenMultiplier
            );
        }
    }


    // =========================================================
    // EFEKT WIZUALNY
    // =========================================================

    private void CreateOrRefreshVisualEffect()
    {
        if (skillInstance == null)
            return;


        if (skillInstance.hitEffectPrefab == null)
        {
            return;
        }


        // =====================================================
        // SPRAWDZAMY CZY EFEKT JUŻ ISTNIEJE
        // =====================================================

        if (visualEffect == null)
        {
            visualEffect =
                GetComponentInChildren<
                    CorruptedVisualEffect
                >();
        }


        // =====================================================
        // JEŚLI ISTNIEJE → ODNOWIENIE
        // =====================================================

        if (visualEffect != null)
        {
            visualEffect.Refresh(
                skillInstance.corruptedDuration
            );

            return;
        }


        // =====================================================
        // TWORZYMY NOWY EFEKT
        // =====================================================

        GameObject effectObject =
            Instantiate(
                skillInstance.hitEffectPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );


        visualEffect =
            effectObject.GetComponent<
                CorruptedVisualEffect
            >();


        // =====================================================
        // AUTOMATYCZNIE DODAJEMY KOMPONENT
        // =====================================================

        if (visualEffect == null)
        {
            visualEffect =
                effectObject.AddComponent<
                    CorruptedVisualEffect
                >();
        }


        visualEffect.Initialize(
            skillInstance.corruptedDuration
        );
    }


    // =========================================================
    // REMOVE
    // =========================================================

    private void RemoveDebuff()
    {
        // =====================================================
        // RESET MOVEMENT
        // =====================================================

        if (enemyMovement != null)
        {
            enemyMovement.SetCorruptedMoveSpeedMultiplier(
                1f
            );


            enemyMovement.SetCorruptedAttackSpeedMultiplier(
                1f
            );
        }


        // =====================================================
        // RESET DAMAGE TAKEN
        // =====================================================

        if (enemyHealth != null)
        {
            enemyHealth.SetCorruptedDamageTakenMultiplier(
                1f
            );
        }


        // =====================================================
        // USUWAMY EFEKT WIZUALNY
        // =====================================================

        if (visualEffect != null)
        {
            Destroy(
                visualEffect.gameObject
            );

            visualEffect = null;
        }
        else
        {
            // =================================================
            // AWARYJNIE SZUKAMY EFEKTU
            // =================================================

            CorruptedVisualEffect effect =
                GetComponentInChildren<
                    CorruptedVisualEffect
                >();

            if (effect != null)
            {
                Destroy(
                    effect.gameObject
                );
            }
        }


        // =====================================================
        // NAJWAŻNIEJSZA POPRAWKA
        // =====================================================
        //
        // Jeśli CorruptedDebuff jest na tym samym GameObject
        // co EnemyHealth, NIE MOŻEMY zrobić:
        //
        // Destroy(gameObject);
        //
        // ponieważ zniszczylibyśmy całego przeciwnika.
        //
        // W takim przypadku usuwamy tylko komponent.
        // =====================================================

        if (enemyHealth != null &&
            enemyHealth.gameObject == gameObject)
        {
            Destroy(this);

            return;
        }


        // =====================================================
        // DEBUFF JEST NA OSOBNYM GAMEOBJECT
        // =====================================================

        Destroy(
            gameObject
        );
    }


    // =========================================================
    // STACKS
    // =========================================================

    public int GetStacks()
    {
        return stacks;
    }


    // =========================================================
    // CZAS
    // =========================================================

    public float GetRemainingTime()
    {
        return remainingTime;
    }


    // =========================================================
    // SKILL INSTANCE
    // =========================================================

    public SkillInstance GetSkillInstance()
    {
        return skillInstance;
    }
}