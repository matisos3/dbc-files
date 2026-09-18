using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    // =========================================================
    // RUCH
    // =========================================================

    [Header("Ruch")]

    public float speed = 3f;

    public float attackRange = 1.5f;


    // =========================================================
    // ATAK
    // =========================================================

    [Header("Atak")]

    public float attackInterval = 1f;

    public float attackSpeed = 1f;

    public int damage = 10;


    // =========================================================
    // ANIMATOR
    // =========================================================

    [Header("Nazwa boola w Animatorze")]

    public string attackBoolName = "IsAttacking";


    // =========================================================
    // SLOW - ZWYKŁY
    // =========================================================

    [Header("Efekt spowolnienia")]

    [SerializeField]
    private Color slowColor =
        new Color(0.65f, 0.85f, 1f);

    [SerializeField]
    private GameObject coldEffectPrefab;

    [SerializeField]
    private GameObject netEffectPrefab;


    private GameObject activeNetEffect;

    private GameObject activeColdEffect;


    // =========================================================
    // FROZEN
    // =========================================================

    [Header("Frozen")]

    [SerializeField]
    private GameObject frozenEffectPrefab;

    private GameObject activeFrozenEffect;

    private Coroutine frozenCoroutine;

    private bool isFrozen = false;


    // =========================================================
    // STUN
    // =========================================================

    [Header("Stun")]

    private Coroutine stunCoroutine;

    private bool isStunned = false;

    private GameObject activeStunEffect;


    // =========================================================
    // KNOCKBACK
    // =========================================================

    [Header("Knockback")]

    private Coroutine knockbackCoroutine;

    private bool isKnockedBack = false;


    // =========================================================
    // PODSTAWOWE KOMPONENTY
    // =========================================================

    private Animator animator;

    private Transform target;

    private TowerHealth targetTowerHealth;

    private EnemyHealth enemyHealth;


    // =========================================================
    // STAN ATAKU
    // =========================================================

    private bool isAttacking = false;

    private float attackTimer;


    // =========================================================
    // WHIRL OF CHAOS
    // =========================================================

    private int whirlOfChaosCount = 0;

    private bool IsUnderWhirlOfChaos
    {
        get
        {
            return whirlOfChaosCount > 0;
        }
    }


    // =========================================================
    // BAZOWE WARTOŚCI
    // =========================================================

    private float originalSpeed;

    private float originalAttackSpeed;


    // =========================================================
    // AFFIX - ATTACK SPEED
    // =========================================================

    private float affixAttackSpeedMultiplier = 1f;


    // =========================================================
    // AFFIX - MOVE SPEED
    // =========================================================

    private float extraMoveSpeedMultiplier = 1f;


    // =========================================================
    // ENRAGE
    // =========================================================

    private float enrageMoveSpeedMultiplier = 1f;

    private float enrageAttackSpeedMultiplier = 1f;

    private float enrageDamageMultiplier = 1f;


    // =========================================================
    // EXTRA DAMAGE
    // =========================================================

    private float extraDamageMultiplier = 1f;


    // =========================================================
    // ZWYKŁY SLOW
    // =========================================================

    private float slowMultiplier = 1f;

    private Coroutine slowCoroutine;


    // =========================================================
    // ROOT
    // =========================================================

    private Coroutine rootCoroutine;

    private bool isRooted = false;


    // =========================================================
    // RENDERERY
    // =========================================================

    private Renderer[] renderers;

    private Color[] originalColors;


    // =========================================================
    // DAMAGE AURA
    // =========================================================

    private Dictionary<EnemyDamageAura, float> damageAuras =
        new Dictionary<EnemyDamageAura, float>();


    // =========================================================
    // CORRUPTED
    // =========================================================

    private float corruptedMoveSpeedMultiplier = 1f;

    private float corruptedAttackSpeedMultiplier = 1f;


    // =========================================================
    // SLIME SLOW
    // =========================================================

    private float slimeSlowMultiplier = 1f;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        animator =
            GetComponent<Animator>();


        originalSpeed =
            speed;


        originalAttackSpeed =
            attackSpeed;


        EnemyAffixes affixes =
            GetComponent<EnemyAffixes>();


        if (affixes != null &&
            affixes.HasAffix(
                EnemyAffixes.AffixType.ExtraAttackSpeed))
        {
            affixAttackSpeedMultiplier =
                1.3f;
        }


        renderers =
            GetComponentsInChildren<Renderer>();


        originalColors =
            new Color[
                renderers.Length
            ];


        for (int i = 0;
             i < renderers.Length;
             i++)
        {
            if (renderers[i] != null &&
                renderers[i].material.HasProperty("_Color"))
            {
                originalColors[i] =
                    renderers[i].material.color;
            }
        }


        UpdateMovementSpeed();
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        enemyHealth =
            GetComponent<EnemyHealth>();


        ResetAttackTimer();

        UpdateAnimatorAttackSpeed();


        GameObject tower =
            GameObject.FindWithTag("Tower");


        if (tower != null)
        {
            target =
                tower.transform;


            targetTowerHealth =
                tower.GetComponent<TowerHealth>();
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (enemyHealth != null &&
            enemyHealth.IsDying)
        {
            return;
        }


        if (target == null)
            return;


        // =====================================================
        // WHIRL OF CHAOS
        // =====================================================

        if (IsUnderWhirlOfChaos)
        {
            KeepWhirlWalkAnimation();

            return;
        }


        // =====================================================
        // KNOCKBACK
        // =====================================================

        if (isKnockedBack)
        {
            return;
        }


        // =====================================================
        // FROZEN
        // =====================================================

        if (isFrozen)
        {
            return;
        }


        // =====================================================
        // STUN
        // =====================================================

        if (isStunned)
        {
            return;
        }


        // =====================================================
        // ROOT
        // =====================================================

        if (isRooted)
        {
            return;
        }


        Vector3 direction =
            target.position -
            transform.position;


        direction.y = 0f;


        float distance =
            direction.magnitude;


        // =====================================================
        // ZA DALEKO → RUCH
        // =====================================================

        if (distance > attackRange)
        {
            if (isAttacking)
            {
                isAttacking = false;


                if (animator != null)
                {
                    animator.SetBool(
                        attackBoolName,
                        false
                    );


                    animator.Play(
                        "Walk",
                        0,
                        0f
                    );


                    animator.Update(0f);
                }
            }


            MoveToTower();

            return;
        }


        // =====================================================
        // W ZASIĘGU → ATAK
        // =====================================================

        if (!isAttacking)
        {
            isAttacking = true;


            ResetAttackTimer();


            if (animator != null)
            {
                animator.SetBool(
                    attackBoolName,
                    true
                );


                UpdateAnimatorAttackSpeed();
            }
        }


        AttackTower();
    }


    // =========================================================
    // WHIRL OF CHAOS - START
    // =========================================================

    public void BeginWhirlOfChaos()
    {
        if (enemyHealth != null &&
            enemyHealth.IsDying)
        {
            return;
        }


        whirlOfChaosCount++;


        isAttacking = false;

        attackTimer = 0f;


        if (animator != null)
        {
            animator.SetBool(
                attackBoolName,
                false
            );


            AnimatorStateInfo stateInfo =
                animator.GetCurrentAnimatorStateInfo(0);


            if (!stateInfo.IsName("Walk"))
            {
                animator.Play(
                    "Walk",
                    0,
                    0f
                );


                animator.Update(0f);
            }


            UpdateAnimatorAttackSpeed();
        }
    }


    // =========================================================
    // WHIRL OF CHAOS - END
    // =========================================================

    public void EndWhirlOfChaos()
    {
        if (whirlOfChaosCount <= 0)
        {
            whirlOfChaosCount = 0;
            return;
        }


        whirlOfChaosCount--;


        if (whirlOfChaosCount > 0)
        {
            return;
        }


        if (enemyHealth != null &&
            enemyHealth.IsDying)
        {
            return;
        }


        if (animator != null)
        {
            animator.SetBool(
                attackBoolName,
                false
            );


            UpdateAnimatorAttackSpeed();
        }


        if (target == null)
            return;


        CheckAttackDistance();
    }


    // =========================================================
    // WHIRL OF CHAOS - WALK ANIMATION
    // =========================================================

    private void KeepWhirlWalkAnimation()
    {
        if (animator == null)
            return;


        animator.SetBool(
            attackBoolName,
            false
        );


        AnimatorStateInfo stateInfo =
            animator.GetCurrentAnimatorStateInfo(0);


        if (!stateInfo.IsName("Walk"))
        {
            animator.Play(
                "Walk",
                0,
                0f
            );


            animator.Update(0f);
        }


        UpdateAnimatorAttackSpeed();
    }


    // =========================================================
    // RUCH DO WIEŻY
    // =========================================================

    private void MoveToTower()
    {
        if (target == null)
            return;


        if (IsUnderWhirlOfChaos)
        {
            return;
        }


        if (isKnockedBack ||
            isStunned ||
            isFrozen ||
            isRooted)
        {
            return;
        }


        Vector3 direction =
            target.position -
            transform.position;


        direction.y = 0f;


        float distance =
            direction.magnitude;


        if (distance > attackRange)
        {
            isAttacking = false;


            if (animator != null)
            {
                animator.SetBool(
                    attackBoolName,
                    false
                );
            }


            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    target.position,
                    GetFinalMoveSpeed() *
                    Time.deltaTime
                );


            if (direction != Vector3.zero)
            {
                transform.rotation =
                    Quaternion.LookRotation(
                        direction
                    );
            }
        }
        else
        {
            if (!isAttacking)
            {
                isAttacking = true;


                ResetAttackTimer();


                if (animator != null)
                {
                    animator.SetBool(
                        attackBoolName,
                        true
                    );


                    UpdateAnimatorAttackSpeed();
                }
            }
        }
    }


    // =========================================================
    // CHECK ATTACK DISTANCE
    // =========================================================

    public void CheckAttackDistance()
    {
        if (enemyHealth != null &&
            enemyHealth.IsDying)
        {
            return;
        }


        if (target == null)
            return;


        if (IsUnderWhirlOfChaos)
        {
            KeepWhirlWalkAnimation();
            return;
        }


        if (isKnockedBack)
            return;


        if (isFrozen)
            return;


        if (isStunned)
            return;


        if (isRooted)
            return;


        Vector3 direction =
            target.position -
            transform.position;


        direction.y = 0f;


        float distance =
            direction.magnitude;


        // =====================================================
        // ZA DALEKO
        // =====================================================

        if (distance > attackRange)
        {
            isAttacking = false;


            if (animator != null)
            {
                animator.SetBool(
                    attackBoolName,
                    false
                );


                animator.Play(
                    "Walk",
                    0,
                    0f
                );


                animator.Update(0f);
            }


            return;
        }


        // =====================================================
        // W ZASIĘGU
        // =====================================================

        if (!isAttacking)
        {
            isAttacking = true;


            ResetAttackTimer();


            if (animator != null)
            {
                animator.SetBool(
                    attackBoolName,
                    true
                );


                UpdateAnimatorAttackSpeed();
            }
        }
    }


    // =========================================================
    // RESUME AFTER REINCARNATION
    // =========================================================

    public void ResumeAfterReincarnation()
    {
        if (enemyHealth != null &&
            enemyHealth.IsDying)
        {
            return;
        }


        if (isFrozen)
            return;


        if (isStunned)
            return;


        if (isKnockedBack)
            return;


        if (IsUnderWhirlOfChaos)
        {
            isAttacking = false;

            attackTimer = 0f;

            KeepWhirlWalkAnimation();

            return;
        }


        isAttacking = false;


        ResetAttackTimer();


        if (animator != null)
        {
            animator.ResetTrigger(
                "Death"
            );


            animator.SetBool(
                attackBoolName,
                false
            );


            animator.Play(
                "Walk",
                0,
                0f
            );


            animator.Update(0f);


            animator.speed = 1f;
        }


        if (target == null)
            return;


        Vector3 direction =
            target.position -
            transform.position;


        direction.y = 0f;


        float distance =
            direction.magnitude;


        if (distance <= attackRange)
        {
            isAttacking = true;


            ResetAttackTimer();


            if (animator != null)
            {
                animator.SetBool(
                    attackBoolName,
                    true
                );


                UpdateAnimatorAttackSpeed();
            }
        }
    }


    // =========================================================
    // ATAK
    // =========================================================

    private void AttackTower()
    {
        if (enemyHealth != null &&
            enemyHealth.IsDying)
        {
            return;
        }


        if (IsUnderWhirlOfChaos)
        {
            return;
        }


        if (isKnockedBack)
            return;


        if (isFrozen)
            return;


        if (isStunned)
            return;


        attackTimer -=
            Time.deltaTime;


        if (attackTimer <= 0f)
        {
            float finalAttackSpeed =
                GetFinalAttackSpeed();


            if (finalAttackSpeed <= 0f)
            {
                finalAttackSpeed = 0.01f;
            }


            attackTimer =
                attackInterval /
                finalAttackSpeed;


            if (animator != null)
            {
                animator.SetBool(
                    attackBoolName,
                    true
                );
            }
        }
    }


    // =========================================================
    // DEAL DAMAGE
    // =========================================================

    public void DealDamage()
    {
        if (enemyHealth != null &&
            enemyHealth.IsDying)
        {
            return;
        }


        if (IsUnderWhirlOfChaos)
            return;


        if (isKnockedBack)
            return;


        if (isFrozen)
            return;


        if (isStunned)
            return;


        if (targetTowerHealth == null)
            return;


        float auraMultiplier =
            GetDamageAuraMultiplier();


        int finalDamage =
            Mathf.RoundToInt(
                damage *
                extraDamageMultiplier *
                enrageDamageMultiplier *
                auraMultiplier
            );


        targetTowerHealth.TakeDamage(
            finalDamage
        );


        Tower tower =
            targetTowerHealth.GetComponent<Tower>();


        if (tower != null)
        {
            ThornsSkill thorns =
                tower.GetComponent<ThornsSkill>();


            if (thorns != null)
            {
                thorns.DealReflectDamage(
                    this
                );
            }
        }
    }


    // =========================================================
    // WAVE DAMAGE SCALING
    // =========================================================

    public void ApplyWaveDamageMultiplier(
        float multiplier)
    {
        if (multiplier <= 0f)
            return;


        damage =
            Mathf.RoundToInt(
                damage *
                multiplier
            );
    }


    // =========================================================
    // ENERGY ORB - KNOCKBACK
    // =========================================================

    public void ApplyKnockback(
        Vector3 direction,
        float distance)
    {
        if (enemyHealth != null &&
            enemyHealth.IsDying)
        {
            return;
        }


        if (distance <= 0f)
            return;


        direction.y = 0f;


        if (direction.sqrMagnitude <=
            0.0001f)
        {
            return;
        }


        direction.Normalize();


        if (knockbackCoroutine != null)
        {
            StopCoroutine(
                knockbackCoroutine
            );


            knockbackCoroutine = null;
        }


        knockbackCoroutine =
            StartCoroutine(
                KnockbackCoroutine(
                    direction,
                    distance
                )
            );
    }


    // =========================================================
    // ENERGY ORB - KNOCKBACK COROUTINE
    // =========================================================

    private IEnumerator KnockbackCoroutine(
        Vector3 direction,
        float distance)
    {
        isKnockedBack = true;


        isAttacking = false;

        attackTimer = 0f;


        if (animator != null)
        {
            animator.SetBool(
                attackBoolName,
                false
            );
        }


        const float knockbackDuration =
            0.2f;


        float elapsed =
            0f;


        Vector3 startPosition =
            transform.position;


        Vector3 targetPosition =
            startPosition +
            direction *
            distance;


        while (elapsed < knockbackDuration)
        {
            if (enemyHealth != null &&
                enemyHealth.IsDying)
            {
                break;
            }


            elapsed +=
                Time.deltaTime;


            float normalizedTime =
                Mathf.Clamp01(
                    elapsed /
                    knockbackDuration
                );


            float easedTime =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    normalizedTime
                );


            Vector3 newPosition =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    easedTime
                );


            newPosition.y =
                startPosition.y;


            transform.position =
                newPosition;


            yield return null;
        }


        if (enemyHealth == null ||
            !enemyHealth.IsDying)
        {
            targetPosition.y =
                startPosition.y;


            transform.position =
                targetPosition;
        }


        isKnockedBack = false;


        knockbackCoroutine = null;


        if (enemyHealth != null &&
            enemyHealth.IsDying)
        {
            yield break;
        }


        if (target == null)
            yield break;


        if (isFrozen ||
            isStunned ||
            isRooted)
        {
            yield break;
        }


        if (IsUnderWhirlOfChaos)
        {
            KeepWhirlWalkAnimation();
            yield break;
        }


        CheckAttackDistance();
    }


    // =========================================================
    // EXTRA DAMAGE
    // =========================================================

    public void SetExtraDamageMultiplier(
        float multiplier)
    {
        extraDamageMultiplier =
            Mathf.Max(
                0f,
                multiplier
            );
    }


    // =========================================================
    // ENRAGE DAMAGE
    // =========================================================

    public void SetDamageMultiplier(
        float multiplier)
    {
        enrageDamageMultiplier =
            Mathf.Max(
                0f,
                multiplier
            );
    }


    // =========================================================
    // EXTRA ATTACK SPEED
    // =========================================================

    public void SetExtraAttackSpeedMultiplier(
        float multiplier)
    {
        affixAttackSpeedMultiplier =
            Mathf.Max(
                0f,
                multiplier
            );


        UpdateAttackSpeed();
    }


    // =========================================================
    // ENRAGE ATTACK SPEED
    // =========================================================

    public void SetAttackSpeedMultiplier(
        float multiplier)
    {
        enrageAttackSpeedMultiplier =
            Mathf.Max(
                0f,
                multiplier
            );


        UpdateAttackSpeed();
    }


    // =========================================================
    // EXTRA MOVE SPEED
    // =========================================================

    public void SetExtraMoveSpeedMultiplier(
        float multiplier)
    {
        extraMoveSpeedMultiplier =
            Mathf.Max(
                0f,
                multiplier
            );


        UpdateMovementSpeed();
    }


    // =========================================================
    // ENRAGE MOVE SPEED
    // =========================================================

    public void SetMoveSpeedMultiplier(
        float multiplier)
    {
        enrageMoveSpeedMultiplier =
            Mathf.Max(
                0f,
                multiplier
            );


        UpdateMovementSpeed();
    }


    // =========================================================
    // FINAL MOVE SPEED
    // =========================================================

    private float GetFinalMoveSpeed()
    {
        return
            originalSpeed *
            extraMoveSpeedMultiplier *
            enrageMoveSpeedMultiplier *
            slowMultiplier *
            slimeSlowMultiplier *
            corruptedMoveSpeedMultiplier;
    }


    // =========================================================
    // UPDATE MOVE SPEED
    // =========================================================

    private void UpdateMovementSpeed()
    {
        speed =
            GetFinalMoveSpeed();
    }


    // =========================================================
    // FINAL ATTACK SPEED
    // =========================================================

    private float GetFinalAttackSpeed()
    {
        return
            originalAttackSpeed *
            affixAttackSpeedMultiplier *
            enrageAttackSpeedMultiplier *
            corruptedAttackSpeedMultiplier;
    }


    // =========================================================
    // UPDATE ATTACK SPEED
    // =========================================================

    private void UpdateAttackSpeed()
    {
        if (isFrozen)
            return;


        if (isStunned)
            return;


        if (isKnockedBack)
            return;


        UpdateAnimatorAttackSpeed();


        if (isAttacking &&
            !IsUnderWhirlOfChaos)
        {
            ResetAttackTimer();
        }
    }


    // =========================================================
    // ANIMATOR ATTACK SPEED
    // =========================================================

    private void UpdateAnimatorAttackSpeed()
    {
        if (animator == null)
            return;


        if (isFrozen ||
            isStunned ||
            isKnockedBack)
        {
            animator.SetFloat(
                "AttackSpeed",
                0f
            );


            return;
        }


        animator.SetFloat(
            "AttackSpeed",
            GetFinalAttackSpeed()
        );
    }


    // =========================================================
    // RESET ATTACK TIMER
    // =========================================================

    private void ResetAttackTimer()
    {
        float finalAttackSpeed =
            GetFinalAttackSpeed();


        if (finalAttackSpeed <= 0f)
        {
            finalAttackSpeed = 0.01f;
        }


        attackTimer =
            attackInterval /
            finalAttackSpeed;
    }


    // =========================================================
    // ROOT
    // =========================================================

    public void ApplyRoot(
        float duration)
    {
        if (enemyHealth != null &&
            enemyHealth.IsDying)
        {
            return;
        }


        if (duration <= 0f)
            return;


        // =====================================================
        // ODPORNOŚĆ NA ROOT
        // =====================================================

        EnemyType enemyType =
            GetComponent<EnemyType>();


        if (enemyType != null)
        {
            duration *=
                enemyType.RootEffectMultiplier;
        }


        // 0 = pełna odporność
        if (duration <= 0f)
            return;


        if (rootCoroutine != null)
        {
            StopCoroutine(
                rootCoroutine
            );
        }


        rootCoroutine =
            StartCoroutine(
                RootCoroutine(
                    duration
                )
            );
    }


    // =========================================================
    // ROOT COROUTINE
    // =========================================================

    private IEnumerator RootCoroutine(
        float duration)
    {
        isRooted = true;


        isAttacking = false;


        ResetAttackTimer();


        if (activeNetEffect == null &&
            netEffectPrefab != null)
        {
            activeNetEffect =
                Instantiate(
                    netEffectPrefab,
                    transform
                );


            activeNetEffect.transform.localPosition =
                Vector3.zero;


            activeNetEffect.transform.localRotation =
                Quaternion.identity;
        }


        if (animator != null)
        {
            animator.SetBool(
                attackBoolName,
                false
            );
        }


        yield return new WaitForSeconds(
            duration
        );


        if (activeNetEffect != null)
        {
            Destroy(
                activeNetEffect
            );


            activeNetEffect = null;
        }


        isRooted = false;


        rootCoroutine = null;


        if (target == null)
            yield break;


        if (isStunned ||
            isFrozen ||
            isKnockedBack)
        {
            yield break;
        }


        if (IsUnderWhirlOfChaos)
        {
            KeepWhirlWalkAnimation();
            yield break;
        }


        Vector3 direction =
            target.position -
            transform.position;


        direction.y = 0f;


        float distance =
            direction.magnitude;


        if (distance <= attackRange)
        {
            isAttacking = true;


            ResetAttackTimer();


            if (animator != null)
            {
                animator.SetBool(
                    attackBoolName,
                    true
                );


                UpdateAnimatorAttackSpeed();
            }
        }
    }


    // =========================================================
    // FROZEN
    // =========================================================

    public void ApplyFrozen(
        float duration)
    {
        if (enemyHealth != null &&
            enemyHealth.IsDying)
        {
            return;
        }


        if (duration <= 0f)
            return;


        // =====================================================
        // ODPORNOŚĆ NA FROZEN
        // =====================================================

        EnemyType enemyType =
            GetComponent<EnemyType>();


        if (enemyType != null)
        {
            duration *=
                enemyType.FrozenEffectMultiplier;
        }


        // 0 = pełna odporność
        if (duration <= 0f)
            return;


        if (frozenCoroutine != null)
        {
            StopCoroutine(
                frozenCoroutine
            );
        }


        frozenCoroutine =
            StartCoroutine(
                FrozenCoroutine(
                    duration
                )
            );
    }


    // =========================================================
    // FROZEN COROUTINE
    // =========================================================

    private IEnumerator FrozenCoroutine(
        float duration)
    {
        isFrozen = true;


        isAttacking = false;


        ResetAttackTimer();


        if (animator != null)
        {
            animator.SetBool(
                attackBoolName,
                false
            );


            animator.SetFloat(
                "AttackSpeed",
                0f
            );
        }


        if (activeFrozenEffect == null &&
            frozenEffectPrefab != null)
        {
            activeFrozenEffect =
                Instantiate(
                    frozenEffectPrefab,
                    transform
                );


            activeFrozenEffect.transform.localPosition =
                Vector3.zero;


            activeFrozenEffect.transform.localRotation =
                Quaternion.identity;
        }


        yield return new WaitForSeconds(
            duration
        );


        if (activeFrozenEffect != null)
        {
            Destroy(
                activeFrozenEffect
            );


            activeFrozenEffect = null;
        }


        isFrozen = false;


        frozenCoroutine = null;


        UpdateAnimatorAttackSpeed();


        if (enemyHealth != null &&
            enemyHealth.IsDying)
        {
            yield break;
        }


        if (target == null)
            yield break;


        if (isStunned)
            yield break;


        if (isRooted)
            yield break;


        if (isKnockedBack)
            yield break;


        if (IsUnderWhirlOfChaos)
        {
            KeepWhirlWalkAnimation();
            yield break;
        }


        Vector3 direction =
            target.position -
            transform.position;


        direction.y = 0f;


        float distance =
            direction.magnitude;


        if (distance <= attackRange)
        {
            isAttacking = true;


            ResetAttackTimer();


            if (animator != null)
            {
                animator.SetBool(
                    attackBoolName,
                    true
                );


                UpdateAnimatorAttackSpeed();
            }
        }
        else
        {
            isAttacking = false;


            if (animator != null)
            {
                animator.SetBool(
                    attackBoolName,
                    false
                );


                animator.Play(
                    "Walk",
                    0,
                    0f
                );


                animator.Update(0f);
            }
        }
    }


    // =========================================================
    // STUN
    // =========================================================

    public void ApplyStun(
        float duration,
        GameObject stunEffectPrefab,
        float effectYOffset)
    {
        if (enemyHealth != null &&
            enemyHealth.IsDying)
        {
            return;
        }


        if (duration <= 0f)
            return;


        // =====================================================
        // ODPORNOŚĆ NA STUN
        // =====================================================

        EnemyType enemyType =
            GetComponent<EnemyType>();


        if (enemyType != null)
        {
            duration *=
                enemyType.StunEffectMultiplier;
        }


        // 0 = pełna odporność
        if (duration <= 0f)
            return;


        if (stunCoroutine != null)
        {
            StopCoroutine(
                stunCoroutine
            );


            stunCoroutine = null;
        }


        DestroyStunEffect();


        float highestPoint =
            transform.position.y;


        if (renderers != null)
        {
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                    continue;


                if (renderer.bounds.max.y >
                    highestPoint)
                {
                    highestPoint =
                        renderer.bounds.max.y;
                }
            }
        }
        else
        {
            Renderer[] modelRenderers =
                GetComponentsInChildren<Renderer>();


            foreach (Renderer renderer in modelRenderers)
            {
                if (renderer == null)
                    continue;


                if (renderer.bounds.max.y >
                    highestPoint)
                {
                    highestPoint =
                        renderer.bounds.max.y;
                }
            }
        }


        Vector3 worldEffectPosition =
            new Vector3(
                transform.position.x,
                highestPoint,
                transform.position.z
            );


        Vector3 localEffectPosition =
            transform.InverseTransformPoint(
                worldEffectPosition
            );


        localEffectPosition.y +=
            effectYOffset;


        if (stunEffectPrefab != null)
        {
            activeStunEffect =
                Instantiate(
                    stunEffectPrefab,
                    transform
                );


            activeStunEffect.transform.localPosition =
                localEffectPosition;


            activeStunEffect.transform.localRotation =
                Quaternion.identity;
        }


        stunCoroutine =
            StartCoroutine(
                StunCoroutine(
                    duration
                )
            );
    }


    // =========================================================
    // STUN COROUTINE
    // =========================================================

    private IEnumerator StunCoroutine(
        float duration)
    {
        isStunned = true;


        isAttacking = false;

        attackTimer = 0f;


        if (animator != null)
        {
            animator.SetBool(
                attackBoolName,
                false
            );


            animator.SetFloat(
                "AttackSpeed",
                0f
            );


            animator.speed = 0f;
        }


        yield return new WaitForSeconds(
            duration
        );


        DestroyStunEffect();


        if (enemyHealth != null &&
            enemyHealth.IsDying)
        {
            isStunned = false;


            if (animator != null)
            {
                animator.speed = 1f;
            }


            stunCoroutine = null;


            yield break;
        }


        isStunned = false;

        stunCoroutine = null;


        if (animator != null)
        {
            animator.speed = 1f;
        }


        if (isFrozen)
        {
            UpdateAnimatorAttackSpeed();

            yield break;
        }


        if (isRooted)
        {
            UpdateAnimatorAttackSpeed();

            yield break;
        }


        if (isKnockedBack)
        {
            UpdateAnimatorAttackSpeed();

            yield break;
        }


        if (IsUnderWhirlOfChaos)
        {
            KeepWhirlWalkAnimation();
            yield break;
        }


        if (target == null)
        {
            UpdateAnimatorAttackSpeed();

            yield break;
        }


        Vector3 direction =
            target.position -
            transform.position;


        direction.y = 0f;


        float distanceToTower =
            direction.magnitude;


        if (distanceToTower <= attackRange)
        {
            isAttacking = true;


            ResetAttackTimer();


            if (animator != null)
            {
                animator.SetBool(
                    attackBoolName,
                    true
                );


                UpdateAnimatorAttackSpeed();
            }
        }
        else
        {
            isAttacking = false;


            if (animator != null)
            {
                animator.SetBool(
                    attackBoolName,
                    false
                );


                UpdateAnimatorAttackSpeed();
            }
        }
    }


    // =========================================================
    // SPRAWDZENIE STUN
    // =========================================================

    public bool IsStunned()
    {
        return isStunned;
    }


    // =========================================================
    // SPRAWDZENIE FROZEN
    // =========================================================

    public bool IsFrozen()
    {
        return isFrozen;
    }


    // =========================================================
    // ZWYKŁY SLOW
    // =========================================================

    public void ApplySlow(
        float slowPercent,
        float duration)
    {
        if (enemyHealth != null &&
            enemyHealth.IsDying)
        {
            return;
        }


        if (duration <= 0f)
            return;


        EnemyType enemyType =
            GetComponent<EnemyType>();


        if (enemyType != null)
        {
            // =================================================
            // ODPORNOŚĆ WPŁYWA NA SIŁĘ SLOW
            // =================================================

            slowPercent *=
                enemyType.SlowEffectMultiplier;


            // =================================================
            // ODPORNOŚĆ WPŁYWA RÓWNIEŻ NA CZAS SLOW
            // =================================================

            duration *=
                enemyType.SlowEffectMultiplier;
        }


        // 0 = pełna odporność
        if (duration <= 0f)
            return;


        if (slowCoroutine != null)
        {
            StopCoroutine(
                slowCoroutine
            );
        }


        slowCoroutine =
            StartCoroutine(
                SlowCoroutine(
                    slowPercent,
                    duration
                )
            );
    }


    // =========================================================
    // SLOW COROUTINE
    // =========================================================

    private IEnumerator SlowCoroutine(
        float slowPercent,
        float duration)
    {
        slowMultiplier =
            Mathf.Clamp01(
                1f - slowPercent
            );


        UpdateMovementSpeed();


        SetSlowColor();


        if (activeColdEffect == null &&
            coldEffectPrefab != null)
        {
            activeColdEffect =
                Instantiate(
                    coldEffectPrefab,
                    transform
                );


            activeColdEffect.transform.localPosition =
                Vector3.zero;


            activeColdEffect.transform.localRotation =
                Quaternion.identity;
        }


        yield return new WaitForSeconds(
            duration
        );


        slowMultiplier = 1f;


        UpdateMovementSpeed();


        RestoreOriginalColor();


        if (activeColdEffect != null)
        {
            Destroy(
                activeColdEffect
            );


            activeColdEffect = null;
        }


        slowCoroutine = null;
    }


    // =========================================================
    // SLOW COLOR
    // =========================================================

    private void SetSlowColor()
    {
        if (renderers == null)
            return;


        for (int i = 0;
             i < renderers.Length;
             i++)
        {
            if (renderers[i] != null &&
                renderers[i].material.HasProperty("_Color"))
            {
                renderers[i].material.color =
                    slowColor;
            }
        }
    }


    // =========================================================
    // RESTORE COLOR
    // =========================================================

    private void RestoreOriginalColor()
    {
        if (renderers == null)
            return;


        for (int i = 0;
             i < renderers.Length;
             i++)
        {
            if (renderers[i] != null &&
                renderers[i].material.HasProperty("_Color"))
            {
                renderers[i].material.color =
                    originalColors[i];
            }
        }
    }


    // =========================================================
    // CORRUPTED MOVE SPEED
    // =========================================================

    public void SetCorruptedMoveSpeedMultiplier(
        float multiplier)
    {
        corruptedMoveSpeedMultiplier =
            Mathf.Max(
                0f,
                multiplier
            );


        UpdateMovementSpeed();
    }


    // =========================================================
    // CORRUPTED ATTACK SPEED
    // =========================================================

    public void SetCorruptedAttackSpeedMultiplier(
        float multiplier)
    {
        corruptedAttackSpeedMultiplier =
            Mathf.Max(
                0f,
                multiplier
            );


        UpdateAttackSpeed();
    }


    // =========================================================
    // SLIME SLOW
    // =========================================================

    public void ApplySlimeSlow(
        float slowPercent)
    {
        if (enemyHealth != null &&
            enemyHealth.IsDying)
        {
            return;
        }


        float finalSlow =
            Mathf.Clamp01(
                slowPercent
            );


        EnemyType enemyType =
            GetComponent<EnemyType>();


        if (enemyType != null)
        {
            finalSlow *=
                enemyType.SlowEffectMultiplier;
        }


        slimeSlowMultiplier =
            Mathf.Clamp01(
                1f - finalSlow
            );


        UpdateMovementSpeed();
    }


    // =========================================================
    // USUNIĘCIE SLIME SLOW
    // =========================================================

    public void RemoveSlimeSlow()
    {
        slimeSlowMultiplier = 1f;


        UpdateMovementSpeed();
    }


    // =========================================================
    // DAMAGE AURA
    // =========================================================

    public void AddDamageAura(
        EnemyDamageAura aura,
        float bonusPercent)
    {
        if (aura == null)
            return;


        damageAuras[aura] =
            bonusPercent;
    }


    // =========================================================
    // REMOVE DAMAGE AURA
    // =========================================================

    public void RemoveDamageAura(
        EnemyDamageAura aura)
    {
        if (aura == null)
            return;


        damageAuras.Remove(
            aura
        );
    }


    // =========================================================
    // DAMAGE AURA MULTIPLIER
    // =========================================================

    private float GetDamageAuraMultiplier()
    {
        float multiplier = 1f;


        foreach (
            float bonusPercent
            in damageAuras.Values)
        {
            multiplier +=
                bonusPercent / 100f;
        }


        return multiplier;
    }


    // =========================================================
    // ANIMATOR PARAMETER
    // =========================================================

    private bool HasAnimatorParameter(
        string parameterName)
    {
        if (animator == null)
            return false;


        foreach (
            AnimatorControllerParameter parameter
            in animator.parameters)
        {
            if (parameter.name ==
                parameterName)
            {
                return true;
            }
        }


        return false;
    }


    // =========================================================
    // DESTROY STUN EFFECT
    // =========================================================

    private void DestroyStunEffect()
    {
        if (activeStunEffect != null)
        {
            Destroy(
                activeStunEffect
            );

            activeStunEffect = null;
        }
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void OnDestroy()
    {
        if (knockbackCoroutine != null)
        {
            StopCoroutine(
                knockbackCoroutine
            );


            knockbackCoroutine = null;
        }


        DestroyStunEffect();
    }
}